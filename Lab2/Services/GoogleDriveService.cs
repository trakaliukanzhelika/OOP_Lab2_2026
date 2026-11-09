using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace Lab2.Services
{
    public record DriveFileInfo(string Id, string Name);

    public class GoogleDriveService
    {
        private static readonly string[] Scopes = { DriveService.Scope.Drive };
        private const string ApplicationName = "MAUI XML Scientists Lab";

        private DriveService? _service;

        // User authorization (only once per app run)
        public async Task AuthorizeAsync()
        {
            if (_service != null)
                return;

            string json = $"{{\"installed\":{{\"client_id\":\"{Secrets.ClientId}\",\"client_secret\":\"{Secrets.ClientSecret}\"}}}}";
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

            // The token is stored locally, separately from Lab 1
            string tokenPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Lab2.GoogleDriveToken");

            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(tokenPath, true));

            _service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
        }

        // List Drive files with the given extensions
        public async Task<List<DriveFileInfo>> ListFilesAsync(params string[] extensions)
        {
            await AuthorizeAsync();

            var request = _service!.Files.List();
            request.Q = "trashed = false and mimeType != 'application/vnd.google-apps.folder'";
            request.Fields = "files(id, name)";
            request.PageSize = 1000;

            var result = await request.ExecuteAsync();

            return result.Files
                .Where(f => extensions.Any(ext => f.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .Select(f => new DriveFileInfo(f.Id, f.Name))
                .OrderBy(f => f.Name)
                .ToList();
        }

        // Download a file from Drive to the local disk
        public async Task DownloadFileAsync(string fileId, string localPath)
        {
            await AuthorizeAsync();

            var request = _service!.Files.Get(fileId);
            using var stream = new FileStream(localPath, FileMode.Create);
            await request.DownloadAsync(stream);
        }

        // Upload a file to Drive (overwrites an existing file with the same name)
        public async Task<string> UploadFileAsync(string fileName, Stream content)
        {
            await AuthorizeAsync();

            string mimeType = GetMimeType(fileName);

            var listRequest = _service!.Files.List();
            listRequest.Q = $"name = '{fileName.Replace("'", "\\'")}' and trashed = false";
            listRequest.Fields = "files(id)";
            DriveFile? existingFile = (await listRequest.ExecuteAsync()).Files.FirstOrDefault();

            var metadata = new DriveFile { Name = fileName };

            if (existingFile != null)
            {
                var updateRequest = _service.Files.Update(metadata, existingFile.Id, content, mimeType);
                EnsureCompleted(await updateRequest.UploadAsync());
                return existingFile.Id;
            }

            var createRequest = _service.Files.Create(metadata, content, mimeType);
            createRequest.Fields = "id";
            EnsureCompleted(await createRequest.UploadAsync());
            return createRequest.ResponseBody.Id;
        }

        private static void EnsureCompleted(IUploadProgress progress)
        {
            if (progress.Status != UploadStatus.Completed)
                throw progress.Exception ?? new IOException("Не вдалося завантажити файл на Google Drive.");
        }

        private static string GetMimeType(string fileName)
        {
            return Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".xml" => "application/xml",
                ".html" => "text/html",
                ".xsl" or ".xslt" => "application/xslt+xml",
                _ => "application/octet-stream"
            };
        }
    }
}

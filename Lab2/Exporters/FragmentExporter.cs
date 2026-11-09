using Lab2.Models;
using Lab2.Services;
using Lab2.Writers;

namespace Lab2.Exporters
{
    public abstract class FragmentExporter
    {
        protected readonly GoogleDriveService _drive;

        protected FragmentExporter(GoogleDriveService drive)
        {
            _drive = drive;
        }

        public async Task<string> ExportAsync(List<Scientist> data, string fileName)
        {
            // Open (create) the file
            string localPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            using var file = new FileStream(localPath, FileMode.Create, FileAccess.ReadWrite);

            // Factory method: the subclass decides which writer to create
            IFragmentWriter writer = CreateWriter();
            writer.Write(data, file);

            // Rewind to the start of the file and upload it to Google Drive
            file.Position = 0;
            return await _drive.UploadFileAsync(fileName, file);
        }

        protected abstract IFragmentWriter CreateWriter();
    }
}

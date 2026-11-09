using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Lab2.Exporters;
using Lab2.Logging;
using Lab2.Models;
using Lab2.Services;
using Lab2.Strategies;

namespace Lab2
{
    public partial class MainPage : ContentPage
    {
        private const string AnyValue = "(усі)";
        private const string FromComputer = "З комп'ютера";
        private const string FromDrive = "З Google Drive";

        private readonly XmlSearchContext _searchContext = new(new DomSearchStrategy());
        private readonly XslTransformer _transformer = new();
        private readonly GoogleDriveService _driveService = new();
        private string? _xmlPath;
        private List<Scientist> _results = new();

        public MainPage()
        {
            InitializeComponent();
            StrategyPicker.ItemsSource = new List<string> { "SAX API", "DOM API", "LINQ to XML" };
            StrategyPicker.SelectedIndex = 1;
        }

        // ---------- XML file selection ----------

        private async void OnChooseFileClicked(object? sender, EventArgs e)
        {
            string? path = await PickLocalFileAsync("Оберіть XML-файл", ".xml");
            if (path == null)
                return;

            await OpenXmlAsync(path, Path.GetFileName(path));
        }

        private async void OnOpenFromDriveClicked(object? sender, EventArgs e)
        {
            string? path;
            try
            {
                path = await DownloadFromDriveAsync(".xml");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Помилка Google Drive", ex.Message, "OK");
                return;
            }

            if (path == null)
                return;

            await OpenXmlAsync(path, $"{Path.GetFileName(path)} (Google Drive)");
        }

        private async Task OpenXmlAsync(string path, string displayName)
        {
            try
            {
                _xmlPath = path;
                FileLabel.Text = displayName;
                LoadAttributeValues();
            }
            catch (XmlException ex)
            {
                _xmlPath = null;
                FileLabel.Text = "Файл не обрано";
                await DisplayAlertAsync("Помилка", $"Некоректний XML-файл: {ex.Message}", "OK");
            }
        }

        private void LoadAttributeValues()
        {
            XDocument doc = XDocument.Load(_xmlPath!);
            List<XElement> scientists = doc.Root!.Elements("Scientist").ToList();

            FacultyPicker.ItemsSource = GetDistinctValues(scientists, "Faculty");
            DepartmentPicker.ItemsSource = GetDistinctValues(scientists, "Department");
            PositionPicker.ItemsSource = GetDistinctValues(scientists, "Position");
        }

        private static List<string> GetDistinctValues(List<XElement> elements, string attributeName)
        {
            List<string> values = elements
                .Select(e => (string?)e.Attribute(attributeName))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!)
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            values.Insert(0, AnyValue);
            return values;
        }

        // ---------- Search ----------

        private async void OnSearchClicked(object? sender, EventArgs e)
        {
            if (_xmlPath == null)
            {
                await DisplayAlertAsync("Помилка", "Спочатку оберіть XML-файл.", "OK");
                return;
            }

            _searchContext.SetStrategy(CreateSelectedStrategy());

            var criteria = new SearchCriteria
            {
                Faculty = GetSelectedValue(FacultyPicker),
                Department = GetSelectedValue(DepartmentPicker),
                Position = GetSelectedValue(PositionPicker),
                Keyword = KeywordEntry.Text?.Trim()
            };

            _results = _searchContext.ExecuteSearch(_xmlPath, criteria);

            Logger.Instance.Log(LogLevel.Filtering,
                $"Спосіб: {StrategyPicker.SelectedItem}. Параметри: {DescribeCriteria(criteria)}. Знайдено: {_results.Count}");

            if (_results.Count > 0)
            {
                ShowResults();
            }
            else
            {
                ResultsView.ItemsSource = null;
                ResultsLabel.Text = "Результати";
                await DisplayAlertAsync("Результат", "Нічого не знайдено.", "OK");
            }
        }

        private static string DescribeCriteria(SearchCriteria criteria)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(criteria.Faculty))
                parts.Add($"«Факультет» – {criteria.Faculty}");
            if (!string.IsNullOrWhiteSpace(criteria.Department))
                parts.Add($"«Кафедра» – {criteria.Department}");
            if (!string.IsNullOrWhiteSpace(criteria.Position))
                parts.Add($"«Посада» – {criteria.Position}");
            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
                parts.Add($"«П.І.П.» – {criteria.Keyword}");

            return parts.Count == 0 ? "не задано" : string.Join(", ", parts);
        }

        private IXmlSearchStrategy CreateSelectedStrategy()
        {
            return StrategyPicker.SelectedIndex switch
            {
                0 => new SaxSearchStrategy(),
                2 => new LinqSearchStrategy(),
                _ => new DomSearchStrategy()
            };
        }

        private static string? GetSelectedValue(Picker picker)
        {
            string? value = picker.SelectedItem as string;
            return value == AnyValue ? null : value;
        }

        private void ShowResults()
        {
            ResultsView.ItemsSource = _results;
            ResultsLabel.Text = $"Результати ({_results.Count})";
        }

        // ---------- XSL transformation to HTML ----------

        private async void OnTransformClicked(object? sender, EventArgs e)
        {
            if (_xmlPath == null)
            {
                await DisplayAlertAsync("Помилка", "Спочатку оберіть XML-файл.", "OK");
                return;
            }

            string? source = await DisplayActionSheetAsync("Звідки взяти XSL-файл?", "Скасувати", null,
                FromComputer, FromDrive);

            string? xslPath;
            try
            {
                if (source == FromComputer)
                    xslPath = await PickLocalFileAsync("Оберіть XSL-файл", ".xsl", ".xslt");
                else if (source == FromDrive)
                    xslPath = await DownloadFromDriveAsync(".xsl", ".xslt");
                else
                    return;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Помилка Google Drive", ex.Message, "OK");
                return;
            }

            if (xslPath == null)
                return;

            string htmlPath = Path.ChangeExtension(_xmlPath, ".html");

            try
            {
                _transformer.Transform(_xmlPath, xslPath, htmlPath);
                Logger.Instance.Log(LogLevel.Transformation, $"Збережено у файл {Path.GetFileName(htmlPath)}");
            }
            catch (Exception ex) when (ex is XsltException || ex is XmlException || ex is IOException)
            {
                await DisplayAlertAsync("Помилка", $"Не вдалося виконати трансформацію: {ex.Message}", "OK");
                return;
            }

            bool open = await DisplayAlertAsync("Трансформація",
                $"HTML збережено у файл:\n{htmlPath}\n\nВідкрити його?", "Так", "Ні");

            if (open)
                await Launcher.Default.OpenAsync(new OpenFileRequest("HTML", new ReadOnlyFile(htmlPath)));
        }

        // ---------- Saving to Google Drive ----------

        private async void OnSaveToDriveClicked(object? sender, EventArgs e)
        {
            if (_results.Count == 0)
            {
                await DisplayAlertAsync("Збереження", "Немає даних для збереження. Спочатку виконайте пошук.", "OK");
                return;
            }

            string? format = await DisplayActionSheetAsync("Формат збереження", "Скасувати", null, "XML", "HTML");
            if (format != "XML" && format != "HTML")
                return;

            string? name = await DisplayPromptAsync("Збереження", "Назва файлу (без розширення):",
                "OK", "Скасувати", initialValue: "filtered");
            if (string.IsNullOrWhiteSpace(name))
                return;

            string fileName = $"{name.Trim()}.{format.ToLowerInvariant()}";

            FragmentExporter exporter = format == "XML"
                ? new XmlFragmentExporter(_driveService)
                : new HtmlFragmentExporter(_driveService);

            try
            {
                await exporter.ExportAsync(_results, fileName);
                Logger.Instance.Log(LogLevel.Saving,
                    $"Збережено відфільтрований фрагмент у файл {fileName} на Google Drive");
                await DisplayAlertAsync("Збереження", $"Файл {fileName} збережено на Google Drive.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Помилка Google Drive", $"Не вдалося зберегти файл: {ex.Message}", "OK");
            }
        }

        // ---------- File helper methods ----------

        private static async Task<string?> PickLocalFileAsync(string title, params string[] extensions)
        {
            var fileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, extensions }
            });

            FileResult? file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = title,
                FileTypes = fileType
            });

            return file?.FullPath;
        }

        private async Task<string?> DownloadFromDriveAsync(params string[] extensions)
        {
            List<DriveFileInfo> files = await _driveService.ListFilesAsync(extensions);

            if (files.Count == 0)
            {
                await DisplayAlertAsync("Google Drive", "На Google Drive не знайдено відповідних файлів.", "OK");
                return null;
            }

            string[] names = files.Select(f => f.Name).ToArray();
            string? chosen = await DisplayActionSheetAsync("Оберіть файл з Google Drive", "Скасувати", null, names);

            DriveFileInfo? file = files.FirstOrDefault(f => f.Name == chosen);
            if (file == null)
                return null;

            string localPath = Path.Combine(FileSystem.CacheDirectory, file.Name);
            await _driveService.DownloadFileAsync(file.Id, localPath);
            return localPath;
        }

        // ---------- Clear, log, exit ----------

        private void OnClearClicked(object? sender, EventArgs e)
        {
            FacultyPicker.SelectedIndex = -1;
            DepartmentPicker.SelectedIndex = -1;
            PositionPicker.SelectedIndex = -1;
            KeywordEntry.Text = string.Empty;

            _results = new List<Scientist>();
            ResultsView.ItemsSource = null;
            ResultsLabel.Text = "Результати";
        }

        private async void OnOpenLogClicked(object? sender, EventArgs e)
        {
            string path = Logger.Instance.LogFilePath;

            if (!File.Exists(path))
            {
                await DisplayAlertAsync("Протокол", "Протокол ще порожній.", "OK");
                return;
            }

            await Launcher.Default.OpenAsync(new OpenFileRequest("Протокол", new ReadOnlyFile(path)));
        }

        private async void OnExitClicked(object? sender, EventArgs e)
        {
            bool confirm = await DisplayAlertAsync("Вихід",
                "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні");

            if (confirm)
            {
                App.ExitConfirmed = true;
                Application.Current?.Quit();
            }
        }
    }
}

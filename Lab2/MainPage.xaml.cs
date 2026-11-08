using System.Xml;
using System.Xml.Linq;
using Lab2.Models;
using Lab2.Strategies;

namespace Lab2
{
    public partial class MainPage : ContentPage
    {
        private const string AnyValue = "(усі)";

        private readonly XmlSearchContext _searchContext = new(new DomSearchStrategy());
        private string? _xmlPath;
        private List<Scientist> _results = new();

        public MainPage()
        {
            InitializeComponent();
            StrategyPicker.ItemsSource = new List<string> { "SAX API", "DOM API", "LINQ to XML" };
            StrategyPicker.SelectedIndex = 1;
        }

        private async void OnChooseFileClicked(object sender, EventArgs e)
        {
            var xmlType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".xml" } }
            });

            FileResult? file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть XML-файл",
                FileTypes = xmlType
            });

            if (file == null)
                return;

            try
            {
                _xmlPath = file.FullPath;
                FileLabel.Text = file.FileName;
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

        private async void OnSearchClicked(object sender, EventArgs e)
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

        private void OnClearClicked(object sender, EventArgs e)
        {
            FacultyPicker.SelectedIndex = -1;
            DepartmentPicker.SelectedIndex = -1;
            PositionPicker.SelectedIndex = -1;
            KeywordEntry.Text = string.Empty;

            _results = new List<Scientist>();
            ResultsView.ItemsSource = null;
            ResultsLabel.Text = "Результати";
        }

        private async void OnExitClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlertAsync("Вихід",
                "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні");

            if (confirm)
                Application.Current?.Quit();
        }
    }
}

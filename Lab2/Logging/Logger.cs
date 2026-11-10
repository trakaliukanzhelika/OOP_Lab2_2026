namespace Lab2.Logging
{
    public sealed class Logger
    {
        private static Logger? _instance;
        private static readonly object _lock = new();

        private readonly string _logFilePath;

        private Logger()
        {
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _logFilePath = Path.Combine(documents, "Lab2_log.txt");
        }

        public static Logger Instance
        {
            get
            {
                lock (_lock)
                {
                    _instance ??= new Logger();
                    return _instance;
                }
            }
        }

        public string LogFilePath => _logFilePath;

        public void Log(LogLevel level, string message)
        {
            string line = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} {GetLevelName(level)}. {message}";

            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_logFilePath, line + Environment.NewLine);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    // Logging must never crash the application
                    System.Diagnostics.Debug.WriteLine($"Log write failed: {ex.Message}");
                }
            }
        }

        private static string GetLevelName(LogLevel level)
        {
            return level switch
            {
                LogLevel.Filtering => "Фільтрація",
                LogLevel.Transformation => "Трансформація",
                LogLevel.Saving => "Збереження",
                _ => level.ToString()
            };
        }
    }
}

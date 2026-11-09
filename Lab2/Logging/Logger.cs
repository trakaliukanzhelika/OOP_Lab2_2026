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
                File.AppendAllText(_logFilePath, line + Environment.NewLine);
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

using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace Lab2
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureLifecycleEvents(events =>
                {
#if WINDOWS
                    events.AddWindows(windows => windows.OnWindowCreated(window =>
                    {
                        bool isAsking = false;

                        // Ask for confirmation when the window is closed with the X button
                        window.AppWindow.Closing += async (sender, args) =>
                        {
                            if (App.ExitConfirmed)
                                return;

                            // Cancel closing until the user answers
                            args.Cancel = true;

                            if (isAsking)
                                return;

                            Page? page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                            if (page == null)
                                return;

                            isAsking = true;
                            bool confirm = await page.DisplayAlertAsync("Вихід",
                                "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні");
                            isAsking = false;

                            if (confirm)
                            {
                                App.ExitConfirmed = true;
                                Microsoft.Maui.Controls.Application.Current?.Quit();
                            }
                        };
                    }));
#endif
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

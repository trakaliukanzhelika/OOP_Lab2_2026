using Microsoft.Extensions.DependencyInjection;

namespace Lab2
{
    public partial class App : Application
    {
        // Set to true when the user has confirmed exit, so the window can close
        public static bool ExitConfirmed { get; set; }

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
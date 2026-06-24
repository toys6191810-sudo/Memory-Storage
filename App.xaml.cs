using Microsoft.Extensions.DependencyInjection;

namespace Memory_Storage
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            UserAppTheme = AppUi.IsDarkMode ? AppTheme.Dark : AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell())
            {
                Title = AppUi.T("AppTitle")
            };
        }
    }
}

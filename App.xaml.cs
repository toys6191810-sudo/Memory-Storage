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
            if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            {
                var mobileRoot = new NavigationPage(new MobileMainPage())
                {
                    BarBackgroundColor = AppUi.PageBackground,
                    BarTextColor = AppUi.Text
                };

                NavigationPage.SetHasNavigationBar(mobileRoot.CurrentPage, false);

                return new Window(mobileRoot)
                {
                    Title = AppUi.T("AppTitle")
                };
            }

            return new Window(new AppShell())
            {
                Title = AppUi.T("AppTitle")
            };
        }
    }
}

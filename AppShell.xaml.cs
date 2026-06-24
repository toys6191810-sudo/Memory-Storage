namespace Memory_Storage
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            AppUi.Changed += AppUi_Changed;
            ApplyUi();
        }

        private void AppUi_Changed(object? sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(ApplyUi);
        }

        private void ApplyUi()
        {
            Title = AppUi.T("AppTitle");

            foreach (var item in Items)
            {
                item.Title = AppUi.T("AppTitle");
            }
        }
    }
}

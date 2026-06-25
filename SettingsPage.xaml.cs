namespace Memory_Storage;

public partial class SettingsPage : ContentPage
{
    private bool isLoading;

    public SettingsPage()
    {
        InitializeComponent();
        AppUi.Changed += AppUi_Changed;
        LoadSettings();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyUi();
    }

    private void LoadSettings()
    {
        isLoading = true;
        LanguagePicker.Items.Clear();

        foreach (var languageName in AppUi.LanguageNames)
        {
            LanguagePicker.Items.Add(languageName);
        }

        LanguagePicker.SelectedIndex = (int)AppUi.CurrentLanguage;
        ThemeSwitch.IsToggled = AppUi.IsDarkMode;
        isLoading = false;
        ApplyUi();
    }

    private void AppUi_Changed(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(ApplyUi);
    }

    private void LanguagePicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (isLoading || LanguagePicker.SelectedIndex < 0)
        {
            return;
        }

        AppUi.SetLanguage((AppLanguage)LanguagePicker.SelectedIndex);
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Settings")} ➡︎ {AppUi.T("Language")} ➡︎ {LanguagePicker.SelectedItem}");
    }

    private void ThemeSwitch_Toggled(object? sender, ToggledEventArgs e)
    {
        if (isLoading)
        {
            return;
        }

        AppUi.SetDarkMode(e.Value);
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Settings")} ➡︎ {(e.Value ? AppUi.T("DarkMode") : AppUi.T("LightMode"))}");
    }

    private void ApplyUi()
    {
        SetValue(NavigationPage.BarBackgroundColorProperty, AppUi.IsDarkMode ? Color.FromArgb("#F7F8FA") : Color.FromArgb("#EAF7EF"));
        SetValue(NavigationPage.BarTextColorProperty, AppUi.IsDarkMode ? Color.FromArgb("#111816") : Color.FromArgb("#14171A"));

        BackgroundColor = AppUi.PageBackground;
        RootGrid.BackgroundColor = AppUi.PageBackground;
        SettingsCard.BackgroundColor = AppUi.Surface;
        SettingsCard.Stroke = AppUi.Border;
        ThemePanel.BackgroundColor = AppUi.IsDarkMode ? Color.FromArgb("#223029") : Color.FromArgb("#EAF5EF");
        ThemePanel.Stroke = AppUi.IsDarkMode ? Color.FromArgb("#3B5146") : Color.FromArgb("#D8E9DF");

        TitleLabel.Text = AppUi.T("Settings");
        LanguageLabel.Text = AppUi.T("Language");
        LanguagePicker.Title = AppUi.T("ChooseLanguage");
        ThemeTitleLabel.Text = AppUi.IsDarkMode ? AppUi.T("DarkMode") : AppUi.T("LightMode");
        ThemeSubtitleLabel.Text = AppUi.IsDarkMode ? AppUi.T("LightMode") : AppUi.T("DarkMode");

        TitleLabel.TextColor = AppUi.Text;
        LanguageLabel.TextColor = AppUi.MutedText;
        ThemeTitleLabel.TextColor = AppUi.IsDarkMode ? Colors.White : AppUi.Text;
        ThemeSubtitleLabel.TextColor = AppUi.IsDarkMode ? Color.FromArgb("#D7E7DE") : AppUi.MutedText;
    }
}

namespace Memory_Storage;

public partial class MobileMainPage : ContentPage
{
    private bool hasPlayedIntro;
    private IDispatcherTimer? geometryTimer;
    private double geometryProgress;

    public MobileMainPage()
    {
        InitializeComponent();
        AppUi.Changed += AppUi_Changed;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyUi();
        StartGeometryAnimation();
        _ = PlayIntroAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        geometryTimer?.Stop();
    }

    private void AppUi_Changed(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(ApplyUi);
    }

    private async void StartButton_Clicked(object? sender, EventArgs e)
    {
        await UiAnimations.PressAsync(sender);
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Start")} -> {AppUi.T("RecordOpenedPrograms")}");
        await Navigation.PushAsync(new MobileActivityPage());
    }

    private void ApplyUi()
    {
        var homeBackground = AppUi.IsDarkMode ? Color.FromArgb("#102018") : Color.FromArgb("#EAF7EF");
        BackgroundColor = homeBackground;
        RootGrid.BackgroundColor = homeBackground;
        EyebrowPill.BackgroundColor = AppUi.IsDarkMode ? Color.FromArgb("#1C3328") : Color.FromArgb("#DDF3E8");
        EyebrowLabel.Text = AppUi.T("HomeEyebrow");
        EyebrowLabel.TextColor = AppUi.Primary;
        TitleLabel.Text = AppUi.T("AppTitle");
        TitleLabel.TextColor = AppUi.Text;
        TaglineLabel.Text = AppUi.T("Tagline");
        TaglineLabel.TextColor = AppUi.MutedText;
        OpenedHintLabel.Text = ShortOpenedProgramsLabel();
        TimelineHintLabel.Text = ShortTimelineLabel();
        StartButton.Text = AppUi.T("Start");
        GeometryBackground.Invalidate();

        if (Window is not null)
        {
            Window.Title = AppUi.T("AppTitle");
        }
    }

    private static string ShortOpenedProgramsLabel()
    {
        return AppUi.CurrentLanguage switch
        {
            AppLanguage.SimplifiedChinese => "开启记录",
            AppLanguage.TraditionalChinese => "開啟記錄",
            AppLanguage.Japanese => "起動記録",
            AppLanguage.Korean => "열림 기록",
            AppLanguage.German => "Startprotokoll",
            AppLanguage.French => "Ouvertures",
            AppLanguage.Italian => "Aperture",
            AppLanguage.BritishEnglish => "Open records",
            _ => "Records"
        };
    }

    private static string ShortTimelineLabel()
    {
        return AppUi.CurrentLanguage switch
        {
            AppLanguage.SimplifiedChinese => "时间线",
            AppLanguage.TraditionalChinese => "時間軸",
            AppLanguage.Japanese => "時系列",
            AppLanguage.Korean => "타임라인",
            AppLanguage.German => "Zeitleiste",
            AppLanguage.French => "Chronologie",
            AppLanguage.Italian => "Sequenza",
            AppLanguage.BritishEnglish => "Timeline",
            _ => "Timeline"
        };
    }

    private void StartGeometryAnimation()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            GeometryBackground.AnimationProgress = 0.65;
            GeometryBackground.Invalidate();
            return;
        }

        if (geometryTimer is not null)
        {
            geometryTimer.Start();
            return;
        }

        geometryTimer = Dispatcher.CreateTimer();
        geometryTimer.Interval = TimeSpan.FromMilliseconds(33);
        geometryTimer.Tick += (_, _) =>
        {
            geometryProgress += 0.018;
            GeometryBackground.AnimationProgress = geometryProgress;
            GeometryBackground.Invalidate();
        };
        geometryTimer.Start();
    }

    private async Task PlayIntroAsync()
    {
        if (hasPlayedIntro)
        {
            return;
        }

        hasPlayedIntro = true;
        TitleLabel.Opacity = 0;
        TaglineLabel.Opacity = 0;
        StartButton.Opacity = 0;

        await TitleLabel.FadeToAsync(1, 180, Easing.CubicOut);
        await Task.WhenAll(
            TaglineLabel.FadeToAsync(1, 180, Easing.CubicOut),
            StartButton.FadeToAsync(1, 220, Easing.CubicOut));
    }
}

using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Memory_Storage;

public partial class MobileActivityPage : ContentPage
{
    private string currentFeatureKey = "UsedApps";
    private string lastConfirmedSearchText = string.Empty;
    private bool isFeaturePanelExpanded = true;
    private bool isDeleteMode;
    private bool hasShownUsageAccessPrompt;
    private bool hasShownAccessibilityAccessPrompt;
    private IDispatcherTimer? durationRefreshTimer;

    public ObservableCollection<MemoryRecord> VisibleRecords { get; } = new();

    public bool IsDeleteMode
    {
        get => isDeleteMode;
        private set
        {
            if (isDeleteMode == value)
            {
                return;
            }

            isDeleteMode = value;
            OnPropertyChanged();
            ApplyDeleteModeUi();
        }
    }

    public MobileActivityPage()
    {
        InitializeComponent();
        BindingContext = this;
        MemoryRecordStore.RecordsChanged += MemoryRecordStore_RecordsChanged;
        MemoryRecordStore.RecordingStateChanged += MemoryRecordStore_RecordingStateChanged;
        AppUi.Changed += AppUi_Changed;
        UserProfileStore.Changed += UserProfileStore_Changed;
        ActivityTrackerService.Start();
        StartDurationRefreshTimer();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        lastConfirmedSearchText = RecordSearchBar.Text?.Trim() ?? string.Empty;
        ApplyUi();
        LoadRecords(lastConfirmedSearchText);
        RefreshFeaturePanel(currentFeatureKey);
        StartDurationRefreshTimer();
        _ = EnsureAndroidUsageAccessAsync();
        _ = EnsureAndroidAccessibilityAccessAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        durationRefreshTimer?.Stop();
    }

    private void AppUi_Changed(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ApplyUi();
            MemoryRecordStore.RefreshLocalizedRecords();
            LoadRecords(RecordSearchBar.Text);
            RefreshFeaturePanel(currentFeatureKey);
        });
    }

    private void MemoryRecordStore_RecordsChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LoadRecords(RecordSearchBar.Text);
            RefreshFeaturePanel(currentFeatureKey);
        });
    }

    private void MemoryRecordStore_RecordingStateChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ApplyRecordingToggleUi();
            RefreshFeaturePanel(currentFeatureKey);
        });
    }

    private void UserProfileStore_Changed(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateAvatarDisplay);
    }

    private async void BackButton_Clicked(object? sender, EventArgs e)
    {
        await UiAnimations.PressAsync(sender);
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Back")} -> {AppUi.T("AppTitle")}");
        await Navigation.PopAsync();
    }

    private async void LoginButton_Clicked(object? sender, EventArgs e)
    {
        await UiAnimations.PressAsync(sender);
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Login")} -> {AppUi.T("LoginProfile")}");
        await Navigation.PushAsync(new LoginPage());
    }

    private async void RegisterButton_Clicked(object? sender, EventArgs e)
    {
        await UiAnimations.PressAsync(sender);
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Register")} -> {AppUi.T("RegisterProfile")}");
        await Navigation.PushAsync(new RegisterPage());
    }

    private async void AvatarButton_Clicked(object? sender, TappedEventArgs e)
    {
        await UiAnimations.PressAsync(AvatarButton);

        if (UserProfileStore.IsLoggedIn)
        {
            MemoryRecordStore.AddSelfOperation($"{AppUi.T("Profile")} -> {AppUi.T("UserName")}");
            await Navigation.PushAsync(new ProfilePage());
            return;
        }

        await DisplayAlertAsync($"✕ {AppUi.T("Warning")}", AppUi.T("AvatarLoginRequired"), "OK");
    }

    private async void SettingsButton_Clicked(object? sender, TappedEventArgs e)
    {
        await UiAnimations.PressAsync(SettingsButton);
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Settings")} -> {AppUi.T("ChooseLanguage")}");
        await Navigation.PushAsync(new SettingsPage());
    }

    private async void FeatureButton_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        await UiAnimations.PressAsync(button);
        RefreshFeaturePanel(button.StyleId);
        ApplyNavigationSelection();
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Functions")} -> {button.Text}");
    }

    private async void FeatureExpandButton_Clicked(object? sender, EventArgs e)
    {
        await UiAnimations.PressAsync(sender);
        isFeaturePanelExpanded = !isFeaturePanelExpanded;
        ApplyFeaturePanelExpansion();
    }

    private async void RecordingToggleButton_Tapped(object? sender, TappedEventArgs e)
    {
        MemoryRecordStore.SetRecordingEnabled(!MemoryRecordStore.IsRecordingEnabled);
        await RecordingToggleButton.ScaleToAsync(0.94, 70, Easing.CubicOut);
        await RecordingToggleButton.ScaleToAsync(1, 140, Easing.SpringOut);
    }

    private async void AppUserGuideButton_Clicked(object? sender, EventArgs e)
    {
        await UiAnimations.PressAsync(sender);
        MemoryRecordStore.AddSelfOperation(AppUi.T("AppUserGuide"));
        await DisplayAlertAsync(AppUi.T("AppUserGuide"), AppUi.T("AppUserGuideBody"), AppUi.T("Done"));
    }

    private async void PrivacyStatementButton_Clicked(object? sender, EventArgs e)
    {
        await UiAnimations.PressAsync(sender);
        MemoryRecordStore.AddSelfOperation(AppUi.T("PrivacyStatement"));
        await DisplayAlertAsync(AppUi.T("PrivacyStatement"), AppUi.T("PrivacyStatementBody"), AppUi.T("Done"));
    }

    private async void TrashButton_Tapped(object? sender, TappedEventArgs e)
    {
        await UiAnimations.PressAsync(TrashButton);
        IsDeleteMode = !IsDeleteMode;
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Trash")} -> {AppUi.T("DeleteMode")}");
    }

    private async void ClearSelectedLabel_Tapped(object? sender, TappedEventArgs e)
    {
        await ClearSelectedRecordsAsync();
    }

    private async void ClearAllLabel_Tapped(object? sender, TappedEventArgs e)
    {
        await ClearAllRecordsAsync();
    }

    private void RecordSearchBar_SearchButtonPressed(object? sender, EventArgs e)
    {
        lastConfirmedSearchText = RecordSearchBar.Text?.Trim() ?? string.Empty;
        LoadRecords(lastConfirmedSearchText);
        RefreshFeaturePanel("Search");
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("SearchLabel")} -> {lastConfirmedSearchText}");
    }

    private void RecordSearchBar_TextChanged(object? sender, TextChangedEventArgs e)
    {
        LoadRecords(e.NewTextValue);

        if (currentFeatureKey == "Search")
        {
            RefreshFeaturePanel("Search");
        }
    }

    private async void RecordCollectionView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }

        if (e.CurrentSelection.FirstOrDefault() is not MemoryRecord record)
        {
            return;
        }

        if (IsDeleteMode)
        {
            record.IsSelectedForDeletion = !record.IsSelectedForDeletion;
            return;
        }

        MemoryRecordStore.AddSelfOperation($"{AppUi.T("MemoryRecordFile")} -> {record.CabinetCode} -> {record.DisplayAppName}");
        await Navigation.PushAsync(new RecordDetailPage(record));
    }

    private void LoadRecords(string? keyword)
    {
        VisibleRecords.Clear();

        foreach (var record in MemoryRecordStore.GetRecords(keyword ?? string.Empty))
        {
            VisibleRecords.Add(record);
        }
    }

    private void RefreshFeaturePanel(string? feature)
    {
        currentFeatureKey = feature ?? "UsedApps";
        var searchKeyword = currentFeatureKey == "Search" ? lastConfirmedSearchText : string.Empty;
        var records = MemoryRecordStore.GetRecords(searchKeyword);

        switch (currentFeatureKey)
        {
            case "Duration":
                FeatureTitleLabel.Text = AppUi.T("RecordActivityDuration");
                FeatureContentLabel.Text = BuildDurationContent(records);
                break;
            case "Timeline":
                FeatureTitleLabel.Text = AppUi.T("TimelineView");
                FeatureContentLabel.Text = BuildTimelineContent(records);
                break;
            case "Search":
                FeatureTitleLabel.Text = AppUi.T("SearchFunction");
                FeatureContentLabel.Text = BuildSearchContent(records, lastConfirmedSearchText);
                break;
            default:
                FeatureTitleLabel.Text = AppUi.T("RecordOpenedPrograms");
                FeatureContentLabel.Text = BuildOpenedProgramsContent(records);
                break;
        }
    }

    private static string BuildOpenedProgramsContent(IReadOnlyList<MemoryRecord> records)
    {
        var entries = GetOperationEntries(records)
            .GroupBy(entry => new
            {
                AppName = entry.AppName.Trim().ToLowerInvariant(),
                Minute = entry.HappenedAt.ToString("HH:mm", CultureInfo.InvariantCulture)
            })
            .Select(group => group.OrderBy(entry => entry.HappenedAt).First())
            .OrderBy(entry => entry.HappenedAt)
            .ToList();

        return entries.Count == 0
            ? AppUi.T("OpenedEmpty")
            : string.Join(Environment.NewLine, entries.Select(entry => $"{entry.HappenedAt:HH:mm} {AppUi.T("Opened")} {entry.AppName}"));
    }

    private static string BuildDurationContent(IReadOnlyList<MemoryRecord> records)
    {
        var totals = GetOperationSegments(records)
            .GroupBy(segment => segment.AppName, StringComparer.OrdinalIgnoreCase)
            .Select(group => new
            {
                AppName = group.First().AppName,
                Duration = group.Aggregate(TimeSpan.Zero, (total, segment) => total + segment.Duration)
            })
            .Where(item => item.Duration > TimeSpan.Zero)
            .OrderByDescending(item => item.Duration)
            .ThenBy(item => item.AppName)
            .ToList();

        return totals.Count == 0
            ? AppUi.T("DurationEmpty")
            : string.Join(Environment.NewLine, totals.Select(item => $"{item.AppName}: {FormatDuration(item.Duration)}"));
    }

    private static string BuildTimelineContent(IReadOnlyList<MemoryRecord> records)
    {
        var header = DateTime.Today.ToString("yyyy/MM/dd (dddd)", AppUi.Culture);
        var segments = GetOperationSegments(records).ToList();

        if (segments.Count == 0)
        {
            return $"{header}{Environment.NewLine}   {AppUi.T("TimelineEmpty")}";
        }

        return $"{header}{Environment.NewLine}{string.Join(Environment.NewLine, segments.Select(segment => $"   {segment.HappenedAt:HH:mm}-{segment.EndedAt:HH:mm} {segment.Operation}"))}";
    }

    private static string BuildSearchContent(IReadOnlyList<MemoryRecord> records, string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return AppUi.T("SearchFirst");
        }

        if (records.Count == 0)
        {
            return $"{AppUi.T("SearchLabel")}: {keyword}{Environment.NewLine}{Environment.NewLine}{AppUi.T("FoundLabel")}:{Environment.NewLine}{AppUi.T("NoSearchResults")}";
        }

        var lines = records.Select(record => $"{AppUi.T("FoundLabel")}: {record.CabinetCode}");
        return $"{AppUi.T("SearchLabel")}: {keyword}{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, lines)}";
    }

    private static IEnumerable<OperationEntry> GetOperationEntries(IReadOnlyList<MemoryRecord> records)
    {
        foreach (var record in records.OrderBy(record => record.StartedAt))
        {
            if (string.IsNullOrWhiteSpace(record.Note))
            {
                yield return new OperationEntry(record.StartedAt, record.DisplayAppName, $"{AppUi.T("Opened")} : {record.DisplayAppName}");
                continue;
            }

            foreach (var line in record.LocalizedNote.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                yield return ParseOperationEntry(record, line);
            }
        }
    }

    private static OperationEntry ParseOperationEntry(MemoryRecord record, string line)
    {
        var happenedAt = record.StartedAt;
        var operation = line.Trim();

        if (operation.Length >= 5 && TimeSpan.TryParseExact(operation[..5], "hh\\:mm", CultureInfo.InvariantCulture, out var time))
        {
            happenedAt = record.StartedAt.Date.Add(time);
            operation = operation[5..].Trim();
        }

        operation = Regex.Replace(operation, @"^(->|-|:|\s)+", string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(operation))
        {
            operation = $"{AppUi.T("Opened")} : {record.DisplayAppName}";
        }

        return new OperationEntry(happenedAt, record.DisplayAppName, operation);
    }

    private static IEnumerable<OperationSegment> GetOperationSegments(IReadOnlyList<MemoryRecord> records)
    {
        var entries = GetOperationEntries(records)
            .OrderBy(entry => entry.HappenedAt)
            .ThenBy(entry => entry.AppName)
            .ToList();

        for (var index = 0; index < entries.Count; index++)
        {
            var entry = entries[index];
            var end = index + 1 < entries.Count ? entries[index + 1].HappenedAt : DateTime.Now;

            if (end < entry.HappenedAt)
            {
                end = entry.HappenedAt;
            }

            yield return new OperationSegment(entry.HappenedAt, end, entry.AppName, entry.Operation);
        }
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalMinutes < 1)
        {
            return $"{Math.Max(1, (int)duration.TotalSeconds)} {SecondsLabel()}";
        }

        if (duration.TotalHours < 1)
        {
            return $"{Math.Max(1, (int)duration.TotalMinutes)} {AppUi.T("Minutes")}";
        }

        var hours = (int)duration.TotalHours;
        var hourLabel = hours == 1 ? AppUi.T("Hour") : AppUi.T("Hours");
        return $"{hours} {hourLabel} {duration.Minutes} {AppUi.T("Minutes")}";
    }

    private static string SecondsLabel()
    {
        return AppUi.CurrentLanguage switch
        {
            AppLanguage.SimplifiedChinese => "秒",
            AppLanguage.TraditionalChinese => "秒",
            AppLanguage.Japanese => "秒",
            AppLanguage.Korean => "초",
            AppLanguage.German => "Sekunden",
            AppLanguage.French => "secondes",
            AppLanguage.Italian => "secondi",
            _ => "seconds"
        };
    }

    private void ApplyUi()
    {
        BackgroundColor = AppUi.PageBackground;
        RootGrid.BackgroundColor = AppUi.PageBackground;
        TitleLabel.Text = AppUi.T("AppTitle");
        TitleLabel.TextColor = AppUi.Text;
        TodayLabel.Text = $"{AppUi.T("Today")} {DateTime.Today.ToString("MM/dd (ddd)", AppUi.Culture)}";
        TodayLabel.TextColor = AppUi.MutedText;
        BackButton.Text = AppUi.T("Back");
        LoginButton.Text = AppUi.T("Login");
        RegisterButton.Text = AppUi.T("Register");
        RecordSearchBar.Placeholder = AppUi.T("SearchPlaceholder");
        OpenedProgramsButton.Text = AppUi.T("RecordOpenedPrograms");
        DurationButton.Text = AppUi.T("RecordActivityDuration");
        TimelineButton.Text = AppUi.T("TimelineView");
        SearchFeatureButton.Text = AppUi.T("SearchFunction");
        ClearSelectedLabel.Text = AppUi.T("ClearSelectedItems");
        ClearAllLabel.Text = AppUi.T("ClearAllItems");
        NoRecordsLabel.Text = AppUi.T("NoRecords");
        FreshSessionLabel.Text = AppUi.T("FreshSession");
        AppUserGuideButton.Text = AppUi.T("AppUserGuide");
        PrivacyStatementButton.Text = AppUi.T("PrivacyStatement");

        FeaturePanel.BackgroundColor = AppUi.Surface;
        FeaturePanel.Stroke = AppUi.Border;
        FeatureTitleLabel.TextColor = AppUi.Text;
        FeatureContentLabel.TextColor = AppUi.MutedText;
        AvatarButton.BackgroundColor = AppUi.SoftSurface;
        SettingsButton.BackgroundColor = AppUi.SoftSurface;
        TrashButton.BackgroundColor = AppUi.Surface;
        TrashButton.Stroke = AppUi.Border;
        BackButton.BackgroundColor = AppUi.Primary;
        LoginButton.BackgroundColor = AppUi.Primary;
        RegisterButton.BackgroundColor = AppUi.Primary;
        FeatureExpandButton.BackgroundColor = AppUi.SoftSurface;
        FeatureExpandButton.TextColor = AppUi.Primary;

        UpdateAvatarDisplay();
        ApplyNavigationSelection();
        ApplyFeaturePanelExpansion();
        ApplyRecordingToggleUi();
        ApplyDeleteModeUi();

        if (Window is not null)
        {
            Window.Title = AppUi.T("AppTitle");
        }
    }

    private void ApplyNavigationSelection()
    {
        foreach (var button in new[] { OpenedProgramsButton, DurationButton, TimelineButton, SearchFeatureButton })
        {
            var selected = button.StyleId == currentFeatureKey;
            button.BackgroundColor = selected ? AppUi.Primary : AppUi.SoftSurface;
            button.TextColor = selected ? Colors.White : AppUi.Primary;
            button.FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None;
        }
    }

    private void ApplyFeaturePanelExpansion()
    {
        FeatureContentScroll.IsVisible = isFeaturePanelExpanded;
        FeatureExpandButton.Text = isFeaturePanelExpanded ? "⌃" : "⌄";
    }

    private void ApplyDeleteModeUi()
    {
        DeleteLinksPanel.IsVisible = IsDeleteMode;

        if (!IsDeleteMode)
        {
            foreach (var record in VisibleRecords)
            {
                record.IsSelectedForDeletion = false;
            }
        }
    }

    private void ApplyRecordingToggleUi()
    {
        var isEnabled = MemoryRecordStore.IsRecordingEnabled;
        RecordingToggleCard.BackgroundColor = Color.FromArgb("#111816");
        RecordingToggleCard.Stroke = Color.FromArgb("#263A33");
        RecordingToggleButton.BackgroundColor = isEnabled ? AppUi.Primary : Color.FromArgb("#D84B3A");
        RecordingToggleStateLabel.Text = RecordingToggleStateText(isEnabled);
        RecordingToggleStateLabel.TextColor = isEnabled ? Color.FromArgb("#6FE0A5") : Color.FromArgb("#FF9A8D");
        ApplyRecordingToggleTitle();

        RecordingToggleThumb.AbortAnimation("MobileRecordingThumbSlide");
        new Animation(value => RecordingToggleThumb.TranslationX = value, RecordingToggleThumb.TranslationX, isEnabled ? 42 : 0)
            .Commit(RecordingToggleThumb, "MobileRecordingThumbSlide", 16, 170, Easing.CubicOut);
    }

    private void ApplyRecordingToggleTitle()
    {
        var title = RecordingToggleTitleParts();
        RecordingToggleLabel.FormattedText = new FormattedString
        {
            Spans =
            {
                new Span { Text = title.OnText, TextColor = Color.FromArgb("#20C978") },
                new Span { Text = title.Separator, TextColor = Colors.White },
                new Span { Text = title.OffText, TextColor = Color.FromArgb("#FF5A48") },
                new Span { Text = title.Suffix, TextColor = Colors.White }
            }
        };
    }

    private static (string OnText, string Separator, string OffText, string Suffix) RecordingToggleTitleParts()
    {
        return AppUi.CurrentLanguage switch
        {
            AppLanguage.SimplifiedChinese => ("开启", "/", "关闭", " 记录"),
            AppLanguage.TraditionalChinese => ("開啟", "/", "關閉", " 記錄"),
            AppLanguage.Japanese => ("オン", "/", "オフ", " 記録"),
            AppLanguage.Korean => ("켜기", "/", "끄기", " 기록"),
            AppLanguage.German => ("Ein", "/", "Aus", " Aufnahme"),
            AppLanguage.French => ("Activer", "/", "Desactiver", " l'enregistrement"),
            AppLanguage.Italian => ("Attiva", "/", "Disattiva", " registrazione"),
            _ => ("On", "/", "Off", " Record")
        };
    }

    private static string RecordingToggleStateText(bool isEnabled)
    {
        return AppUi.CurrentLanguage switch
        {
            AppLanguage.SimplifiedChinese => isEnabled ? "记录开启" : "记录关闭",
            AppLanguage.TraditionalChinese => isEnabled ? "記錄開啟" : "記錄關閉",
            AppLanguage.Japanese => isEnabled ? "記録オン" : "記録オフ",
            AppLanguage.Korean => isEnabled ? "기록 켜짐" : "기록 꺼짐",
            AppLanguage.German => isEnabled ? "Aufnahme EIN" : "Aufnahme AUS",
            AppLanguage.French => isEnabled ? "Enregistrement actif" : "Enregistrement inactif",
            AppLanguage.Italian => isEnabled ? "Registrazione attiva" : "Registrazione disattivata",
            _ => isEnabled ? "Recording ON" : "Recording OFF"
        };
    }

    private void UpdateAvatarDisplay()
    {
        var hasAvatar = !string.IsNullOrWhiteSpace(UserProfileStore.AvatarPath) && File.Exists(UserProfileStore.AvatarPath);
        AvatarImage.IsVisible = hasAvatar;
        AvatarIcon.IsVisible = !hasAvatar;

        if (hasAvatar)
        {
            AvatarImage.Source = ImageSource.FromFile(UserProfileStore.AvatarPath);
        }
    }

    private async Task ClearSelectedRecordsAsync()
    {
        var selected = VisibleRecords.Where(record => record.IsSelectedForDeletion).ToList();

        if (selected.Count == 0)
        {
            await DisplayAlertAsync(AppUi.T("DeleteMode"), AppUi.T("NoSelectedItems"), AppUi.T("Done"));
            return;
        }

        var confirmed = await DisplayAlertAsync(AppUi.T("DeleteMode"), AppUi.T("ConfirmClearSelected"), AppUi.T("Clear"), AppUi.T("Cancel"));

        if (!confirmed)
        {
            return;
        }

        MemoryRecordStore.DeleteRecords(selected);
        IsDeleteMode = false;
    }

    private async Task ClearAllRecordsAsync()
    {
        if (VisibleRecords.Count == 0)
        {
            return;
        }

        var confirmed = await DisplayAlertAsync(AppUi.T("DeleteMode"), AppUi.T("ConfirmClearAll"), AppUi.T("Clear"), AppUi.T("Cancel"));

        if (!confirmed)
        {
            return;
        }

        MemoryRecordStore.DeleteAllRecords();
        IsDeleteMode = false;
    }

    private void StartDurationRefreshTimer()
    {
        if (durationRefreshTimer is not null)
        {
            durationRefreshTimer.Start();
            return;
        }

        durationRefreshTimer = Dispatcher.CreateTimer();
        durationRefreshTimer.Interval = TimeSpan.FromSeconds(1);
        durationRefreshTimer.Tick += (_, _) =>
        {
            if (currentFeatureKey == "Duration")
            {
                RefreshFeaturePanel("Duration");
            }
        };
        durationRefreshTimer.Start();
    }

    private async Task EnsureAndroidUsageAccessAsync()
    {
#if ANDROID
        if (hasShownUsageAccessPrompt || AndroidUsageTracker.HasUsageAccess())
        {
            return;
        }

        hasShownUsageAccessPrompt = true;
        var openSettings = await DisplayAlert(
            AppUi.T("AppTitle"),
            AndroidUsageAccessMessage(),
            AppUi.T("Settings"),
            AppUi.T("Cancel"));

        if (openSettings)
        {
            AndroidUsageTracker.OpenUsageAccessSettings();
        }
#else
        await Task.CompletedTask;
#endif
    }

    private async Task EnsureAndroidAccessibilityAccessAsync()
    {
#if ANDROID
        if (hasShownAccessibilityAccessPrompt || AndroidUsageTracker.HasAccessibilityAccess())
        {
            return;
        }

        hasShownAccessibilityAccessPrompt = true;
        var openSettings = await DisplayAlert(
            AppUi.T("MobileInteractionAccessTitle"),
            AppUi.T("MobileInteractionAccessMessage"),
            AppUi.T("Settings"),
            AppUi.T("Cancel"));

        if (openSettings)
        {
            AndroidUsageTracker.OpenAccessibilitySettings();
        }
#else
        await Task.CompletedTask;
#endif
    }

    private static string AndroidUsageAccessMessage()
    {
        return AppUi.CurrentLanguage switch
        {
            AppLanguage.SimplifiedChinese => "若要记录手机上开启过的应用程序，请在系统设置中允许 Memory Storage 使用「使用情况存取」。",
            AppLanguage.TraditionalChinese => "若要記錄手機上開啟過的 App，請在系統設定中允許 Memory Storage 使用「使用情況存取」。",
            AppLanguage.Japanese => "スマートフォンで開いたアプリを記録するには、システム設定で Memory Storage の使用状況へのアクセスを許可してください。",
            AppLanguage.Korean => "휴대폰에서 연 앱을 기록하려면 시스템 설정에서 Memory Storage의 사용 정보 접근을 허용해 주세요.",
            AppLanguage.German => "Um geoeffnete Apps auf dem Smartphone zu erfassen, erlaube Memory Storage in den Systemeinstellungen den Nutzungszugriff.",
            AppLanguage.French => "Pour enregistrer les applications ouvertes sur le telephone, autorisez l'acces aux donnees d'utilisation pour Memory Storage dans les reglages systeme.",
            AppLanguage.Italian => "Per registrare le app aperte sul telefono, consenti a Memory Storage l'accesso ai dati di utilizzo nelle impostazioni di sistema.",
            AppLanguage.BritishEnglish => "To record apps opened on your mobile, allow Memory Storage usage access in system settings.",
            _ => "To record apps opened on your phone, allow Memory Storage usage access in system settings."
        };
    }

    private sealed record OperationEntry(DateTime HappenedAt, string AppName, string Operation);

    private sealed record OperationSegment(DateTime HappenedAt, DateTime EndedAt, string AppName, string Operation)
    {
        public TimeSpan Duration => EndedAt - HappenedAt;
    }
}

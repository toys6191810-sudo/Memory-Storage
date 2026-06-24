using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Memory_Storage;

public partial class ActivityPage : ContentPage
{
    private string currentFeatureTitle = AppUi.T("RecordOpenedPrograms");
    private string currentFeatureContent = AppUi.T("OpenedEmpty");
    private string currentFeatureKey = "UsedApps";
    private string lastConfirmedSearchText = string.Empty;
    private bool hasPlayedSidebarIntro;
    private bool isFeaturePanelExpanded = true;
    private bool isDeleteMode;
    private IDispatcherTimer? durationRefreshTimer;

    public ObservableCollection<MemoryRecord> VisibleRecords { get; } = new();

    public string TodayTitle => $"{AppUi.T("Today")} {DateTime.Today.ToString("MM/dd (ddd)", AppUi.Culture)}";

    public string CurrentFeatureTitle
    {
        get => currentFeatureTitle;
        private set
        {
            currentFeatureTitle = value;
            OnPropertyChanged();
        }
    }

    public string CurrentFeatureContent
    {
        get => currentFeatureContent;
        private set
        {
            currentFeatureContent = value;
            OnPropertyChanged();
        }
    }

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

    public ActivityPage()
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
        LoadRecords(lastConfirmedSearchText);
        ApplyUi();
        RefreshFeaturePanel(currentFeatureKey);
        StartDurationRefreshTimer();
        _ = PlaySidebarIntroAsync();
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
            OnPropertyChanged(nameof(TodayTitle));
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

    private void UserProfileStore_Changed(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateAvatarDisplay);
    }

    private void MemoryRecordStore_RecordingStateChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ApplyRecordingToggleUi();
            RefreshFeaturePanel(currentFeatureKey);
        });
    }

    private async void BackButton_Clicked(object? sender, EventArgs e)
    {
        await UiAnimations.PressAsync(sender);
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Back")} ➡︎ {AppUi.T("AppTitle")}");
        await Navigation.PopAsync();
    }

    private async void LoginButton_Clicked(object? sender, EventArgs e)
    {
        await UiAnimations.PressAsync(sender);
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Login")} ➡︎ {AppUi.T("LoginProfile")}");
        await Navigation.PushAsync(new LoginPage());
    }

    private async void RegisterButton_Clicked(object? sender, EventArgs e)
    {
        await UiAnimations.PressAsync(sender);
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Register")} ➡︎ {AppUi.T("RegisterProfile")}");
        await Navigation.PushAsync(new RegisterPage());
    }

    private async void AvatarButton_Clicked(object? sender, TappedEventArgs e)
    {
        await UiAnimations.PressAsync(AvatarButton);

        if (UserProfileStore.IsLoggedIn)
        {
            MemoryRecordStore.AddSelfOperation($"{AppUi.T("Profile")} ➡︎ {AppUi.T("UserName")}");
            await Navigation.PushAsync(new ProfilePage());
            return;
        }

        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Profile")} ➡︎ {AppUi.T("Warning")}");
        await DisplayAlertAsync($"✕ {AppUi.T("Warning")}", AppUi.T("AvatarLoginRequired"), "OK");
    }

    private async void SettingsButton_Clicked(object? sender, TappedEventArgs e)
    {
        await UiAnimations.PressAsync(SettingsButton);
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Settings")} ➡︎ {AppUi.T("ChooseLanguage")}");
        await Navigation.PushAsync(new SettingsPage());
    }

    private async void FeatureButton_Clicked(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            await UiAnimations.PressAsync(sender);
            RefreshFeaturePanel(button.StyleId);
            ApplyNavigationSelection();
            MemoryRecordStore.AddSelfOperation($"{AppUi.T("Functions")} ➡︎ {button.Text}");
        }
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
        await PressInfoButtonAsync(AppUserGuideButton);
        MemoryRecordStore.AddSelfOperation(AppUi.T("AppUserGuide"));
        await DisplayAlertAsync(
            AppUi.T("AppUserGuide"),
            AppUi.T("AppUserGuideBody"),
            AppUi.T("Done"));
    }

    private async void PrivacyStatementButton_Clicked(object? sender, EventArgs e)
    {
        await PressInfoButtonAsync(PrivacyStatementButton);
        MemoryRecordStore.AddSelfOperation(AppUi.T("PrivacyStatement"));
        await DisplayAlertAsync(
            AppUi.T("PrivacyStatement"),
            AppUi.T("PrivacyStatementBody"),
            AppUi.T("Done"));
    }

    private async void TrashButton_Tapped(object? sender, TappedEventArgs e)
    {
        await UiAnimations.PressAsync(TrashButton);
        IsDeleteMode = !IsDeleteMode;
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Trash")} → {AppUi.T("DeleteMode")}");
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
        LoadRecords(RecordSearchBar.Text);
        RefreshFeaturePanel("Search");
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("SearchLabel")} ➡︎ {RecordSearchBar.Text}");
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

        if (e.CurrentSelection.FirstOrDefault() is MemoryRecord record)
        {
            if (IsDeleteMode)
            {
                record.IsSelectedForDeletion = !record.IsSelectedForDeletion;
                return;
            }

            MemoryRecordStore.AddSelfOperation($"{AppUi.T("MemoryRecordFile")} ➡︎ {record.CabinetCode} ➡︎ {record.DisplayAppName}");
            await Navigation.PushAsync(new RecordDetailPage(record));
        }
    }

    private void LoadRecords(string? keyword)
    {
        var records = MemoryRecordStore.GetRecords(keyword ?? string.Empty);

        VisibleRecords.Clear();

        foreach (var record in records)
        {
            VisibleRecords.Add(record);
        }
    }

    private void RefreshFeaturePanel(string? feature)
    {
        currentFeatureKey = feature ?? "UsedApps";
        var searchKeyword = currentFeatureKey == "Search" ? lastConfirmedSearchText : string.Empty;
        var records = MemoryRecordStore.GetRecords(searchKeyword);

        switch (feature)
        {
            case "Duration":
                CurrentFeatureTitle = AppUi.T("RecordActivityDuration");
                CurrentFeatureContent = BuildDurationContent(records);
                break;
            case "Timeline":
                CurrentFeatureTitle = AppUi.T("TimelineView");
                CurrentFeatureContent = BuildTimelineContent(records);
                break;
            case "Search":
                CurrentFeatureTitle = AppUi.T("SearchFunction");
                CurrentFeatureContent = BuildSearchContent(records, lastConfirmedSearchText);
                break;
            default:
                CurrentFeatureTitle = AppUi.T("RecordOpenedPrograms");
                CurrentFeatureContent = BuildOpenedProgramsContent(records);
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

        if (entries.Count == 0)
        {
            return AppUi.T("OpenedEmpty");
        }

        return string.Join(Environment.NewLine, entries.Select(entry =>
            $"{entry.HappenedAt:HH:mm} {AppUi.T("Opened")} {entry.AppName}"));
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

        if (totals.Count == 0)
        {
            return AppUi.T("DurationEmpty");
        }

        return string.Join(Environment.NewLine, totals.Select(item =>
            $"{item.AppName}: {FormatDuration(item.Duration)}"));
    }

    private static string BuildTimelineContent(IReadOnlyList<MemoryRecord> records)
    {
        var header = $"▌ {DateTime.Today.ToString("yyyy/MM/dd (dddd)", AppUi.Culture)}";
        var segments = GetOperationSegments(records).ToList();

        if (segments.Count == 0)
        {
            return $"{header}{Environment.NewLine}   {AppUi.T("TimelineEmpty")}";
        }

        var lines = segments.Select(segment =>
            $"   {segment.HappenedAt:HH:mm}-{segment.EndedAt:HH:mm} {segment.Operation}");

        return $"{header}{Environment.NewLine}{string.Join(Environment.NewLine, lines)}";
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

        var lines = records.Select(BuildFoundCabinetLine);

        return $"{AppUi.T("SearchLabel")}: {keyword}{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, lines)}";
    }

    private static IEnumerable<OperationEntry> GetOperationEntries(IReadOnlyList<MemoryRecord> records)
    {
        foreach (var record in records.OrderBy(record => record.StartedAt))
        {
            if (string.IsNullOrWhiteSpace(record.Note))
            {
                yield return new OperationEntry(record.StartedAt, record.DisplayAppName, $"{AppUi.T("Opened")} : {record.DisplayAppName}", record);
                continue;
            }

            foreach (var line in record.Note.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                yield return ParseOperationEntry(record, line);
            }
        }
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
            var end = index + 1 < entries.Count
                ? entries[index + 1].HappenedAt
                : DateTime.Now;

            if (end < entry.HappenedAt)
            {
                end = entry.HappenedAt;
            }

            yield return new OperationSegment(entry.HappenedAt, end, entry.AppName, entry.Operation);
        }
    }

    private static OperationEntry ParseOperationEntry(MemoryRecord record, string line)
    {
        var happenedAt = record.StartedAt;
        var operation = line.Trim();

        if (operation.Length >= 5
            && TimeSpan.TryParseExact(operation[..5], "hh\\:mm", CultureInfo.InvariantCulture, out var time))
        {
            happenedAt = record.StartedAt.Date.Add(time);
            operation = operation[5..].Trim();
        }

        operation = Regex.Replace(operation, @"^(➡|➡︎|→|>|-|\?|∴|\s)+", string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(operation))
        {
            operation = $"{AppUi.T("Opened")} : {record.DisplayAppName}";
        }

        return new OperationEntry(happenedAt, record.DisplayAppName, operation, record);
    }

    private static string BuildFoundCabinetLine(MemoryRecord record)
    {
        return AppUi.CurrentLanguage switch
        {
            AppLanguage.SimplifiedChinese => $"找到: 编号为 {record.CabinetCode} 的文件档案柜",
            AppLanguage.TraditionalChinese => $"找到: 編號為 {record.CabinetCode} 的文件檔案櫃",
            AppLanguage.Japanese => $"見つかった項目: 番号 {record.CabinetCode} の記録キャビネット",
            AppLanguage.Korean => $"찾음: 번호 {record.CabinetCode} 기록 캐비닛",
            AppLanguage.German => $"Gefunden: Aktenschrank Nr. {record.CabinetCode}",
            AppLanguage.French => $"Trouve : classeur numero {record.CabinetCode}",
            AppLanguage.Italian => $"Trovato: schedario numero {record.CabinetCode}",
            _ => $"Found: cabinet {record.CabinetCode}"
        };
    }

    private sealed record OperationEntry(DateTime HappenedAt, string AppName, string Operation, MemoryRecord Record);

    private sealed record OperationSegment(DateTime HappenedAt, DateTime EndedAt, string AppName, string Operation)
    {
        public TimeSpan Duration => EndedAt - HappenedAt;
    }

    private static string BuildContextSuffix(MemoryRecord? record)
    {
        if (record is null)
        {
            return string.Empty;
        }

        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(record.ProjectName) && record.ProjectName != "Foreground app")
        {
            parts.Add(record.ProjectName);
        }

        if (!string.IsNullOrWhiteSpace(record.FileName) && record.FileName != "App launch")
        {
            parts.Add(record.FileName);
        }

        return parts.Count == 0 ? string.Empty : $" ➡ {FileItemLabel()} : {string.Join(" ➡ ", parts.Distinct(StringComparer.OrdinalIgnoreCase))}";
    }

    private static string FileItemLabel()
    {
        return AppUi.CurrentLanguage switch
        {
            AppLanguage.SimplifiedChinese => "文件项目",
            AppLanguage.TraditionalChinese => "檔案項目",
            AppLanguage.Japanese => "ファイル項目",
            AppLanguage.Korean => "파일 항목",
            AppLanguage.German => "Dateielement",
            AppLanguage.French => "Element fichier",
            AppLanguage.Italian => "Elemento file",
            _ => "File item"
        };
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalMinutes < 1)
        {
            return $"{Math.Max(1, (int)duration.TotalSeconds)} {SecondsLabel()}";
        }

        var hours = (int)duration.TotalHours;
        var minutes = Math.Max(0, duration.Minutes);

        if (hours == 0)
        {
            return $"{Math.Max(1, (int)duration.TotalMinutes)} {AppUi.T("Minutes")}";
        }

        var hourLabel = hours == 1 ? AppUi.T("Hour") : AppUi.T("Hours");
        return $"{hours} {hourLabel} {minutes} {AppUi.T("Minutes")}";
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
        SidebarBorder.BackgroundColor = AppUi.Surface;
        SidebarBorder.Stroke = AppUi.Border;
        FeaturePanel.BackgroundColor = AppUi.Surface;
        FeaturePanel.Stroke = AppUi.Border;
        FeatureExpandButton.BackgroundColor = AppUi.SoftSurface;
        FeatureExpandButton.TextColor = AppUi.Primary;

        BackButton.Text = AppUi.T("Back");
        FunctionsLabel.Text = AppUi.T("Functions");
        SidePanelLabel.Text = AppUi.T("SidePanel");
        OpenedProgramsButton.Text = AppUi.T("RecordOpenedPrograms");
        DurationButton.Text = AppUi.T("RecordActivityDuration");
        TimelineButton.Text = AppUi.T("TimelineView");
        SearchFeatureButton.Text = AppUi.T("SearchFunction");
        AppUserGuideButton.Text = AppUi.T("AppUserGuide");
        PrivacyStatementButton.Text = AppUi.T("PrivacyStatement");
        ApplyRecordingToggleTitle();
        LoginButton.Text = AppUi.T("Login");
        RegisterButton.Text = AppUi.T("Register");
        RecordSearchBar.Placeholder = AppUi.T("SearchPlaceholder");
        TitleLabel.Text = AppUi.T("AppTitle");
        NoRecordsLabel.Text = AppUi.T("NoRecords");
        FreshSessionLabel.Text = AppUi.T("FreshSession");
        ClearSelectedLabel.Text = AppUi.T("ClearSelectedItems");
        ClearAllLabel.Text = AppUi.T("ClearAllItems");
        UpdateAvatarDisplay();

        if (Window is not null)
        {
            Window.Title = AppUi.T("AppTitle");
        }

        TitleLabel.TextColor = AppUi.Text;
        TodayLabel.TextColor = AppUi.MutedText;
        FunctionsLabel.TextColor = AppUi.Text;
        SidePanelLabel.TextColor = AppUi.MutedText;
        FeatureTitleLabel.TextColor = AppUi.Text;
        FeatureContentLabel.TextColor = AppUi.MutedText;
        TrashButton.BackgroundColor = AppUi.Surface;
        TrashButton.Stroke = AppUi.Border;
        ClearSelectedLabel.TextColor = AppUi.Primary;
        ClearAllLabel.TextColor = Color.FromArgb("#B42318");
        ApplyFeaturePanelExpansion();
        ApplyRecordingToggleUi();
        ApplyDeleteModeUi();

        var menuButtons = new[] { OpenedProgramsButton, DurationButton, TimelineButton, SearchFeatureButton };
        foreach (var button in menuButtons)
        {
            button.BackgroundColor = AppUi.SoftSurface;
            button.TextColor = AppUi.Primary;
        }

        AvatarButton.BackgroundColor = AppUi.SoftSurface;
        SettingsButton.BackgroundColor = AppUi.SoftSurface;
        AvatarIcon.Invalidate();
        SettingsIcon.Invalidate();

        BackButton.BackgroundColor = AppUi.Primary;
        BackButton.TextColor = Colors.White;
        StyleInfoButton(AppUserGuideButton);
        StyleInfoButton(PrivacyStatementButton);
        ApplyNavigationSelection();

        OnPropertyChanged(nameof(TodayTitle));
    }

    private void ApplyDeleteModeUi()
    {
        DeleteLinksPanel.IsVisible = IsDeleteMode;
        TrashButton.Scale = IsDeleteMode ? 1.06 : 1;
        TrashButton.BackgroundColor = IsDeleteMode ? Color.FromArgb("#EEF2F0") : AppUi.Surface;

        if (!IsDeleteMode)
        {
            foreach (var record in VisibleRecords)
            {
                record.IsSelectedForDeletion = false;
            }
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

        var confirmed = await DisplayAlertAsync(
            AppUi.T("DeleteMode"),
            AppUi.T("ConfirmClearSelected"),
            AppUi.T("Clear"),
            AppUi.T("Cancel"));

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

        var confirmed = await DisplayAlertAsync(
            AppUi.T("DeleteMode"),
            AppUi.T("ConfirmClearAll"),
            AppUi.T("Clear"),
            AppUi.T("Cancel"));

        if (!confirmed)
        {
            return;
        }

        MemoryRecordStore.DeleteAllRecords();
        IsDeleteMode = false;
    }

    private void ApplyRecordingToggleUi()
    {
        var isEnabled = MemoryRecordStore.IsRecordingEnabled;
        var accentColor = isEnabled ? AppUi.Primary : Color.FromArgb("#D84B3A");

        RecordingToggleCard.BackgroundColor = Color.FromArgb("#111816");
        RecordingToggleCard.Stroke = Color.FromArgb("#263A33");
        ApplyRecordingToggleTitle();
        RecordingToggleStateLabel.Text = RecordingToggleStateText(isEnabled);
        RecordingToggleStateLabel.TextColor = isEnabled ? Color.FromArgb("#6FE0A5") : Color.FromArgb("#FF9A8D");
        RecordingToggleButton.BackgroundColor = accentColor;
        RecordingToggleThumb.AbortAnimation("RecordingThumbSlide");
        new Animation(value => RecordingToggleThumb.TranslationX = value, RecordingToggleThumb.TranslationX, isEnabled ? 46 : 0)
            .Commit(RecordingToggleThumb, "RecordingThumbSlide", 16, 170, Easing.CubicOut);
    }

    private static void StyleInfoButton(Button button)
    {
        button.BackgroundColor = Color.FromArgb("#C9362B");
        button.TextColor = Colors.White;
        button.FontAttributes = FontAttributes.Bold;
        button.Shadow = new Shadow
        {
            Brush = Color.FromArgb("#552B0000"),
            Offset = new Point(0, 6),
            Radius = 12,
            Opacity = 0.35f
        };
    }

    private static async Task PressInfoButtonAsync(Button button)
    {
        button.AbortAnimation("InfoButtonPress");
        await Task.WhenAll(
            button.ScaleToAsync(0.94, 70, Easing.CubicOut),
            button.FadeToAsync(0.86, 70, Easing.CubicOut));
        await Task.WhenAll(
            button.ScaleToAsync(1, 160, Easing.SpringOut),
            button.FadeToAsync(1, 130, Easing.CubicOut));
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
            AppLanguage.BritishEnglish => ("On", "/", "Off", " Record"),
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
            AppLanguage.BritishEnglish => isEnabled ? "Recording ON" : "Recording OFF",
            _ => isEnabled ? "Recording ON" : "Recording OFF"
        };
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

    private void ApplyFeaturePanelExpansion()
    {
        FeatureContentScroll.IsVisible = isFeaturePanelExpanded;
        FeatureExpandButton.Text = isFeaturePanelExpanded ? "⌃" : "⌄";
        FeaturePanel.HeightRequest = isFeaturePanelExpanded ? -1 : 58;
    }

    private async Task PickAvatarImageAsync()
    {
        var image = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = AppUi.T("ChooseAvatarImage"),
            FileTypes = FilePickerFileType.Images
        });

        if (image is null)
        {
            return;
        }

        UserProfileStore.AvatarPath = image.FullPath;
        UpdateAvatarDisplay();
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

    private void ApplyNavigationSelection()
    {
        var navigationButtons = new[] { OpenedProgramsButton, DurationButton, TimelineButton, SearchFeatureButton };

        foreach (var button in navigationButtons)
        {
            var isSelected = button.StyleId == currentFeatureKey;
            button.BackgroundColor = isSelected ? AppUi.Primary : AppUi.SoftSurface;
            button.TextColor = isSelected ? Colors.White : AppUi.Primary;
            button.FontAttributes = isSelected ? FontAttributes.Bold : FontAttributes.None;
            button.Scale = isSelected ? 1.02 : 1;
        }
    }

    private async Task PlaySidebarIntroAsync()
    {
        if (hasPlayedSidebarIntro)
        {
            return;
        }

        hasPlayedSidebarIntro = true;
        SidebarStack.Opacity = 0;
        SidebarStack.TranslationX = -18;

        await Task.WhenAll(
            SidebarStack.FadeToAsync(1, 320, Easing.CubicOut),
            SidebarStack.TranslateToAsync(0, 0, 320, Easing.CubicOut));

        var buttons = new[] { BackButton, OpenedProgramsButton, DurationButton, TimelineButton, SearchFeatureButton, AppUserGuideButton, PrivacyStatementButton };

        foreach (var button in buttons)
        {
            button.Scale = 0.96;
            await button.ScaleToAsync(button.StyleId == currentFeatureKey ? 1.02 : 1, 110, Easing.CubicOut);
        }
    }
}

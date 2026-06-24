namespace Memory_Storage;

public partial class AddRecordPage : ContentPage
{
    public AddRecordPage()
    {
        InitializeComponent();
        AppUi.Changed += AppUi_Changed;

        var now = DateTime.Now;
        MeridiemPicker.SelectedItem = now.Hour >= 12 ? "PM" : "AM";
        HourEntry.Text = (now.Hour % 12 == 0 ? 12 : now.Hour % 12).ToString("00");
        MinuteEntry.Text = now.Minute.ToString("00");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyUi();
    }

    private void AppUi_Changed(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(ApplyUi);
    }

    private void MoveToMinute_Completed(object? sender, EventArgs e)
    {
        MinuteEntry.Focus();
    }

    private void MoveToAppName_Completed(object? sender, EventArgs e)
    {
        AppNameEntry.Focus();
    }

    private void MoveToProjectName_Completed(object? sender, EventArgs e)
    {
        ProjectNameEntry.Focus();
    }

    private void MoveToFileName_Completed(object? sender, EventArgs e)
    {
        FileNameEntry.Focus();
    }

    private void MoveToNote_Completed(object? sender, EventArgs e)
    {
        NoteEditor.Focus();
    }

    private async void SaveButton_Clicked(object? sender, EventArgs e)
    {
        await UiAnimations.PressAsync(sender);

        if (string.IsNullOrWhiteSpace(AppNameEntry.Text))
        {
            await DisplayAlertAsync("Missing app name", "Enter at least one app name, such as Chrome or VS Code.", "OK");
            return;
        }

        if (!TryGetSelectedTime(out var selectedTime))
        {
            await DisplayAlertAsync("Invalid time", "Enter a valid English AM/PM time.", "OK");
            return;
        }

        MemoryRecordStore.Add(MemoryRecordStore.CreateRecord(
            DateTime.Today.Add(selectedTime),
            AppNameEntry.Text.Trim(),
            ProjectNameEntry.Text?.Trim() ?? string.Empty,
            FileNameEntry.Text?.Trim() ?? string.Empty,
            NoteEditor.Text?.Trim() ?? string.Empty));

        await Navigation.PopAsync();
    }

    private bool TryGetSelectedTime(out TimeSpan selectedTime)
    {
        selectedTime = default;

        if (!int.TryParse(HourEntry.Text, out var hour)
            || !int.TryParse(MinuteEntry.Text, out var minute)
            || hour < 1
            || hour > 12
            || minute < 0
            || minute > 59)
        {
            return false;
        }

        var meridiem = MeridiemPicker.SelectedItem?.ToString() ?? "AM";

        if (meridiem == "PM" && hour < 12)
        {
            hour += 12;
        }
        else if (meridiem == "AM" && hour == 12)
        {
            hour = 0;
        }

        selectedTime = new TimeSpan(hour, minute, 0);
        return true;
    }

    private void ApplyUi()
    {
        BackgroundColor = AppUi.PageBackground;
        RootScroll.BackgroundColor = AppUi.PageBackground;
        FormStack.BackgroundColor = AppUi.PageBackground;

        TitleLabel.Text = AppUi.T("AddRecord");
        TimeLabel.Text = AppUi.T("Time");
        AppNameLabel.Text = AppUi.T("AppName");
        ProjectNameLabel.Text = AppUi.T("ProjectName");
        FileNameLabel.Text = AppUi.T("FileName");
        NoteLabel.Text = AppUi.T("Note");
        SaveButton.Text = AppUi.T("Save");

        foreach (var label in new[] { TitleLabel, TimeLabel, AppNameLabel, ProjectNameLabel, FileNameLabel, NoteLabel })
        {
            label.TextColor = label == TitleLabel ? AppUi.Text : AppUi.MutedText;
        }
    }
}

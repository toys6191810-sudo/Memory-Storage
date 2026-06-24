using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Memory_Storage;

public sealed class MemoryRecord : INotifyPropertyChanged
{
    public string CabinetCode { get; set; } = string.Empty;

    public DateTime StartedAt { get; init; }

    public DateTime? EndedAt { get; set; }

    public string AppName { get; init; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string Note { get; set; } = string.Empty;

    private bool isSelectedForDeletion;

    [JsonIgnore]
    public bool IsSelectedForDeletion
    {
        get => isSelectedForDeletion;
        set
        {
            if (isSelectedForDeletion == value)
            {
                return;
            }

            isSelectedForDeletion = value;
            OnPropertyChanged();
        }
    }

    public string TimeText => StartedAt.ToString("HH:mm", CultureInfo.InvariantCulture);

    public string EndTimeText => EndedAt?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? AppUi.T("Now");

    public string DurationText
    {
        get
        {
            var duration = (EndedAt ?? DateTime.Now) - StartedAt;

            if (duration.TotalMinutes < 1)
            {
                return AppUi.T("LessThanOneMinute");
            }

            if (duration.TotalHours < 1)
            {
                return $"{Math.Max(1, (int)duration.TotalMinutes)} {AppUi.T("Minutes")}";
            }

            var hours = (int)duration.TotalHours;
            var hourLabel = hours == 1 ? AppUi.T("Hour") : AppUi.T("Hours");
            return $"{hours} {hourLabel} {duration.Minutes} {AppUi.T("Minutes")}";
        }
    }

    public string DateBadge => StartedAt.ToString("yyyy/MM/dd ddd", AppUi.Culture);

    public string FileTitle => AppUi.T("MemoryRecordFile");

    public string DisplayAppName => AppUi.DisplayAppName(AppName);

    public string LocalizedNote => AppUi.LocalizeRecordText(Note);

    public string DetailText
    {
        get
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(ProjectName))
            {
                parts.Add($"{AppUi.T("Project")}: {ProjectName}");
            }

            if (!string.IsNullOrWhiteSpace(FileName))
            {
                parts.Add($"{AppUi.T("File")}: {FileName}");
            }

            return parts.Count == 0 ? AppUi.T("NoProjectOrFile") : string.Join("  ", parts);
        }
    }

    public bool HasNote => !string.IsNullOrWhiteSpace(Note);

    public event PropertyChangedEventHandler? PropertyChanged;

    public void RefreshLocalizedText()
    {
        OnPropertyChanged(nameof(FileTitle));
        OnPropertyChanged(nameof(DateBadge));
        OnPropertyChanged(nameof(DisplayAppName));
        OnPropertyChanged(nameof(DetailText));
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(EndTimeText));
        OnPropertyChanged(nameof(Note));
        OnPropertyChanged(nameof(LocalizedNote));
        OnPropertyChanged(nameof(HasNote));
        OnPropertyChanged(nameof(IsSelectedForDeletion));
    }

    public void AppendOperation(DateTime happenedAt, string operation)
    {
        if (string.IsNullOrWhiteSpace(operation))
        {
            return;
        }

        var line = $"{happenedAt:HH:mm} ➡ {operation}";
        Note = string.IsNullOrWhiteSpace(Note) ? line : $"{Note}{Environment.NewLine}{line}";
        EndedAt = happenedAt;
        OnPropertyChanged(nameof(Note));
        OnPropertyChanged(nameof(LocalizedNote));
        OnPropertyChanged(nameof(HasNote));
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(EndTimeText));
    }

    public bool Contains(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return true;
        }

        return AppName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || CabinetCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || ProjectName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || FileName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || Note.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || LocalizedNote.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || DurationText.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || StartedAt.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture).Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

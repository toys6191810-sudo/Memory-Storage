using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json;

namespace Memory_Storage;

public static class MemoryRecordStore
{
    private static readonly DateTime SessionStartedAt = DateTime.Now;
    private static int nextNumber = 2;
    private const string SelfAppName = "Memory Storage";
    private static readonly string RecordsDirectory = Path.Combine(FileSystem.AppDataDirectory, "profile-records");

    private static readonly ObservableCollection<MemoryRecord> Records = new()
    {
        new MemoryRecord
        {
            CabinetCode = "A1",
            StartedAt = SessionStartedAt,
            AppName = SelfAppName,
            ProjectName = "Current session",
            FileName = "App launch",
            Note = $"{SessionStartedAt:HH:mm} ➡ App launch"
        }
    };

    public static event EventHandler? RecordsChanged;
    public static event EventHandler? RecordingStateChanged;

    public static bool IsRecordingEnabled { get; private set; } = true;

    public static void SetRecordingEnabled(bool isEnabled)
    {
        if (IsRecordingEnabled == isEnabled)
        {
            return;
        }

        IsRecordingEnabled = isEnabled;
        RecordingStateChanged?.Invoke(null, EventArgs.Empty);
    }

    public static IReadOnlyList<MemoryRecord> GetRecords(string keyword = "")
    {
        return Records
            .Where(record => UserProfileStore.IsLoggedIn || record.StartedAt >= SessionStartedAt)
            .Where(record => record.Contains(keyword))
            .OrderBy(record => record.StartedAt)
            .ToList();
    }

    public static void InitializeForAppLaunch()
    {
        if (UserProfileStore.IsLoggedIn)
        {
            RestoreForCurrentUser();
            return;
        }

        StartGuestSession();
    }

    public static void Add(MemoryRecord record)
    {
        if (!IsRecordingEnabled)
        {
            return;
        }

        if (record.StartedAt.Date != DateTime.Today)
        {
            return;
        }

        var existing = FindExistingAppRecord(record.AppName);
        if (existing is not null)
        {
            existing.ProjectName = string.IsNullOrWhiteSpace(record.ProjectName) ? existing.ProjectName : record.ProjectName;
            existing.FileName = string.IsNullOrWhiteSpace(record.FileName) ? existing.FileName : record.FileName;

            if (!string.IsNullOrWhiteSpace(record.Note))
            {
                foreach (var line in record.Note.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
                {
                    existing.AppendOperation(record.StartedAt, line);
                }
            }

            NotifyRecordsChanged();
            return;
        }

        if (Records.Count >= 2600)
        {
            return;
        }

        Records.Add(record);
        NotifyRecordsChanged();
    }

    public static MemoryRecord CreateRecord(DateTime startedAt, string appName, string projectName, string fileName, string note)
    {
        var code = GetCabinetCode(nextNumber);
        nextNumber++;

        return new MemoryRecord
        {
            CabinetCode = code,
            StartedAt = startedAt,
            AppName = appName,
            ProjectName = projectName,
            FileName = fileName,
            Note = note
        };
    }

    public static void AddTrackedActivity(DateTime startedAt, string appName, string windowTitle)
    {
        if (!IsRecordingEnabled)
        {
            return;
        }

        if (Records.Count >= 2600)
        {
            return;
        }

        MergeDuplicateAppRecords(appName);
        var context = ParseWindowContext(appName, windowTitle);
        if (IsLikelyFalseExplorerRecord(appName, context.ProjectName, context.FileName))
        {
            return;
        }

        var operation = BuildTrackedOperation(appName, context.ProjectName, context.FileName);
        var record = GetOrCreateAppRecord(startedAt, appName, context.ProjectName, context.FileName);

        record.ProjectName = string.IsNullOrWhiteSpace(context.ProjectName) ? record.ProjectName : context.ProjectName;
        record.FileName = string.IsNullOrWhiteSpace(context.FileName) ? record.FileName : context.FileName;
        record.AppendOperation(startedAt, operation);

        NotifyRecordsChanged();
    }

    public static void AddSelfOperation(string operation)
    {
        if (!IsRecordingEnabled)
        {
            return;
        }

        AddOperation(SelfAppName, "Memory Storage", "App operation", operation);
    }

    public static void AddOperation(string appName, string projectName, string fileName, string operation)
    {
        if (!IsRecordingEnabled)
        {
            return;
        }

        var happenedAt = DateTime.Now;
        MergeDuplicateAppRecords(appName);
        var record = GetOrCreateAppRecord(happenedAt, appName, projectName, fileName);
        record.ProjectName = string.IsNullOrWhiteSpace(projectName) ? record.ProjectName : projectName;
        record.FileName = string.IsNullOrWhiteSpace(fileName) ? record.FileName : fileName;
        record.AppendOperation(happenedAt, operation);
        NotifyRecordsChanged();
    }

    public static void AddMobileInteraction(DateTime happenedAt, string appName, string actionLabel, string itemName)
    {
        if (!IsRecordingEnabled || string.IsNullOrWhiteSpace(appName))
        {
            return;
        }

        MergeDuplicateAppRecords(appName);
        var cleanItem = CleanMobileItemName(itemName, appName);
        var record = GetOrCreateAppRecord(happenedAt, appName, "Mobile app", cleanItem);

        if (!string.IsNullOrWhiteSpace(cleanItem))
        {
            record.FileName = cleanItem;
        }

        var operationParts = new List<string> { $"{actionLabel} : {appName}" };

        if (!string.IsNullOrWhiteSpace(cleanItem))
        {
            operationParts.Add($"{AppUi.RecordFileItemLabel} : {cleanItem}");
        }

        record.AppendOperation(happenedAt, string.Join(" ??", operationParts));
        NotifyRecordsChanged();
    }

    private static void MergeDuplicateAppRecords(string appName)
    {
        var identity = NormalizeAppIdentity(appName);
        if (string.IsNullOrWhiteSpace(identity))
        {
            return;
        }

        var duplicates = Records
            .Where(record => NormalizeAppIdentity(record.AppName).Equals(identity, StringComparison.OrdinalIgnoreCase))
            .OrderBy(record => record.StartedAt)
            .ToList();

        if (duplicates.Count <= 1)
        {
            return;
        }

        var primary = duplicates[0];

        foreach (var duplicate in duplicates.Skip(1))
        {
            primary.ProjectName = string.IsNullOrWhiteSpace(primary.ProjectName) ? duplicate.ProjectName : primary.ProjectName;
            primary.FileName = string.IsNullOrWhiteSpace(primary.FileName) ? duplicate.FileName : primary.FileName;

            if (!string.IsNullOrWhiteSpace(duplicate.Note))
            {
                primary.Note = string.IsNullOrWhiteSpace(primary.Note)
                    ? duplicate.Note
                    : $"{primary.Note}{Environment.NewLine}{duplicate.Note}";
                primary.EndedAt = duplicate.EndedAt ?? primary.EndedAt;
            }

            Records.Remove(duplicate);
        }

        primary.RefreshLocalizedText();
    }

    private static MemoryRecord GetOrCreateAppRecord(DateTime startedAt, string appName, string projectName, string fileName)
    {
        var existing = FindExistingAppRecord(appName);

        if (existing is not null)
        {
            return existing;
        }

        var created = CreateRecord(startedAt, appName, projectName, fileName, string.Empty);
        Records.Add(created);
        return created;
    }

    public static void RestoreForCurrentUser()
    {
        if (!UserProfileStore.IsLoggedIn)
        {
            return;
        }

        Records.Clear();

        foreach (var record in LoadCurrentUserRecords())
        {
            Records.Add(record);
        }

        if (Records.Count == 0)
        {
            Records.Add(CreateSessionLaunchRecord());
        }

        nextNumber = Math.Max(2, Records.Select(record => CabinetCodeToNumber(record.CabinetCode)).DefaultIfEmpty(1).Max() + 1);
        NotifyRecordsChanged();
    }

    public static void SaveForCurrentUser()
    {
        if (!UserProfileStore.IsLoggedIn)
        {
            return;
        }

        SaveCurrentUserRecords();
    }

    public static void StartGuestSession()
    {
        Records.Clear();
        nextNumber = 2;
        Records.Add(CreateSessionLaunchRecord(DateTime.Now));
        NotifyRecordsChanged();
    }

    public static void DeleteRecords(IEnumerable<MemoryRecord> records)
    {
        var targets = records.ToHashSet();
        if (targets.Count == 0)
        {
            return;
        }

        foreach (var record in targets.ToList())
        {
            Records.Remove(record);
        }

        RenumberCabinets();
        NotifyRecordsChanged();
    }

    public static void DeleteAllRecords()
    {
        if (Records.Count == 0)
        {
            return;
        }

        Records.Clear();
        nextNumber = 1;
        NotifyRecordsChanged();
    }

    private static MemoryRecord? FindExistingAppRecord(string appName)
    {
        var targetIdentity = NormalizeAppIdentity(appName);

        if (string.IsNullOrWhiteSpace(targetIdentity))
        {
            return null;
        }

        return Records.FirstOrDefault(record =>
            NormalizeAppIdentity(record.AppName).Equals(targetIdentity, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeAppIdentity(string appName)
    {
        if (string.IsNullOrWhiteSpace(appName))
        {
            return string.Empty;
        }

        var value = appName.Trim()
            .Replace(".exe", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("Microsoft Office ", string.Empty, StringComparison.OrdinalIgnoreCase);

        value = value.ToLowerInvariant() switch
        {
            "powerpnt" or "microsoft powerpoint" => "powerpoint",
            "winword" or "microsoft word" => "word",
            "excel" or "microsoft excel" => "excel",
            "onenote" or "microsoft onenote" => "onenote",
            "outlook" or "microsoft outlook" => "outlook",
            "msedge" or "microsoftedge" or "edge" or "microsoft edge" => "microsoftedge",
            "winstore.app" or "storeexperiencehost" or "microsoft store client" or "microsoft store" => "microsoftstore",
            "code" or "visual studio code" or "vs code" => "vscode",
            "devenv" or "microsoft visual studio" or "visual studio" => "visualstudio",
            _ => value
        };

        return new string(value.Where(char.IsLetterOrDigit).ToArray());
    }

    private static (string ProjectName, string FileName) ParseWindowContext(string appName, string windowTitle)
    {
        if (string.IsNullOrWhiteSpace(windowTitle))
        {
            return (string.Empty, string.Empty);
        }

        var cleanedTitle = CleanWindowTitle(windowTitle, appName);
        if (string.IsNullOrWhiteSpace(cleanedTitle))
        {
            return (string.Empty, string.Empty);
        }

        var separators = new[] { " - ", " — ", " – ", " | " };
        var parts = separators
            .Aggregate(new[] { cleanedTitle }, (current, separator) =>
                current.SelectMany(part => part.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToArray())
            .Where(part => !IsAppNamePart(part, appName))
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();

        if (parts.Length >= 3)
        {
            return (parts[^2], parts[0]);
        }

        if (parts.Length == 2)
        {
            return (parts[1], parts[0]);
        }

        if (parts.Length == 1)
        {
            return (string.Empty, parts[0]);
        }

        return (string.Empty, cleanedTitle);
    }

    private static string BuildTrackedOperation(string appName, string projectName, string fileName)
    {
        var parts = new List<string> { $"{AppUi.T("Opened")} : {AppUi.DisplayAppName(appName)}" };
        var clickedItems = new[] { projectName, fileName }
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (clickedItems.Length > 0)
        {
            parts.Add($"{AppUi.RecordFileItemLabel} : {string.Join(" ➡ ", clickedItems)}");
        }

        return string.Join(" ➡ ", parts);
    }

    private static bool IsLikelyFalseExplorerRecord(string appName, string projectName, string fileName)
    {
        if (!appName.Equals("File Explorer", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(projectName)
            && string.IsNullOrWhiteSpace(fileName);
    }

    private static string CleanWindowTitle(string windowTitle, string appName)
    {
        var value = Regex.Replace(windowTitle.Trim(), @"\s+", " ");

        if (value.Equals(appName, StringComparison.OrdinalIgnoreCase)
            || value.Equals(AppUi.DisplayAppName(appName), StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return value;
    }

    private static string CleanMobileItemName(string itemName, string appName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return string.Empty;
        }

        var value = Regex.Replace(itemName.Trim(), @"\s+", " ");

        if (value.Equals(appName, StringComparison.OrdinalIgnoreCase)
            || value.Equals(AppUi.DisplayAppName(appName), StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return value.Length > 80 ? value[..80] : value;
    }

    private static bool IsAppNamePart(string part, string appName)
    {
        return part.Equals(appName, StringComparison.OrdinalIgnoreCase)
            || part.Equals(AppUi.DisplayAppName(appName), StringComparison.OrdinalIgnoreCase)
            || NormalizeAppIdentity(part).Equals(NormalizeAppIdentity(appName), StringComparison.OrdinalIgnoreCase);
    }

    private static string GetCabinetCode(int number)
    {
        var letterIndex = (number - 1) / 100;
        var itemNumber = ((number - 1) % 100) + 1;

        if (letterIndex >= 26)
        {
            return "Z100";
        }

        var letter = (char)('A' + letterIndex);
        return $"{letter}{itemNumber}";
    }

    private static void RenumberCabinets()
    {
        var records = Records
            .OrderBy(record => record.StartedAt)
            .ThenBy(record => record.AppName)
            .ToList();

        Records.Clear();
        nextNumber = 1;

        foreach (var record in records)
        {
            var code = GetCabinetCode(nextNumber++);
            record.CabinetCode = code;
            record.IsSelectedForDeletion = false;
            Records.Add(record);
        }
    }

    public static void RefreshLocalizedRecords()
    {
        foreach (var record in Records)
        {
            record.RefreshLocalizedText();
        }

        RecordsChanged?.Invoke(null, EventArgs.Empty);
    }

    private static void NotifyRecordsChanged()
    {
        SaveForCurrentUser();
        RecordsChanged?.Invoke(null, EventArgs.Empty);
    }

    private static MemoryRecord CreateSessionLaunchRecord(DateTime? startedAt = null)
    {
        var timestamp = startedAt ?? SessionStartedAt;
        return new MemoryRecord
        {
            CabinetCode = "A1",
            StartedAt = timestamp,
            AppName = SelfAppName,
            ProjectName = "Current session",
            FileName = "App launch",
            Note = $"{timestamp:HH:mm} ??App launch"
        };
    }

    private static IReadOnlyList<MemoryRecord> LoadCurrentUserRecords()
    {
        try
        {
            var path = CurrentUserRecordsPath();
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return Array.Empty<MemoryRecord>();
            }

            var records = JsonSerializer.Deserialize<List<MemoryRecord>>(File.ReadAllText(path));
            return records?
                .OrderBy(record => record.StartedAt)
                .ToList() ?? new List<MemoryRecord>();
        }
        catch
        {
            return Array.Empty<MemoryRecord>();
        }
    }

    private static void SaveCurrentUserRecords()
    {
        var path = CurrentUserRecordsPath();
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        Directory.CreateDirectory(RecordsDirectory);
        var records = Records
            .OrderBy(record => record.StartedAt)
            .ToList();
        var json = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    private static string CurrentUserRecordsPath()
    {
        if (string.IsNullOrWhiteSpace(UserProfileStore.Email))
        {
            return string.Empty;
        }

        var id = Convert.ToHexString(Encoding.UTF8.GetBytes(UserProfileStore.Email.Trim().ToLowerInvariant()));
        return Path.Combine(RecordsDirectory, $"{id}.json");
    }

    private static int CabinetCodeToNumber(string cabinetCode)
    {
        if (string.IsNullOrWhiteSpace(cabinetCode) || cabinetCode.Length < 2)
        {
            return 1;
        }

        var letter = char.ToUpperInvariant(cabinetCode[0]);
        if (letter < 'A' || letter > 'Z')
        {
            return 1;
        }

        return int.TryParse(cabinetCode[1..], out var number)
            ? ((letter - 'A') * 100) + number
            : 1;
    }
}

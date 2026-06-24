using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Memory_Storage;

public static class ActivityTrackerService
{
    private static Timer? timer;
    private static readonly object StateLock = new();
    private static string lastAppName = string.Empty;
    private static string lastWindowTitle = string.Empty;

    public static void Start()
    {
        if (timer is not null)
        {
            return;
        }

        timer = new Timer(_ => CaptureForegroundApp(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        CaptureForegroundApp();
    }

    private static void CaptureForegroundApp()
    {
        if (!MemoryRecordStore.IsRecordingEnabled)
        {
            lock (StateLock)
            {
                lastAppName = string.Empty;
                lastWindowTitle = string.Empty;
            }

            return;
        }

        var snapshot = GetForegroundSnapshot();

        if (snapshot.AppName.Length == 0)
        {
            return;
        }

        lock (StateLock)
        {
            if (snapshot.AppName == lastAppName && snapshot.WindowTitle == lastWindowTitle)
            {
                return;
            }

            lastAppName = snapshot.AppName;
            lastWindowTitle = snapshot.WindowTitle;
        }

        MainThread.BeginInvokeOnMainThread(() =>
            MemoryRecordStore.AddTrackedActivity(DateTime.Now, snapshot.AppName, snapshot.WindowTitle));
    }

    private static (string AppName, string WindowTitle) GetForegroundSnapshot()
    {
#if WINDOWS
        var handle = GetForegroundWindow();

        if (handle == IntPtr.Zero)
        {
            return (string.Empty, string.Empty);
        }

        _ = GetWindowThreadProcessId(handle, out var processId);

        try
        {
            using var process = Process.GetProcessById((int)processId);
            var windowTitle = GetWindowTitle(handle, process.MainWindowTitle);
            var windowClassName = GetWindowClassName(handle);

            if (!IsTrackableWindow(process.ProcessName, windowTitle, windowClassName))
            {
                return (string.Empty, string.Empty);
            }

            var appName = GetFriendlyAppName(process, windowTitle);
            return (appName, windowTitle);
        }
        catch
        {
            return (string.Empty, string.Empty);
        }
#else
        return (string.Empty, string.Empty);
#endif
    }

#if WINDOWS
    private static bool IsTrackableWindow(string processName, string windowTitle, string windowClassName)
    {
        if (!processName.Equals("explorer", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(windowTitle)
            || windowTitle.Equals("Program Manager", StringComparison.OrdinalIgnoreCase)
            || windowTitle.Equals("Desktop", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return windowClassName.Equals("CabinetWClass", StringComparison.OrdinalIgnoreCase)
            || windowClassName.Equals("ExploreWClass", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetFriendlyAppName(Process process, string windowTitle)
    {
        var processName = process.ProcessName;

        if (processName.Equals("ApplicationFrameHost", StringComparison.OrdinalIgnoreCase))
        {
            var titleName = TryGetNameFromWindowTitle(windowTitle);
            return string.IsNullOrWhiteSpace(titleName) ? processName : titleName;
        }

        var mappedName = GetKnownApplicationName(processName);
        if (!string.IsNullOrWhiteSpace(mappedName))
        {
            return mappedName;
        }

        var versionName = GetVersionName(process);
        if (!string.IsNullOrWhiteSpace(versionName))
        {
            return versionName;
        }

        var titleFallback = TryGetNameFromWindowTitle(windowTitle);
        if (!string.IsNullOrWhiteSpace(titleFallback) && processName.Length <= 3)
        {
            return titleFallback;
        }

        return NormalizeProcessName(processName);
    }

    private static string GetVersionName(Process process)
    {
        try
        {
            var fileName = process.MainModule?.FileName;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return string.Empty;
            }

            var versionInfo = FileVersionInfo.GetVersionInfo(fileName);
            var candidates = new[]
            {
                versionInfo.ProductName,
                versionInfo.FileDescription,
                versionInfo.OriginalFilename
            };

            foreach (var candidate in candidates)
            {
                var normalized = NormalizeApplicationName(candidate);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    return normalized;
                }
            }
        }
        catch
        {
            return string.Empty;
        }

        return string.Empty;
    }

    private static string GetKnownApplicationName(string processName)
    {
        return processName.ToLowerInvariant() switch
        {
            "powerpnt" => "PowerPoint",
            "winword" => "Word",
            "excel" => "Excel",
            "onenote" => "OneNote",
            "outlook" => "Outlook",
            "msaccess" => "Access",
            "mspub" => "Publisher",
            "msedge" => "Microsoft Edge",
            "microsoftedge" => "Microsoft Edge",
            "winstore.app" => "Microsoft Store",
            "storeexperiencehost" => "Microsoft Store",
            "textinputhost" => "Text Input",
            "explorer" => "File Explorer",
            "code" => "VS Code",
            "devenv" => "Visual Studio",
            "chrome" => "Chrome",
            "firefox" => "Firefox",
            "photoshop" => "Adobe Photoshop",
            "illustrator" => "Adobe Illustrator",
            "acrobat" => "Adobe Acrobat",
            "acrodist" => "Adobe Acrobat Distiller",
            _ => string.Empty
        };
    }

    private static string TryGetNameFromWindowTitle(string windowTitle)
    {
        if (string.IsNullOrWhiteSpace(windowTitle))
        {
            return string.Empty;
        }

        var title = windowTitle.Trim();
        var separators = new[] { " - ", " | ", " — ", " – " };

        foreach (var separator in separators)
        {
            var parts = title.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length > 1)
            {
                var last = NormalizeApplicationName(parts[^1]);
                if (!string.IsNullOrWhiteSpace(last))
                {
                    return last;
                }
            }
        }

        return NormalizeApplicationName(title);
    }

    private static string NormalizeProcessName(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            return string.Empty;
        }

        return processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? processName[..^4]
            : processName;
    }

    private static string NormalizeApplicationName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var value = name.Trim();
        value = value.Replace(".exe", string.Empty, StringComparison.OrdinalIgnoreCase);
        value = value.Replace("®", string.Empty, StringComparison.OrdinalIgnoreCase);
        value = value.Replace("™", string.Empty, StringComparison.OrdinalIgnoreCase);
        value = Regex.Replace(value, @"\s+", " ");

        if (value.Equals("Microsoft Corporation", StringComparison.OrdinalIgnoreCase)
            || value.Equals("Microsoft", StringComparison.OrdinalIgnoreCase)
            || value.Equals("Windows", StringComparison.OrdinalIgnoreCase)
            || value.Equals("ApplicationFrameHost", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        value = value switch
        {
            "Microsoft PowerPoint" or "Microsoft Office PowerPoint" => "PowerPoint",
            "Microsoft Word" or "Microsoft Office Word" => "Word",
            "Microsoft Excel" or "Microsoft Office Excel" => "Excel",
            "Microsoft OneNote" => "OneNote",
            "Microsoft Outlook" => "Outlook",
            "Visual Studio Code" => "VS Code",
            "Microsoft Store Client" => "Microsoft Store",
            _ => value
        };

        if (value.Contains("PowerPoint", StringComparison.OrdinalIgnoreCase))
        {
            return "PowerPoint";
        }

        if (value.Contains("Microsoft Edge", StringComparison.OrdinalIgnoreCase))
        {
            return "Microsoft Edge";
        }

        if (value.Contains("Microsoft Store", StringComparison.OrdinalIgnoreCase))
        {
            return "Microsoft Store";
        }

        return value;
    }

    private static string GetWindowTitle(IntPtr handle, string fallback)
    {
        var length = GetWindowTextLength(handle);

        if (length <= 0)
        {
            return fallback;
        }

        var buffer = new char[length + 1];
        var copied = GetWindowText(handle, buffer, buffer.Length);
        return copied <= 0 ? fallback : new string(buffer, 0, copied);
    }

    private static string GetWindowClassName(IntPtr handle)
    {
        var buffer = new char[256];
        var copied = GetClassName(handle, buffer, buffer.Length);
        return copied <= 0 ? string.Empty : new string(buffer, 0, copied);
    }
#endif

#if WINDOWS
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, char[] lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hWnd, char[] lpClassName, int nMaxCount);
#endif
}

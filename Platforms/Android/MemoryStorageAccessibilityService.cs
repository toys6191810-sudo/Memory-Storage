#if ANDROID
using Android.AccessibilityServices;
using Android.App;
using Android.Content.PM;
using Android.Views.Accessibility;

namespace Memory_Storage;

[Service(
    Label = "Memory Storage",
    Permission = Android.Manifest.Permission.BindAccessibilityService,
    Exported = true)]
[IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
[MetaData("android.accessibilityservice", Resource = "@xml/memory_storage_accessibility_service")]
public sealed class MemoryStorageAccessibilityService : AccessibilityService
{
    private static readonly object EventLock = new();
    private static string lastPackageName = string.Empty;
    private static string lastEventKey = string.Empty;
    private static DateTime lastEventAt = DateTime.MinValue;

    public override void OnAccessibilityEvent(AccessibilityEvent? e)
    {
        if (e is null || !MemoryRecordStore.IsRecordingEnabled)
        {
            return;
        }

        var packageName = e.PackageName?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(packageName) || packageName.Equals(PackageName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var appName = AndroidUsageTracker.GetApplicationLabel(packageName);
        var eventType = e.EventType;
        var itemName = ExtractEventItem(e);
        var actionLabel = string.Empty;

        if (eventType == EventTypes.WindowStateChanged)
        {
            actionLabel = packageName.Equals(lastPackageName, StringComparison.OrdinalIgnoreCase)
                ? "Screen"
                : "Opened";
            lastPackageName = packageName;
        }
        else if (eventType == EventTypes.ViewClicked)
        {
            actionLabel = "Tapped";
        }
        else
        {
            return;
        }

        var now = DateTime.Now;
        var eventKey = $"{packageName}|{actionLabel}|{itemName}";

        lock (EventLock)
        {
            if (eventKey == lastEventKey && now - lastEventAt < TimeSpan.FromMilliseconds(900))
            {
                return;
            }

            lastEventKey = eventKey;
            lastEventAt = now;
        }

        MainThread.BeginInvokeOnMainThread(() =>
            MemoryRecordStore.AddMobileInteraction(now, appName, actionLabel, itemName));
    }

    public override void OnInterrupt()
    {
    }

    private static string ExtractEventItem(AccessibilityEvent e)
    {
        var candidates = new[]
        {
            e.ContentDescription?.ToString(),
            e.Text is null ? string.Empty : string.Join(" ", e.Text.Select(item => item?.ToString()).Where(text => !string.IsNullOrWhiteSpace(text))),
            ShortResourceName(e.Source?.ViewIdResourceName),
            ShortClassName(e.ClassName?.ToString())
        };

        return candidates
            .Select(candidate => candidate?.Trim() ?? string.Empty)
            .FirstOrDefault(candidate => !string.IsNullOrWhiteSpace(candidate) && !IsNoisyItem(candidate))
            ?? string.Empty;
    }

    private static string ShortResourceName(string? resourceName)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            return string.Empty;
        }

        var slashIndex = resourceName.LastIndexOf('/');
        return slashIndex >= 0 && slashIndex + 1 < resourceName.Length
            ? resourceName[(slashIndex + 1)..]
            : resourceName;
    }

    private static string ShortClassName(string? className)
    {
        if (string.IsNullOrWhiteSpace(className))
        {
            return string.Empty;
        }

        var dotIndex = className.LastIndexOf('.');
        return dotIndex >= 0 && dotIndex + 1 < className.Length
            ? className[(dotIndex + 1)..]
            : className;
    }

    private static bool IsNoisyItem(string value)
    {
        return value.Equals("View", StringComparison.OrdinalIgnoreCase)
            || value.Equals("TextView", StringComparison.OrdinalIgnoreCase)
            || value.Equals("Button", StringComparison.OrdinalIgnoreCase)
            || value.Equals("ImageView", StringComparison.OrdinalIgnoreCase)
            || value.Equals("FrameLayout", StringComparison.OrdinalIgnoreCase)
            || value.Equals("LinearLayout", StringComparison.OrdinalIgnoreCase)
            || value.Equals("RecyclerView", StringComparison.OrdinalIgnoreCase);
    }
}
#endif

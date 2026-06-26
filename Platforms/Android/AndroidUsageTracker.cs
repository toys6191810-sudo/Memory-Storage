#if ANDROID
using Android.App;
using Android.App.Usage;
using Android.Content;
using Android.OS;
using Android.Provider;

namespace Memory_Storage;

public static class AndroidUsageTracker
{
    private static readonly Dictionary<string, string> KnownPackageLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["com.google.android.youtube"] = "YouTube",
        ["com.google.android.apps.youtube.music"] = "YT Music",
        ["com.google.android.apps.photos"] = "Photos",
        ["com.android.vending"] = "Play Store",
        ["com.android.chrome"] = "Chrome",
        ["com.google.android.apps.messaging"] = "Messages",
        ["com.google.android.gm"] = "Gmail",
        ["com.google.android.apps.maps"] = "Maps",
        ["com.google.android.apps.docs"] = "Drive",
        ["com.google.android.calendar"] = "Calendar",
        ["com.google.android.contacts"] = "Contacts",
        ["com.google.android.apps.nbu.files"] = "Files",
        ["com.google.android.dialer"] = "Phone",
        ["com.google.android.apps.safetycenter"] = "Safety",
        ["com.google.android.googlequicksearchbox"] = "Google",
        ["com.google.android.apps.camera"] = "Camera",
        ["com.google.android.deskclock"] = "Clock"
    };

    private static readonly string[] LauncherOrShellPackageNames =
    [
        "com.google.android.apps.nexuslauncher",
        "com.android.launcher",
        "com.android.launcher3",
        "com.android.systemui"
    ];

    public static bool HasUsageAccess()
    {
        var context = Platform.AppContext;
        var appOps = (AppOpsManager?)context.GetSystemService(Context.AppOpsService);

        if (appOps is null)
        {
            return false;
        }

        var mode = Build.VERSION.SdkInt >= BuildVersionCodes.Q
            ? appOps.UnsafeCheckOpNoThrow(AppOpsManager.OpstrGetUsageStats!, Process.MyUid(), context.PackageName!)
            : appOps.CheckOpNoThrow(AppOpsManager.OpstrGetUsageStats!, Process.MyUid(), context.PackageName!);

        return mode == AppOpsManagerMode.Allowed;
    }

    public static void OpenUsageAccessSettings()
    {
        var activity = Platform.CurrentActivity;
        if (activity is null)
        {
            return;
        }

        var intent = new Intent(Settings.ActionUsageAccessSettings);
        intent.AddFlags(ActivityFlags.NewTask);
        activity.StartActivity(intent);
    }

    public static bool HasAccessibilityAccess()
    {
        var context = Platform.AppContext;
        var enabledServices = Settings.Secure.GetString(context.ContentResolver, Settings.Secure.EnabledAccessibilityServices);

        if (string.IsNullOrWhiteSpace(enabledServices))
        {
            return false;
        }

        return enabledServices
            .Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(service => service.Contains(context.PackageName!, StringComparison.OrdinalIgnoreCase)
                && service.Contains(nameof(MemoryStorageAccessibilityService), StringComparison.OrdinalIgnoreCase));
    }

    public static void OpenAccessibilitySettings()
    {
        var activity = Platform.CurrentActivity;
        if (activity is null)
        {
            return;
        }

        var intent = new Intent(Settings.ActionAccessibilitySettings);
        intent.AddFlags(ActivityFlags.NewTask);
        activity.StartActivity(intent);
    }

    public static (string AppName, string WindowTitle) GetForegroundSnapshot()
    {
        if (!HasUsageAccess())
        {
            return (string.Empty, string.Empty);
        }

        var context = Platform.AppContext;
        var usageStatsManager = (UsageStatsManager?)context.GetSystemService(Context.UsageStatsService);

        if (usageStatsManager is null)
        {
            return (string.Empty, string.Empty);
        }

        var end = Java.Lang.JavaSystem.CurrentTimeMillis();
        var begin = end - 30_000;
        var usageEvents = usageStatsManager.QueryEvents(begin, end);
        var usageEvent = new UsageEvents.Event();
        string latestPackageName = string.Empty;
        long latestTime = 0;

        while (usageEvents.HasNextEvent)
        {
            usageEvents.GetNextEvent(usageEvent);

            if ((int)usageEvent.EventType != 1)
            {
                continue;
            }

            if (!IsTrackableApplicationPackage(usageEvent.PackageName))
            {
                continue;
            }

            if (usageEvent.TimeStamp < latestTime)
            {
                continue;
            }

            latestTime = usageEvent.TimeStamp;
            latestPackageName = usageEvent.PackageName ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(latestPackageName))
        {
            return (string.Empty, string.Empty);
        }

        return (GetApplicationLabel(latestPackageName), "Mobile app foreground");
    }

    public static bool IsTrackableApplicationPackage(string? packageName)
    {
        if (string.IsNullOrWhiteSpace(packageName))
        {
            return false;
        }

        var context = Platform.AppContext;
        if (packageName.Equals(context.PackageName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !LauncherOrShellPackageNames.Any(packageName.Equals);
    }

    public static string GetApplicationLabel(string packageName)
    {
        if (KnownPackageLabels.TryGetValue(packageName, out var knownName))
        {
            return knownName;
        }

        var context = Platform.AppContext;
        var packageManager = context.PackageManager;

        if (packageManager is null)
        {
            return packageName;
        }

        try
        {
            var applicationInfo = packageManager.GetApplicationInfo(packageName, 0);
            var label = packageManager.GetApplicationLabel(applicationInfo)?.ToString();
            if (!string.IsNullOrWhiteSpace(label) && !label.Equals(packageName, StringComparison.OrdinalIgnoreCase))
            {
                return label.Trim();
            }

            var launchIntent = packageManager.GetLaunchIntentForPackage(packageName);
            if (launchIntent is not null)
            {
                var resolveInfo = packageManager.ResolveActivity(launchIntent, 0);
                var activityLabel = resolveInfo?.LoadLabel(packageManager)?.ToString();
                if (!string.IsNullOrWhiteSpace(activityLabel) && !activityLabel.Equals(packageName, StringComparison.OrdinalIgnoreCase))
                {
                    return activityLabel.Trim();
                }
            }

            return FormatPackageNameFallback(packageName);
        }
        catch
        {
            return FormatPackageNameFallback(packageName);
        }
    }

    private static string FormatPackageNameFallback(string packageName)
    {
        if (KnownPackageLabels.TryGetValue(packageName, out var knownName))
        {
            return knownName;
        }

        var lastSegment = packageName.Split('.', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? packageName;
        return string.IsNullOrWhiteSpace(lastSegment)
            ? packageName
            : string.Join(" ", lastSegment.Split(['_', '-'], StringSplitOptions.RemoveEmptyEntries)
                .Select(segment => char.ToUpperInvariant(segment[0]) + segment[1..]));
    }
}
#endif

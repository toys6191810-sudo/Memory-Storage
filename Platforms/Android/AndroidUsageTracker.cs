#if ANDROID
using Android.App;
using Android.App.Usage;
using Android.Content;
using Android.OS;
using Android.Provider;

namespace Memory_Storage;

public static class AndroidUsageTracker
{
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

    public static string GetApplicationLabel(string packageName)
    {
        var context = Platform.AppContext;
        var packageManager = context.PackageManager;

        if (packageManager is null)
        {
            return packageName;
        }

        try
        {
            var applicationInfo = packageManager.GetApplicationInfo(packageName, 0);
            return packageManager.GetApplicationLabel(applicationInfo)?.ToString() ?? packageName;
        }
        catch
        {
            return packageName;
        }
    }
}
#endif

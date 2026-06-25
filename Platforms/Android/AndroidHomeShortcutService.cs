using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;

namespace Memory_Storage;

public static class AndroidHomeShortcutService
{
    private static bool hasRequestedThisSession;

    public static void EnsureHomeShortcut()
    {
        if (hasRequestedThisSession || Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            return;
        }

        var context = Android.App.Application.Context;
        var shortcutManager = context.GetSystemService(Java.Lang.Class.FromType(typeof(ShortcutManager))) as ShortcutManager;

        if (shortcutManager is null || !shortcutManager.IsRequestPinShortcutSupported)
        {
            return;
        }

        if (shortcutManager.PinnedShortcuts.Any(shortcut => shortcut.Id == "memory-storage-home"))
        {
            hasRequestedThisSession = true;
            return;
        }

        var launchIntent = new Intent(context, typeof(MainActivity));
        launchIntent.SetAction(Intent.ActionMain);
        launchIntent.AddCategory(Intent.CategoryLauncher);
        launchIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);

        var shortcut = new ShortcutInfo.Builder(context, "memory-storage-home")
            .SetShortLabel("Memory Storage")
            .SetLongLabel("Memory Storage")
            .SetIcon(Icon.CreateWithResource(context, Resource.Mipmap.memory_storage_appicon))
            .SetIntent(launchIntent)
            .Build();

        hasRequestedThisSession = true;
        shortcutManager.RequestPinShortcut(shortcut, null);
    }
}

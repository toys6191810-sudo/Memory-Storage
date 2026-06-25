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

        var shortcut = BuildShortcut(context);

        hasRequestedThisSession = true;

        if (shortcutManager.PinnedShortcuts.Any(pinnedShortcut => pinnedShortcut.Id == shortcut.Id))
        {
            shortcutManager.UpdateShortcuts(new[] { shortcut });
            return;
        }

        shortcutManager.RequestPinShortcut(shortcut, null);
    }

    private static ShortcutInfo BuildShortcut(Context context)
    {
        var launchIntent = new Intent(context, typeof(MainActivity));
        launchIntent.SetAction(Intent.ActionMain);
        launchIntent.AddCategory(Intent.CategoryLauncher);
        launchIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);

        return new ShortcutInfo.Builder(context, "memory-storage-home")
            .SetShortLabel("Memory Storage")
            .SetLongLabel("Memory Storage")
            .SetIcon(Icon.CreateWithResource(context, Resource.Drawable.memory_storage_shortcut_icon))
            .SetIntent(launchIntent)
            .Build();
    }
}

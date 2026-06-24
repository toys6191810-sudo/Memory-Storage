using System.Runtime.CompilerServices;

#if WINDOWS
using Microsoft.UI.Xaml;
using Windows.System;
#endif

namespace Memory_Storage;

public static class EnterKeyHelper
{
#if WINDOWS
    private static readonly HashSet<int> AttachedViews = new();
#endif

    public static void MoveNextOnEnter(Entry entry, VisualElement next)
    {
        entry.Completed += (_, _) => next.Focus();
        AttachEnter(entry, () => next.Focus());
    }

    public static void SubmitOnEnter(Entry entry, Action submit)
    {
        entry.Completed += (_, _) => submit();
        AttachEnter(entry, submit);
    }

    public static void ClickOnEnter(Button button, Action click)
    {
        AttachEnter(button, click);
    }

    public static void PageEnter(ContentPage page, Action action)
    {
        AttachEnter(page, action);
    }

    private static void AttachEnter(VisualElement element, Action action)
    {
#if WINDOWS
        element.HandlerChanged += (_, _) =>
        {
            if (element.Handler?.PlatformView is not UIElement platformView)
            {
                return;
            }

            var key = RuntimeHelpers.GetHashCode(platformView);
            if (!AttachedViews.Add(key))
            {
                return;
            }

            platformView.KeyDown += (_, args) =>
            {
                if (args.Key != VirtualKey.Enter)
                {
                    return;
                }

                args.Handled = true;
                MainThread.BeginInvokeOnMainThread(action);
            };
        };
#endif
    }
}

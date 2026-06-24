namespace Memory_Storage;

public partial class SuccessDialogPage : ContentPage
{
    public static SuccessDialogPage? ActiveDialog { get; private set; }

    private readonly string message;
    private readonly Func<Task>? afterDone;
    private bool isClosing;

#if WINDOWS
    private Microsoft.UI.Xaml.Window? nativeWindow;
#endif

    public SuccessDialogPage(string message, Func<Task>? afterDone = null)
    {
        InitializeComponent();
        this.message = message;
        this.afterDone = afterDone;
        EnterKeyHelper.ClickOnEnter(DoneButton, CloseDialog);
        EnterKeyHelper.PageEnter(this, CloseDialog);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ActiveDialog = this;
        MessageLabel.Text = message;
        DoneButton.Text = AppUi.T("Done");
        Focus();
        DoneButton.Focus();
#if WINDOWS
        AttachWindowEnterKey();
        AttachDoneButtonAccelerator();
#endif
        _ = PlayIntroAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (ActiveDialog == this)
        {
            ActiveDialog = null;
        }

#if WINDOWS
        if (nativeWindow is not null)
        {
            nativeWindow.Content.KeyDown -= NativeWindow_KeyDown;
            nativeWindow = null;
        }
#endif
    }

    private async void DoneButton_Clicked(object? sender, EventArgs e)
    {
        await CloseDialogAsync(sender);
    }

    private void CloseDialog()
    {
        _ = CloseDialogAsync(DoneButton);
    }

    public void RequestClose()
    {
        CloseDialog();
    }

    private async Task CloseDialogAsync(object? sender)
    {
        if (isClosing)
        {
            return;
        }

        isClosing = true;
        await UiAnimations.PressAsync(sender);
        await Navigation.PopModalAsync();

        if (afterDone is not null)
        {
            await afterDone();
        }
    }

    private async Task PlayIntroAsync()
    {
        DialogCard.Opacity = 0;
        DialogCard.Scale = 0.92;

        await Task.WhenAll(
            DialogCard.FadeToAsync(1, 180, Easing.CubicOut),
            DialogCard.ScaleToAsync(1, 220, Easing.CubicOut));
    }

#if WINDOWS
    private void AttachWindowEnterKey()
    {
        nativeWindow = Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (nativeWindow?.Content is null)
        {
            return;
        }

        nativeWindow.Content.KeyDown -= NativeWindow_KeyDown;
        nativeWindow.Content.KeyDown += NativeWindow_KeyDown;
    }

    private void NativeWindow_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs args)
    {
        if (args.Key != Windows.System.VirtualKey.Enter)
        {
            return;
        }

        args.Handled = true;
        CloseDialog();
    }

    private void AttachDoneButtonAccelerator()
    {
        if (DoneButton.Handler?.PlatformView is not Microsoft.UI.Xaml.Controls.Button nativeButton)
        {
            return;
        }

        nativeButton.KeyboardAccelerators.Clear();
        nativeButton.KeyboardAccelerators.Add(new Microsoft.UI.Xaml.Input.KeyboardAccelerator
        {
            Key = Windows.System.VirtualKey.Enter
        });
        nativeButton.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }
#endif
}

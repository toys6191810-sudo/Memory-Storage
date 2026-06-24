namespace Memory_Storage;

public partial class RecordDetailPage : ContentPage
{
    public RecordDetailPage(MemoryRecord record)
    {
        InitializeComponent();
        BindingContext = record;
        AppUi.Changed += AppUi_Changed;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyUi();
    }

    private void AppUi_Changed(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(ApplyUi);
    }

    private void ApplyUi()
    {
        if (BindingContext is MemoryRecord record)
        {
            record.RefreshLocalizedText();
        }

        BackgroundColor = AppUi.PageBackground;
        RootScroll.BackgroundColor = AppUi.PageBackground;
        DetailStack.BackgroundColor = AppUi.PageBackground;
        TitleLabel.Text = AppUi.T("MemoryRecordFile");
        TitleLabel.TextColor = AppUi.Text;
    }
}

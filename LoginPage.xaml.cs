using System.Text.RegularExpressions;

namespace Memory_Storage;

public partial class LoginPage : ContentPage
{
    private string _validationKey = string.Empty;
    private bool isSubmitting;

    public LoginPage()
    {
        InitializeComponent();
        AppUi.Changed += AppUi_Changed;
        EnterKeyHelper.ClickOnEnter(ConfirmButton, SubmitForm);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        isSubmitting = false;
        ApplyUi();
    }

    private void AppUi_Changed(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(ApplyUi);
    }

    private void EmailEntry_Completed(object? sender, EventArgs e)
    {
        PasswordEntry.Focus();
    }

    private void PasswordEntry_Completed(object? sender, EventArgs e)
    {
        ConfirmPasswordEntry.Focus();
    }

    private async void PasswordToggleButton_Clicked(object? sender, EventArgs e)
    {
        await UiAnimations.PressAsync(PasswordToggleButton);
        SetPasswordEntryVisibility(PasswordEntry.IsPassword);
    }

    private async void ConfirmPasswordToggleButton_Clicked(object? sender, EventArgs e)
    {
        await UiAnimations.PressAsync(ConfirmPasswordToggleButton);
        SetConfirmPasswordEntryVisibility(ConfirmPasswordEntry.IsPassword);
    }

    private async void ConfirmButton_Clicked(object? sender, EventArgs e)
    {
        await SubmitFormAsync(sender);
    }

    private void SubmitForm()
    {
        _ = SubmitFormAsync(ConfirmButton);
    }

    private async Task SubmitFormAsync(object? sender)
    {
        try
        {
            if (isSubmitting)
            {
                SuccessDialogPage.ActiveDialog?.RequestClose();
                return;
            }

            isSubmitting = true;
            await UiAnimations.PressAsync(sender);

            if (!ValidateForm())
            {
                isSubmitting = false;
                return;
            }

            if (!UserProfileStore.TryLogin(EmailEntry.Text!.Trim(), PasswordEntry.Text!))
            {
                SetValidation("ValidationLoginMismatch");
                isSubmitting = false;
                return;
            }

            MemoryRecordStore.RestoreForCurrentUser();
            MemoryRecordStore.AddSelfOperation($"{AppUi.T("Login")} ➡︎ {AppUi.T("MemberLoginSuccess")}");
            await Navigation.PushModalAsync(new SuccessDialogPage(
                AppUi.T("MemberLoginSuccess"),
                async () => await Navigation.PopAsync()));
        }
        catch (Exception ex)
        {
            SetValidation(ex.Message);
            isSubmitting = false;
        }
    }

    private bool ValidateForm()
    {
        if (!IsValidEmail(EmailEntry.Text))
        {
            SetValidation("ValidationEmail");
            return false;
        }

        if (!IsValidPassword(PasswordEntry.Text))
        {
            SetValidation("ValidationPassword");
            return false;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            SetValidation("ValidationPasswordMatch");
            return false;
        }

        SetValidation(string.Empty);
        return true;
    }

    private void SetPasswordEntryVisibility(bool shouldShow)
    {
        PasswordEntry.IsPassword = !shouldShow;
        ApplyPasswordToggleText();
    }

    private void SetConfirmPasswordEntryVisibility(bool shouldShow)
    {
        ConfirmPasswordEntry.IsPassword = !shouldShow;
        ApplyPasswordToggleText();
    }

    private void SetValidation(string key)
    {
        _validationKey = key;
        ValidationLabel.Text = string.IsNullOrWhiteSpace(key) ? string.Empty : AppUi.T(key);
        if (ValidationLabel.Text == key && !key.StartsWith("Validation", StringComparison.Ordinal))
        {
            ValidationLabel.Text = key;
        }

        ValidationLabel.IsVisible = !string.IsNullOrWhiteSpace(key);
    }

    private static bool IsValidEmail(string? email)
    {
        return !string.IsNullOrWhiteSpace(email)
            && Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private static bool IsValidPassword(string? password)
    {
        return !string.IsNullOrWhiteSpace(password)
            && password.Length >= 8
            && password.Any(char.IsLetter)
            && password.Any(char.IsDigit);
    }

    private void ApplyUi()
    {
        BackgroundColor = AppUi.PageBackground;
        RootGrid.BackgroundColor = AppUi.PageBackground;
        FormCard.BackgroundColor = Colors.Transparent;
        TitleLabel.Text = AppUi.T("LoginProfile");
        TitleLabel.TextColor = AppUi.Text;
        EmailEntry.Placeholder = AppUi.T("Email");
        PasswordEntry.Placeholder = AppUi.T("Password");
        ConfirmPasswordEntry.Placeholder = AppUi.T("ConfirmPassword");
        PasswordToggleButton.BackgroundColor = Colors.Transparent;
        ConfirmPasswordToggleButton.BackgroundColor = Colors.Transparent;
        ApplyPasswordToggleText();
        ConfirmButton.Text = AppUi.T("Confirm");
        ValidationLabel.Text = string.IsNullOrWhiteSpace(_validationKey) ? string.Empty : AppUi.T(_validationKey);
        ValidationLabel.IsVisible = !string.IsNullOrWhiteSpace(_validationKey);
    }

    private void ApplyPasswordToggleText()
    {
        var isShowingPassword = !PasswordEntry.IsPassword;
        var isShowingConfirmPassword = !ConfirmPasswordEntry.IsPassword;

        PasswordToggleLabel.Text = AppUi.T(isShowingPassword ? "Hide" : "Show");
        ConfirmPasswordToggleLabel.Text = AppUi.T(isShowingConfirmPassword ? "Hide" : "Show");
        PasswordToggleLabel.TextColor = isShowingPassword ? Color.FromArgb("#D33A2C") : Color.FromArgb("#168A52");
        ConfirmPasswordToggleLabel.TextColor = isShowingConfirmPassword ? Color.FromArgb("#D33A2C") : Color.FromArgb("#168A52");
    }
}

namespace Memory_Storage;

public partial class ProfilePage : ContentPage
{
    private bool isPasswordVisible;

    public ProfilePage()
    {
        InitializeComponent();
        AppUi.Changed += AppUi_Changed;
        UserProfileStore.Changed += UserProfileStore_Changed;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyUi();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        AppUi.Changed -= AppUi_Changed;
        UserProfileStore.Changed -= UserProfileStore_Changed;
    }

    private void AppUi_Changed(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(ApplyUi);
    }

    private void UserProfileStore_Changed(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(ApplyUi);
    }

    private async void ChangeAvatarButton_Clicked(object? sender, EventArgs e)
    {
        await UiAnimations.PressAsync(sender);

        if (!UserProfileStore.IsLoggedIn)
        {
            await DisplayAlertAsync($"✕ {AppUi.T("Warning")}", AppUi.T("AvatarLoginRequired"), "OK");
            return;
        }

        var image = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = AppUi.T("ChooseAvatarImage"),
            FileTypes = FilePickerFileType.Images
        });

        if (image is null)
        {
            return;
        }

        UserProfileStore.AvatarPath = image.FullPath;
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Profile")} ➡︎ {AppUi.T("ChangeAvatar")}");
    }

    private async void PasswordToggleButton_Tapped(object? sender, TappedEventArgs e)
    {
        await UiAnimations.PressAsync(PasswordToggleButton);
        isPasswordVisible = !isPasswordVisible;
        ApplyPasswordDisplay();
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Profile")} ➡︎ {AppUi.T("Password")} ➡︎ {AppUi.T(isPasswordVisible ? "Show" : "Hide")}");
    }

    private async void LogoutButton_Clicked(object? sender, EventArgs e)
    {
        await UiAnimations.PressAsync(sender);
        MemoryRecordStore.AddSelfOperation($"{AppUi.T("Profile")} ➡︎ {AppUi.T("Logout")}");
        UserProfileStore.Logout();
        await Navigation.PopAsync();
    }

    private void ApplyUi()
    {
        BackgroundColor = AppUi.PageBackground;
        RootGrid.BackgroundColor = AppUi.PageBackground;
        ProfileCard.BackgroundColor = AppUi.Surface;
        ProfileCard.Stroke = AppUi.Border;
        AvatarFrame.BackgroundColor = AppUi.SoftSurface;
        NameField.BackgroundColor = AppUi.IsDarkMode ? Color.FromArgb("#111820") : Color.FromArgb("#F8FAFC");
        EmailField.BackgroundColor = NameField.BackgroundColor;
        PasswordField.BackgroundColor = NameField.BackgroundColor;
        NameField.Stroke = AppUi.Border;
        EmailField.Stroke = AppUi.Border;
        PasswordField.Stroke = AppUi.Border;

        TitleLabel.Text = AppUi.T("Profile");
        ChangeAvatarButton.Text = AppUi.T("ChangeAvatar");
        NameCaptionLabel.Text = AppUi.T("UserName");
        EmailCaptionLabel.Text = AppUi.T("Email");
        PasswordCaptionLabel.Text = AppUi.T("Password");
        LogoutButton.Text = AppUi.T("Logout");

        TitleLabel.TextColor = AppUi.Text;
        NameCaptionLabel.TextColor = AppUi.MutedText;
        EmailCaptionLabel.TextColor = AppUi.MutedText;
        PasswordCaptionLabel.TextColor = AppUi.MutedText;
        NameValueLabel.TextColor = AppUi.Text;
        EmailValueLabel.TextColor = AppUi.Text;
        PasswordValueLabel.TextColor = AppUi.Text;
        ChangeAvatarButton.BackgroundColor = AppUi.SoftSurface;
        ChangeAvatarButton.TextColor = AppUi.Primary;

        NameValueLabel.Text = string.IsNullOrWhiteSpace(UserProfileStore.Name) ? AppUi.T("NotLoggedIn") : UserProfileStore.Name;
        EmailValueLabel.Text = string.IsNullOrWhiteSpace(UserProfileStore.Email) ? AppUi.T("NotLoggedIn") : UserProfileStore.Email;

        UpdateAvatar();
        ApplyPasswordDisplay();
    }

    private void UpdateAvatar()
    {
        var hasAvatar = !string.IsNullOrWhiteSpace(UserProfileStore.AvatarPath) && File.Exists(UserProfileStore.AvatarPath);
        AvatarImage.IsVisible = hasAvatar;
        AvatarIcon.IsVisible = !hasAvatar;
        AvatarIcon.Invalidate();

        if (hasAvatar)
        {
            AvatarImage.Source = ImageSource.FromFile(UserProfileStore.AvatarPath);
        }
    }

    private void ApplyPasswordDisplay()
    {
        if (string.IsNullOrWhiteSpace(UserProfileStore.Password))
        {
            PasswordValueLabel.Text = AppUi.T("NotLoggedIn");
        }
        else
        {
            PasswordValueLabel.Text = isPasswordVisible
                ? UserProfileStore.Password
                : new string('●', Math.Max(6, UserProfileStore.Password.Length));
        }

        PasswordToggleLabel.Text = AppUi.T(isPasswordVisible ? "Hide" : "Show");
        PasswordToggleLabel.TextColor = isPasswordVisible ? Color.FromArgb("#D33A2C") : Color.FromArgb("#168A52");
    }
}

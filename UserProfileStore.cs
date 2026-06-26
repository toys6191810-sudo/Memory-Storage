namespace Memory_Storage;

public static class UserProfileStore
{
    public static event EventHandler? Changed;

    private static readonly string StoreDirectory = Path.Combine(FileSystem.AppDataDirectory, "profiles");
    private static readonly string AccountsFilePath = Path.Combine(StoreDirectory, "accounts.json");
    private static readonly string SessionFilePath = Path.Combine(StoreDirectory, "active-session.txt");
    private static readonly Dictionary<string, UserProfileData> Accounts = LoadAccounts();
    private static string avatarPath = string.Empty;
    private static bool isLoggedIn;

    static UserProfileStore()
    {
        RestoreLoginSession();
    }

    public static string Name { get; set; } = string.Empty;

    public static string Email { get; set; } = string.Empty;

    public static string Password { get; set; } = string.Empty;

    public static string AvatarPath
    {
        get => avatarPath;
        set
        {
            avatarPath = value;
            SaveCurrentProfile();
            Changed?.Invoke(null, EventArgs.Empty);
        }
    }

    public static bool IsLoggedIn
    {
        get => isLoggedIn;
        set
        {
            isLoggedIn = value;
            Changed?.Invoke(null, EventArgs.Empty);
        }
    }

    public static void InitializeSession()
    {
        // Triggers the static constructor before app launch data is restored.
    }

    public static void NotifyChanged()
    {
        SaveCurrentProfile();
        Changed?.Invoke(null, EventArgs.Empty);
    }

    public static bool Register(string name, string email, string password)
    {
        var key = NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        Accounts[key] = new UserProfileData
        {
            Name = name,
            Email = email,
            Password = password,
            AvatarPath = AvatarPath
        };

        Name = name;
        Email = email;
        Password = password;
        SaveAccounts();
        Changed?.Invoke(null, EventArgs.Empty);
        return true;
    }

    public static bool TryLogin(string email, string password)
    {
        var key = NormalizeEmail(email);

        if (!Accounts.TryGetValue(key, out var account) || account.Password != password)
        {
            return false;
        }

        Name = account.Name;
        Email = account.Email;
        Password = account.Password;
        avatarPath = account.AvatarPath;
        isLoggedIn = true;
        SaveLoginSession();
        Changed?.Invoke(null, EventArgs.Empty);
        return true;
    }

    public static void Logout()
    {
        SaveCurrentProfile();
        MemoryRecordStore.SaveForCurrentUser();
        DeleteLoginSession();
        isLoggedIn = false;
        Name = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        avatarPath = string.Empty;
        Changed?.Invoke(null, EventArgs.Empty);
        MemoryRecordStore.StartGuestSession();
    }

    private static void RestoreLoginSession()
    {
        try
        {
            if (!File.Exists(SessionFilePath))
            {
                return;
            }

            var key = NormalizeEmail(File.ReadAllText(SessionFilePath));
            if (!Accounts.TryGetValue(key, out var account))
            {
                DeleteLoginSession();
                return;
            }

            Name = account.Name;
            Email = account.Email;
            Password = account.Password;
            avatarPath = account.AvatarPath;
            isLoggedIn = true;
        }
        catch
        {
            isLoggedIn = false;
        }
    }

    private static void SaveLoginSession()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            return;
        }

        Directory.CreateDirectory(StoreDirectory);
        File.WriteAllText(SessionFilePath, NormalizeEmail(Email));
    }

    private static void DeleteLoginSession()
    {
        try
        {
            if (File.Exists(SessionFilePath))
            {
                File.Delete(SessionFilePath);
            }
        }
        catch
        {
        }
    }

    private static void SaveCurrentProfile()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            return;
        }

        Accounts[NormalizeEmail(Email)] = new UserProfileData
        {
            Name = Name,
            Email = Email,
            Password = Password,
            AvatarPath = AvatarPath
        };

        SaveAccounts();
    }

    private static Dictionary<string, UserProfileData> LoadAccounts()
    {
        try
        {
            if (!File.Exists(AccountsFilePath))
            {
                return new Dictionary<string, UserProfileData>(StringComparer.OrdinalIgnoreCase);
            }

            var accounts = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, UserProfileData>>(File.ReadAllText(AccountsFilePath));
            return accounts is null
                ? new Dictionary<string, UserProfileData>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, UserProfileData>(accounts, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, UserProfileData>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static void SaveAccounts()
    {
        Directory.CreateDirectory(StoreDirectory);
        var json = System.Text.Json.JsonSerializer.Serialize(Accounts, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(AccountsFilePath, json);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private sealed class UserProfileData
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AvatarPath { get; set; } = string.Empty;
    }
}

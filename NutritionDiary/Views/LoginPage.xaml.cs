using NutritionDiary.Services;
namespace NutritionDiary.Views;

public partial class LoginPage : ContentPage
{
    private DatabaseHelper _dbHelper;
    public LoginPage()
    {
        InitializeComponent();
        _dbHelper = new DatabaseHelper();
        LoginButton.Clicked += OnLoginClicked;
        RegisterButton.Clicked += OnRegisterClicked;
        SkipButton.Clicked += OnSkipClicked;

    }
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Ошибка", "Введите логин и пароль", "OK");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        LoginButton.IsEnabled = false;
        RegisterButton.IsEnabled = false;
        SkipButton.IsEnabled = false;

        try
        {
            int userId = await _dbHelper.CheckLogin(username, password);

            if (userId > 0)
            {
                Preferences.Set("UserId", userId);
                Preferences.Set("Username", username);

                // Успешный вход - переходим на AppShell
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                await DisplayAlert("Ошибка", "Неверный логин или пароль", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка входа: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            LoginButton.IsEnabled = true;
            RegisterButton.IsEnabled = true;
            SkipButton.IsEnabled = true;
        }
    }
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        try
        {
            // Используем NavigationPage навигацию
            await Navigation.PushAsync(new RegistrationPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось открыть регистрацию: {ex.Message}", "OK");
        }
    }
    private async void OnSkipClicked(object sender, EventArgs e)
    {
        try
        {
            Preferences.Set("UserId", 0);
            Preferences.Set("Username", "Гость");

            // Переходим на AppShell через NavigationPage
            Application.Current.MainPage = new AppShell();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось перейти: {ex.Message}", "OK");
        }
    }
}
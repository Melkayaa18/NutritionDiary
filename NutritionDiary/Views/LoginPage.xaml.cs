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
        string username = UsernameEntry.Text?.Trim();
        string password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Внимание", "Пожалуйста, заполните все поля", "OK");
            await AnimateShake(UsernameEntry);
            await AnimateShake(PasswordEntry);
            return;
        }

        // Анимация нажатия кнопки
        await AnimateButtonClick(LoginButton);

        SetLoadingState(true);

        try
        {
            int userId = await _dbHelper.CheckLogin(username, password);

            if (userId > 0)
            {
                Preferences.Set("UserId", userId);
                Preferences.Set("Username", username);

                // Анимация успешного входа
                await AnimateSuccess();
                await Task.Delay(500);

                Application.Current.MainPage = new AppShell();
            }
            else
            {
                await DisplayAlert("Ошибка", "Неверный логин или пароль", "OK");
                await AnimateShake(LoginButton);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка входа: {ex.Message}", "OK");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(RegisterButton);
        await Navigation.PushAsync(new RegistrationPage());
    }

    private async void OnSkipClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(SkipButton);

        Preferences.Set("UserId", 0);
        Preferences.Set("Username", "Гость");

        Application.Current.MainPage = new AppShell();
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LoginButton.IsEnabled = !isLoading;
        RegisterButton.IsEnabled = !isLoading;
        SkipButton.IsEnabled = !isLoading;
    }

    private async Task AnimateButtonClick(Button button)
    {
        if (button != null)
        {
            await button.ScaleTo(0.95, 50, Easing.SpringIn);
            await button.ScaleTo(1, 100, Easing.SpringOut);
        }
    }

    private async Task AnimateShake(View view)
    {
        await view.TranslateTo(-10, 0, 50);
        await view.TranslateTo(10, 0, 50);
        await view.TranslateTo(-5, 0, 50);
        await view.TranslateTo(5, 0, 50);
        await view.TranslateTo(0, 0, 50);
    }

    private async Task AnimateSuccess()
    {
        await LoginButton.ScaleTo(1.1, 200);
        await LoginButton.ScaleTo(1, 200);
    }
}
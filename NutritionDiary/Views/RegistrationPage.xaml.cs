//using Java.Nio.FileNio.Attributes;
using NutritionDiary.Services;
namespace NutritionDiary.Views;

public partial class RegistrationPage : ContentPage
{
    private DatabaseHelper _dbHelper;
    public RegistrationPage()
	{
		InitializeComponent();
        _dbHelper = new DatabaseHelper();
    }
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;
        string email = EmailEntry.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Ошибка", "Введите логин и пароль", "OK");
            return;
        }

        if (!int.TryParse(AgeEntry.Text, out int age) || age <= 0)
        {
            await DisplayAlert("Ошибка", "Введите корректный возраст", "OK");
            return;
        }

        if (!decimal.TryParse(HeightEntry.Text, out decimal height) || height <= 0)
        {
            await DisplayAlert("Ошибка", "Введите корректный рост", "OK");
            return;
        }

        if (!decimal.TryParse(CurrentWeightEntry.Text, out decimal currentWeight) || currentWeight <= 0)
        {
            await DisplayAlert("Ошибка", "Введите корректный текущий вес", "OK");
            return;
        }

        if (!decimal.TryParse(DesiredWeightEntry.Text, out decimal desiredWeight) || desiredWeight <= 0)
        {
            await DisplayAlert("Ошибка", "Введите корректный желаемый вес", "OK");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        RegisterButton.IsEnabled = false;

        try
        {
            bool success = await _dbHelper.RegisterUser(username, password, email, age, height, currentWeight, desiredWeight);
            if (success)
            {
                await DisplayAlert("Успех", "Регистрация прошла успешно! Теперь войдите в систему.", "OK");
                await Navigation.PopAsync(); // Возвращаемся на страницу входа
            }
            else
            {
                await DisplayAlert("Ошибка", "Пользователь с таким логином уже существует", "OK");
            }

        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка регистрации: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            RegisterButton.IsEnabled = true;
        }
    }

}
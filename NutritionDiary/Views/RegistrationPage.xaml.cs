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

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(sender as Button);
        await Navigation.PopAsync();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // Анимация нажатия кнопки
        await AnimateButtonClick(RegisterButton);

        // Валидация данных
        if (!ValidateInputs())
            return;

        SetLoadingState(true);

        try
        {
            bool success = await _dbHelper.RegisterUser(
                UsernameEntry.Text.Trim(),
                PasswordEntry.Text,
                EmailEntry.Text?.Trim(),
                int.Parse(AgeEntry.Text),
                decimal.Parse(HeightEntry.Text),
                decimal.Parse(CurrentWeightEntry.Text),
                decimal.Parse(DesiredWeightEntry.Text)
            );

            if (success)
            {
                await AnimateSuccess();
                await DisplayAlert("Успех", "Регистрация прошла успешно! Теперь войдите в систему.", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Ошибка", "Пользователь с таким логином уже существует", "OK");
                await AnimateShake(UsernameEntry);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка регистрации: {ex.Message}", "OK");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private bool ValidateInputs()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(UsernameEntry.Text))
            errors.Add("Введите логин");

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
            errors.Add("Введите пароль");

        if (!int.TryParse(AgeEntry.Text, out int age) || age <= 0 || age > 120)
            errors.Add("Введите корректный возраст (1-120 лет)");

        if (!decimal.TryParse(HeightEntry.Text, out decimal height) || height <= 0 || height > 250)
            errors.Add("Введите корректный рост (1-250 см)");

        if (!decimal.TryParse(CurrentWeightEntry.Text, out decimal currentWeight) || currentWeight <= 0 || currentWeight > 500)
            errors.Add("Введите корректный текущий вес");

        if (!decimal.TryParse(DesiredWeightEntry.Text, out decimal desiredWeight) || desiredWeight <= 0 || desiredWeight > 500)
            errors.Add("Введите корректный желаемый вес");

        if (errors.Any())
        {
            DisplayAlert("Внимание", string.Join("\n", errors), "OK");
            return false;
        }

        return true;
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        RegisterButton.IsEnabled = !isLoading;
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
        await RegisterButton.ScaleTo(1.1, 200);
        await RegisterButton.ScaleTo(1, 200);
    }
}
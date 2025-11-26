using System.Formats.Tar;
using NutritionDiary.Models;
using NutritionDiary.Services;
namespace NutritionDiary.Views;

public partial class AddCustomProductPage : ContentPage
{
    private DatabaseHelper _dbHelper;
    private int _userId;

    public AddCustomProductPage()
    {
        InitializeComponent();
        _dbHelper = new DatabaseHelper();
        _userId = Preferences.Get("UserId", 0);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Анимация кнопки
            await AnimateButtonClick(sender as Button);

            // Проверяем авторизацию
            if (_userId == 0)
            {
                await DisplayAlert("Ошибка", "Для создания продуктов необходимо войти в систему", "OK");
                return;
            }

            // Валидация данных
            if (!ValidateInputs())
                return;

            SetLoadingState(true);

            // Сохраняем продукт
            bool success = await _dbHelper.AddCustomProduct(
                ProductNameEntry.Text.Trim(),
                decimal.Parse(CaloriesEntry.Text),
                decimal.Parse(ProteinEntry.Text),
                decimal.Parse(FatEntry.Text),
                decimal.Parse(CarbsEntry.Text),
                _userId
            );

            if (success)
            {
                await AnimateSuccess();
                await DisplayAlert("Успех", "Продукт успешно сохранен! ??", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось сохранить продукт", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось сохранить продукт: {ex.Message}", "OK");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private bool ValidateInputs()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ProductNameEntry.Text))
            errors.Add("Введите название продукта");

        if (!decimal.TryParse(CaloriesEntry.Text, out decimal calories) || calories < 0)
            errors.Add("Введите корректное количество калорий");

        if (!decimal.TryParse(ProteinEntry.Text, out decimal protein) || protein < 0)
            errors.Add("Введите корректное количество белков");

        if (!decimal.TryParse(FatEntry.Text, out decimal fat) || fat < 0)
            errors.Add("Введите корректное количество жиров");

        if (!decimal.TryParse(CarbsEntry.Text, out decimal carbs) || carbs < 0)
            errors.Add("Введите корректное количество углеводов");

        if (errors.Any())
        {
            DisplayAlert("Внимание", string.Join("\n", errors), "OK");
            return false;
        }

        return true;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(sender as Button);
        await Navigation.PopAsync();
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        SaveButton.IsEnabled = !isLoading;
        CancelButton.IsEnabled = !isLoading;
    }

    private async Task AnimateButtonClick(Button button)
    {
        if (button != null)
        {
            await button.ScaleTo(0.95, 50, Easing.SpringIn);
            await button.ScaleTo(1, 100, Easing.SpringOut);
        }
    }

    private async Task AnimateSuccess()
    {
        await SaveButton.ScaleTo(1.1, 200);
        await SaveButton.ScaleTo(1, 200);
    }
}
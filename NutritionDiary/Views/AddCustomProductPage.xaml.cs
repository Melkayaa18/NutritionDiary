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
            // Проверяем авторизацию
            if (_userId == 0)
            {
                await DisplayAlert("Ошибка", "Для создания продуктов необходимо войти в систему", "OK");
                return;
            }

            // Валидация данных
            if (string.IsNullOrWhiteSpace(ProductNameEntry.Text))
            {
                await DisplayAlert("Ошибка", "Введите название продукта", "OK");
                return;
            }

            if (!decimal.TryParse(CaloriesEntry.Text, out decimal calories) || calories < 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное количество калорий", "OK");
                return;
            }

            if (!decimal.TryParse(ProteinEntry.Text, out decimal protein) || protein < 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное количество белков", "OK");
                return;
            }

            if (!decimal.TryParse(FatEntry.Text, out decimal fat) || fat < 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное количество жиров", "OK");
                return;
            }

            if (!decimal.TryParse(CarbsEntry.Text, out decimal carbs) || carbs < 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное количество углеводов", "OK");
                return;
            }

            // УБЕРИТЕ эту строку - она вызывает ошибку:
            // var loadingTask = DisplayAlert("Сохранение", "Сохраняем продукт...", null);

            // Вместо этого просто показываем простой alert без отмены
            // Или используйте ActivityIndicator если хотите более профессиональный вид

            // Показываем индикатор загрузки
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            // Блокируем кнопки
            SaveButton.IsEnabled = false;
            CancelButton.IsEnabled = false;

            // Сохраняем продукт
            bool success = await _dbHelper.AddCustomProduct(
                ProductNameEntry.Text.Trim(),
                calories,
                protein,
                fat,
                carbs,
                _userId
            );
            // Скрываем индикатор
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            SaveButton.IsEnabled = true;
            CancelButton.IsEnabled = true;

            if (success)
            {
                await DisplayAlert("Успех", "Продукт успешно сохранен!", "OK");

                // Возвращаемся на предыдущую страницу
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось сохранить продукт", "OK");
            }
        }
        catch (Exception ex)
        {
            // Всегда сбрасываем индикатор при ошибке
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            SaveButton.IsEnabled = true;
            CancelButton.IsEnabled = true;

            await DisplayAlert("Ошибка", $"Не удалось сохранить продукт: {ex.Message}", "OK");
        }

    }
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
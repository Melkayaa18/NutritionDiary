using System.Formats.Tar;
using NutritionDiary.Models;
using NutritionDiary.Services;
namespace NutritionDiary.Views;

public partial class AddRecipePage : ContentPage
{
    private DatabaseHelper _dbHelper;
    private int _userId;
    public AddRecipePage(int userId)
	{
		InitializeComponent();
        _dbHelper = new DatabaseHelper();
        _userId = userId;

        // Устанавливаем первую категорию по умолчанию
        CategoryPicker.SelectedIndex = 0;
    }
    private async void OnSaveRecipeClicked(object sender, EventArgs e)
    {
        try
        {
            // Валидация данных
            if (string.IsNullOrWhiteSpace(TitleEntry.Text))
            {
                await DisplayAlert("Ошибка", "Введите название рецепта", "OK");
                return;
            }

            if (!int.TryParse(CaloriesEntry.Text, out int calories) || calories < 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное количество калорий", "OK");
                return;
            }

            if (!int.TryParse(ProteinEntry.Text, out int protein) || protein < 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное количество белков", "OK");
                return;
            }

            if (!int.TryParse(FatEntry.Text, out int fat) || fat < 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное количество жиров", "OK");
                return;
            }

            if (!int.TryParse(CarbsEntry.Text, out int carbs) || carbs < 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное количество углеводов", "OK");
                return;
            }

            if (!int.TryParse(CookingTimeEntry.Text, out int cookingTime) || cookingTime <= 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное время приготовления", "OK");
                return;
            }

            // Создаем объект рецепта
            var recipe = new Recipe
            {
                Title = TitleEntry.Text.Trim(),
                Description = DescriptionEditor.Text?.Trim(),
                Category = CategoryPicker.SelectedItem?.ToString() ?? "Другое",
                CaloriesPerServing = calories,
                ProteinPerServing = protein,
                FatPerServing = fat,
                CarbsPerServing = carbs,
                CookingSteps = FormatCookingSteps(),
                CookingTime = cookingTime,
                CreatedByUserId = _userId, 
                IsActive = true
            };

            // Сохраняем рецепт
            bool success = await _dbHelper.AddRecipe(recipe);

            if (success)
            {
                await DisplayAlert("Успех", "Рецепт успешно сохранен!", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось сохранить рецепт", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось сохранить рецепт: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Ошибка сохранения рецепта: {ex.Message}");
        }
    }

    private string FormatCookingSteps()
    {
        var ingredients = IngredientsEditor.Text?.Trim();
        var steps = CookingStepsEditor.Text?.Trim();

        if (string.IsNullOrEmpty(ingredients) && string.IsNullOrEmpty(steps))
            return string.Empty;

        var result = "ИНГРЕДИЕНТЫ:\n" + (ingredients ?? "Не указаны") +
                    "\n\nШАГИ ПРИГОТОВЛЕНИЯ:\n" + (steps ?? "Не указаны");

        return result;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Подтверждение",
            "Вы уверены, что хотите отменить создание рецепта? Все несохраненные данные будут потеряны.",
            "Да, отменить", "Нет, продолжить");

        if (confirm)
        {
            await Navigation.PopAsync();
        }
    }




    //private async void OnAddPhotoClicked(object sender, EventArgs e)
    //{
    //    try
    //    {
    //        var photo = await MediaPicker.PickPhotoAsync();
    //        if (photo != null)
    //        {
    //            RecipeImage.Source = ImageSource.FromFile(photo.FullPath);
    //            // Сохраните путь к фото в рецепте
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        await DisplayAlert("Ошибка", "Не удалось выбрать фото", "OK");
    //    }
    //}
}

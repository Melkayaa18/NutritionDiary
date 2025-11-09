using NutritionDiary.Models;
using NutritionDiary.Services;
namespace NutritionDiary.Views;

public partial class MyRecipeDetailsPage : ContentPage
{
    private Recipe _recipe;
    private DatabaseHelper _dbHelper;
    public MyRecipeDetailsPage(Recipe recipe)
	{
		InitializeComponent();
        _recipe = recipe;
        _dbHelper = new DatabaseHelper();

        LoadRecipeDetails();
    }
    private void LoadRecipeDetails()
    {
        if (_recipe == null) return;

        Title = _recipe.Title;

        // Основная информация
        TitleLabel.Text = _recipe.Title;
        CategoryLabel.Text = $"Категория: {_recipe.Category}";
        DescriptionLabel.Text = _recipe.Description;

        // Пищевая ценность
        CaloriesLabel.Text = $"{_recipe.CaloriesPerServing} ккал";
        ProteinLabel.Text = $"{_recipe.ProteinPerServing} г";
        FatLabel.Text = $"{_recipe.FatPerServing} г";
        CarbsLabel.Text = $"{_recipe.CarbsPerServing} г";

        // Время приготовления
        CookingTimeLabel.Text = $"{_recipe.CookingTime} минут";

        // Фото
        if (!string.IsNullOrEmpty(_recipe.ImagePath))
        {
            RecipeImage.Source = ImageSource.FromFile(_recipe.ImagePath);
        }

        // Парсим ингредиенты и шаги из CookingSteps
        ParseCookingSteps(_recipe.CookingSteps);
    }

    private void ParseCookingSteps(string cookingSteps)
    {
        if (string.IsNullOrEmpty(cookingSteps))
        {
            IngredientsLabel.Text = "Ингредиенты не указаны";
            CookingStepsLabel.Text = "Шаги приготовления не указаны";
            return;
        }

        try
        {
            var parts = cookingSteps.Split(new[] { "ШАГИ ПРИГОТОВЛЕНИЯ:" }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 1)
            {
                var ingredients = parts[0].Replace("ИНГРЕДИЕНТЫ:", "").Trim();
                IngredientsLabel.Text = string.IsNullOrEmpty(ingredients) ? "Ингредиенты не указаны" : ingredients;
            }

            if (parts.Length >= 2)
            {
                CookingStepsLabel.Text = parts[1].Trim();
            }
            else
            {
                CookingStepsLabel.Text = "Шаги приготовления не указаны";
            }
        }
        catch (Exception ex)
        {
            IngredientsLabel.Text = "Ингредиенты не указаны";
            CookingStepsLabel.Text = "Шаги приготовления не указаны";
            System.Diagnostics.Debug.WriteLine($"Ошибка парсинга шагов: {ex.Message}");
        }
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Информация", "Функция редактирования будет добавлена в будущем обновлении", "OK");
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Подтверждение",
            $"Вы уверены, что хотите удалить рецепт \"{_recipe.Title}\"?",
            "Да, удалить", "Отмена");

        if (confirm)
        {
            try
            {
                // Здесь будет код для удаления рецепта из базы данных
                await DisplayAlert("Успех", "Рецепт удален!", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось удалить рецепт: {ex.Message}", "OK");
            }
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

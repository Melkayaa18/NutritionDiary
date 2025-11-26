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
        CategoryLabel.Text = _recipe.Category ?? "Без категории";
        DescriptionLabel.Text = _recipe.Description ?? "Описание отсутствует";

        // Пищевая ценность
        CaloriesLabel.Text = _recipe.CaloriesPerServing.ToString();
        ProteinLabel.Text = _recipe.ProteinPerServing.ToString();
        FatLabel.Text = _recipe.FatPerServing.ToString();
        CarbsLabel.Text = _recipe.CarbsPerServing.ToString();

        // Время приготовления
        CookingTimeLabel.Text = _recipe.CookingTime > 0 ? $"{_recipe.CookingTime} минут" : "Время не указано";

        // Фото
        LoadRecipeImage();

        // Парсим ингредиенты и шаги из CookingSteps
        ParseCookingSteps(_recipe.CookingSteps);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Обновляем данные при возвращении на страницу
        if (_recipe != null)
        {
            // Обновляем рецепт из базы данных
            var updatedRecipe = await _dbHelper.GetRecipeById(_recipe.RecipeId);
            if (updatedRecipe != null)
            {
                _recipe = updatedRecipe;
                LoadRecipeDetails();
            }
        }
    }

    private void LoadRecipeImage()
    {
        try
        {
            if (!string.IsNullOrEmpty(_recipe.ImagePath) && File.Exists(_recipe.ImagePath))
            {
                RecipeImage.Source = ImageSource.FromFile(_recipe.ImagePath);
                PlaceholderFrame.IsVisible = false;
            }
            else
            {
                RecipeImage.Source = null;
                PlaceholderFrame.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
            RecipeImage.Source = null;
            PlaceholderFrame.IsVisible = true;
        }
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
        await AnimateButtonClick(sender as Button);

        try
        {
            // Переходим на страницу редактирования
            await Navigation.PushAsync(new EditRecipePage(_recipe));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось открыть редактор рецепта: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Ошибка открытия редактора: {ex.Message}");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(sender as Button);

        bool confirm = await DisplayAlert("Подтверждение",
            $"Вы уверены, что хотите удалить рецепт \"{_recipe.Title}\"?",
            "Да, удалить", "Отмена");

        if (confirm)
        {
            try
            {
                bool success = await _dbHelper.DeleteRecipe(_recipe.RecipeId);

                if (success)
                {
                    await AnimateSuccess();
                    await DisplayAlert("Успех", "Рецепт успешно удален! ???", "OK");

                    // Возвращаемся на предыдущую страницу
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось удалить рецепт", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось удалить рецепт: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления рецепта: {ex.Message}");
            }
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(sender as Button);
        await Navigation.PopAsync();
    }

    // Методы анимации
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
        await EditButton.ScaleTo(1.1, 200);
        await EditButton.ScaleTo(1, 200);
    }
}
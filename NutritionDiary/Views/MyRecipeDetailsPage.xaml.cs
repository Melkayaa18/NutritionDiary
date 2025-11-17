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
        DescriptionLabel.Text = _recipe.Description ?? "Описание отсутствует";

        // Пищевая ценность
        CaloriesLabel.Text = $"{_recipe.CaloriesPerServing} ккал";
        ProteinLabel.Text = $"{_recipe.ProteinPerServing} г";
        FatLabel.Text = $"{_recipe.FatPerServing} г";
        CarbsLabel.Text = $"{_recipe.CarbsPerServing} г";

        // Время приготовления
        CookingTimeLabel.Text = _recipe.CookingTime > 0 ? $"{_recipe.CookingTime} минут" : "Время не указано";

        // Фото - УЛУЧШЕННАЯ ЛОГИКА
        LoadRecipeImage();

        // Парсим ингредиенты и шаги из CookingSteps
        ParseCookingSteps(_recipe.CookingSteps);
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Диагностика базы данных
        if (_recipe != null)
        {
            var dbImagePath = await _dbHelper.GetRecipeImagePath(_recipe.RecipeId);
            System.Diagnostics.Debug.WriteLine($"ImagePath из БД при загрузке: '{dbImagePath}'");
            System.Diagnostics.Debug.WriteLine($"Совпадает с объектом: {dbImagePath == _recipe.ImagePath}");
        }

        LoadRecipeDetails();
    }
    private void LoadRecipeImage()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== ДИАГНОСТИКА ИЗОБРАЖЕНИЯ ===");
            System.Diagnostics.Debug.WriteLine($"ImagePath из БД: '{_recipe.ImagePath}'");
            System.Diagnostics.Debug.WriteLine($"ImagePath is null or empty: {string.IsNullOrEmpty(_recipe.ImagePath)}");

            if (!string.IsNullOrEmpty(_recipe.ImagePath))
            {
                System.Diagnostics.Debug.WriteLine($"File.Exists: {File.Exists(_recipe.ImagePath)}");
                System.Diagnostics.Debug.WriteLine($"Полный путь: {Path.GetFullPath(_recipe.ImagePath)}");
                System.Diagnostics.Debug.WriteLine($"Директория существует: {Directory.Exists(Path.GetDirectoryName(_recipe.ImagePath))}");

                if (File.Exists(_recipe.ImagePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Размер файла: {new FileInfo(_recipe.ImagePath).Length} байт");

                    // Пробуем загрузить разными способами
                    try
                    {
                        RecipeImage.Source = ImageSource.FromFile(_recipe.ImagePath);
                        System.Diagnostics.Debug.WriteLine($"? Фото загружено через FromFile: {_recipe.ImagePath}");
                    }
                    catch (Exception ex1)
                    {
                        System.Diagnostics.Debug.WriteLine($"? Ошибка FromFile: {ex1.Message}");

                        // Пробуем альтернативный способ
                        try
                        {
                            var imageBytes = File.ReadAllBytes(_recipe.ImagePath);
                            RecipeImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                            System.Diagnostics.Debug.WriteLine($"? Фото загружено через FromStream");
                        }
                        catch (Exception ex2)
                        {
                            System.Diagnostics.Debug.WriteLine($"? Ошибка FromStream: {ex2.Message}");
                            ShowPlaceholder();
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"? Файл не существует по пути: {_recipe.ImagePath}");
                    ShowPlaceholder();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"? ImagePath пустой или null");
                ShowPlaceholder();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Общая ошибка загрузки изображения: {ex.Message}");
            ShowPlaceholder();
        }
    }
    private void ShowPlaceholder()
    {
        // Показываем placeholder
        RecipeImage.Source = "placeholder_recipe.png";
        System.Diagnostics.Debug.WriteLine($"?? Показан placeholder");
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
        bool confirm = await DisplayAlert("Подтверждение",
        $"Вы уверены, что хотите удалить рецепт \"{_recipe.Title}\"?",
        "Да, удалить", "Отмена");

        if (confirm)
        {
            try
            {
                // Показываем индикатор загрузки
                var loadingTask = DisplayAlert("Удаление", "Удаляем рецепт...", null);

                bool success = await _dbHelper.DeleteRecipe(_recipe.RecipeId);

                await loadingTask; // Закрываем alert

                if (success)
                {
                    await DisplayAlert("Успех", "Рецепт успешно удален!", "OK");

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
        await Navigation.PopAsync();
    }
}

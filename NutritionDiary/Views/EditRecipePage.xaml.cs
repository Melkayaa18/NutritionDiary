using NutritionDiary.Models;
using NutritionDiary.Services;
using Microsoft.Maui.Storage;

namespace NutritionDiary.Views;

public partial class EditRecipePage : ContentPage
{
    private DatabaseHelper _dbHelper;
    private Recipe _recipe;
    private string _selectedImagePath;

    public EditRecipePage(Recipe recipe)
    {
        InitializeComponent();
        _dbHelper = new DatabaseHelper();
        _recipe = recipe;

        LoadRecipeData();
    }

    private void LoadRecipeData()
    {
        if (_recipe == null) return;

        // Заполняем поля данными рецепта
        TitleEntry.Text = _recipe.Title;
        DescriptionEditor.Text = _recipe.Description;

        // Устанавливаем категорию
        if (!string.IsNullOrEmpty(_recipe.Category))
        {
            var index = CategoryPicker.Items.IndexOf(_recipe.Category);
            if (index >= 0)
                CategoryPicker.SelectedIndex = index;
            else
                CategoryPicker.SelectedIndex = 0;
        }
        else
        {
            CategoryPicker.SelectedIndex = 0;
        }

        // Пищевая ценность
        CaloriesEntry.Text = _recipe.CaloriesPerServing.ToString();
        ProteinEntry.Text = _recipe.ProteinPerServing.ToString();
        FatEntry.Text = _recipe.FatPerServing.ToString();
        CarbsEntry.Text = _recipe.CarbsPerServing.ToString();

        // Время приготовления
        CookingTimeEntry.Text = _recipe.CookingTime.ToString();

        // Фото
        if (!string.IsNullOrEmpty(_recipe.ImagePath) && File.Exists(_recipe.ImagePath))
        {
            RecipeImage.Source = ImageSource.FromFile(_recipe.ImagePath);
            _selectedImagePath = _recipe.ImagePath;
            ImageFrame.IsVisible = true;
            RemovePhotoButton.IsVisible = true;
        }

        // Парсим ингредиенты и шаги из CookingSteps
        ParseCookingSteps(_recipe.CookingSteps);
    }

    private void ParseCookingSteps(string cookingSteps)
    {
        if (string.IsNullOrEmpty(cookingSteps))
        {
            IngredientsEditor.Text = "";
            CookingStepsEditor.Text = "";
            return;
        }

        try
        {
            var parts = cookingSteps.Split(new[] { "ШАГИ ПРИГОТОВЛЕНИЯ:" }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 1)
            {
                var ingredients = parts[0].Replace("ИНГРЕДИЕНТЫ:", "").Trim();
                IngredientsEditor.Text = ingredients;
            }

            if (parts.Length >= 2)
            {
                CookingStepsEditor.Text = parts[1].Trim();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка парсинга шагов: {ex.Message}");
            IngredientsEditor.Text = "";
            CookingStepsEditor.Text = "";
        }
    }

    private async void OnAddPhotoClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(sender as Button);

        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Выберите фото для рецепта",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                // Копируем файл в постоянное хранилище приложения
                _selectedImagePath = await CopyFileToAppData(result.FullPath);

                // Отображаем фото
                RecipeImage.Source = ImageSource.FromFile(_selectedImagePath);
                ImageFrame.IsVisible = true;
                RemovePhotoButton.IsVisible = true;

                await DisplayAlert("Успех", "Фото успешно обновлено!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось выбрать фото: {ex.Message}", "OK");
        }
    }

    private async Task<string> CopyFileToAppData(string sourcePath)
    {
        try
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var recipeImagesFolder = Path.Combine(appDataPath, "NutritionDiary", "RecipeImages");

            if (!Directory.Exists(recipeImagesFolder))
            {
                Directory.CreateDirectory(recipeImagesFolder);
            }

            var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{Path.GetExtension(sourcePath)}";
            var targetPath = Path.Combine(recipeImagesFolder, fileName);

            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            using (var targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
            {
                await sourceStream.CopyToAsync(targetStream);
            }

            return targetPath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка копирования фото: {ex.Message}");
            return sourcePath;
        }
    }

    private async void OnRemovePhotoClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(sender as Button);

        // Удаляем фото из хранилища
        if (!string.IsNullOrEmpty(_selectedImagePath) && File.Exists(_selectedImagePath))
        {
            try
            {
                File.Delete(_selectedImagePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления фото: {ex.Message}");
            }
        }

        // Очищаем фото
        _selectedImagePath = null;
        RecipeImage.Source = null;
        ImageFrame.IsVisible = false;
        RemovePhotoButton.IsVisible = false;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(sender as Button);

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

            // Обновляем объект рецепта
            _recipe.Title = TitleEntry.Text.Trim();
            _recipe.Description = DescriptionEditor.Text?.Trim();
            _recipe.Category = CategoryPicker.SelectedItem?.ToString() ?? "Другое";
            _recipe.CaloriesPerServing = calories;
            _recipe.ProteinPerServing = protein;
            _recipe.FatPerServing = fat;
            _recipe.CarbsPerServing = carbs;
            _recipe.ImagePath = _selectedImagePath ?? _recipe.ImagePath; // Сохраняем старый путь, если новый не выбран
            _recipe.CookingSteps = FormatCookingSteps();
            _recipe.CookingTime = cookingTime;

            // Сохраняем изменения
            bool success = await _dbHelper.UpdateRecipe(_recipe);

            if (success)
            {
                await AnimateSuccess();
                await DisplayAlert("Успех", "Рецепт успешно обновлен! ??", "OK");

                // Возвращаемся на предыдущую страницу
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось обновить рецепт", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось обновить рецепт: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Ошибка обновления рецепта: {ex.Message}");
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
        await AnimateButtonClick(sender as Button);

        bool confirm = await DisplayAlert("Подтверждение",
            "Вы уверены, что хотите отменить редактирование? Все несохраненные изменения будут потеряны.",
            "Да, отменить", "Нет, продолжить");

        if (confirm)
        {
            await Navigation.PopAsync();
        }
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
        await SaveButton.ScaleTo(1.1, 200);
        await SaveButton.ScaleTo(1, 200);
    }
}
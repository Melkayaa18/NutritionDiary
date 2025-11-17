using System.Formats.Tar;
using NutritionDiary.Models;
using NutritionDiary.Services;
namespace NutritionDiary.Views;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
public partial class AddRecipePage : ContentPage
{
    private DatabaseHelper _dbHelper;
    private int _userId;
    private string _selectedImagePath;
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
            System.Diagnostics.Debug.WriteLine($"=== СОХРАНЕНИЕ РЕЦЕПТА ===");
            System.Diagnostics.Debug.WriteLine($"ImagePath для сохранения: '{_selectedImagePath}'");
            System.Diagnostics.Debug.WriteLine($"File.Exists перед сохранением: {File.Exists(_selectedImagePath)}");

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
                ImagePath = _selectedImagePath,
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



    private async void OnAddPhotoClicked(object sender, EventArgs e)
    {
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
                AddPhotoButton.Text = "?? Изменить фото";

                await DisplayAlert("Успех", "Фото успешно добавлено!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось выбрать фото: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Ошибка выбора фото: {ex.Message}");
        }
    }
    // Метод для копирования файла в постоянное хранилище
    private async Task<string> CopyFileToAppData(string sourcePath)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== КОПИРОВАНИЕ ФАЙЛА ===");
            System.Diagnostics.Debug.WriteLine($"Источник: {sourcePath}");
            System.Diagnostics.Debug.WriteLine($"Файл существует: {File.Exists(sourcePath)}");

            // Используем специальную папку для данных приложения
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var recipeImagesFolder = Path.Combine(appDataPath, "NutritionDiary", "RecipeImages");

            System.Diagnostics.Debug.WriteLine($"Целевая папка: {recipeImagesFolder}");

            if (!Directory.Exists(recipeImagesFolder))
            {
                Directory.CreateDirectory(recipeImagesFolder);
                System.Diagnostics.Debug.WriteLine($"? Создана папка: {recipeImagesFolder}");
            }

            // Генерируем уникальное имя файла
            var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{Path.GetExtension(sourcePath)}";
            var targetPath = Path.Combine(recipeImagesFolder, fileName);

            System.Diagnostics.Debug.WriteLine($"Целевой путь: {targetPath}");

            // Копируем файл с буферизацией
            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            using (var targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
            {
                await sourceStream.CopyToAsync(targetStream);
                await targetStream.FlushAsync();
            }

            // Проверяем, что файл скопировался
            if (File.Exists(targetPath))
            {
                var fileInfo = new FileInfo(targetPath);
                System.Diagnostics.Debug.WriteLine($"? Файл скопирован успешно");
                System.Diagnostics.Debug.WriteLine($"Размер: {fileInfo.Length} байт");
                System.Diagnostics.Debug.WriteLine($"Путь для БД: {targetPath}");
                return targetPath;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"? Файл не скопировался!");
                return sourcePath; // Возвращаем оригинальный путь как fallback
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Ошибка копирования фото: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            return sourcePath; // Возвращаем оригинальный путь в случае ошибки
        }
    }
    private void OnRemovePhotoClicked(object sender, EventArgs e)
    {
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

        // Удаляем фото
        _selectedImagePath = null;
        RecipeImage.Source = null;
        ImageFrame.IsVisible = false;
        RemovePhotoButton.IsVisible = false;
        AddPhotoButton.Text = "?? Добавить фото";
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


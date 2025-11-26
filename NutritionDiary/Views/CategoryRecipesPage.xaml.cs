using NutritionDiary.Models;
using NutritionDiary.Services;
using System.Collections.ObjectModel;

namespace NutritionDiary.Views;

public partial class CategoryRecipesPage : ContentPage
{
    private string _categoryName;
    private DatabaseHelper _dbHelper;
    private ObservableCollection<Recipe> _recipes;

    public CategoryRecipesPage(string categoryName)
    {
        InitializeComponent();
        _categoryName = categoryName;
        _dbHelper = new DatabaseHelper();
        _recipes = new ObservableCollection<Recipe>();

        LoadCategoryData();
        LoadRecipes();
    }

    private void LoadCategoryData()
    {
        Title = _categoryName;
        TitleLabel.Text = _categoryName;
        DescriptionLabel.Text = GetCategoryDescription(_categoryName);
    }

    private async void LoadRecipes()
    {
        try
        {
            // Получаем рецепты из базы данных
            var recipes = await _dbHelper.GetRecipesByCategory(_categoryName);

            _recipes.Clear();
            foreach (var recipe in recipes)
            {
                _recipes.Add(recipe);
            }

            RecipesCollectionView.ItemsSource = _recipes;

            // Обновляем счетчик
            RecipesCountLabel.Text = $"Найдено рецептов: {_recipes.Count}";

            // Показываем/скрываем сообщение о пустом списке
            NoRecipesLabel.IsVisible = _recipes.Count == 0;
            RecipesCollectionView.IsVisible = _recipes.Count > 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить рецепты: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки рецептов: {ex.Message}");
        }
    }

    private string GetCategoryDescription(string category)
    {
        return category switch
        {
            "Завтрак" => "Энергичные и питательные завтраки для начала дня",
            "Обед" => "Сытные и сбалансированные обеды для поддержания энергии",
            "Ужин" => "Легкие ужины для комфортного пищеварения вечером",
            "Перекус" => "Полезные перекусы между основными приемами пищи",
            "Десерт" => "Вкусные и полезные десерты",
            "Напиток" => "Освежающие и питательные напитки",
            _ => "Коллекция вкусных и полезных рецептов"
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Обновляем список при возвращении на страницу
        LoadRecipes();
    }
}
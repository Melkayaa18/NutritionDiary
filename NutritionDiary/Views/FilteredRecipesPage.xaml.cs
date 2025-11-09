using NutritionDiary.Models;
using System.Collections.ObjectModel;
namespace NutritionDiary.Views;

public partial class FilteredRecipesPage : ContentPage
{
    private ObservableCollection<Recipe> _recipes;
    private RecipeFilter _appliedFilter;
    public FilteredRecipesPage(List<Recipe> recipes, RecipeFilter filter)
    {
        InitializeComponent();
        _recipes = new ObservableCollection<Recipe>(recipes);
        _appliedFilter = filter;

        LoadResults();
    }
    private void LoadResults()
    {
        if (_recipes == null || _recipes.Count == 0)
        {
            NoResultsLabel.IsVisible = true;
            RecipesCollectionView.IsVisible = false;
            ResultsTitle.Text = "Результаты не найдены";
            ResultsCount.Text = "";
        }
        else
        {
            NoResultsLabel.IsVisible = false;
            RecipesCollectionView.IsVisible = true;
            RecipesCollectionView.ItemsSource = _recipes;

            ResultsTitle.Text = "Найденные рецепты";
            ResultsCount.Text = $"Найдено: {_recipes.Count} рецептов";

            // Добавляем информацию о примененных фильтрах
            if (!string.IsNullOrEmpty(_appliedFilter.SearchText))
            {
                ResultsCount.Text += $"\nПо запросу: \"{_appliedFilter.SearchText}\"";
            }

            if (_appliedFilter.MaxCalories < 1000)
            {
                ResultsCount.Text += $"\nДо {_appliedFilter.MaxCalories} ккал";
            }
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}


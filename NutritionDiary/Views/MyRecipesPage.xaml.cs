using NutritionDiary.Models;
using NutritionDiary.Services;
using System.Collections.ObjectModel;

namespace NutritionDiary.Views;

public partial class MyRecipesPage : ContentPage
{
    private DatabaseHelper _dbHelper;
    private int _userId;
    private List<Recipe> _allRecipes;
    private ObservableCollection<Recipe> _filteredRecipes;

    public MyRecipesPage(int userId)
    {
        InitializeComponent();
        _dbHelper = new DatabaseHelper();
        _userId = userId;
        _filteredRecipes = new ObservableCollection<Recipe>();

        RecipesCollectionView.ItemsSource = _filteredRecipes;
        RefreshView.Refreshing += OnRefreshing;

        LoadMyRecipes();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadMyRecipes();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await LoadMyRecipes();
        RefreshView.IsRefreshing = false;
    }

    private async Task LoadMyRecipes()
    {
        try
        {
            _allRecipes = await _dbHelper.GetRecipesByUserId(_userId);
            UpdateRecipesDisplay();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить рецепты: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки рецептов: {ex.Message}");
        }
    }

    private void UpdateRecipesDisplay()
    {
        _filteredRecipes.Clear();

        if (_allRecipes == null || _allRecipes.Count == 0)
        {
            NoRecipesFrame.IsVisible = true;
            RecipesCollectionView.IsVisible = false;
            RecipesCountLabel.Text = "У вас 0 рецептов";
            return;
        }

        NoRecipesFrame.IsVisible = false;
        RecipesCollectionView.IsVisible = true;

        foreach (var recipe in _allRecipes)
        {
            _filteredRecipes.Add(recipe);
        }

        RecipesCountLabel.Text = $"У вас {_allRecipes.Count} рецептов";
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue ?? string.Empty;

        _filteredRecipes.Clear();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            foreach (var recipe in _allRecipes)
            {
                _filteredRecipes.Add(recipe);
            }
        }
        else
        {
            var filtered = _allRecipes.Where(r =>
                r.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                (r.Description != null && r.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (r.Category != null && r.Category.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            foreach (var recipe in filtered)
            {
                _filteredRecipes.Add(recipe);
            }
        }
    }

    private async void OnAddNewRecipeClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(sender as Button);
        await Navigation.PushAsync(new AddRecipePage(_userId));
    }

    private async void OnViewRecipeClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(sender as Button);

        try
        {
            var button = sender as Button;
            var recipe = button?.CommandParameter as Recipe;

            if (recipe != null)
            {
                await Navigation.PushAsync(new MyRecipeDetailsPage(recipe));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось открыть рецепт: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Ошибка открытия рецепта: {ex.Message}");
        }
    }

    private async Task AnimateButtonClick(Button button)
    {
        if (button != null)
        {
            await button.ScaleTo(0.95, 50, Easing.SpringIn);
            await button.ScaleTo(1, 100, Easing.SpringOut);
        }
    }
}
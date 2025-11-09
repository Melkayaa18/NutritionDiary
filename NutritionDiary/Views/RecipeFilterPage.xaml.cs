using NutritionDiary.Models;
using System.Collections.ObjectModel;
namespace NutritionDiary.Views;

public partial class RecipeFilterPage : ContentPage
{
    private RecipeFilter _currentFilter;
    private ObservableCollection<Recipe> _filteredRecipes;
    private string[] _categories = new[]
        {
            "Завтрак", "Обед", "Ужин", "Перекус",
            "Низкоуглеводные", "Высокобелковые", "Низкокалорийные",
            "Быстрое приготовление", "Веганские", "Без сахара",
            "Для похудения", "Для набора массы", "Детские"
        };
    // Диетические предпочтения
    private string[] _preferences = new[]
    {
            "Вегетарианские", "Веганские", "Без глютена", "Без лактозы",
            "Низкоуглеводные", "Высокобелковые", "Низкожировые",
            "Быстрые (до 15 мин)", "На скорую руку"
        };

    public RecipeFilterPage()
	{
		InitializeComponent();
        _currentFilter = new RecipeFilter();
        InitializeFilters();
    }
    private void InitializeFilters()
    {
        CreateCategoryButtons();
        CreatePreferenceButtons();
    }
    private void CreateCategoryButtons()
    {
        CategoriesFlexLayout.Children.Clear();

        foreach (var category in _categories)
        {
            var button = new Button
            {
                Text = category,
                BackgroundColor = Colors.LightGray,
                TextColor = Colors.Black,
                FontSize = 12,
                Padding = new Thickness(12, 6),
                CornerRadius = 15,
                Margin = new Thickness(0, 0, 8, 8)
            };

            button.Clicked += (s, e) => OnCategoryToggled(category, button);
            CategoriesFlexLayout.Children.Add(button);
        }
    }

    private void CreatePreferenceButtons()
    {
        PreferencesFlexLayout.Children.Clear();

        foreach (var preference in _preferences)
        {
            var button = new Button
            {
                Text = preference,
                BackgroundColor = Colors.LightGray,
                TextColor = Colors.Black,
                FontSize = 12,
                Padding = new Thickness(12, 6),
                CornerRadius = 15,
                Margin = new Thickness(0, 0, 8, 8)
            };

            button.Clicked += (s, e) => OnPreferenceToggled(preference, button);
            PreferencesFlexLayout.Children.Add(button);
        }
    }
    private void OnCategoryToggled(string category, Button button)
    {
        if (_currentFilter.Categories.Contains(category))
        {
            _currentFilter.Categories.Remove(category);
            button.BackgroundColor = Colors.LightGray;
            button.TextColor = Colors.Black;
        }
        else
        {
            _currentFilter.Categories.Add(category);
            button.BackgroundColor = Color.FromArgb("#4CAF50");
            button.TextColor = Colors.White;
        }
    }

    private void OnPreferenceToggled(string preference, Button button)
    {
        if (_currentFilter.DietaryPreferences.Contains(preference))
        {
            _currentFilter.DietaryPreferences.Remove(preference);
            button.BackgroundColor = Colors.LightGray;
            button.TextColor = Colors.Black;
        }
        else
        {
            _currentFilter.DietaryPreferences.Add(preference);
            button.BackgroundColor = Color.FromArgb("#2196F3");
            button.TextColor = Colors.White;
        }
    }
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _currentFilter.SearchText = e.NewTextValue ?? string.Empty;
    }

    private async void OnApplyFiltersClicked(object sender, EventArgs e)
    {
        try
        {
            // Обновляем значения фильтров
            _currentFilter.MaxCalories = (int)CalorieSlider.Value;
            _currentFilter.MaxCookingTime = (int)TimeSlider.Value;

            // Получаем отфильтрованные рецепты
            var filteredRecipes = await FilterRecipes(_currentFilter);

            // Переходим на страницу с результатами
            await Navigation.PushAsync(new FilteredRecipesPage(filteredRecipes, _currentFilter));

        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось применить фильтры: {ex.Message}", "OK");
        }
    }

    private void OnResetFiltersClicked(object sender, EventArgs e)
    {
        try
        {
            // Сбрасываем фильтры
            _currentFilter = new RecipeFilter();
            SearchBar.Text = string.Empty;
            CalorieSlider.Value = CalorieSlider.Maximum;
            TimeSlider.Value = TimeSlider.Maximum;

            // Сбрасываем кнопки категорий
            foreach (var child in CategoriesFlexLayout.Children)
            {
                if (child is Button button)
                {
                    button.BackgroundColor = Colors.LightGray;
                    button.TextColor = Colors.Black;
                }
            }

            // Сбрасываем кнопки предпочтений
            foreach (var child in PreferencesFlexLayout.Children)
            {
                if (child is Button button)
                {
                    button.BackgroundColor = Colors.LightGray;
                    button.TextColor = Colors.Black;
                }
            }

        }
        catch (Exception ex)
        {
            DisplayAlert("Ошибка", $"Не удалось сбросить фильтры: {ex.Message}", "OK");
        }
    }
    private async Task<List<Recipe>> FilterRecipes(RecipeFilter filter)
    {
        // Здесь будет логика фильтрации рецептов из базы данных
        // Пока используем заглушку

        var allRecipes = await GetAllRecipes(); // Этот метод нужно будет реализовать

        var filtered = allRecipes.Where(recipe =>
            (string.IsNullOrEmpty(filter.SearchText) ||
             recipe.Title.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase)) &&
            recipe.CaloriesPerServing <= filter.MaxCalories &&
            (!filter.Categories.Any() || filter.Categories.Contains(recipe.Category))
        ).ToList();

        return filtered;
    }

    private async Task<List<Recipe>> GetAllRecipes()
    {
        // Заглушка - в реальности будем получать из базы данных
        return new List<Recipe>
            {
                new Recipe { Title = "Овсянка с ягодами", Category = "Завтрак", CaloriesPerServing = 280 },
                new Recipe { Title = "Куриный салат", Category = "Обед", CaloriesPerServing = 350 },
                new Recipe { Title = "Тушеные овощи", Category = "Ужин", CaloriesPerServing = 180 },
                new Recipe { Title = "Гречка с грибами", Category = "Обед", CaloriesPerServing = 320 },
                new Recipe { Title = "Смузи-боул", Category = "Завтрак", CaloriesPerServing = 250 },
                new Recipe { Title = "Рыба на пару", Category = "Ужин", CaloriesPerServing = 280 },
                new Recipe { Title = "Творожная запеканка", Category = "Завтрак", CaloriesPerServing = 180 },
                new Recipe { Title = "Овощной суп", Category = "Обед", CaloriesPerServing = 200 }
            };
    }
}

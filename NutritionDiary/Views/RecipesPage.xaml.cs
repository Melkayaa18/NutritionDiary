using NutritionDiary.Models;
using NutritionDiary.Services;

using NutritionDiary.Models;
using NutritionDiary.Services;

namespace NutritionDiary.Views;

public partial class RecipesPage : ContentPage
{
    private RecipeData _dailyRecipe;
    private int _userId;
    private DatabaseHelper _dbHelper;

    public RecipesPage()
    {
        InitializeComponent();
        _dbHelper = new DatabaseHelper();
        _userId = Preferences.Get("UserId", 0);
        LoadDailyRecipe();
        LoadCategories();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadDailyRecipe();
    }

    private async void LoadDailyRecipe()
    {
        try
        {
            var randomRecipe = await _dbHelper.GetRandomRecipe();

            if (randomRecipe != null)
            {
                _dailyRecipe = new RecipeData
                {
                    Name = randomRecipe.Title,
                    Calories = (int)randomRecipe.CaloriesPerServing,
                    Protein = (int)randomRecipe.ProteinPerServing,
                    Fat = (int)randomRecipe.FatPerServing,
                    Carbs = (int)randomRecipe.CarbsPerServing,
                    Description = randomRecipe.Description
                };

                DailyRecipeName.Text = _dailyRecipe.Name;
                DailyRecipeDescription.Text = _dailyRecipe.Description;
            }
            else
            {
                LoadFallbackDailyRecipe();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки рецепта дня: {ex.Message}");
            LoadFallbackDailyRecipe();
        }
    }

    private void LoadFallbackDailyRecipe()
    {
        var fallbackRecipes = new[]
        {
            new RecipeData {
                Name = "Смузи-боул с ягодами",
                Calories = 280, Protein = 12, Fat = 8, Carbs = 40,
                Description = "Питательный смузи-боул для энергичного начала дня с свежими ягодами и орехами"
            },
            new RecipeData {
                Name = "Куриный салат с авокадо",
                Calories = 350, Protein = 25, Fat = 12, Carbs = 28,
                Description = "Свежий салат с куриной грудкой, авокадо и сезонными овощами"
            }
        };

        var random = new Random();
        _dailyRecipe = fallbackRecipes[random.Next(fallbackRecipes.Length)];

        DailyRecipeName.Text = _dailyRecipe.Name;
        DailyRecipeDescription.Text = _dailyRecipe.Description;
    }

    private void LoadCategories()
    {
        // Очищаем существующие категории
        CategoriesLayout.Children.Clear();

        var categories = new[]
        {
        new { Name = "Завтрак", Emoji = "🍳", Color = "#FFB74D" },
        new { Name = "Обед", Emoji = "🍽️", Color = "#4DB6AC" },
        new { Name = "Ужин", Emoji = "🌙", Color = "#7986CB" },
        new { Name = "Перекус", Emoji = "🍎", Color = "#A1887F" },
        new { Name = "Десерт", Emoji = "🍰", Color = "#FF9E6D" },
        new { Name = "Напиток", Emoji = "🥤", Color = "#4DB6AC" }
    };

        foreach (var category in categories)
        {
            // Создаем Frame для каждой категории
            var categoryFrame = new Frame
            {
                BackgroundColor = Color.FromArgb(category.Color),
                Padding = 15,
                CornerRadius = 10,
                HasShadow = false
            };

            // Создаем кнопку
            var button = new Button
            {
                Text = $"{category.Emoji} {category.Name}",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HeightRequest = 50
            };

            // Привязываем обработчик события
            button.Clicked += (s, e) => OnCategoryClicked(category.Name);

            // Добавляем кнопку во Frame
            categoryFrame.Content = button;

            // Добавляем Frame в CategoriesLayout
            CategoriesLayout.Children.Add(categoryFrame);
        }
    }

    private async void OnFilterRecipesClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(sender as Button);
        await Navigation.PushAsync(new RecipeFilterPage());
    }

    private async void OnDailyRecipeClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(sender as Button);
        await Navigation.PushAsync(new RecipeDetailsPage(_dailyRecipe));
    }

    private async void OnCategoryClicked(string categoryName)
    {
        await Navigation.PushAsync(new CategoryRecipesPage(categoryName));
    }

    private async void OnAddRecipeClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(sender as Button);

        if (_userId == 0)
        {
            await DisplayAlert("Требуется вход", "Для добавления рецептов необходимо войти в систему", "OK");
            return;
        }

        await Navigation.PushAsync(new AddRecipePage(_userId));
    }

    private async void OnMyRecipesClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(sender as Button);

        if (_userId == 0)
        {
            await DisplayAlert("Требуется вход", "Для просмотра рецептов необходимо войти в систему", "OK");
            return;
        }

        await Navigation.PushAsync(new MyRecipesPage(_userId));
    }

    private async void OnRandomRecipeClicked(object sender, EventArgs e)
    {
        await AnimateButtonClick(sender as Button);
        LoadDailyRecipe();
        await DisplayAlert("🎲", "Рецепт дня обновлен!", "OK");
    }

    private async Task AnimateButtonClick(Button button)
    {
        if (button != null)
        {
            await button.ScaleTo(0.95, 50, Easing.SpringIn);
            await button.ScaleTo(1, 100, Easing.SpringOut);
        }
    }
    private async void OnCategoryClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            await AnimateButtonClick(button);

            // Получаем название категории из текста кнопки (убираем эмодзи и пробелы)
            var categoryName = button.Text.Trim();
            if (categoryName.StartsWith("🍳")) categoryName = "Завтрак";
            else if (categoryName.StartsWith("🍽️")) categoryName = "Обед";
            else if (categoryName.StartsWith("🌙")) categoryName = "Ужин";
            else if (categoryName.StartsWith("🍎")) categoryName = "Перекус";
            else if (categoryName.StartsWith("🍰")) categoryName = "Десерт";
            else if (categoryName.StartsWith("🥤")) categoryName = "Напиток";
            else categoryName = categoryName.Substring(2).Trim(); // Убираем эмодзи если он есть

            // Переходим на страницу категории
            await Navigation.PushAsync(new CategoryRecipesPage(categoryName));
        }
    }
}
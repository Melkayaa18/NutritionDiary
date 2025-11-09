using NutritionDiary.Models;
using NutritionDiary.Services;

namespace NutritionDiary.Views;

public partial class RecipesPage : ContentPage
{
    private RecipeData _dailyRecipe;
    private int _userId;
    public RecipesPage()
	{
		InitializeComponent();
        LoadDailyRecipe();
        _userId = Preferences.Get("UserId", 0);
        LoadCategories();
    }
    private async void LoadDailyRecipe()
    {
        try
        {
            var dbHelper = new DatabaseHelper();
            var randomRecipe = await dbHelper.GetRandomRecipe();

            if (randomRecipe != null)
            {
                // Используем рецепт из базы данных
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

                System.Diagnostics.Debug.WriteLine($"Загружен рецепт дня: {_dailyRecipe.Name}");
            }
            else
            {
                // Если в базе нет рецептов, используем fallback
                LoadFallbackDailyRecipe();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки рецепта дня: {ex.Message}");
            LoadFallbackDailyRecipe();
        }
    }
    // Резервный метод на случай проблем с базой
    private void LoadFallbackDailyRecipe()
    {
        // Создаем массив fallback рецептов
        var fallbackRecipes = new[]
        {
        new RecipeData { Name = "Смузи-боул с ягодами", Calories = 280, Protein = 12, Fat = 8, Carbs = 40, Description = "Питательный смузи-боул для энергичного начала дня" },
        new RecipeData { Name = "Куриный салат", Calories = 350, Protein = 25, Fat = 12, Carbs = 28, Description = "Свежий салат с куриной грудкой и овощами" },
        new RecipeData { Name = "Овсянка с ягодами", Calories = 280, Protein = 10, Fat = 5, Carbs = 45, Description = "Питательная овсяная каша со свежими ягодами" },
        new RecipeData { Name = "Тушеные овощи", Calories = 180, Protein = 6, Fat = 4, Carbs = 28, Description = "Ассорти из сезонных тушеных овощей" },
        new RecipeData { Name = "Гречка с грибами", Calories = 320, Protein = 12, Fat = 8, Carbs = 50, Description = "Гречка с тушеными грибами и луком" }
    };

        // Выбираем случайный рецепт из fallback
        var random = new Random();
        _dailyRecipe = fallbackRecipes[random.Next(fallbackRecipes.Length)];

        DailyRecipeName.Text = _dailyRecipe.Name;
        DailyRecipeDescription.Text = _dailyRecipe.Description;

        System.Diagnostics.Debug.WriteLine($"Использован fallback рецепт: {_dailyRecipe.Name}");
    }

    private void LoadCategories()
    {
        var categories = new[]
   {
        new {
            Name = "Утренний заряд",
            Description = "Энергичные завтраки для начала дня",
            Color = Color.FromArgb("#FFF8E1"), // Светло-желтый
            Icon = "🌅",
            ButtonColor = Color.FromArgb("#FF9800") // Оранжевый
        },
        new {
            Name = "Энергия дня",
            Description = "Сытные и сбалансированные обеды",
            Color = Color.FromArgb("#E3F2FD"), // Светло-голубой
            Icon = "☀️",
            ButtonColor = Color.FromArgb("#2196F3") // Синий
        },
        new {
            Name = "Лёгкий вечер",
            Description = "Легкие ужины для комфортного пищеварения",
            Color = Color.FromArgb("#F3E5F5"), // Светло-фиолетовый
            Icon = "🌙",
            ButtonColor = Color.FromArgb("#9C27B0") // Фиолетовый
        },
        new {
            Name = "Здоровый перекус",
            Description = "Полезные перекусы между приемами пищи",
            Color = Color.FromArgb("#E8F5E8"), // Светло-зеленый
            Icon = "🍎",
            ButtonColor = Color.FromArgb("#4CAF50") // Зеленый
        }
    };

        foreach (var category in categories)
        {
            var categoryFrame = new Frame
            {
                BackgroundColor = category.Color,
                Padding = 20,
                CornerRadius = 15,
                BorderColor = Colors.LightGray,
                HasShadow = true, // Добавляем тень
                Shadow = new Shadow
                {
                    Brush = Brush.Black,
                    Offset = new Point(4, 4),
                    Opacity = 0.1f
                }
            };

            var layout = new HorizontalStackLayout { Spacing = 15 };

            // Иконка
            layout.Children.Add(new Label
            {
                Text = category.Icon,
                FontSize = 28,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.Black
            });

            var verticalLayout = new VerticalStackLayout { Spacing = 8 };

            // Название категории
            verticalLayout.Children.Add(new Label
            {
                Text = category.Name,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                FontSize = 16
            });

            // Описание
            verticalLayout.Children.Add(new Label
            {
                Text = category.Description,
                FontSize = 12,
                TextColor = Colors.DarkSlateGray
            });

            layout.Children.Add(verticalLayout);

            var button = new Button
            {
                Text = "Смотреть →",
                BackgroundColor = category.ButtonColor,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                CornerRadius = 8,
                FontSize = 12,
                Padding = new Thickness(15, 8)
            };

            button.Clicked += (s, e) => OnCategoryClicked(category.Name);

            layout.Children.Add(button);

            categoryFrame.Content = layout;
            CategoriesLayout.Children.Add(categoryFrame);
        }
    }

    private async void OnFilterRecipesClicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new RecipeFilterPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось открыть фильтры: {ex.Message}", "OK");
        }
    }
    private async void OnDailyRecipeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RecipeDetailsPage(_dailyRecipe));
    }

    private async void OnCategoryClicked(string categoryName)
    {
        await Navigation.PushAsync(new CategoryRecipesPage(categoryName));
    }

    private async void OnAddRecipeClicked(object sender, EventArgs e)
    {
        try
        {
            if (_userId == 0)
            {
                await DisplayAlert("Вход required", "Для добавления рецептов необходимо войти в систему", "OK");
                return;
            }

            await Navigation.PushAsync(new AddRecipePage(_userId));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось открыть страницу добавления рецепта: {ex.Message}", "OK");
        }
    }

    private async void OnMyRecipesClicked(object sender, EventArgs e)
    {
        try
        {
            if (_userId == 0)
            {
                await DisplayAlert("Вход required", "Для просмотра рецептов необходимо войти в систему", "OK");
                return;
            }

            await Navigation.PushAsync(new MyRecipesPage(_userId));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось открыть страницу моих рецептов: {ex.Message}", "OK");
        }
    }
}
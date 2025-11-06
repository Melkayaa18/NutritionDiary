using NutritionDiary.Models;

namespace NutritionDiary.Views;

public partial class RecipesPage : ContentPage
{
    private RecipeData _dailyRecipe;

    public RecipesPage()
	{
		InitializeComponent();
        LoadDailyRecipe();
        LoadCategories();
    }
    private void LoadDailyRecipe()
    {
        // Заглушка для рецепта дня
        _dailyRecipe = new RecipeData
        {
            Name = "Смузи-боул с ягодами",
            Calories = 280,
            Protein = 12,
            Fat = 8,
            Carbs = 40,
            Description = "Питательный смузи-боул для энергичного начала дня"
        };

        DailyRecipeName.Text = _dailyRecipe.Name;
        DailyRecipeDescription.Text = _dailyRecipe.Description;
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

    private async void OnDailyRecipeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RecipeDetailsPage(_dailyRecipe));
    }

    private async void OnCategoryClicked(string categoryName)
    {
        await Navigation.PushAsync(new CategoryRecipesPage(categoryName));
    }
}
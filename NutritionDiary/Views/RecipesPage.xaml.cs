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
                new { Name = "Утренний заряд", Description = "Идеи для завтрака", Color = Colors.LightGoldenrodYellow, Icon = "🌅" },
                new { Name = "Энергия дня", Description = "Варианты обеда", Color = Colors.LightBlue, Icon = "☀️" },
                new { Name = "Лёгкий вечер", Description = "Рецепты для ужина", Color = Colors.Lavender, Icon = "🌙" },
                new { Name = "Здоровый перекус", Description = "Быстрые варианты", Color = Colors.LightGreen, Icon = "🍎" }
            };

        foreach (var category in categories)
        {
            var categoryFrame = new Frame
            {
                BackgroundColor = category.Color,
                Padding = 15,
                CornerRadius = 10
            };

            var layout = new HorizontalStackLayout { Spacing = 10 };

            layout.Children.Add(new Label
            {
                Text = category.Icon,
                FontSize = 24,
                VerticalOptions = LayoutOptions.Center
            });

            var verticalLayout = new VerticalStackLayout { Spacing = 5 };
            verticalLayout.Children.Add(new Label
            {
                Text = category.Name,
                FontAttributes = FontAttributes.Bold
            });
            verticalLayout.Children.Add(new Label
            {
                Text = category.Description,
                FontSize = 12
            });

            layout.Children.Add(verticalLayout);

            var button = new Button
            {
                Text = "Смотреть",
                BackgroundColor = Colors.DarkSlateBlue,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.EndAndExpand
            };

            // Передаем название категории в событие
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
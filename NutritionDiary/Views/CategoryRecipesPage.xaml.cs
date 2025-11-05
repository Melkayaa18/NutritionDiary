using NutritionDiary.Models;
namespace NutritionDiary.Views;

public partial class CategoryRecipesPage : ContentPage
{
    private string _categoryName;
    private Label _titleLabel;
    private Label _descriptionLabel;
    private VerticalStackLayout _recipesLayout;

    public CategoryRecipesPage(string categoryName)
    {
        _categoryName = categoryName;
        BuildUI();
        LoadRecipes();
    }

    private void BuildUI()
    {
        // Создаем элементы управления вручную
        _titleLabel = new Label
        {
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center
        };

        _descriptionLabel = new Label();

        _recipesLayout = new VerticalStackLayout { Spacing = 10 };

        var scrollView = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 15,
                Padding = 20,
                Children =
                    {
                        _titleLabel,
                        _descriptionLabel,
                        _recipesLayout
                    }
            }
        };

        Content = scrollView;
    }

    private void LoadRecipes()
    {
        Title = _categoryName;
        _titleLabel.Text = _categoryName;
        _descriptionLabel.Text = GetCategoryDescription(_categoryName);

        var recipes = GetRecipesForCategory(_categoryName);

        foreach (var recipe in recipes)
        {
            var recipeFrame = new Frame
            {
                BackgroundColor = Colors.AliceBlue,
                Padding = 10,
                CornerRadius = 5
            };

            var layout = new VerticalStackLayout { Spacing = 5 };

            layout.Children.Add(new Label
            {
                Text = recipe.Name,
                FontAttributes = FontAttributes.Bold
            });

            layout.Children.Add(new Label
            {
                Text = recipe.Description,
                FontSize = 12
            });

            layout.Children.Add(new Label
            {
                Text = $"⚡ {recipe.Calories} ккал • 🥚 {recipe.Protein}г • 🥑 {recipe.Fat}г • 🌾 {recipe.Carbs}г • ⏱️ {recipe.CookingTime}",
                FontSize = 10
            });

            var detailsButton = new Button
            {
                Text = "Подробнее",
                BackgroundColor = Colors.SeaGreen,
                TextColor = Colors.White
            };

            detailsButton.Clicked += (s, e) => OnRecipeDetailsClicked(recipe);

            layout.Children.Add(detailsButton);

            recipeFrame.Content = layout;
            _recipesLayout.Children.Add(recipeFrame);
        }
    }

    private string GetCategoryDescription(string category)
    {
        return category switch
        {
            "Утренний заряд" => "Энергичные и питательные завтраки для начала дня",
            "Энергия дня" => "Сытные и сбалансированные обеды для поддержания энергии",
            "Лёгкий вечер" => "Легкие ужины для комфортного пищеварения вечером",
            "Здоровый перекус" => "Полезные перекусы между основными приемами пищи",
            _ => "Коллекция вкусных и полезных рецептов"
        };
    }

    private RecipeData[] GetRecipesForCategory(string category)
    {
        return category switch
        {
            "Утренний заряд" => new[]
            {
                    new RecipeData { Name = "Овсянка с ягодами", Calories = 280, Protein = 10, Fat = 5, Carbs = 45, Description = "Питательная овсяная каша со свежими ягодами", CookingTime = "15 мин" },
                    new RecipeData { Name = "Тост с авокадо", Calories = 320, Protein = 8, Fat = 12, Carbs = 35, Description = "Хрустящий тост с пюре из авокадо и яйцом-пашот", CookingTime = "10 мин" },
                    new RecipeData { Name = "Гречневая каша", Calories = 250, Protein = 9, Fat = 4, Carbs = 42, Description = "Ароматная гречневая каша с маслом", CookingTime = "20 мин" },
                    new RecipeData { Name = "Творожная запеканка", Calories = 180, Protein = 15, Fat = 6, Carbs = 18, Description = "Нежная творожная запеканка с изюмом", CookingTime = "30 мин" }
                },
            "Энергия дня" => new[]
            {
                    new RecipeData { Name = "Куриный салат", Calories = 350, Protein = 25, Fat = 12, Carbs = 28, Description = "Свежий салат с куриной грудкой и овощами", CookingTime = "20 мин" },
                    new RecipeData { Name = "Гречка с грибами", Calories = 320, Protein = 12, Fat = 8, Carbs = 50, Description = "Гречка с тушеными грибами и луком", CookingTime = "25 мин" },
                    new RecipeData { Name = "Рыба на пару", Calories = 280, Protein = 30, Fat = 6, Carbs = 15, Description = "Нежная рыба, приготовленная на пару с овощами", CookingTime = "30 мин" },
                    new RecipeData { Name = "Овощной суп", Calories = 200, Protein = 8, Fat = 5, Carbs = 30, Description = "Легкий овощной суп с зеленью", CookingTime = "40 мин" }
                },
            "Лёгкий вечер" => new[]
            {
                    new RecipeData { Name = "Тушеные овощи", Calories = 180, Protein = 6, Fat = 4, Carbs = 28, Description = "Ассорти из сезонных тушеных овощей", CookingTime = "25 мин" },
                    new RecipeData { Name = "Куриные котлеты", Calories = 220, Protein = 20, Fat = 8, Carbs = 12, Description = "Нежные куриные котлеты на пару", CookingTime = "35 мин" },
                    new RecipeData { Name = "Салат из морепродуктов", Calories = 190, Protein = 18, Fat = 6, Carbs = 15, Description = "Легкий салат с морепродуктами", CookingTime = "20 мин" },
                    new RecipeData { Name = "Тыквенный суп-пюре", Calories = 160, Protein = 5, Fat = 7, Carbs = 20, Description = "Нежный крем-суп из тыквы", CookingTime = "30 мин" }
                },
            "Здоровый перекус" => new[]
            {
                    new RecipeData { Name = "Йогурт с орехами", Calories = 150, Protein = 8, Fat = 9, Carbs = 12, Description = "Натуральный йогурт с грецкими орехами", CookingTime = "2 мин" },
                    new RecipeData { Name = "Фруктовый салат", Calories = 120, Protein = 2, Fat = 1, Carbs = 28, Description = "Свежий салат из сезонных фруктов", CookingTime = "10 мин" },
                    new RecipeData { Name = "Овощные палочки", Calories = 80, Protein = 3, Fat = 1, Carbs = 15, Description = "Хрустящие овощные палочки с хумусом", CookingTime = "5 мин" },
                    new RecipeData { Name = "Протеиновый батончик", Calories = 200, Protein = 15, Fat = 8, Carbs = 20, Description = "Домашний протеиновый батончик", CookingTime = "15 мин" }
                },
            _ => new[]
            {
                    new RecipeData { Name = "Базовый рецепт", Calories = 200, Protein = 10, Fat = 5, Carbs = 25, Description = "Простой и полезный рецепт", CookingTime = "20 мин" }
                }
        };
    }

    private async void OnRecipeDetailsClicked(RecipeData recipe)
    {
        await Navigation.PushAsync(new RecipeDetailsPage(recipe));
    }

}
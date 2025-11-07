using Microsoft.Maui.Graphics.Text;
using NutritionDiary.Models;

namespace NutritionDiary.Views
{
   
        public partial class RecipeDetailsPage : ContentPage
        {
            private RecipeData _recipe;

            public RecipeDetailsPage(RecipeData recipe)
            {
                InitializeComponent();
                _recipe = recipe;
                LoadRecipe();
            }

            private void LoadRecipe()
            {
                if (_recipe == null) return;

                Title = _recipe.Name;
                TitleLabel.Text = _recipe.Name;
                NutritionLabel.Text = $"🍽️ Пищевая ценность на порцию:\n" +
                                      $"⚡ Калории: {_recipe.Calories} ккал\n" +
                                      $"🥚 Белки: {_recipe.Protein}г\n" +
                                      $"🥑 Жиры: {_recipe.Fat}г\n" +
                                      $"🌾 Углеводы: {_recipe.Carbs}г";

                DescriptionLabel.Text = $"📝 Описание:\n{_recipe.Description}";
                IngredientsLabel.Text = GetIngredients(_recipe.Name);
                CookingLabel.Text = GetCookingSteps(_recipe.Name);
            
            }

            private string GetIngredients(string recipeName)
            {
                return recipeName switch
                {
                    "Овсянка с ягодами" => "🛒 Ингредиенты:\n• Овсяные хлопья - 50г\n• Молоко/вода - 200мл\n• Ягоды - 100г\n• Мед - 1 ч.л.\n• Орехи - 20г",
                    "Куриный салат" => "🛒 Ингредиенты:\n• Куриная грудка - 150г\n• Салат айсберг - 100г\n• Помидоры черри - 100г\n• Огурец - 1 шт\n• Оливковое масло - 1 ст.л.",
                    _ => "🛒 Ингредиенты будут добавлены скоро"
                };
            }

            private string GetCookingSteps(string recipeName)
            {
                return recipeName switch
                {
                    "Овсянка с ягодами" => "👩‍🍳 Приготовление:\n1. Залейте овсянку молоком\n2. Варите 5-7 минут\n3. Добавьте ягоды и мед\n\n⏱️ Время: 15 минут",
                    "Куриный салат" => "👩‍🍳 Приготовление:\n1. Отварите куриную грудку\n2. Нарежьте овощи\n3. Смешайте ингредиенты\n\n⏱️ Время: 20 минут",
                    _ => "👩‍🍳 Приготовление будет добавлено скоро"
                };
            }

            private async void OnCloseClicked(object sender, EventArgs e)
            {
                await Navigation.PopAsync();
            }
        }
   }

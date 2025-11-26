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

            // Обновляем значения питательной ценности
            CaloriesLabel.Text = _recipe.Calories.ToString();
            ProteinLabel.Text = _recipe.Protein.ToString();
            FatLabel.Text = _recipe.Fat.ToString();
            CarbsLabel.Text = _recipe.Carbs.ToString();

            DescriptionLabel.Text = _recipe.Description;
            IngredientsLabel.Text = GetIngredients(_recipe.Name);
            CookingLabel.Text = GetCookingSteps(_recipe.Name);
        }

        private string GetIngredients(string recipeName)
        {
            return recipeName switch
            {
                "Смузи-боул с ягодами" => "• Овсяные хлопья - 50г\n• Молоко/вода - 200мл\n• Смесь ягод - 100г\n• Мед - 1 ч.л.\n• Грецкие орехи - 20г\n• Семена чиа - 1 ч.л.",
                "Куриный салат с авокадо" => "• Куриная грудка - 150г\n• Авокадо - 1 шт\n• Салат айсберг - 100г\n• Помидоры черри - 100г\n• Огурец - 1 шт\n• Оливковое масло - 1 ст.л.\n• Лимонный сок - 1 ч.л.",
                "Овсянка с ягодами" => "• Овсяные хлопья - 50г\n• Молоко/вода - 200мл\n• Ягоды - 100г\n• Мед - 1 ч.л.\n• Орехи - 20г",
                "Куриный салат" => "• Куриная грудка - 150г\n• Салат айсберг - 100г\n• Помидоры черри - 100г\n• Огурец - 1 шт\n• Оливковое масло - 1 ст.л.",
                _ => "• Ингредиенты будут добавлены скоро\n• Проверьте позже"
            };
        }

        private string GetCookingSteps(string recipeName)
        {
            return recipeName switch
            {
                "Смузи-боул с ягодами" => "1. Залейте овсянку молоком и оставьте на 5 минут\n2. Взбейте в блендере до однородной массы\n3. Добавьте ягоды и мед, перемешайте\n4. Посыпьте орехами и семенами чиа\n\n⏱️ Время приготовления: 10 минут\n🍽️ Порций: 1",
                "Куриный салат с авокадо" => "1. Отварите куриную грудку и нарежьте кубиками\n2. Авокадо нарежьте ломтиками\n3. Смешайте все овощи в миске\n4. Заправьте оливковым маслом и лимонным соком\n\n⏱️ Время приготовления: 20 минут\n🍽️ Порций: 2",
                "Овсянка с ягодами" => "1. Залейте овсянку молоком\n2. Варите 5-7 минут\n3. Добавьте ягоды и мед\n\n⏱️ Время: 15 минут\n🍽️ Порций: 1",
                "Куриный салат" => "1. Отварите куриную грудку\n2. Нарежьте овощи\n3. Смешайте ингредиенты\n\n⏱️ Время: 20 минут\n🍽️ Порций: 2",
                _ => "1. Приготовление будет добавлено скоро\n2. Проверьте обновления позже\n\n⏱️ Время: уточняется\n🍽️ Порций: 1"
            };
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
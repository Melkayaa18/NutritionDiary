using NutritionDiary.Views;
namespace NutritionDiary
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            try
            {
                InitializeComponent();
                RegisterRoutes();
            }
            catch (Exception ex)
            {
                // Обработка ошибок инициализации
                Application.Current?.MainPage?.DisplayAlert("Ошибка",
                    $"Ошибка инициализации Shell: {ex.Message}", "OK");
            }
        }

        private void RegisterRoutes()
        {
            // Регистрируем все маршруты для навигации
            Routing.RegisterRoute("DiaryEntryPage", typeof(DiaryEntryPage));
            Routing.RegisterRoute("CategoryRecipesPage", typeof(CategoryRecipesPage));
            Routing.RegisterRoute("RecipeDetailsPage", typeof(RecipeDetailsPage));
            Routing.RegisterRoute("AddCustomProductPage", typeof(AddCustomProductPage));
            Routing.RegisterRoute("RecipeFilterPage", typeof(RecipeFilterPage));
            Routing.RegisterRoute("FilteredRecipesPage", typeof(FilteredRecipesPage));
        }

        private void SetStartupPage()
        {
            // Устанавливаем стартовую страницу - логин
            // Убираем MainPage из Shell и устанавливаем LoginPage
            CurrentItem = new ShellContent
            {
                Title = "Вход",
                Content = new LoginPage()
            };
        }
    }
}

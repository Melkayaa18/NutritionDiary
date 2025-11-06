using NutritionDiary.Views;

namespace NutritionDiary
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                InitializeComponent();

                // Создаем NavigationPage с LoginPage
                MainPage = new NavigationPage(new LoginPage())
                {
                    BarBackgroundColor = Color.FromArgb("#4CAF50"), // Зеленый цвет
                    BarTextColor = Colors.White
                };

                // Скрываем панель навигации на LoginPage
                NavigationPage.SetHasNavigationBar(MainPage, false);
            }
            catch (Exception ex)
            {
                MainPage = new ContentPage
                {
                    Content = new Label
                    {
                        Text = $"Ошибка запуска: {ex.Message}",
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
                    }
                };
            }
        }
    }
}
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

                // Просто устанавливаем LoginPage как стартовую
                MainPage = new LoginPage();
            }
            catch (Exception ex)
            {
                // Простая страница с ошибкой
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
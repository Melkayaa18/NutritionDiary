using NutritionDiary.Services;
namespace NutritionDiary.Views;

public partial class MainPage : ContentPage
{
    private DatabaseHelper _dbHelper;
    private int _userId;
    private int _dailyCalorieGoal = 2000;
    public MainPage()
	{
		InitializeComponent();
        _dbHelper = new DatabaseHelper();
        _userId = Preferences.Get("UserId", 0);

        RefreshView.Refreshing += OnRefreshing;
        LoadDailyProgress();
    }
    //private async void OnShowStatisticsClicked(object sender, EventArgs e)
    //{
    //    await Navigation.PushAsync(new StatisticsPage());
    //}

    //private async void OnShowRecipesClicked(object sender, EventArgs e)
    //{
    //    await Navigation.PushAsync(new RecipesPage());
    //}
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadDailyProgress();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await LoadDailyProgress();
        RefreshView.IsRefreshing = false;
    }

    private async Task LoadDailyProgress()
    {
        _userId = Preferences.Get("UserId", 0);

        if (_userId == 0)
        {
            // Гостевой режим - показываем нули
            CalorieProgressBar.Progress = 0;
            CalorieLabel.Text = "Гостевой режим";
            ProteinLabel.Text = "Б: 0г";
            FatLabel.Text = "Ж: 0г";
            CarbsLabel.Text = "У: 0г";
            TrafficLightFrame.BackgroundColor = Colors.Gray;
            return;
        }

        // Остальной код загрузки данных для авторизованного пользователя...
        var (calories, protein, fat, carbs) = await _dbHelper.GetDailySummary(_userId, DateTime.Today);

        // Обновляем прогресс калорий
        double progress = _dailyCalorieGoal > 0 ? Math.Min((double)calories / _dailyCalorieGoal, 1.0) : 0;
        CalorieProgressBar.Progress = progress;
        CalorieLabel.Text = $"Съедено: {calories:F0}/{_dailyCalorieGoal} ккал";

        // Обновляем БЖУ
        ProteinLabel.Text = $"Б: {protein:F1}г";
        FatLabel.Text = $"Ж: {fat:F1}г";
        CarbsLabel.Text = $"У: {carbs:F1}г";

        // Обновляем светофор
        if (progress < 0.8)
            TrafficLightFrame.BackgroundColor = Colors.Green;
        else if (progress < 1.0)
            TrafficLightFrame.BackgroundColor = Colors.Orange;
        else
            TrafficLightFrame.BackgroundColor = Colors.Red;
    }

    private async void OnAddBreakfastClicked(object sender, EventArgs e)
    {
        await OpenDiaryEntry(1, "Завтрак");
    }

    private async void OnAddLunchClicked(object sender, EventArgs e)
    {
        await OpenDiaryEntry(2, "Обед");
    }

    private async void OnAddDinnerClicked(object sender, EventArgs e)
    {
        await OpenDiaryEntry(3, "Ужин");
    }

    private async void OnAddSnackClicked(object sender, EventArgs e)
    {
        await OpenDiaryEntry(4, "Перекус");
    }

    private async Task OpenDiaryEntry(int mealTypeId, string mealTypeName)
    {
        try
        {
            await Navigation.PushAsync(new DiaryEntryPage(mealTypeId, mealTypeName));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось открыть страницу добавления: {ex.Message}", "OK");
        }
    }

    private async void OnShowStatisticsClicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new StatisticsPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось открыть статистику: {ex.Message}", "OK");
        }
    }

    private async void OnShowRecipesClicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new RecipesPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось открыть рецепты: {ex.Message}", "OK");
        }
    }

}
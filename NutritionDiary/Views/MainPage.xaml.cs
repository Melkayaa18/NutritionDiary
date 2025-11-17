using NutritionDiary.Services;
namespace NutritionDiary.Views;

public partial class MainPage : ContentPage
{
    private DatabaseHelper _dbHelper;
    private int _userId;
    private int _dailyCalorieGoal = 2000;

    private decimal _waterGoal = 2000; // Цель по воде в мл
    private decimal _currentWaterIntake = 0;

    public MainPage()
	{
		InitializeComponent();
        _dbHelper = new DatabaseHelper();
        _userId = Preferences.Get("UserId", 0);

        RefreshView.Refreshing += OnRefreshing;
        LoadDailyProgress();
        InitializeWaterTracker();

    }
    //private async void OnShowStatisticsClicked(object sender, EventArgs e)
    //{
    //    await Navigation.PushAsync(new StatisticsPage());
    //}

    //private async void OnShowRecipesClicked(object sender, EventArgs e)
    //{
    //    await Navigation.PushAsync(new RecipesPage());
    //}
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDailyProgress();
        await LoadWaterIntake();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await LoadDailyProgress();
        await LoadWaterIntake();
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








    // ===== МЕТОДЫ ДЛЯ ВОДЫ =====

    private async void InitializeWaterTracker()
    {
        await LoadWaterIntake();
        UpdateWaterDisplay();
    }

    private async Task LoadWaterIntake()
    {
        if (_userId == 0) return;

        _currentWaterIntake = await _dbHelper.GetTodayWaterIntake(_userId);
        UpdateWaterDisplay();
    }

    private void UpdateWaterDisplay()
    {
        // Обновляем текстовые поля
        WaterAmountLabel.Text = _currentWaterIntake.ToString("F0");
        WaterGoalLabel.Text = $"Цель: {_waterGoal} мл";

        // Рассчитываем процент
        double percentage = _waterGoal > 0 ? (double)(_currentWaterIntake / _waterGoal) : 0;
        percentage = Math.Min(percentage, 1.0); // Не больше 100%
        WaterPercentageLabel.Text = $"{percentage:P0}";

        // Обновляем мотивационное сообщение
        UpdateWaterMessage(percentage);

        // Анимируем круговой прогресс-бар
        AnimateWaterProgress(percentage);
    }

    private void UpdateWaterMessage(double percentage)
    {
        if (percentage == 0)
            WaterMessageLabel.Text = "Начните пить воду!";
        else if (percentage < 0.25)
            WaterMessageLabel.Text = "Отличное начало!";
        else if (percentage < 0.5)
            WaterMessageLabel.Text = "Продолжайте в том же духе!";
        else if (percentage < 0.75)
            WaterMessageLabel.Text = "Вы на полпути!";
        else if (percentage < 1.0)
            WaterMessageLabel.Text = "Почти у цели!";
        else
            WaterMessageLabel.Text = "Цель достигнута!";
    }

    private void AnimateWaterProgress(double percentage)
    {
        // Рассчитываем длину штриха для кругового прогресса
        double circumference = 2 * Math.PI * 56; // Примерный радиус
        double strokeLength = circumference * percentage;
        double gapLength = circumference - strokeLength;

        // Анимируем изменение StrokeDashArray
        var animation = new Animation(
            callback: v => WaterProgressCircle.StrokeDashArray = new double[] { v, 1000 },
            start: WaterProgressCircle.StrokeDashArray[0],
            end: strokeLength,
            easing: Easing.SpringOut
        );

        animation.Commit(
            owner: WaterProgressCircle,
            name: "WaterProgressAnimation",
            length: 1000,
            finished: (v, c) => { }
        );
    }

    private async void OnAddWaterClicked(object sender, EventArgs e)
    {
        if (_userId == 0)
        {
            await DisplayAlert("Ошибка", "Для отслеживания воды необходимо войти в систему", "OK");
            return;
        }

        var button = (Button)sender;
        var amount = decimal.Parse(button.CommandParameter.ToString());

        await AddWaterWithAnimation(amount, button);
    }

    private async Task AddWaterWithAnimation(decimal amount, Button button = null)
    {
        try
        {
            // Анимация кнопки (если кнопка передана)
            if (button != null)
            {
                await button.ScaleTo(1.2, 100);
                await button.ScaleTo(1.0, 100);
            }

            // Добавляем воду в базу
            bool success = await _dbHelper.AddWaterIntake(_userId, amount);

            if (success)
            {
                _currentWaterIntake += amount;
                UpdateWaterDisplay();

                // Дополнительные анимации
                await PlayWaterAdditionAnimation();

                // Показываем всплывающее сообщение
                await DisplayAlert("?", $"Добавлено {amount} мл воды", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось добавить воду", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось добавить воду: {ex.Message}", "OK");
        }
    }

    private async void OnCustomWaterClicked(object sender, EventArgs e)
    {
        if (_userId == 0)
        {
            await DisplayAlert("Ошибка", "Для отслеживания воды необходимо войти в систему", "OK");
            return;
        }

        // Исправленный вызов DisplayPromptAsync
        string result = await DisplayPromptAsync(
            "Добавить воду",
            "Введите количество воды в мл:",
            "Добавить",
            "Отмена",
            "250",
            5, // maxLength
            Keyboard.Numeric
        );

        if (!string.IsNullOrEmpty(result) && decimal.TryParse(result, out decimal amount) && amount > 0)
        {
            await AddWaterWithAnimation(amount);
        }
    }

    private async void OnUndoWaterClicked(object sender, EventArgs e)
    {
        if (_userId == 0 || _currentWaterIntake == 0) return;

        bool confirm = await DisplayAlert("Отмена", "Отменить последнее добавление воды?", "Да", "Нет");

        if (confirm)
        {
            bool success = await _dbHelper.RemoveLastWaterIntake(_userId);

            if (success)
            {
                // Получаем актуальные данные из базы
                await LoadWaterIntake();
                await DisplayAlert("?", "Последнее добавление воды отменено", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось отменить добавление воды", "OK");
            }
        }
    }


    private async Task PlayWaterAdditionAnimation()
    {
        // Анимация "всплеска" воды
        await WaterProgressCircle.ScaleTo(1.1, 200, Easing.SpringOut);
        await WaterProgressCircle.ScaleTo(1.0, 200, Easing.SpringIn);

        // Анимация текста счетчика
        _ = WaterAmountLabel.FadeTo(0.5, 100);
        await WaterAmountLabel.ScaleTo(1.2, 150);
        _ = WaterAmountLabel.FadeTo(1, 100);
        await WaterAmountLabel.ScaleTo(1.0, 150);
    }

}
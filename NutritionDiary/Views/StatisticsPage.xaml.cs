using NutritionDiary.Services;

namespace NutritionDiary.Views
{
    public partial class StatisticsPage : ContentPage
    {
        private DatabaseHelper _dbHelper;
        private int _userId;

        public StatisticsPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _userId = Preferences.Get("UserId", 0);
            LoadStatistics();
        }

        private async void LoadStatistics()
        {
            if (_userId == 0) return;

            var (calories, protein, fat, carbs) = await _dbHelper.GetDailySummary(_userId, DateTime.Today);

            // Прогресс калорий
            double calorieProgress = 2000 > 0 ? Math.Min((double)calories / 2000, 1.0) : 0;
            TodayCaloriesProgress.Progress = calorieProgress;
            TodayCaloriesLabel.Text = $"Калории: {calories:F0}/2000 ккал";

            // Прогресс БЖУ (примерные нормы)
            TodayProteinProgress.Progress = (double)protein / 50;
            TodayFatProgress.Progress = (double)fat / 40;
            TodayCarbsProgress.Progress = (double)carbs / 200;

            // Статистика за неделю (заглушка)
            WeekStatsLabel.Text = "Статистика за последние 7 дней:\n\n" +
                                 "• Среднее потребление калорий: 1800 ккал/день\n" +
                                 "• Самый калорийный день: Понедельник (2100 ккал)\n" +
                                 "• Дней в норме: 5 из 7\n" +
                                 "• Общий баланс БЖУ: Хороший";

            // Достижения (заглушка)
            AchievementsLabel.Text = "Ваши достижения:\n\n" +
                                    "✓ Первый шаг - первая запись в дневнике\n" +
                                    "○ Стабильность - 7 дней подряд ведения дневника\n" +
                                    "○ Баланс - соблюдение норм БЖУ 3 дня подряд";
        }
    }
}
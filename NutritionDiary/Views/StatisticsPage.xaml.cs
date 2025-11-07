using NutritionDiary.Models;
using NutritionDiary.Services;

namespace NutritionDiary.Views
{
    public partial class StatisticsPage : ContentPage
    {
        private DatabaseHelper _dbHelper;
        private int _userId;
        private List<DailyChallenge> _todayChallenges;
        public StatisticsPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _userId = Preferences.Get("UserId", 0);
            LoadStatistics();
            LoadDailyChallenges();
        }
        

        private async void LoadDailyChallenges()
        {
            try
            {
                if (_userId == 0)
                {
                    ChallengesInfoLabel.Text = "Войдите в систему для получения челленджей";
                    return;
                }

                _todayChallenges = await _dbHelper.GetTodayChallenges(_userId);
                DisplayChallenges();
            }
            catch (Exception ex)
            {
                ChallengesInfoLabel.Text = "Ошибка загрузки челленджей";
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки челленджей: {ex.Message}");
            }
        }
        private void DisplayChallenges()
        {
            ChallengesLayout.Children.Clear();

            if (_todayChallenges == null || _todayChallenges.Count == 0)
            {
                ChallengesInfoLabel.Text = "Нет активных челленджей";
                ChallengesProgressBar.Progress = 0;
                return;
            }

            int completedCount = _todayChallenges.Count(c => c.IsCompleted);
            double progress = (double)completedCount / _todayChallenges.Count;

            ChallengesInfoLabel.Text = $"Выполнено: {completedCount}/{_todayChallenges.Count}";
            ChallengesProgressBar.Progress = progress;

            foreach (var challenge in _todayChallenges)
            {
                var challengeFrame = new Frame
                {
                    BackgroundColor = challenge.IsCompleted ? Color.FromArgb("#E8F5E8") : Color.FromArgb("#FFF3E0"),
                    Padding = 15,
                    CornerRadius = 10,
                    BorderColor = challenge.IsCompleted ? Colors.Green : Colors.LightGray,
                    HasShadow = true
                };

                var layout = new Grid
                {
                    ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto }, // Иконка
                new ColumnDefinition { Width = GridLength.Star }, // Текст
                new ColumnDefinition { Width = GridLength.Auto }  // Чекбокс
            },
                    ColumnSpacing = 12
                };

                // Иконка (первая колонка)
                var iconLabel = new Label
                {
                    Text = challenge.Icon,
                    FontSize = 24,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };
                Grid.SetColumn(iconLabel, 0);
                layout.Children.Add(iconLabel);

                // Текст (вторая колонка)
                var textLayout = new VerticalStackLayout { Spacing = 6 };

                textLayout.Children.Add(new Label
                {
                    Text = challenge.Title,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = challenge.IsCompleted ? Colors.Green : Colors.Black,
                    FontSize = 14
                });

                textLayout.Children.Add(new Label
                {
                    Text = challenge.Description,
                    FontSize = 12,
                    TextColor = Colors.DarkSlateGray
                });

                // Категория с цветным бейджем
                var categoryLabel = new Label
                {
                    Text = GetCategoryEmoji(challenge.Category) + " " + challenge.Category,
                    FontSize = 10,
                    TextColor = GetCategoryColor(challenge.Category),
                    FontAttributes = FontAttributes.Bold
                };
                textLayout.Children.Add(categoryLabel);

                Grid.SetColumn(textLayout, 1);
                layout.Children.Add(textLayout);

                // Чекбокс (третья колонка)
                var checkBox = new CheckBox
                {
                    IsChecked = challenge.IsCompleted,
                    Color = Colors.Green,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.End
                };

                checkBox.CheckedChanged += (s, e) => OnChallengeToggled(challenge, e.Value);

                Grid.SetColumn(checkBox, 2);
                layout.Children.Add(checkBox);

                challengeFrame.Content = layout;
                ChallengesLayout.Children.Add(challengeFrame);
            }
        }
        // Вспомогательные методы для категорий
        private string GetCategoryEmoji(string category)
        {
            return category switch
            {
                "Питание" => "🍎",
                "Спорт" => "💪",
                "Здоровье" => "❤️",
                _ => "🎯"
            };
        }

        private Color GetCategoryColor(string category)
        {
            return category switch
            {
                "Питание" => Color.FromArgb("#FF6B35"),
                "Спорт" => Color.FromArgb("#2E86AB"),
                "Здоровье" => Color.FromArgb("#A23B72"),
                _ => Colors.Gray
            };
        }
        private async void OnChallengeToggled(DailyChallenge challenge, bool isCompleted)
        {
            try
            {
                if (isCompleted && !challenge.IsCompleted)
                {
                    // Отмечаем как выполненное в базе
                    bool success = await _dbHelper.CompleteChallenge(challenge.ChallengeId);
                    if (success)
                    {
                        challenge.IsCompleted = true;
                        DisplayChallenges(); // Обновляем отображение

                        if (_todayChallenges.Count(c => c.IsCompleted) == 1)
                        {
                            await DisplayAlert("🎉 Отлично!", "Первый челлендж выполнен! Так держать!", "OK");
                        }
                    }
                }
                else if (!isCompleted && challenge.IsCompleted)
                {
                    // СНИМАЕМ отметку о выполнении
                    bool success = await _dbHelper.UncompleteChallenge(challenge.ChallengeId);
                    if (success)
                    {
                        challenge.IsCompleted = false;
                        DisplayChallenges(); // Обновляем отображение
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка переключения челленджа: {ex.Message}");
            }
        }
        private async void OnRefreshChallengesClicked(object sender, EventArgs e)
        {
            try
            {
                // Можно добавить логику для генерации новых челленджей
                // Пока просто перезагружаем
                await DisplayAlert("Инфо", "Новые челленджи появятся завтра! А сегодня доведите текущие до конца 😊", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", "Не удалось обновить челленджи", "OK");
            }
        }




        private async void OnDailyTipClicked(object sender, EventArgs e)
        {
            var tips = new[]
            {
        "💡 Пейте воду перед едой - это поможет съесть меньше",
        "💡 10-минутная прогулка после еды улучшает пищеварение",
        "💡 Здоровый сон - ключ к контролю аппетита",
        "💡 Готовьте еду заранее, чтобы избежать вредных перекусов",
        "💡 Медленные приемы пищи помогают лучше чувствовать насыщение",
        "💡 Белок на завтрак помогает контролировать голод в течение дня"
    };

            var random = new Random();
            var tip = tips[random.Next(tips.Length)];

            await DisplayAlert("💡 Совет дня", tip, "Спасибо!");
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
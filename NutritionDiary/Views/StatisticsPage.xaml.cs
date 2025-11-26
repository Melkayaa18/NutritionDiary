using NutritionDiary.Models;
using NutritionDiary.Services;

namespace NutritionDiary.Views
{
    public partial class StatisticsPage : ContentPage
    {
        private DatabaseHelper _dbHelper;
        private int _userId;
        private List<DailyChallenge> _todayChallenges;
        private double _progressBarWidth = 0;

        public StatisticsPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _userId = Preferences.Get("UserId", 0);

            RefreshView.Refreshing += OnRefreshing;

            // Подписываемся на событие изменения размера, чтобы получить ширину прогресс-баров
            SizeChanged += OnPageSizeChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadStatistics();
            LoadDailyChallenges();
        }

        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            // Получаем ширину для прогресс-баров (примерно 90% ширины контейнера)
            _progressBarWidth = this.Width * 0.9;
            LoadStatistics();
        }

        private async void OnRefreshing(object sender, EventArgs e)
        {
            await LoadStatistics();
            await LoadDailyChallenges();
            RefreshView.IsRefreshing = false;
        }

        private async Task LoadStatistics()
        {
            if (_userId == 0)
            {
                TodayCaloriesLabel.Text = "Войдите в систему";
                WeekStatsLabel.Text = "Для просмотра статистики войдите в систему";
                return;
            }

            var (calories, protein, fat, carbs) = await _dbHelper.GetDailySummary(_userId, DateTime.Today);

            // Анимируем прогресс бары
            double calorieProgress = 2000 > 0 ? Math.Min((double)calories / 2000, 1.0) : 0;
            await AnimateProgressBar(TodayCaloriesProgressFill, _progressBarWidth * calorieProgress);
            TodayCaloriesLabel.Text = $"{calories:F0}/2000 ккал";

            // Для БЖУ используем меньшую ширину (т.к. они в Grid с тремя колонками)
            double smallProgressBarWidth = _progressBarWidth / 3 - 30; // учитываем отступы

            double proteinProgress = Math.Min((double)protein / 50, 1.0);
            double fatProgress = Math.Min((double)fat / 40, 1.0);
            double carbsProgress = Math.Min((double)carbs / 200, 1.0);

            await AnimateProgressBar(TodayProteinProgressFill, smallProgressBarWidth * proteinProgress);
            await AnimateProgressBar(TodayFatProgressFill, smallProgressBarWidth * fatProgress);
            await AnimateProgressBar(TodayCarbsProgressFill, smallProgressBarWidth * carbsProgress);

            // Статистика за неделю
            WeekStatsLabel.Text = "📊 Статистика за последние 7 дней:\n\n" +
                                 "• Среднее потребление калорий: 1800 ккал/день\n" +
                                 "• Самый калорийный день: Понедельник (2100 ккал)\n" +
                                 "• Дней в норме: 5 из 7\n" +
                                 "• Общий баланс БЖУ: Хороший";
        }

        private async Task AnimateProgressBar(BoxView progressFill, double targetWidth)
        {
            if (targetWidth < 0) targetWidth = 0;

            var animation = new Animation(v => progressFill.WidthRequest = v, progressFill.WidthRequest, targetWidth);
            animation.Commit(progressFill, "ProgressAnimation", 16, 800, Easing.SpringOut);
        }

        private async Task LoadDailyChallenges()
        {
            try
            {
                if (_userId == 0)
                {
                    ChallengesInfoLabel.Text = "Войдите в систему";
                    return;
                }

                _todayChallenges = await _dbHelper.GetTodayChallenges(_userId);
                DisplayChallenges();
            }
            catch (Exception ex)
            {
                ChallengesInfoLabel.Text = "Ошибка загрузки";
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки челленджей: {ex.Message}");
            }
        }

        private void DisplayChallenges()
        {
            ChallengesLayout.Children.Clear();

            if (_todayChallenges == null || _todayChallenges.Count == 0)
            {
                ChallengesInfoLabel.Text = "Нет активных челленджей";
                ChallengesProgressFill.WidthRequest = 0;
                return;
            }

            int completedCount = _todayChallenges.Count(c => c.IsCompleted);
            double progress = (double)completedCount / _todayChallenges.Count;

            ChallengesInfoLabel.Text = $"{completedCount}/{_todayChallenges.Count}";

            // Анимируем прогресс челленджей
            var targetWidth = _progressBarWidth * progress;
            var animation = new Animation(
                v => ChallengesProgressFill.WidthRequest = v,
                ChallengesProgressFill.WidthRequest,
                targetWidth
            );
            animation.Commit(ChallengesProgressFill, "ChallengesProgressAnimation", 16, 800, Easing.SpringOut);

            foreach (var challenge in _todayChallenges)
            {
                var challengeFrame = new Frame
                {
                    BackgroundColor = challenge.IsCompleted ? Color.FromArgb("#E8F5E8") : Color.FromArgb("#FFF3E0"),
                    Padding = 20,
                    CornerRadius = 12,
                    BorderColor = challenge.IsCompleted ? Colors.Green : Color.FromArgb("#FFE0B2"),
                    HasShadow = true
                };

                var layout = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    ColumnSpacing = 15
                };

                // Иконка
                var iconLabel = new Label
                {
                    Text = challenge.Icon,
                    FontSize = 20,
                    VerticalOptions = LayoutOptions.Center
                };
                Grid.SetColumn(iconLabel, 0);
                layout.Children.Add(iconLabel);

                // Текст
                var textLayout = new VerticalStackLayout { Spacing = 6 };

                textLayout.Children.Add(new Label
                {
                    Text = challenge.Title,
                    FontAttributes = challenge.IsCompleted ? FontAttributes.Italic : FontAttributes.Bold,
                    TextColor = challenge.IsCompleted ? Colors.Green : Color.FromArgb("#5D4037"),
                    FontSize = 14
                });

                textLayout.Children.Add(new Label
                {
                    Text = challenge.Description,
                    FontSize = 12,
                    TextColor = Color.FromArgb("#8D6E63")
                });

                // Категория
                var categoryLabel = new Label
                {
                    Text = challenge.Category,
                    FontSize = 10,
                    TextColor = GetCategoryColor(challenge.Category),
                    FontAttributes = FontAttributes.Bold
                };
                textLayout.Children.Add(categoryLabel);

                Grid.SetColumn(textLayout, 1);
                layout.Children.Add(textLayout);

                // Чекбокс
                var checkBox = new CheckBox
                {
                    IsChecked = challenge.IsCompleted,
                    Color = Colors.Green,
                    VerticalOptions = LayoutOptions.Center
                };

                checkBox.CheckedChanged += (s, e) => OnChallengeToggled(challenge, e.Value);

                Grid.SetColumn(checkBox, 2);
                layout.Children.Add(checkBox);

                challengeFrame.Content = layout;
                ChallengesLayout.Children.Add(challengeFrame);
            }
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
                    bool success = await _dbHelper.CompleteChallenge(challenge.ChallengeId);
                    if (success)
                    {
                        challenge.IsCompleted = true;
                        DisplayChallenges();

                        if (_todayChallenges.Count(c => c.IsCompleted) == 1)
                        {
                            await DisplayAlert("🎉 Отлично!", "Первый челлендж выполнен! Так держать!", "OK");
                        }
                    }
                }
                else if (!isCompleted && challenge.IsCompleted)
                {
                    bool success = await _dbHelper.UncompleteChallenge(challenge.ChallengeId);
                    if (success)
                    {
                        challenge.IsCompleted = false;
                        DisplayChallenges();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка переключения челленджа: {ex.Message}");
            }
        }

        private async void OnDailyTipClicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);

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

        private async Task AnimateButtonClick(Button button)
        {
            if (button != null)
            {
                await button.ScaleTo(0.95, 50, Easing.SpringIn);
                await button.ScaleTo(1, 100, Easing.SpringOut);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Отписываемся от событий
            SizeChanged -= OnPageSizeChanged;
        }
    }
}
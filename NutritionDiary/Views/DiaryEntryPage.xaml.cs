using NutritionDiary.Models;
using NutritionDiary.Services;

namespace NutritionDiary.Views;

public partial class DiaryEntryPage : ContentPage
{
    private DatabaseHelper _dbHelper;
    private int _userId;
    private int _mealTypeId;
    private string _mealTypeName;
    private List<Product> _products;
    public DiaryEntryPage(int mealTypeId, string mealTypeName)
	{
        try
        {
            System.Diagnostics.Debug.WriteLine("=== Инициализация DiaryEntryPage ===");

            InitializeComponent();

            // Инициализируем _dbHelper ПЕРВЫМ делом
            _dbHelper = new DatabaseHelper();
            System.Diagnostics.Debug.WriteLine("_dbHelper инициализирован");

            _userId = Preferences.Get("UserId", 0);
            _mealTypeId = mealTypeId;
            _mealTypeName = mealTypeName;

            System.Diagnostics.Debug.WriteLine($"Параметры: UserId={_userId}, MealTypeId={_mealTypeId}, MealTypeName={_mealTypeName}");

            // Не вызываем InitializePage здесь, сделаем это в OnAppearing
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка в конструкторе DiaryEntryPage: {ex.Message}");
            throw;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            System.Diagnostics.Debug.WriteLine("DiaryEntryPage.OnAppearing вызван");

            // Проверяем, что _dbHelper не null
            if (_dbHelper == null)
            {
                System.Diagnostics.Debug.WriteLine("_dbHelper is NULL в OnAppearing! Создаем заново.");
                _dbHelper = new DatabaseHelper();
            }

            await InitializePage();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка в OnAppearing: {ex.Message}");
            await DisplayAlert("Ошибка", $"Не удалось загрузить страницу: {ex.Message}", "OK");
        }
    }
    private async Task InitializePage()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Начало InitializePage");

            // Двойная проверка _dbHelper
            if (_dbHelper == null)
            {
                System.Diagnostics.Debug.WriteLine("Экстренное создание _dbHelper в InitializePage");
                _dbHelper = new DatabaseHelper();
            }

            // Устанавливаем заголовок
            TitleLabel.Text = $"Добавление продуктов для {_mealTypeName}";

            // Явно инициализируем значения БЖУ
            ResetNutritionLabels();

            // Загружаем продукты
            await LoadProducts();

            // Настраиваем обработчики событий
            ProductPicker.SelectedIndexChanged += OnProductSelected;
            QuantityEntry.TextChanged += OnQuantityChanged;

            // Первоначальный расчет
            CalculateNutrition();

            System.Diagnostics.Debug.WriteLine("InitializePage завершен успешно");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка в InitializePage: {ex.Message}");
            throw;
        }
    }

    private async Task LoadProducts()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Начало LoadProducts");

            // Защита от null
            if (_dbHelper == null)
            {
                System.Diagnostics.Debug.WriteLine("_dbHelper is NULL в LoadProducts! Создаем новый.");
                _dbHelper = new DatabaseHelper();
            }

            _products = await _dbHelper.GetProducts();
            System.Diagnostics.Debug.WriteLine($"Загружено продуктов: {_products?.Count ?? 0}");

            if (_products == null || _products.Count == 0)
            {
                await DisplayAlert("Информация", "В базе данных нет продуктов", "OK");
                return;
            }

            // Очищаем Picker и добавляем продукты
            ProductPicker.ItemsSource = null;
            ProductPicker.ItemsSource = _products;

            if (_products.Count > 0)
            {
                ProductPicker.SelectedIndex = 0;
                System.Diagnostics.Debug.WriteLine($"Первый продукт: {_products[0].Name}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка в LoadProducts: {ex.Message}");
            await DisplayAlert("Ошибка", $"Не удалось загрузить продукты: {ex.Message}", "OK");
        }
    }
    private void OnProductSelectedFromList(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Product selectedProduct)
        {
            // Обновляем Picker выбранным продуктом
            ProductPicker.SelectedItem = selectedProduct;
            CalculateNutrition();
        }
    }
    private async void OnAddCustomProductClicked(object sender, EventArgs e)
    {
        try
        {
            // Проверяем авторизацию
            if (_userId == 0)
            {
                await DisplayAlert("Вход required", "Для создания своих продуктов необходимо войти в систему", "OK");
                return;
            }

            // Переходим на страницу создания продукта
            await Navigation.PushAsync(new AddCustomProductPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось открыть страницу создания продукта: {ex.Message}", "OK");
        }
    }
    private void OnProductSelected(object sender, EventArgs e)
    {
        CalculateNutrition();
    }

    private void OnQuantityChanged(object sender, TextChangedEventArgs e)
    {
        CalculateNutrition();
    }

    private void CalculateNutrition()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== CalculateNutrition called ===");

            // Проверяем выбран ли продукт
            if (ProductPicker.SelectedItem == null)
            {
                System.Diagnostics.Debug.WriteLine("Продукт не выбран");
                ResetNutritionLabels();
                return;
            }

            // Проверяем введено ли количество
            if (string.IsNullOrEmpty(QuantityEntry.Text) || !decimal.TryParse(QuantityEntry.Text, out decimal grams) || grams <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"Некорректное количество: {QuantityEntry.Text}");
                ResetNutritionLabels();
                return;
            }

            // Получаем выбранный продукт
            var selectedProduct = ProductPicker.SelectedItem as Product;
            if (selectedProduct == null)
            {
                System.Diagnostics.Debug.WriteLine("Выбранный продукт = null");
                ResetNutritionLabels();
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Выбран продукт: {selectedProduct.Name}, Количество: {grams}г");

            // Рассчитываем питательную ценность
            decimal ratio = grams / 100.0m;

            decimal calories = selectedProduct.CaloriesPer100g * ratio;
            decimal protein = selectedProduct.ProteinPer100g * ratio;
            decimal fat = selectedProduct.FatPer100g * ratio;
            decimal carbs = selectedProduct.CarbsPer100g * ratio;

            System.Diagnostics.Debug.WriteLine($"Рассчитано - Калории: {calories}, Белки: {protein}, Жиры: {fat}, Углеводы: {carbs}");

            // Обновляем UI напрямую
            UpdateNutritionLabels(calories, protein, fat, carbs);

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка в CalculateNutrition: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
    // Новый метод для обновления Label'ов
    private void UpdateNutritionLabels(decimal calories, decimal protein, decimal fat, decimal carbs)
    {
        // Убеждаемся, что обновление происходит в UI потоке
        if (MainThread.IsMainThread)
        {
            CaloriesLabel.Text = $"{calories:F1}";
            ProteinLabel.Text = $"{protein:F1} г";
            FatLabel.Text = $"{fat:F1} г";
            CarbsLabel.Text = $"{carbs:F1} г";
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CaloriesLabel.Text = $"{calories:F1}";
                ProteinLabel.Text = $"{protein:F1} г";
                FatLabel.Text = $"{fat:F1} г";
                CarbsLabel.Text = $"{carbs:F1} г";
            });
        }

        System.Diagnostics.Debug.WriteLine($"UI обновлен: Калории={CaloriesLabel.Text}, Белки={ProteinLabel.Text}, Жиры={FatLabel.Text}, Углеводы={CarbsLabel.Text}");
    }



    private void ResetNutritionLabels()
    {
        CaloriesLabel.Text = "0";
        ProteinLabel.Text = "0 г";
        FatLabel.Text = "0 г";
        CarbsLabel.Text = "0 г";
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== НАЧАЛО OnSaveClicked ===");

            // Проверяем авторизацию пользователя
            if (_userId == 0)
            {
                await DisplayAlert("Ошибка", "Для сохранения записей необходимо войти в систему", "OK");
                return;
            }

            // Проверяем _dbHelper
            if (_dbHelper == null)
            {
                System.Diagnostics.Debug.WriteLine("_dbHelper is NULL в OnSaveClicked! Создаем новый.");
                _dbHelper = new DatabaseHelper();
            }

            // Остальная логика сохранения...
            if (ProductPicker.SelectedItem == null)
            {
                await DisplayAlert("Ошибка", "Выберите продукт", "OK");
                return;
            }

            if (!decimal.TryParse(QuantityEntry.Text, out decimal grams) || grams <= 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное количество (больше 0)", "OK");
                return;
            }

            var selectedProduct = ProductPicker.SelectedItem as Product;
            if (selectedProduct == null)
            {
                await DisplayAlert("Ошибка", "Ошибка при получении данных продукта", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Сохранение: {selectedProduct.Name}, {grams}г");

            // Рассчитываем питательную ценность
            decimal ratio = grams / 100.0m;
            decimal calories = selectedProduct.CaloriesPer100g * ratio;
            decimal protein = selectedProduct.ProteinPer100g * ratio;
            decimal fat = selectedProduct.FatPer100g * ratio;
            decimal carbs = selectedProduct.CarbsPer100g * ratio;

            bool success = await _dbHelper.AddDiaryEntry(
                _userId, _mealTypeId, selectedProduct.ProductId, grams,
                calories, protein, fat, carbs
            );

            if (success)
            {
                System.Diagnostics.Debug.WriteLine("✅ Запись успешно сохранена!");
                await DisplayAlert("Успех", "Запись добавлена в дневник!", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ Не удалось сохранить запись");
                await DisplayAlert("Ошибка", "Не удалось сохранить запись", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Исключение в OnSaveClicked: {ex.Message}");
            await DisplayAlert("Ошибка", $"Не удалось сохранить запись: {ex.Message}", "OK");
        }
        finally
        {
            System.Diagnostics.Debug.WriteLine("=== КОНЕЦ OnSaveClicked ===");
        }
    }
   
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Отписываемся от событий
        if (ProductPicker != null)
        {
            ProductPicker.SelectedIndexChanged -= OnProductSelected;
        }

        if (QuantityEntry != null)
        {
            QuantityEntry.TextChanged -= OnQuantityChanged;
        }

        System.Diagnostics.Debug.WriteLine("DiaryEntryPage закрыт");
    }

    // Метод для обработки параметров от Shell
    //protected override void OnAppearing()
    //{
    //    base.OnAppearing();

    //    // Если параметры не были переданы через конструктор, пытаемся получить их из Query
    //    if (_mealTypeId == 0)
    //    {
    //        try
    //        {
    //            var parameters = Shell.Current?.CurrentState?.Location;
    //            if (parameters != null)
    //            {
    //                // Парсим параметры из URL
    //                var query = System.Web.HttpUtility.ParseQueryString(parameters.ToString());
    //                if (int.TryParse(query["mealTypeId"], out int mealId))
    //                {
    //                    _mealTypeId = mealId;
    //                    _mealTypeName = query["mealTypeName"] ?? "Прием пищи";
    //                    _userId = Preferences.Get("UserId", 0);
    //                    InitializePage();
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            DisplayAlert("Ошибка", $"Не удалось загрузить параметры: {ex.Message}", "OK");
    //        }
    //    }
    //}

}

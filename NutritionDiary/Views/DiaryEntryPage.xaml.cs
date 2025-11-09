using System.Collections.ObjectModel;
using NutritionDiary.Converters;
using NutritionDiary.Models;
using NutritionDiary.Services;

namespace NutritionDiary.Views;

public partial class DiaryEntryPage : ContentPage
{
    private DatabaseHelper _dbHelper;
    private int _userId;
    private int _mealTypeId;
    private string _mealTypeName;
    private List<Product> _allProducts;
    private ObservableCollection<Product> _filteredProducts;
    private Product _selectedProduct;
    public DiaryEntryPage(int mealTypeId, string mealTypeName)
	{
        try
        {
            InitializeComponent();

            // Регистрируем конвертеры ресурсов
            Resources.Add("SelectedColorConverter", new SelectedColorConverter());
            Resources.Add("TextColorConverter", new TextColorConverter());
            Resources.Add("SubtextColorConverter", new SubtextColorConverter());

            _dbHelper = new DatabaseHelper();
            _userId = Preferences.Get("UserId", 0);
            _mealTypeId = mealTypeId;
            _mealTypeName = mealTypeName;

            InitializePage();
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

            if (_dbHelper == null)
            {
                _dbHelper = new DatabaseHelper();
            }

            // Устанавливаем заголовок
            TitleLabel.Text = $"Добавление продуктов для {_mealTypeName}";

            // Явно инициализируем значения БЖУ
            ResetNutritionLabels();

            // Загружаем продукты
            await LoadProducts();

            // Настраиваем обработчики событий
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

            if (_dbHelper == null)
            {
                System.Diagnostics.Debug.WriteLine("_dbHelper is NULL в LoadProducts! Создаем новый.");
                _dbHelper = new DatabaseHelper();
            }

            _allProducts = await _dbHelper.GetProducts();
            _filteredProducts = new ObservableCollection<Product>(_allProducts);

            ProductsListView.ItemsSource = _filteredProducts;

            System.Diagnostics.Debug.WriteLine($"Загружено продуктов: {_allProducts?.Count ?? 0}");

            if (_allProducts == null || _allProducts.Count == 0)
            {
                await DisplayAlert("Информация", "В базе данных нет продуктов", "OK");
                return;
            }

            if (_allProducts.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Первый продукт: {_allProducts[0].Name}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка в LoadProducts: {ex.Message}");
            await DisplayAlert("Ошибка", $"Не удалось загрузить продукты: {ex.Message}", "OK");
        }
    }

    // Обработчик поиска
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            var searchText = e.NewTextValue ?? string.Empty;

            // Сбрасываем выделение при поиске
            if (_selectedProduct != null)
            {
                _selectedProduct.IsSelected = false;
                _selectedProduct = null;
                SelectedProductFrame.IsVisible = false;
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Если поиск пустой - показываем все продукты
                _filteredProducts.Clear();
                foreach (var product in _allProducts)
                {
                    _filteredProducts.Add(product);
                }
            }
            else
            {
                // Фильтруем продукты по введенному тексту
                var filtered = _allProducts.Where(p =>
                    p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    p.DisplayName.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                _filteredProducts.Clear();
                foreach (var product in filtered)
                {
                    _filteredProducts.Add(product);
                }
            }

            System.Diagnostics.Debug.WriteLine($"Поиск: '{searchText}', найдено: {_filteredProducts.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка поиска: {ex.Message}");
        }
    }


    private void OnProductSelectedFromList(object sender, SelectedItemChangedEventArgs e)
    {
        try
        {
            // Снимаем выделение с предыдущего продукта
            if (_selectedProduct != null)
            {
                _selectedProduct.IsSelected = false;
            }

            // Устанавливаем выделение на новый продукт
            _selectedProduct = e.SelectedItem as Product;
            if (_selectedProduct != null)
            {
                _selectedProduct.IsSelected = true;
                System.Diagnostics.Debug.WriteLine($"Выбран продукт: {_selectedProduct.Name}");

                // Обновляем отображение выбранного продукта
                UpdateSelectedProductDisplay();

                CalculateNutrition();
            }
            else
            {
                // Если продукт не выбран, скрываем блок
                SelectedProductFrame.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка выбора продукта: {ex.Message}");
        }
    }
    private void UpdateSelectedProductDisplay()
    {
        if (_selectedProduct != null)
        {
            SelectedProductName.Text = _selectedProduct.Name;
            SelectedProductCalories.Text = $"{_selectedProduct.CaloriesPer100g} ккал/100г";
            SelectedProductFrame.IsVisible = true;
        }
        else
        {
            SelectedProductFrame.IsVisible = false;
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
            if (_selectedProduct == null)
            {
                System.Diagnostics.Debug.WriteLine("Продукт не выбран");
                ResetNutritionLabels();
                return;
            }

            if (string.IsNullOrEmpty(QuantityEntry.Text) || !decimal.TryParse(QuantityEntry.Text, out decimal grams) || grams <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"Некорректное количество: {QuantityEntry.Text}");
                ResetNutritionLabels();
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Выбран продукт: {_selectedProduct.Name}, Количество: {grams}г");

            decimal ratio = grams / 100.0m;
            decimal calories = _selectedProduct.CaloriesPer100g * ratio;
            decimal protein = _selectedProduct.ProteinPer100g * ratio;
            decimal fat = _selectedProduct.FatPer100g * ratio;
            decimal carbs = _selectedProduct.CarbsPer100g * ratio;

            System.Diagnostics.Debug.WriteLine($"Рассчитано - Калории: {calories}, Белки: {protein}, Жиры: {fat}, Углеводы: {carbs}");

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

            if (_userId == 0)
            {
                await DisplayAlert("Ошибка", "Для сохранения записей необходимо войти в систему", "OK");
                return;
            }

            // Проверяем выбран ли продукт
            if (_selectedProduct == null)
            {
                await DisplayAlert("Ошибка", "Выберите продукт из списка", "OK");
                return;
            }

            if (!decimal.TryParse(QuantityEntry.Text, out decimal grams) || grams <= 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное количество (больше 0)", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Сохранение: {_selectedProduct.Name}, {grams}г");

            decimal ratio = grams / 100.0m;
            decimal calories = _selectedProduct.CaloriesPer100g * ratio;
            decimal protein = _selectedProduct.ProteinPer100g * ratio;
            decimal fat = _selectedProduct.FatPer100g * ratio;
            decimal carbs = _selectedProduct.CarbsPer100g * ratio;

            bool success = await _dbHelper.AddDiaryEntry(
                _userId, _mealTypeId, _selectedProduct.ProductId, grams,
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
    }
   
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Отписываемся от событий (теперь только QuantityEntry, т.к. ProductPicker удален)
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

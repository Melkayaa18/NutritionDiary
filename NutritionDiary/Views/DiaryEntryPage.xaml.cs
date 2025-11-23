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
    private BarcodeScannerPage _barcodeScannerPage;
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


















    private async void OnScanBarcodeClicked(object sender, EventArgs e)
    {
        //try
        //{
        //    // Проверяем разрешения камеры
        //    var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        //    if (status != PermissionStatus.Granted)
        //    {
        //        status = await Permissions.RequestAsync<Permissions.Camera>();
        //        if (status != PermissionStatus.Granted)
        //        {
        //            await DisplayAlert("Ошибка", "Для сканирования штрих-кодов необходимо разрешение на использование камеры", "OK");
        //            return;
        //        }
        //    }

        //    // Используем упрощенную версию сканера
        //    var scannerPage = new BarcodeScannerPage();
        //    scannerPage.OnBarcodeScanned += OnBarcodeScanned;

        //    await Navigation.PushAsync(scannerPage);
        //}
        //catch (Exception ex)
        //{
        //    await DisplayAlert("Ошибка", $"Не удалось запустить сканирование: {ex.Message}", "OK");
        //}
    }

    private async void OnBarcodeScanned(string barcode)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Отсканирован штрих-код: {barcode}");

            // Показываем индикатор загрузки
            await DisplayAlert("Сканирование", "Ищем продукт по штрих-коду...", "OK");

            // Ищем продукт в базе данных по штрих-коду
            var product = await FindProductByBarcode(barcode);

            if (product != null)
            {
                // Продукт найден - выбираем его
                await SelectScannedProduct(product);
            }
            else
            {
                // Продукт не найден - ищем в открытой базе данных
                await SearchProductInOpenDatabase(barcode);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка обработки штрих-кода: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Ошибка обработки штрих-кода: {ex.Message}");
        }
    }

    private async Task<Product> FindProductByBarcode(string barcode)
    {
        // Ищем продукт в нашей базе данных по штрих-коду
        // Для этого нужно добавить поле Barcode в таблицу Products

        // Временная реализация - ищем по имени, содержащему штрих-код
        var allProducts = await _dbHelper.GetProducts();
        return allProducts.FirstOrDefault(p =>
            p.Name.Contains(barcode) ||
            (p.Name.ToLower().Contains("штрих") && p.Name.Contains(barcode.Substring(0, Math.Min(5, barcode.Length))))
        );
    }

    private async Task SearchProductInOpenDatabase(string barcode)
    {
        try
        {
            // Используем Open Food Facts API для поиска продукта
            var productInfo = await GetProductFromOpenFoodFacts(barcode);

            if (productInfo != null)
            {
                // Предлагаем пользователю добавить продукт
                bool addProduct = await DisplayAlert("Продукт найден",
                    $"Название: {productInfo.Name}\nКалории: {productInfo.CaloriesPer100g} ккал/100г\n\nДобавить этот продукт?",
                    "Да", "Нет");

                if (addProduct)
                {
                    // Добавляем продукт в базу данных
                    bool success = await _dbHelper.AddCustomProduct(
                        productInfo.Name,
                        productInfo.CaloriesPer100g,
                        productInfo.ProteinPer100g,
                        productInfo.FatPer100g,
                        productInfo.CarbsPer100g,
                        _userId
                    );

                    if (success)
                    {
                        // Обновляем список продуктов и выбираем новый продукт
                        await LoadProducts();
                        var newProduct = _allProducts.FirstOrDefault(p => p.Name == productInfo.Name);
                        if (newProduct != null)
                        {
                            await SelectScannedProduct(newProduct);
                        }
                    }
                }
            }
            else
            {
                await DisplayAlert("Не найдено", "Продукт с таким штрих-кодом не найден в базе данных", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось найти продукт: {ex.Message}", "OK");
        }
    }

    private async Task SelectScannedProduct(Product product)
    {
        try
        {
            // Снимаем выделение с предыдущего продукта
            if (_selectedProduct != null)
            {
                _selectedProduct.IsSelected = false;
            }

            // Устанавливаем выделение на найденный продукт
            _selectedProduct = product;
            _selectedProduct.IsSelected = true;

            // Обновляем отображение
            UpdateSelectedProductDisplay();

            // Устанавливаем стандартное количество (100г)
            QuantityEntry.Text = "100";

            // Пересчитываем питательную ценность
            CalculateNutrition();

            await DisplayAlert("Успех", $"Продукт '{product.Name}' выбран!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось выбрать продукт: {ex.Message}", "OK");
        }
    }

    // Метод для получения информации о продукте из Open Food Facts
    private async Task<Product> GetProductFromOpenFoodFacts(string barcode)
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            var url = $"https://world.openfoodfacts.org/api/v0/product/{barcode}.json";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var productData = System.Text.Json.JsonDocument.Parse(json);

                var status = productData.RootElement.GetProperty("status").GetInt32();
                if (status == 1)
                {
                    var product = productData.RootElement.GetProperty("product");

                    var productName = product.GetProperty("product_name").GetString() ?? "Неизвестный продукт";
                    var brands = product.GetProperty("brands").GetString() ?? "";

                    // Формируем полное название
                    var fullName = string.IsNullOrEmpty(brands) ? productName : $"{brands} - {productName}";

                    // Получаем питательную ценность
                    decimal calories = 0;
                    decimal protein = 0;
                    decimal fat = 0;
                    decimal carbs = 0;

                    if (product.TryGetProperty("nutriments", out var nutriments))
                    {
                        // Калории (может быть в ккал или кДж)
                        if (nutriments.TryGetProperty("energy-kcal_100g", out var kcalElement) &&
                            kcalElement.ValueKind != System.Text.Json.JsonValueKind.Null)
                        {
                            calories = kcalElement.GetDecimal();
                        }
                        else if (nutriments.TryGetProperty("energy_100g", out var energyElement) &&
                                 energyElement.ValueKind != System.Text.Json.JsonValueKind.Null)
                        {
                            // Конвертируем кДж в ккал (1 ккал = 4.184 кДж)
                            var energyKj = energyElement.GetDecimal();
                            calories = energyKj / 4.184m;
                        }

                        // Белки
                        if (nutriments.TryGetProperty("proteins_100g", out var proteinElement) &&
                            proteinElement.ValueKind != System.Text.Json.JsonValueKind.Null)
                        {
                            protein = proteinElement.GetDecimal();
                        }

                        // Жиры
                        if (nutriments.TryGetProperty("fat_100g", out var fatElement) &&
                            fatElement.ValueKind != System.Text.Json.JsonValueKind.Null)
                        {
                            fat = fatElement.GetDecimal();
                        }

                        // Углеводы
                        if (nutriments.TryGetProperty("carbohydrates_100g", out var carbsElement) &&
                            carbsElement.ValueKind != System.Text.Json.JsonValueKind.Null)
                        {
                            carbs = carbsElement.GetDecimal();
                        }
                    }

                    return new Product
                    {
                        Name = fullName,
                        CaloriesPer100g = calories,
                        ProteinPer100g = protein,
                        FatPer100g = fat,
                        CarbsPer100g = carbs,
                        CategoryId = 1,
                        IsCustom = true,
                        CreatedByUserId = _userId
                    };
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка получения данных от Open Food Facts: {ex.Message}");
        }

        return null;
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

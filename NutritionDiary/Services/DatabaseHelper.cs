using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using NutritionDiary.Models;



namespace NutritionDiary.Services
{
    public class DatabaseHelper
    {
        private string connectionString = @"Data Source=ICI5-4670\SQLEXPRESS; Initial Catalog=NutritionDiary; Integrated Security=true; TrustServerCertificate=true;";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        // Проверка логина
        public async Task<int> CheckLogin(string username, string password)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = "SELECT UserId FROM Users WHERE Username = @Username AND Password = @Password";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);

                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка входа: {ex.Message}");
                return 0;
            }
        }

        // Получение дневной статистики
        public async Task<(decimal, decimal, decimal, decimal)> GetDailySummary(int userId, DateTime date)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
                    SELECT 
                        ISNULL(SUM(Calories), 0) as TotalCalories,
                        ISNULL(SUM(Protein), 0) as TotalProtein,
                        ISNULL(SUM(Fat), 0) as TotalFat, 
                        ISNULL(SUM(Carbs), 0) as TotalCarbs
                    FROM DiaryEntries 
                    WHERE UserId = @UserId AND CAST(EntryDate AS DATE) = @Date";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Date", date.Date);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return (reader.GetDecimal(0), reader.GetDecimal(1),
                            reader.GetDecimal(2), reader.GetDecimal(3));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения статистики: {ex.Message}");
            }

            return (0, 0, 0, 0);
        }

        // Добавление записи в дневник
        public async Task<bool> AddDiaryEntry(int userId, int mealTypeId, int productId,
                                    decimal quantity, decimal calories,
                                    decimal protein, decimal fat, decimal carbs)
        {
            System.Diagnostics.Debug.WriteLine("=== НАЧАЛО AddDiaryEntry ===");
            System.Diagnostics.Debug.WriteLine($"Параметры: UserId={userId}, MealTypeId={mealTypeId}, ProductId={productId}");
            System.Diagnostics.Debug.WriteLine($"Quantity={quantity}, Calories={calories}, Protein={protein}, Fat={fat}, Carbs={carbs}");

            try
            {
                using var conn = GetConnection();
                System.Diagnostics.Debug.WriteLine("Создано подключение к БД");

                await conn.OpenAsync();
                System.Diagnostics.Debug.WriteLine("Подключение к БД открыто");

                string query = @"INSERT INTO DiaryEntries 
                           (UserId, MealTypeId, ProductId, Quantity, Calories, Protein, Fat, Carbs, EntryDate)
                           VALUES (@UserId, @MealTypeId, @ProductId, @Quantity, @Calories, @Protein, @Fat, @Carbs, GETDATE())";

                System.Diagnostics.Debug.WriteLine($"SQL запрос: {query}");

                using var cmd = new SqlCommand(query, conn);

                // Добавляем параметры с явным указанием типа
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@MealTypeId", mealTypeId);
                cmd.Parameters.AddWithValue("@ProductId", productId);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                cmd.Parameters.AddWithValue("@Calories", calories);
                cmd.Parameters.AddWithValue("@Protein", protein);
                cmd.Parameters.AddWithValue("@Fat", fat);
                cmd.Parameters.AddWithValue("@Carbs", carbs);

                System.Diagnostics.Debug.WriteLine("Параметры добавлены в команду");

                var result = await cmd.ExecuteNonQueryAsync();
                System.Diagnostics.Debug.WriteLine($"ExecuteNonQueryAsync завершен, результат: {result}");

                bool success = result > 0;
                System.Diagnostics.Debug.WriteLine($"Операция {(success ? "УСПЕШНА" : "НЕ УДАЛАСЬ")}");

                return success;
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Ошибка: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Номер ошибки: {sqlEx.Number}");
                System.Diagnostics.Debug.WriteLine($"Процедура: {sqlEx.Procedure}");
                System.Diagnostics.Debug.WriteLine($"Line Number: {sqlEx.LineNumber}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Общая ошибка добавления записи: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("=== КОНЕЦ AddDiaryEntry ===");
            }
        }

        // Получение списка продуктов - ИСПРАВЛЕННАЯ ВЕРСИЯ
        public async Task<List<Product>> GetProducts()
        {
            var products = new List<Product>();

            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
            SELECT 
                ProductId, 
                Name, 
                CaloriesPer100g, 
                ProteinPer100g, 
                FatPer100g, 
                CarbsPer100g, 
                CategoryId, 
                IsCustom, 
                CreatedByUserId 
            FROM Products 
            ORDER BY Name";

                using var cmd = new SqlCommand(query, conn);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    try
                    {
                        var product = new Product
                        {
                            ProductId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            CaloriesPer100g = reader.GetDecimal(2),
                            ProteinPer100g = reader.GetDecimal(3),
                            FatPer100g = reader.GetDecimal(4),
                            CarbsPer100g = reader.GetDecimal(5),
                            CategoryId = reader.GetInt32(6),
                            IsCustom = reader.GetBoolean(7),
                            // Обрабатываем возможный NULL в CreatedByUserId
                            CreatedByUserId = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                        };
                        products.Add(product);

                        System.Diagnostics.Debug.WriteLine($"Загружен продукт: {product.Name}, CreatedByUserId: {product.CreatedByUserId}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка при чтении продукта: {ex.Message}");
                        // Продолжаем загрузку остальных продуктов
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Всего загружено продуктов: {products.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки продуктов: {ex.Message}");
            }

            return products;
        }
        // Метод для диагностики данных продуктов
        public async Task<string> DiagnoseProducts()
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
            SELECT 
                COUNT(*) as Total,
                COUNT(CASE WHEN CreatedByUserId IS NULL THEN 1 END) as NullUserIds,
                COUNT(CASE WHEN CreatedByUserId IS NOT NULL THEN 1 END) as NotNullUserIds,
                MIN(ProductId) as MinId,
                MAX(ProductId) as MaxId
            FROM Products";

                using var cmd = new SqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return $"Диагностика продуктов:\n" +
                           $"Всего продуктов: {reader.GetInt32(0)}\n" +
                           $"С NULL в CreatedByUserId: {reader.GetInt32(1)}\n" +
                           $"С NOT NULL в CreatedByUserId: {reader.GetInt32(2)}\n" +
                           $"ID от {reader.GetInt32(3)} до {reader.GetInt32(4)}";
                }

                return "Не удалось получить диагностику";
            }
            catch (Exception ex)
            {
                return $"Ошибка диагностики: {ex.Message}";
            }
        }
        // Метод для проверки подключения и данных
        public async Task<string> TestDatabaseConnection()
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                // Проверяем количество продуктов
                string countQuery = "SELECT COUNT(*) FROM Products";
                using var countCmd = new SqlCommand(countQuery, conn);
                var productCount = await countCmd.ExecuteScalarAsync();

                // Проверяем структуру таблицы
                string sampleQuery = "SELECT TOP 3 ProductId, Name FROM Products";
                using var sampleCmd = new SqlCommand(sampleQuery, conn);
                using var reader = await sampleCmd.ExecuteReaderAsync();

                string result = $"Продуктов в базе: {productCount}\n";
                result += "Примеры продуктов:\n";

                while (await reader.ReadAsync())
                {
                    result += $"- {reader.GetString(1)} (ID: {reader.GetInt32(0)})\n";
                }

                return result;
            }
            catch (Exception ex)
            {
                return $"Ошибка подключения: {ex.Message}";
            }
        }
        // Регистрация пользователя
        public async Task<bool> RegisterUser(string username, string password, string email,
                                           int age, decimal height, decimal currentWeight,
                                           decimal desiredWeight)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                // Проверяем, нет ли уже такого пользователя
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                using var checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@Username", username);
                int count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                if (count > 0) return false;

                // Рассчитываем калории
                decimal bmr = 10 * currentWeight + 6.25m * height - 5 * age - 161;
                decimal maintenanceCalories = bmr * 1.55m;
                int dailyCalorieGoal = (int)(maintenanceCalories * 0.85m);

                // Сохраняем пользователя
                string insertQuery = @"INSERT INTO Users 
                    (Username, Password, Email, Age, Height, CurrentWeight, DesiredWeight, DailyCalorieGoal, RegistrationDate) 
                    VALUES (@Username, @Password, @Email, @Age, @Height, @CurrentWeight, @DesiredWeight, @DailyCalorieGoal, GETDATE())";

                using var cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? DBNull.Value : email);
                cmd.Parameters.AddWithValue("@Age", age);
                cmd.Parameters.AddWithValue("@Height", height);
                cmd.Parameters.AddWithValue("@CurrentWeight", currentWeight);
                cmd.Parameters.AddWithValue("@DesiredWeight", desiredWeight);
                cmd.Parameters.AddWithValue("@DailyCalorieGoal", dailyCalorieGoal);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка регистрации: {ex.Message}");
                return false;
            }
        }


        // Добавление пользовательского продукта
        public async Task<bool> AddCustomProduct(string name, decimal calories, decimal protein,
                                       decimal fat, decimal carbs, int userId, string barcode = null)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
        INSERT INTO Products 
            (Name, CaloriesPer100g, ProteinPer100g, FatPer100g, CarbsPer100g, 
             CategoryId, IsCustom, CreatedByUserId, Barcode)
        VALUES 
            (@Name, @Calories, @Protein, @Fat, @Carbs, 
             1, 1, @UserId, @Barcode)";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Calories", calories);
                cmd.Parameters.AddWithValue("@Protein", protein);
                cmd.Parameters.AddWithValue("@Fat", fat);
                cmd.Parameters.AddWithValue("@Carbs", carbs);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Barcode", barcode ?? (object)DBNull.Value);

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка добавления продукта: {ex.Message}");
                return false;
            }
        }

        // Получение случайного рецепта для "Рецепта дня"
        public async Task<Recipe> GetRandomRecipe()
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
            SELECT TOP 1 
                RecipeId, Title, Description, Category, 
                CaloriesPerServing, ProteinPerServing, FatPerServing, CarbsPerServing,
                ImagePath, CookingSteps, IsActive
            FROM Recipes 
            WHERE IsActive = 1
            ORDER BY NEWID()"; // NEWID() - случайная сортировка в SQL Server

                using var cmd = new SqlCommand(query, conn);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Recipe
                    {
                        RecipeId = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Category = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        CaloriesPerServing = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                        ProteinPerServing = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                        FatPerServing = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                        CarbsPerServing = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),
                        ImagePath = reader.IsDBNull(8) ? "" : reader.GetString(8),
                        CookingSteps = reader.IsDBNull(9) ? "" : reader.GetString(9),
                        IsActive = reader.GetBoolean(10)
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения случайного рецепта: {ex.Message}");
            }

            return null;
        }

        // Проверка структуры таблицы DiaryEntries
        public async Task<string> CheckDiaryEntriesStructure()
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
            SELECT 
                COLUMN_NAME,
                DATA_TYPE,
                IS_NULLABLE,
                COLUMN_DEFAULT
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'DiaryEntries'
            ORDER BY ORDINAL_POSITION";

                using var cmd = new SqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                var result = "Структура таблицы DiaryEntries:\n\n";
                while (await reader.ReadAsync())
                {
                    result += $"{reader.GetString(0)} - {reader.GetString(1)} - NULL: {reader.GetString(2)} - DEFAULT: {(reader.IsDBNull(3) ? "нет" : reader.GetString(3))}\n";
                }

                return result;
            }
            catch (Exception ex)
            {
                return $"Ошибка проверки структуры: {ex.Message}";
            }
        }






        
        // Получение челленджей на сегодня для пользователя
        public async Task<List<DailyChallenge>> GetTodayChallenges(int userId)
        {
            var challenges = new List<DailyChallenge>();

            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
            SELECT ChallengeId, Title, Description, Category, IsCompleted, DateAssigned
            FROM DailyChallenges 
            WHERE UserId = @UserId AND CAST(DateAssigned AS DATE) = CAST(GETDATE() AS DATE)
            ORDER BY ChallengeId";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    challenges.Add(new DailyChallenge
                    {
                        ChallengeId = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Description = reader.GetString(2),
                        Category = reader.GetString(3),
                        IsCompleted = reader.GetBoolean(4),
                        DateAssigned = reader.GetDateTime(5)
                    });
                }

                // Если челленджей на сегодня нет - создаем новые
                if (challenges.Count == 0)
                {
                    await reader.CloseAsync();
                    challenges = await GenerateDailyChallenges(userId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения челленджей: {ex.Message}");
            }

            return challenges;
        }
        // Генерация случайных челленджей на день
        private async Task<List<DailyChallenge>> GenerateDailyChallenges(int userId)
        {
            var challenges = new List<DailyChallenge>();
            var random = new Random();

            // База челленджей БЕЗ иконок
            var allChallenges = new[]
            {
        // Питание
        new { Title = "Выпить 2 литра воды", Description = "Следите за водным балансом в течение дня", Category = "Питание" },
        new { Title = "Съесть 5 порций овощей", Description = "Разнообразьте рацион свежими овощами", Category = "Питание" },
        new { Title = "Без сахара", Description = "Проведите день без добавленного сахара", Category = "Питание" },
        new { Title = "Новый полезный продукт", Description = "Попробуйте новый полезный продукт", Category = "Питание" },
        new { Title = "Приготовить здоровый ужин", Description = "Приготовьте ужин самостоятельно из полезных продуктов", Category = "Питание" },
        
        // Спорт
        new { Title = "Утренняя зарядка", Description = "10-15 минут физической активности утром", Category = "Спорт" },
        new { Title = "Прогулка 30 минут", Description = "Совершите пешую прогулку на свежем воздухе", Category = "Спорт" },
        new { Title = "Растяжка", Description = "Выполните комплекс упражнений на растяжку", Category = "Спорт" },
        new { Title = "Лестница вместо лифта", Description = "Используйте лестницу вместо лифта весь день", Category = "Спорт" },
        new { Title = "10-минутная тренировка", Description = "Короткая интенсивная тренировка", Category = "Спорт" },
        
        // Здоровье
        new { Title = "Ранний подъем", Description = "Проснитесь на 30 минут раньше обычного", Category = "Здоровье" },
        new { Title = "Цифровой детокс", Description = "Проведите 2 часа без гаджетов", Category = "Здоровье" },
        new { Title = "Медитация 10 минут", Description = "Практика осознанности и релаксации", Category = "Здоровье" },
        new { Title = "Полноценный сон", Description = "Ложитесь спать до 23:00", Category = "Здоровье" },
        new { Title = "Запись в дневнике", Description = "Запишите 3 вещи, за которые вы благодарны", Category = "Здоровье" }
    };

            // Выбираем 3 случайных челленджа
            var selectedChallenges = allChallenges.OrderBy(x => random.Next()).Take(3).ToList();

            // Сохраняем в базу БЕЗ иконок
            using var conn = GetConnection();
            await conn.OpenAsync();

            foreach (var challenge in selectedChallenges)
            {
                string query = @"
            INSERT INTO DailyChallenges 
                (UserId, Title, Description, Category, IsCompleted, DateAssigned)
            VALUES 
                (@UserId, @Title, @Description, @Category, 0, GETDATE())";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Title", challenge.Title);
                cmd.Parameters.AddWithValue("@Description", challenge.Description);
                cmd.Parameters.AddWithValue("@Category", challenge.Category);

                await cmd.ExecuteNonQueryAsync();

                // Добавляем в возвращаемый список
                challenges.Add(new DailyChallenge
                {
                    Title = challenge.Title,
                    Description = challenge.Description,
                    Category = challenge.Category,
                    IsCompleted = false,
                    DateAssigned = DateTime.Today
                });
            }

            return challenges;
        }
        // Отметка челленджа как выполненного
        public async Task<bool> CompleteChallenge(int challengeId)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = "UPDATE DailyChallenges SET IsCompleted = 1 WHERE ChallengeId = @ChallengeId";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ChallengeId", challengeId);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отметки челленджа: {ex.Message}");
                return false;
            }
        }

        // Снятие отметки о выполнении челленджа
        public async Task<bool> UncompleteChallenge(int challengeId)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = "UPDATE DailyChallenges SET IsCompleted = 0 WHERE ChallengeId = @ChallengeId";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ChallengeId", challengeId);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка снятия отметки челленджа: {ex.Message}");
                return false;
            }
        }






        public async Task<bool> AddRecipe(Recipe recipe)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
        INSERT INTO Recipes 
            (Title, Description, Category, CaloriesPerServing, ProteinPerServing, 
             FatPerServing, CarbsPerServing, ImagePath, CookingSteps, IsActive, CreatedByUserId, CookingTime)
        VALUES 
            (@Title, @Description, @Category, @Calories, @Protein, 
             @Fat, @Carbs, @ImagePath, @CookingSteps, 1, @UserId, @CookingTime)";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Title", recipe.Title);
                cmd.Parameters.AddWithValue("@Description", recipe.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Category", recipe.Category);
                cmd.Parameters.AddWithValue("@Calories", recipe.CaloriesPerServing);
                cmd.Parameters.AddWithValue("@Protein", recipe.ProteinPerServing);
                cmd.Parameters.AddWithValue("@Fat", recipe.FatPerServing);
                cmd.Parameters.AddWithValue("@Carbs", recipe.CarbsPerServing);
                cmd.Parameters.AddWithValue("@ImagePath", recipe.ImagePath ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CookingSteps", recipe.CookingSteps ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@UserId", recipe.CreatedByUserId);
                cmd.Parameters.AddWithValue("@CookingTime", recipe.CookingTime); 

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка добавления рецепта: {ex.Message}");
                return false;
            }
        }
        

        // Получение рецептов пользователя
        public async Task<List<Recipe>> GetRecipesByUserId(int userId)
        {
            var recipes = new List<Recipe>();

            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
        SELECT RecipeId, Title, Description, Category, CaloriesPerServing, 
               ProteinPerServing, FatPerServing, CarbsPerServing, ImagePath,
               CookingSteps, IsActive, CreatedByUserId, CookingTime
        FROM Recipes 
        WHERE CreatedByUserId = @UserId AND IsActive = 1
        ORDER BY Title";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    recipes.Add(new Recipe
                    {
                        RecipeId = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Category = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        CaloriesPerServing = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                        ProteinPerServing = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                        FatPerServing = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                        CarbsPerServing = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),
                        ImagePath = reader.IsDBNull(8) ? "" : reader.GetString(8),
                        CookingSteps = reader.IsDBNull(9) ? "" : reader.GetString(9),
                        IsActive = reader.GetBoolean(10),
                        CreatedByUserId = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                        CookingTime = reader.IsDBNull(12) ? 0 : reader.GetInt32(12) // Добавьте эту строку
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения рецептов пользователя: {ex.Message}");
            }

            return recipes;
        }




        public async Task<string> GetRecipeImagePath(int recipeId)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = "SELECT ImagePath FROM Recipes WHERE RecipeId = @RecipeId";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@RecipeId", recipeId);

                var result = await cmd.ExecuteScalarAsync();
                var imagePath = result?.ToString() ?? "NULL";

                System.Diagnostics.Debug.WriteLine($"=== ПРОВЕРКА БАЗЫ ДАННЫХ ===");
                System.Diagnostics.Debug.WriteLine($"RecipeId: {recipeId}");
                System.Diagnostics.Debug.WriteLine($"ImagePath в БД: '{imagePath}'");

                return imagePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка проверки ImagePath: {ex.Message}");
                return $"Ошибка: {ex.Message}";
            }
        }


        public class ChallengesStatistics
        {
            public int TotalCompleted { get; set; }
            public int TotalAssigned { get; set; }
            public int CurrentStreak { get; set; }
            public int BestStreak { get; set; }
        }

        public async Task<ChallengesStatistics> GetChallengesStatistics(int userId)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                var stats = new ChallengesStatistics();

                // Общее количество выполненных
                string completedQuery = "SELECT COUNT(*) FROM DailyChallenges WHERE UserId = @UserId AND IsCompleted = 1";
                using var completedCmd = new SqlCommand(completedQuery, conn);
                completedCmd.Parameters.AddWithValue("@UserId", userId);
                stats.TotalCompleted = Convert.ToInt32(await completedCmd.ExecuteScalarAsync());

                // Общее количество назначенных
                string assignedQuery = "SELECT COUNT(*) FROM DailyChallenges WHERE UserId = @UserId";
                using var assignedCmd = new SqlCommand(assignedQuery, conn);
                assignedCmd.Parameters.AddWithValue("@UserId", userId);
                stats.TotalAssigned = Convert.ToInt32(await assignedCmd.ExecuteScalarAsync());

                // Здесь можно добавить логику для вычисления серий
                // Пока заглушки
                stats.CurrentStreak = 3;
                stats.BestStreak = 7;

                return stats;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения статистики: {ex.Message}");
                return new ChallengesStatistics();
            }
        }
        public async Task<bool> DeleteRecipe(int recipeId)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                // Сначала получим информацию о рецепте для удаления фото
                string getQuery = "SELECT ImagePath FROM Recipes WHERE RecipeId = @RecipeId";
                string imagePath = null;

                using (var getCmd = new SqlCommand(getQuery, conn))
                {
                    getCmd.Parameters.AddWithValue("@RecipeId", recipeId);
                    var result = await getCmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        imagePath = result.ToString();
                    }
                }

                // Удаляем рецепт из базы данных
                string deleteQuery = "DELETE FROM Recipes WHERE RecipeId = @RecipeId";

                using var deleteCmd = new SqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@RecipeId", recipeId);

                var rowsAffected = await deleteCmd.ExecuteNonQueryAsync();
                bool success = rowsAffected > 0;

                // Если рецепт удален и есть фото - удаляем файл фото
                if (success && !string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    try
                    {
                        File.Delete(imagePath);
                        System.Diagnostics.Debug.WriteLine($"Фото удалено: {imagePath}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка удаления фото: {ex.Message}");
                        // Продолжаем, даже если не удалось удалить фото
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Удаление рецепта: RecipeId={recipeId}, success={success}");
                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления рецепта: {ex.Message}");
                return false;
            }
        }




        public async Task<bool> UpdateRecipe(Recipe recipe)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
        UPDATE Recipes 
        SET 
            Title = @Title,
            Description = @Description,
            Category = @Category,
            CaloriesPerServing = @Calories,
            ProteinPerServing = @Protein,
            FatPerServing = @Fat,
            CarbsPerServing = @Carbs,
            ImagePath = @ImagePath,
            CookingSteps = @CookingSteps,
            CookingTime = @CookingTime
        WHERE 
            RecipeId = @RecipeId";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Title", recipe.Title);
                cmd.Parameters.AddWithValue("@Description", recipe.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Category", recipe.Category);
                cmd.Parameters.AddWithValue("@Calories", recipe.CaloriesPerServing);
                cmd.Parameters.AddWithValue("@Protein", recipe.ProteinPerServing);
                cmd.Parameters.AddWithValue("@Fat", recipe.FatPerServing);
                cmd.Parameters.AddWithValue("@Carbs", recipe.CarbsPerServing);
                cmd.Parameters.AddWithValue("@ImagePath", recipe.ImagePath ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CookingSteps", recipe.CookingSteps ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CookingTime", recipe.CookingTime);
                cmd.Parameters.AddWithValue("@RecipeId", recipe.RecipeId);

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления рецепта: {ex.Message}");
                return false;
            }
        }




        // Добавить запись о выпитой воде
        public async Task<bool> AddWaterIntake(int userId, decimal amount)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
        INSERT INTO WaterIntake (UserId, IntakeDate, Amount)
        VALUES (@UserId, CAST(GETDATE() AS DATE), @Amount)";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Amount", amount);

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка добавления воды: {ex.Message}");
                return false;
            }
        }

        // Получить общее количество воды за сегодня
        public async Task<decimal> GetTodayWaterIntake(int userId)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
        SELECT ISNULL(SUM(Amount), 0) 
        FROM WaterIntake 
        WHERE UserId = @UserId AND IntakeDate = CAST(GETDATE() AS DATE)";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToDecimal(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения воды: {ex.Message}");
                return 0;
            }
        }

        // Удалить последнюю запись о воде (для кнопки отмены)
        public async Task<bool> RemoveLastWaterIntake(int userId)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
        DELETE FROM WaterIntake 
        WHERE WaterId = (
            SELECT TOP 1 WaterId 
            FROM WaterIntake 
            WHERE UserId = @UserId AND IntakeDate = CAST(GETDATE() AS DATE)
            ORDER BY CreatedAt DESC
        )";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления воды: {ex.Message}");
                return false;
            }
        }






        public async Task<string> CheckRecipeStructure(int recipeId)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
        SELECT 
            RecipeId, Title, CookingTime, ImagePath, CreatedByUserId
        FROM Recipes 
        WHERE RecipeId = @RecipeId";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@RecipeId", recipeId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return $"Рецепт #{reader.GetInt32(0)}: {reader.GetString(1)}\n" +
                           $"CookingTime: {(reader.IsDBNull(2) ? "NULL" : reader.GetInt32(2).ToString())}\n" +
                           $"ImagePath: {(reader.IsDBNull(3) ? "NULL" : reader.GetString(3))}\n" +
                           $"CreatedByUserId: {reader.GetInt32(4)}";
                }

                return "Рецепт не найден";
            }
            catch (Exception ex)
            {
                return $"Ошибка проверки: {ex.Message}";
            }
        }













        public async Task<Product> GetProductByBarcode(string barcode)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
        SELECT ProductId, Name, CaloriesPer100g, ProteinPer100g, FatPer100g, CarbsPer100g, 
               CategoryId, IsCustom, CreatedByUserId, Barcode
        FROM Products 
        WHERE Barcode = @Barcode";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Barcode", barcode);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Product
                    {
                        ProductId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        CaloriesPer100g = reader.GetDecimal(2),
                        ProteinPer100g = reader.GetDecimal(3),
                        FatPer100g = reader.GetDecimal(4),
                        CarbsPer100g = reader.GetDecimal(5),
                        CategoryId = reader.GetInt32(6),
                        IsCustom = reader.GetBoolean(7),
                        CreatedByUserId = reader.GetInt32(8),
                        Barcode = reader.IsDBNull(9) ? "" : reader.GetString(9)
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка поиска по штрих-коду: {ex.Message}");
            }

            return null;
        }
    }
}

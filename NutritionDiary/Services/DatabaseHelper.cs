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
                                               decimal fat, decimal carbs, int userId)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"
            INSERT INTO Products 
                (Name, CaloriesPer100g, ProteinPer100g, FatPer100g, CarbsPer100g, 
                 CategoryId, IsCustom, CreatedByUserId)
            VALUES 
                (@Name, @Calories, @Protein, @Fat, @Carbs, 
                 1, 1, @UserId)";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Calories", calories);
                cmd.Parameters.AddWithValue("@Protein", protein);
                cmd.Parameters.AddWithValue("@Fat", fat);
                cmd.Parameters.AddWithValue("@Carbs", carbs);
                cmd.Parameters.AddWithValue("@UserId", userId);

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка добавления продукта: {ex.Message}");
                return false;
            }
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
    }
}

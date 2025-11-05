using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutritionDiary.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty; // Инициализация по умолчанию
        public decimal CaloriesPer100g { get; set; }
        public decimal ProteinPer100g { get; set; }
        public decimal FatPer100g { get; set; }
        public decimal CarbsPer100g { get; set; }
        public int CategoryId { get; set; }
        public bool IsCustom { get; set; }
        public int CreatedByUserId { get; set; }

        // Вычисляемое свойство для отображения
        public string DisplayName
        {
            get
            {
                var name = string.IsNullOrEmpty(Name) ? "Без названия" : Name;
                return $"{name} ({CaloriesPer100g} ккал/100г)";
            }
        }
    }
}

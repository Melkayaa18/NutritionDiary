using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutritionDiary.Models
{
    public class DiaryEntry
    {
        public int EntryId { get; set; }
        public int UserId { get; set; }
        public int MealTypeId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime EntryDate { get; set; }
        public decimal Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Fat { get; set; }
        public decimal Carbs { get; set; }
        public string ProductName { get; set; }
        public string MealTypeName { get; set; }
    }
}

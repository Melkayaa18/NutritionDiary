using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutritionDiary.Models
{
    public class RecipeFilter
    {
        public List<string> Categories { get; set; } = new List<string>();
        public int MinCalories { get; set; } = 0;
        public int MaxCalories { get; set; } = 1000;
        public string SearchText { get; set; } = string.Empty;
        public List<string> DietaryPreferences { get; set; } = new List<string>();
        public int MaxCookingTime { get; set; } = 120; // в минутах
    }
}

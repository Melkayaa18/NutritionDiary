using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NutritionDiary.Models
{
    public class RecipeData
    {
        public string Name { get; set; }
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Fat { get; set; }
        public int Carbs { get; set; }
        public string Description { get; set; }
        public string CookingTime { get; set; }

        // Вычисляемое свойство для отображения
        public string NutritionInfo => $"⚡ {Calories} ккал • 🥚 {Protein}г • 🥑 {Fat}г • 🌾 {Carbs}г";


    }
}

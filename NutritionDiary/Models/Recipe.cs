using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutritionDiary.Models
{
    public class Recipe
    {
        public int RecipeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal CaloriesPerServing { get; set; }
        public decimal ProteinPerServing { get; set; }
        public decimal FatPerServing { get; set; }
        public decimal CarbsPerServing { get; set; }
        public string ImagePath { get; set; }
        public string CookingSteps { get; set; }
        public bool IsActive { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutritionDiary.Models
{
    public class DailyChallenge
    {
        public int ChallengeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } // "Питание", "Спорт", "Здоровье"
        public string Icon { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime DateAssigned { get; set; }
    }
}

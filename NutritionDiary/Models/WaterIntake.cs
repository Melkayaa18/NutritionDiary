using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutritionDiary.Models
{
    public class WaterIntake
    {
        public int WaterId { get; set; }
        public int UserId { get; set; }
        public DateTime IntakeDate { get; set; }
        public decimal Amount { get; set; } // в миллилитрах
        public DateTime CreatedAt { get; set; }
    }
}

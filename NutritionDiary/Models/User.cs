using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NutritionDiary.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public decimal Height { get; set; }
        public decimal CurrentWeight { get; set; }
        public decimal DesiredWeight { get; set; }
        public int DailyCalorieGoal { get; set; }
        public DateTime RegistrationDate { get; set; }

    }

}


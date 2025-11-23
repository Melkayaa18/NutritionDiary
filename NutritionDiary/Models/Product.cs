using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NutritionDiary.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal CaloriesPer100g { get; set; }
        public decimal ProteinPer100g { get; set; }
        public decimal FatPer100g { get; set; }
        public decimal CarbsPer100g { get; set; }
        public int CategoryId { get; set; }
        public bool IsCustom { get; set; }
        public int CreatedByUserId { get; set; }



        public string Barcode { get; set; } = string.Empty;

        // Вычисляемое свойство для отображения
        public string DisplayName => $"{Name} ({CaloriesPer100g} ккал/100г)";

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

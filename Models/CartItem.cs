using StoreManagement.ViewModels;

namespace StoreManagement.Models
{
    public class CartItem : ViewModelBase
    {
        public Product Product { get; set; }
        
        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set 
            { 
                _quantity = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public decimal TotalPrice 
        { 
            get { return Product.Price * Quantity; } 
        }
    }
}

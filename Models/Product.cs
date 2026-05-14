namespace StoreManagement.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImagePath { get; set; }

        public string AbsoluteImagePath 
        { 
            get 
            {
                if (string.IsNullOrEmpty(ImagePath)) return null;
                return System.IO.Path.GetFullPath(ImagePath);
            } 
        }

        public bool IsLowStock 
        { 
            get { return StockQuantity <= 5; } // threshold is 5
        }
    }
}

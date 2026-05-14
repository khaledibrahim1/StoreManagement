using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using StoreManagement.Models;
using StoreManagement.Database;

namespace StoreManagement.ViewModels
{
    public class InventoryViewModel : ViewModelBase
    {
        public ObservableCollection<Product> Products { get; set; }

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
                if (value != null)
                {
                    Name = value.Name;
                    SKU = value.SKU;
                    Price = value.Price;
                    StockQuantity = value.StockQuantity;
                    ImagePath = value.ImagePath;
                }
            }
        }

        private string _imagePath;
        public string ImagePath
        {
            get { return _imagePath; }
            set 
            { 
                _imagePath = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(AbsoluteImagePath));
            }
        }

        public string AbsoluteImagePath 
        { 
            get 
            {
                if (string.IsNullOrEmpty(ImagePath)) return null;
                return System.IO.Path.GetFullPath(ImagePath);
            } 
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        private string _sku;
        public string SKU
        {
            get { return _sku; }
            set { _sku = value; OnPropertyChanged(); }
        }

        private decimal _price;
        public decimal Price
        {
            get { return _price; }
            set { _price = value; OnPropertyChanged(); }
        }

        private int _stockQuantity;
        public int StockQuantity
        {
            get { return _stockQuantity; }
            set { _stockQuantity = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand UploadImageCommand { get; }

        public InventoryViewModel()
        {
            Products = new ObservableCollection<Product>();
            LoadProducts();

            AddCommand = new RelayCommand(param => AddProduct(), param => CanAdd());
            UpdateCommand = new RelayCommand(param => UpdateProduct(), param => SelectedProduct != null);
            DeleteCommand = new RelayCommand(param => DeleteProduct(), param => SelectedProduct != null);
            ClearCommand = new RelayCommand(param => ClearForm());
            UploadImageCommand = new RelayCommand(param => UploadImage());
        }

        private void LoadProducts()
        {
            Products.Clear();
            var productsFromDb = DatabaseHelper.GetProducts();
            foreach (var p in productsFromDb)
            {
                Products.Add(p);
            }
        }

        private bool CanAdd()
        {
            return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(SKU) && Price >= 0 && StockQuantity >= 0;
        }

        private void AddProduct()
        {
            Product p = new Product
            {
                Name = Name,
                SKU = SKU,
                Price = Price,
                StockQuantity = StockQuantity,
                ImagePath = ImagePath
            };
            DatabaseHelper.AddProduct(p);
            LoadProducts();
            ClearForm();
        }

        private void UpdateProduct()
        {
            if (SelectedProduct != null)
            {
                SelectedProduct.Name = Name;
                SelectedProduct.SKU = SKU;
                SelectedProduct.Price = Price;
                SelectedProduct.StockQuantity = StockQuantity;
                SelectedProduct.ImagePath = ImagePath;
                
                DatabaseHelper.UpdateProduct(SelectedProduct);
                LoadProducts();
                ClearForm();
            }
        }

        private void DeleteProduct()
        {
            if (SelectedProduct != null)
            {
                DatabaseHelper.DeleteProduct(SelectedProduct.Id);
                LoadProducts();
                ClearForm();
            }
        }

        private void ClearForm()
        {
            SelectedProduct = null;
            Name = string.Empty;
            SKU = string.Empty;
            Price = 0;
            StockQuantity = 0;
            ImagePath = string.Empty;
        }

        private void UploadImage()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            if (dlg.ShowDialog() == true)
            {
                if (!Directory.Exists("uploads"))
                {
                    Directory.CreateDirectory("uploads");
                }
                
                string ext = Path.GetExtension(dlg.FileName);
                string newFileName = System.Guid.NewGuid().ToString() + ext;
                string destPath = Path.Combine("uploads", newFileName);
                
                File.Copy(dlg.FileName, destPath, true);
                ImagePath = destPath;
            }
        }
    }
}

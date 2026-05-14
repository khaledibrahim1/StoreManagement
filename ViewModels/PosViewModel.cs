using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using StoreManagement.Models;
using StoreManagement.Database;
using System.Windows;

namespace StoreManagement.ViewModels
{
    public class PosViewModel : ViewModelBase
    {
        public ObservableCollection<Product> AvailableProducts { get; set; }
        public ObservableCollection<CartItem> Cart { get; set; }

        private string _searchQuery;
        public string SearchQuery
        {
            get { return _searchQuery; }
            set 
            { 
                _searchQuery = value; 
                OnPropertyChanged();
                FilterProducts();
            }
        }

        private decimal _subtotal;
        public decimal Subtotal
        {
            get { return _subtotal; }
            set { _subtotal = value; OnPropertyChanged(); OnPropertyChanged(nameof(Total)); }
        }

        private decimal _taxRate = 0.15m; // 15% VAT

        public decimal Tax
        {
            get { return Subtotal * _taxRate; }
        }

        public decimal Total
        {
            get { return Subtotal + Tax; }
        }

        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand CheckoutCommand { get; }

        public PosViewModel()
        {
            AvailableProducts = new ObservableCollection<Product>();
            Cart = new ObservableCollection<CartItem>();
            LoadProducts();

            AddToCartCommand = new RelayCommand(AddToCart);
            RemoveFromCartCommand = new RelayCommand(RemoveFromCart);
            CheckoutCommand = new RelayCommand(param => Checkout(), param => Cart.Count > 0);
        }

        public void LoadProducts()
        {
            AvailableProducts.Clear();
            var products = DatabaseHelper.GetProducts();
            foreach (var p in products)
            {
                AvailableProducts.Add(p);
            }
        }

        private void FilterProducts()
        {
            var products = DatabaseHelper.GetProducts();
            AvailableProducts.Clear();
            
            foreach (var p in products)
            {
                if (string.IsNullOrWhiteSpace(SearchQuery) || 
                    p.Name.ToLower().Contains(SearchQuery.ToLower()) || 
                    p.SKU.ToLower().Contains(SearchQuery.ToLower()))
                {
                    AvailableProducts.Add(p);
                }
            }
        }

        private void AddToCart(object param)
        {
            if (param is Product product)
            {
                if (product.StockQuantity <= 0)
                {
                    MessageBox.Show("Out of stock!");
                    return;
                }

                var existingItem = Cart.FirstOrDefault(c => c.Product.Id == product.Id);
                if (existingItem != null)
                {
                    if (existingItem.Quantity < product.StockQuantity)
                    {
                        existingItem.Quantity++;
                    }
                    else
                    {
                        MessageBox.Show("Cannot add more than available stock.");
                    }
                }
                else
                {
                    Cart.Add(new CartItem { Product = product, Quantity = 1 });
                }
                CalculateTotals();
            }
        }

        private void RemoveFromCart(object param)
        {
            if (param is CartItem item)
            {
                Cart.Remove(item);
                CalculateTotals();
            }
        }

        private void CalculateTotals()
        {
            decimal sub = 0;
            foreach (var item in Cart)
            {
                sub += item.TotalPrice;
            }
            Subtotal = sub;
            OnPropertyChanged(nameof(Tax));
            OnPropertyChanged(nameof(Total));
        }

        private void Checkout()
        {
            if (Cart.Count == 0) return;

            Transaction t = new Transaction
            {
                Date = DateTime.Now,
                Subtotal = this.Subtotal,
                Tax = this.Tax,
                Total = this.Total
            };

            var checkoutVM = new CheckoutViewModel(t);
            var popup = new Views.CheckoutPopup { DataContext = checkoutVM };
            
            popup.ShowDialog();

            if (checkoutVM.IsConfirmed)
            {
                try
                {
                    DatabaseHelper.ProcessCheckout(t, Cart.ToList());
                    MessageBox.Show("Checkout successful!");
                    Cart.Clear();
                    CalculateTotals();
                    LoadProducts(); // Refresh stock
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}

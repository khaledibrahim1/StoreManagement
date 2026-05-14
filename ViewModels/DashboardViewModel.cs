using StoreManagement.Database;
using System;
using System.Collections.ObjectModel;

namespace StoreManagement.ViewModels
{
    public class DailySaleRecord
    {
        public string Date { get; set; }
        public decimal Amount { get; set; }
        public double Height { get; set; }
    }
    public class DashboardViewModel : ViewModelBase
    {
        private int _totalProducts;
        public int TotalProducts
        {
            get { return _totalProducts; }
            set { _totalProducts = value; OnPropertyChanged(); }
        }

        private int _totalTransactions;
        public int TotalTransactions
        {
            get { return _totalTransactions; }
            set { _totalTransactions = value; OnPropertyChanged(); }
        }

        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get { return _totalRevenue; }
            set { _totalRevenue = value; OnPropertyChanged(); }
        }

        public ObservableCollection<DailySaleRecord> DailySales { get; set; }

        public DashboardViewModel()
        {
            DailySales = new ObservableCollection<DailySaleRecord>();
            LoadData();
        }

        public void LoadData()
        {
            var products = DatabaseHelper.GetProducts();
            TotalProducts = products.Count;

            var transactions = DatabaseHelper.GetTransactions();
            TotalTransactions = transactions.Count;

            decimal revenue = 0;
            DailySales.Clear();
            var groupedSales = new System.Collections.Generic.Dictionary<string, decimal>();

            foreach (var t in transactions)
            {
                revenue += t.Total;
                string dateStr = t.Date.ToString("MM/dd");
                if (groupedSales.ContainsKey(dateStr))
                    groupedSales[dateStr] += t.Total;
                else
                    groupedSales[dateStr] = t.Total;
            }
            TotalRevenue = revenue;

            decimal maxAmount = 1;
            foreach (var kvp in groupedSales)
            {
                if (kvp.Value > maxAmount) maxAmount = kvp.Value;
            }

            if (groupedSales.Count == 0)
            {
                groupedSales.Add(DateTime.Now.AddDays(-2).ToString("MM/dd"), 150m);
                groupedSales.Add(DateTime.Now.AddDays(-1).ToString("MM/dd"), 320m);
                groupedSales.Add(DateTime.Now.ToString("MM/dd"), 45m);
                maxAmount = 320m;
            }

            foreach (var kvp in groupedSales)
            {
                DailySales.Add(new DailySaleRecord 
                { 
                    Date = kvp.Key, 
                    Amount = kvp.Value,
                    Height = (double)(kvp.Value / maxAmount) * 150
                });
            }
        }
    }
}

using System.Collections.ObjectModel;
using StoreManagement.Models;
using StoreManagement.Database;

namespace StoreManagement.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {
        public ObservableCollection<Transaction> Transactions { get; set; }
        public ObservableCollection<TransactionItem> SelectedTransactionItems { get; set; }

        private Transaction _selectedTransaction;
        public Transaction SelectedTransaction
        {
            get { return _selectedTransaction; }
            set 
            { 
                _selectedTransaction = value; 
                OnPropertyChanged();
                LoadTransactionItems();
            }
        }

        public HistoryViewModel()
        {
            Transactions = new ObservableCollection<Transaction>();
            SelectedTransactionItems = new ObservableCollection<TransactionItem>();
            LoadTransactions();
        }

        public void LoadTransactions()
        {
            Transactions.Clear();
            var trans = DatabaseHelper.GetTransactions();
            foreach (var t in trans)
            {
                Transactions.Add(t);
            }
        }

        private void LoadTransactionItems()
        {
            SelectedTransactionItems.Clear();
            if (SelectedTransaction != null)
            {
                var items = DatabaseHelper.GetTransactionItems(SelectedTransaction.Id);
                foreach (var item in items)
                {
                    SelectedTransactionItems.Add(item);
                }
            }
        }
    }
}

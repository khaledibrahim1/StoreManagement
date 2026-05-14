using System;
using System.Windows.Input;
using System.Windows;
using StoreManagement.Models;

namespace StoreManagement.ViewModels
{
    public class CheckoutViewModel : ViewModelBase
    {
        public Transaction TransactionData { get; }

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public bool IsConfirmed { get; private set; }

        public CheckoutViewModel(Transaction transaction)
        {
            TransactionData = transaction;

            ConfirmCommand = new RelayCommand(ExecuteConfirm);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        private void ExecuteConfirm(object parameter)
        {
            IsConfirmed = true;
            if (parameter is Window window)
            {
                window.Close();
            }
        }

        private void ExecuteCancel(object parameter)
        {
            IsConfirmed = false;
            if (parameter is Window window)
            {
                window.Close();
            }
        }
    }
}

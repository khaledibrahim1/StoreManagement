using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StoreManagement.Database;

namespace StoreManagement.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private string _username;
        public string Username
        {
            get { return _username; }
            set { _username = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand RegisterCommand { get; }
        public ICommand BackToLoginCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(ExecuteRegister);
            BackToLoginCommand = new RelayCommand(ExecuteBackToLogin);
        }

        private void ExecuteRegister(object parameter)
        {
            if (parameter is Window window)
            {
                var txtPassword = window.FindName("txtPassword") as PasswordBox;
                var txtConfirmPassword = window.FindName("txtConfirmPassword") as PasswordBox;

                if (txtPassword == null || txtConfirmPassword == null)
                {
                    ErrorMessage = "System error: Password fields not found.";
                    return;
                }

                string username = Username;
                string password = txtPassword.Password;
                string confirmPassword = txtConfirmPassword.Password;

                if (string.IsNullOrWhiteSpace(username))
                {
                    ErrorMessage = "Username is required.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    ErrorMessage = "Password is required.";
                    return;
                }

                if (password != confirmPassword)
                {
                    ErrorMessage = "Passwords do not match.";
                    return;
                }

                // Register user
                string dbErrorMessage;
                bool success = DatabaseHelper.RegisterUser(username, password, out dbErrorMessage);
                if (success)
                {
                    MessageBox.Show("Registration successful! You can now log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Navigate back to Login
                    var loginWindow = new Views.LoginWindow();
                    loginWindow.Show();
                    window.Close();
                }
                else
                {
                    ErrorMessage = dbErrorMessage;
                }
            }
        }

        private void ExecuteBackToLogin(object parameter)
        {
            if (parameter is Window window)
            {
                var loginWindow = new Views.LoginWindow();
                loginWindow.Show();
                window.Close();
            }
        }
    }
}

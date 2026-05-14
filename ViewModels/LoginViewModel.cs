using System.Windows;
using System.Windows.Input;

namespace StoreManagement.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _username;
        public string Username
        {
            get { return _username; }
            set { _username = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private void ExecuteLogin(object parameter)
        {
            if (parameter is System.Windows.Controls.PasswordBox passwordBox)
            {
                string password = passwordBox.Password;
                if (Username == "admin" && password == "admin")
                {
                    var window = Window.GetWindow(passwordBox);
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    window?.Close();
                }
                else
                {
                    ErrorMessage = "Invalid username or password.";
                }
            }
        }
    }
}

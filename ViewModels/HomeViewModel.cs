using System.Windows;
using System.Windows.Input;

namespace StoreManagement.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        public ICommand GoToLoginCommand { get; }

        public HomeViewModel()
        {
            GoToLoginCommand = new RelayCommand(ExecuteGoToLogin);
        }

        private void ExecuteGoToLogin(object parameter)
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

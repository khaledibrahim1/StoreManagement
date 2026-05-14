using System.Windows.Input;
using StoreManagement.Database;

namespace StoreManagement.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set { _currentViewModel = value; OnPropertyChanged(); }
        }

        private DashboardViewModel _dashboardViewModel;
        private InventoryViewModel _inventoryViewModel;
        private PosViewModel _posViewModel;
        private HistoryViewModel _historyViewModel;

        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowInventoryCommand { get; }
        public ICommand ShowPosCommand { get; }
        public ICommand ShowHistoryCommand { get; }

        public MainViewModel()
        {
            DatabaseHelper.InitializeDatabase();

            _dashboardViewModel = new DashboardViewModel();
            _inventoryViewModel = new InventoryViewModel();
            _posViewModel = new PosViewModel();
            _historyViewModel = new HistoryViewModel();

            // Default view
            CurrentViewModel = _dashboardViewModel;

            ShowDashboardCommand = new RelayCommand(param => 
            { 
                _dashboardViewModel.LoadData();
                CurrentViewModel = _dashboardViewModel; 
            });

            ShowInventoryCommand = new RelayCommand(param => 
            { 
                _inventoryViewModel = new InventoryViewModel(); // Refresh
                CurrentViewModel = _inventoryViewModel; 
            });
            
            ShowPosCommand = new RelayCommand(param => 
            { 
                _posViewModel.LoadProducts(); // Refresh
                CurrentViewModel = _posViewModel; 
            });
            
            ShowHistoryCommand = new RelayCommand(param => 
            { 
                _historyViewModel.LoadTransactions(); // Refresh
                CurrentViewModel = _historyViewModel; 
            });
        }
    }
}

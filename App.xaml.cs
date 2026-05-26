using System.Configuration;
using System.Data;
using System.Windows;
using StoreManagement.Database;

namespace StoreManagement;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DatabaseHelper.InitializeDatabase();
    }
}


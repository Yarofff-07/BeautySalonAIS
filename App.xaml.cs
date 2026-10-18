using System.Configuration;
using System.Data;
using System.Windows;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using System.Windows;

namespace UsersApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Програмна реєстрація провайдера SQLite, щоб уникнути помилок в App.config
            try
            {
                var instance = SQLiteFactory.Instance;
                System.Data.Common.DbProviderFactories.RegisterFactory("System.Data.SQLite", instance);
                System.Data.Common.DbProviderFactories.RegisterFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
            }
            catch
            {
                // Якщо вже зареєстровано — ігноруємо
            }
        }
    }
}
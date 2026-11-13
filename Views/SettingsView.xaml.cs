using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace UsersApp.Views
{
    public partial class SettingsView : UserControl
    {
        private bool _isInitializing = true;

        public SettingsView()
        {
            InitializeComponent();
            VersionText.Text = "Версія: " + (Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "невідома");
            var isDark = new PaletteHelper().GetTheme().GetBaseTheme() == BaseTheme.Dark;
            DarkThemeRadio.IsChecked = isDark;
            LightThemeRadio.IsChecked = !isDark;
            _isInitializing = false;
        }

        private void LightTheme_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitializing)
            {
                ApplyTheme(Theme.Light);
            }
        }

        private void DarkTheme_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitializing)
            {
                ApplyTheme(Theme.Dark);
            }
        }

        private static void ApplyTheme(IBaseTheme baseTheme)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(baseTheme);
            paletteHelper.SetTheme(theme);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Window.GetWindow(this), "Вийти з облікового запису?",
                    "Підтвердження виходу", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            var authWindow = new AuthWindow();
            authWindow.Show();
            Window.GetWindow(this)?.Close();
        }
    }
}

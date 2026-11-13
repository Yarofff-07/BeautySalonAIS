using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UsersApp.Views;

namespace UsersApp
{
    /// <summary>
    /// Логика взаимодействия для UserPageWindow.xaml
    /// </summary>
    public partial class UserPageWindow : Window
    {
        private static readonly Brush InactiveBackground = Brushes.Transparent;
        private static readonly Brush InactiveForeground = new SolidColorBrush(Color.FromRgb(248, 244, 240));
        private static readonly Brush ActiveBackground = new SolidColorBrush(Color.FromRgb(128, 97, 75));
        private static readonly Brush ActiveForeground = Brushes.White;

        public UserPageWindow()
        {
            InitializeComponent();
            ShowHome();
        }

        private void HomeNav_Click(object sender, RoutedEventArgs e)
        {
            ShowHome();
        }

        private void ClientsNav_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = new ClientsView();
            SetActiveNavigation(clientsNavButton);
        }

        private void ServicesNav_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = new ServicesView();
            SetActiveNavigation(servicesNavButton);
        }

        private void MastersNav_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = new MastersView();
            SetActiveNavigation(mastersNavButton);
        }

        private void ScheduleNav_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = new ScheduleView();
            SetActiveNavigation(scheduleNavButton);
        }

        private void SettingsNav_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = new SettingsView();
            SetActiveNavigation(settingsNavButton);
        }

        private void ShowHome()
        {
            ContentHost.Content = new HomeView();
            SetActiveNavigation(homeNavButton);
        }

        private void SetActiveNavigation(Button activeButton)
        {
            homeNavButton.Background = InactiveBackground;
            homeNavButton.Foreground = InactiveForeground;
            clientsNavButton.Background = InactiveBackground;
            clientsNavButton.Foreground = InactiveForeground;
            servicesNavButton.Background = InactiveBackground;
            servicesNavButton.Foreground = InactiveForeground;
            mastersNavButton.Background = InactiveBackground;
            mastersNavButton.Foreground = InactiveForeground;
            scheduleNavButton.Background = InactiveBackground;
            scheduleNavButton.Foreground = InactiveForeground;
            settingsNavButton.Background = InactiveBackground;
            settingsNavButton.Foreground = InactiveForeground;

            activeButton.Background = ActiveBackground;
            activeButton.Foreground = ActiveForeground;
        }
    }
}

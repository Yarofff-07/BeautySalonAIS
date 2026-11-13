using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using UsersApp.Data;
using UsersApp.Models;

namespace UsersApp.Views
{
    public partial class HomeView : UserControl
    {
        private readonly ClientRepository _clientRepository = new();
        private readonly OrderRepository _orderRepository = new();

        public HomeView()
        {
            InitializeComponent();
            orderDatePicker.SelectedDate = DateTime.Today;
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                var selectedDate = orderDatePicker.SelectedDate ?? DateTime.Today;
                var orders = _orderRepository.GetForDate(selectedDate);
                ordersList.ItemsSource = orders;
                emptyOrdersText.Visibility = orders.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception exception)
            {
                ordersList.ItemsSource = null;
                emptyOrdersText.Visibility = Visibility.Visible;
                emptyOrdersText.Text = "Не вдалося завантажити замовлення.";
                ShowError(exception.Message);
            }
        }

        private void ShowOrderEditor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var clients = _clientRepository.GetAll();
                var masters = _orderRepository.GetMasters();
                var services = _orderRepository.GetServices();

                if (clients.Count == 0 || masters.Count == 0 || services.Count == 0)
                {
                    ShowError("Для створення замовлення спочатку мають бути додані клієнти, майстри та послуги.");
                    return;
                }

                clientComboBox.ItemsSource = clients;
                masterComboBox.ItemsSource = masters;
                serviceComboBox.ItemsSource = services;
                clientComboBox.SelectedIndex = 0;
                masterComboBox.SelectedIndex = 0;
                serviceComboBox.SelectedIndex = 0;
                orderDatePicker.SelectedDate = DateTime.Today;
                orderTimeTextBox.Clear();
                orderFormErrorText.Text = string.Empty;
                orderEditorOverlay.Visibility = Visibility.Visible;
            }
            catch (Exception exception)
            {
                ShowError(exception.Message);
            }
        }

        private void ServiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (orderPriceText is null)
            {
                return;
            }

            orderPriceText.Text = serviceComboBox.SelectedItem is SalonService service
                ? service.Price.ToString("N2", CultureInfo.CurrentCulture)
                : "Оберіть послугу";
        }

        private void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (clientComboBox.SelectedValue is not int clientId ||
                masterComboBox.SelectedValue is not int masterId ||
                serviceComboBox.SelectedItem is not SalonService service)
            {
                orderFormErrorText.Text = "Оберіть клієнта, майстра та послугу.";
                return;
            }

            if (orderDatePicker.SelectedDate is not DateTime orderDate)
            {
                orderFormErrorText.Text = "Вкажіть дату замовлення.";
                return;
            }

            if (!TimeOnly.TryParseExact(orderTimeTextBox.Text.Trim(), "HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var orderTime))
            {
                orderFormErrorText.Text = "Введіть час у форматі ГГ:ХХ, наприклад 14:30.";
                return;
            }

            try
            {
                _orderRepository.Add(new SalonOrder
                {
                    ClientID = clientId,
                    MasterID = masterId,
                    ServiceID = service.ServiceID,
                    OrderDate = orderDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    OrderTime = orderTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                    TotalPrice = service.Price,
                    Status = "Заплановано"
                });

                orderEditorOverlay.Visibility = Visibility.Collapsed;
                LoadOrders();
            }
            catch (Exception exception)
            {
                orderFormErrorText.Text = "Не вдалося створити замовлення: " + exception.Message;
            }
        }

        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            orderEditorOverlay.Visibility = Visibility.Collapsed;
            orderFormErrorText.Text = string.Empty;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(Window.GetWindow(this), message, "Beauty Salon",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

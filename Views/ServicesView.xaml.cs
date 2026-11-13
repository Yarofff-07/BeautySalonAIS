using System;
using System.Data.SQLite;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using UsersApp.Data;
using UsersApp.Models;

namespace UsersApp.Views
{
    public partial class ServicesView : UserControl
    {
        private readonly ServiceRepository _serviceRepository = new();
        private SalonService? _editingService;

        public ServicesView()
        {
            InitializeComponent();
            LoadServices();
        }

        private void LoadServices(string? searchText = null)
        {
            try
            {
                servicesGrid.ItemsSource = _serviceRepository.GetAll(searchText);
            }
            catch (Exception exception)
            {
                servicesGrid.ItemsSource = null;
                ShowError("Не вдалося завантажити послуги: " + exception.Message);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadServices(searchTextBox.Text);
        }

        private void ServiceActionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu is ContextMenu contextMenu)
            {
                contextMenu.PlacementTarget = button;
                contextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        private void AddService_Click(object sender, RoutedEventArgs e)
        {
            _editingService = null;
            serviceEditorTitle.Text = "Нова послуга";
            serviceNameTextBox.Clear();
            serviceDescriptionTextBox.Clear();
            servicePriceTextBox.Clear();
            serviceDurationTextBox.Clear();
            serviceFormErrorText.Text = string.Empty;
            serviceEditorOverlay.Visibility = Visibility.Visible;
            serviceNameTextBox.Focus();
        }

        private void EditService_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.CommandParameter is not SalonService service)
            {
                return;
            }

            _editingService = service;
            serviceEditorTitle.Text = "Редагування послуги";
            serviceNameTextBox.Text = service.ServiceName;
            serviceDescriptionTextBox.Text = service.Description ?? string.Empty;
            servicePriceTextBox.Text = service.Price.ToString(CultureInfo.CurrentCulture);
            serviceDurationTextBox.Text = service.Duration.ToString(CultureInfo.CurrentCulture);
            serviceFormErrorText.Text = string.Empty;
            serviceEditorOverlay.Visibility = Visibility.Visible;
            serviceNameTextBox.Focus();
        }

        private void DeleteService_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.CommandParameter is not SalonService service)
            {
                return;
            }

            var result = MessageBox.Show(Window.GetWindow(this),
                $"Видалити послугу «{service.ServiceName}»?",
                "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _serviceRepository.Delete(service.ServiceID);
                LoadServices(searchTextBox.Text);
            }
            catch (SQLiteException exception)
            {
                ShowError("Не вдалося видалити послугу. Можливо, вона використовується в замовленнях. " + exception.Message);
            }
            catch (Exception exception)
            {
                ShowError("Не вдалося видалити послугу: " + exception.Message);
            }
        }

        private void SaveService_Click(object sender, RoutedEventArgs e)
        {
            var serviceName = serviceNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                serviceFormErrorText.Text = "Вкажіть назву послуги.";
                serviceNameTextBox.Focus();
                return;
            }

            if (!decimal.TryParse(servicePriceTextBox.Text.Trim(), NumberStyles.Number,
                    CultureInfo.CurrentCulture, out var price) || price <= 0)
            {
                serviceFormErrorText.Text = "Вкажіть коректну вартість, більшу за нуль.";
                servicePriceTextBox.Focus();
                return;
            }

            if (!int.TryParse(serviceDurationTextBox.Text.Trim(), NumberStyles.Integer,
                    CultureInfo.CurrentCulture, out var duration) || duration <= 0)
            {
                serviceFormErrorText.Text = "Вкажіть тривалість у хвилинах, більшу за нуль.";
                serviceDurationTextBox.Focus();
                return;
            }

            var service = new SalonService
            {
                ServiceID = _editingService?.ServiceID ?? 0,
                ServiceName = serviceName,
                Description = NullIfEmpty(serviceDescriptionTextBox.Text),
                Price = price,
                Duration = duration
            };

            try
            {
                if (_editingService is null)
                {
                    _serviceRepository.Add(service);
                }
                else
                {
                    _serviceRepository.Update(service);
                }

                serviceEditorOverlay.Visibility = Visibility.Collapsed;
                LoadServices(searchTextBox.Text);
            }
            catch (Exception exception)
            {
                serviceFormErrorText.Text = "Не вдалося зберегти послугу: " + exception.Message;
            }
        }

        private void CancelService_Click(object sender, RoutedEventArgs e)
        {
            serviceEditorOverlay.Visibility = Visibility.Collapsed;
            serviceFormErrorText.Text = string.Empty;
        }

        private static string? NullIfEmpty(string value)
        {
            var trimmedValue = value.Trim();
            return trimmedValue.Length == 0 ? null : trimmedValue;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(Window.GetWindow(this), message, "Beauty Salon",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

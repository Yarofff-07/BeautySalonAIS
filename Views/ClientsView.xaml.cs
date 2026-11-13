using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using UsersApp.Data;
using UsersApp.Models;

namespace UsersApp.Views
{
    public partial class ClientsView : UserControl
    {
        private readonly ClientRepository _clientRepository = new();
        private Client? _editingClient;

        public ClientsView()
        {
            InitializeComponent();
            LoadClients();
        }

        private void LoadClients(string? searchText = null)
        {
            try
            {
                clientsGrid.ItemsSource = _clientRepository.GetAll(searchText);
            }
            catch (Exception exception)
            {
                clientsGrid.ItemsSource = null;
                ShowError("Не вдалося завантажити клієнтів: " + exception.Message);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadClients(searchTextBox.Text);
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            _editingClient = null;
            clientEditorTitle.Text = "Новий клієнт";
            clientNameTextBox.Clear();
            clientPhoneTextBox.Clear();
            clientEmailTextBox.Clear();
            clientFormErrorText.Text = string.Empty;
            clientEditorOverlay.Visibility = Visibility.Visible;
            clientNameTextBox.Focus();
        }

        private void ClientActionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu is ContextMenu contextMenu)
            {
                contextMenu.PlacementTarget = button;
                contextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.CommandParameter is not Client client)
            {
                return;
            }

            _editingClient = client;
            clientEditorTitle.Text = "Редагування клієнта";
            clientNameTextBox.Text = client.FullName;
            clientPhoneTextBox.Text = client.Phone ?? string.Empty;
            clientEmailTextBox.Text = client.Email ?? string.Empty;
            clientFormErrorText.Text = string.Empty;
            clientEditorOverlay.Visibility = Visibility.Visible;
            clientNameTextBox.Focus();
        }

        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.CommandParameter is not Client client)
            {
                return;
            }

            var result = MessageBox.Show(Window.GetWindow(this),
                $"Видалити клієнта «{client.FullName}»?",
                "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _clientRepository.Delete(client.ClientID);
                LoadClients(searchTextBox.Text);
            }
            catch (SQLiteException exception)
            {
                ShowError("Не вдалося видалити клієнта. Можливо, з ним пов'язані замовлення. " + exception.Message);
            }
            catch (Exception exception)
            {
                ShowError("Не вдалося видалити клієнта: " + exception.Message);
            }
        }

        private void SaveClient_Click(object sender, RoutedEventArgs e)
        {
            var fullName = clientNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                clientFormErrorText.Text = "Вкажіть ПІБ клієнта.";
                clientNameTextBox.Focus();
                return;
            }

            var client = new Client
            {
                ClientID = _editingClient?.ClientID ?? 0,
                FullName = fullName,
                Phone = NullIfEmpty(clientPhoneTextBox.Text),
                Email = NullIfEmpty(clientEmailTextBox.Text)
            };

            try
            {
                if (_editingClient is null)
                {
                    _clientRepository.Add(client);
                }
                else
                {
                    _clientRepository.Update(client);
                }

                clientEditorOverlay.Visibility = Visibility.Collapsed;
                LoadClients(searchTextBox.Text);
            }
            catch (Exception exception)
            {
                clientFormErrorText.Text = "Не вдалося зберегти клієнта: " + exception.Message;
            }
        }

        private void CancelClient_Click(object sender, RoutedEventArgs e)
        {
            clientEditorOverlay.Visibility = Visibility.Collapsed;
            clientFormErrorText.Text = string.Empty;
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

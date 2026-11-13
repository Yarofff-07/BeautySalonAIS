using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using UsersApp.Data;
using UsersApp.Models;

namespace UsersApp.Views
{
    public partial class MastersView : UserControl
    {
        private readonly MasterRepository _masterRepository = new();
        private Master? _editingMaster;

        public MastersView()
        {
            InitializeComponent();
            LoadMasters();
        }

        private void LoadMasters(string? searchText = null)
        {
            try
            {
                mastersGrid.ItemsSource = _masterRepository.GetAll(searchText);
            }
            catch (Exception exception)
            {
                mastersGrid.ItemsSource = null;
                ShowError("Не вдалося завантажити майстрів: " + exception.Message);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadMasters(searchTextBox.Text);
        }

        private void MasterActionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu is ContextMenu contextMenu)
            {
                contextMenu.PlacementTarget = button;
                contextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        private void AddMaster_Click(object sender, RoutedEventArgs e)
        {
            _editingMaster = null;
            masterEditorTitle.Text = "Новий майстер";
            masterNameTextBox.Clear();
            masterPhoneTextBox.Clear();
            masterSpecializationTextBox.Clear();
            masterFormErrorText.Text = string.Empty;
            masterEditorOverlay.Visibility = Visibility.Visible;
            masterNameTextBox.Focus();
        }

        private void EditMaster_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.CommandParameter is not Master master)
            {
                return;
            }

            _editingMaster = master;
            masterEditorTitle.Text = "Редагування майстра";
            masterNameTextBox.Text = master.FullName;
            masterPhoneTextBox.Text = master.Phone ?? string.Empty;
            masterSpecializationTextBox.Text = master.Specialization ?? string.Empty;
            masterFormErrorText.Text = string.Empty;
            masterEditorOverlay.Visibility = Visibility.Visible;
            masterNameTextBox.Focus();
        }

        private void DeleteMaster_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.CommandParameter is not Master master)
            {
                return;
            }

            var result = MessageBox.Show(Window.GetWindow(this),
                $"Видалити майстра «{master.FullName}»?",
                "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _masterRepository.Delete(master.MasterID);
                LoadMasters(searchTextBox.Text);
            }
            catch (SQLiteException exception)
            {
                ShowError("Не вдалося видалити майстра. Можливо, з ним пов'язані замовлення. " + exception.Message);
            }
            catch (Exception exception)
            {
                ShowError("Не вдалося видалити майстра: " + exception.Message);
            }
        }

        private void SaveMaster_Click(object sender, RoutedEventArgs e)
        {
            var fullName = masterNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                masterFormErrorText.Text = "Вкажіть ПІБ майстра.";
                masterNameTextBox.Focus();
                return;
            }

            var master = new Master
            {
                MasterID = _editingMaster?.MasterID ?? 0,
                FullName = fullName,
                Phone = NullIfEmpty(masterPhoneTextBox.Text),
                Specialization = NullIfEmpty(masterSpecializationTextBox.Text)
            };

            try
            {
                if (_editingMaster is null)
                {
                    _masterRepository.Add(master);
                }
                else
                {
                    _masterRepository.Update(master);
                }

                masterEditorOverlay.Visibility = Visibility.Collapsed;
                LoadMasters(searchTextBox.Text);
            }
            catch (Exception exception)
            {
                masterFormErrorText.Text = "Не вдалося зберегти майстра: " + exception.Message;
            }
        }

        private void CancelMaster_Click(object sender, RoutedEventArgs e)
        {
            masterEditorOverlay.Visibility = Visibility.Collapsed;
            masterFormErrorText.Text = string.Empty;
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

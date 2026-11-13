using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UsersApp.Data;
using UsersApp.Models;

namespace UsersApp.Views
{
    public partial class ScheduleView : UserControl
    {
        private static readonly string[] OrderStatuses = ["Заплановано", "У процесі", "Виконано"];
        private static readonly CultureInfo UkrainianCulture = CultureInfo.GetCultureInfo("uk-UA");
        private readonly OrderRepository _orderRepository = new();
        private readonly ClientRepository _clientRepository = new();
        private DateTime _displayedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        private DateTime _selectedDate = DateTime.Today;
        private SalonOrder? _editingOrder;
        private HashSet<DateTime> _datesWithOrders = [];

        public ScheduleView()
        {
            InitializeComponent();
            StatusComboBox.ItemsSource = OrderStatuses;
            RefreshCalendarAndOrders();
        }

        private void RefreshCalendarAndOrders()
        {
            try
            {
                _datesWithOrders = _orderRepository.GetDatesWithOrders(_displayedMonth).ToHashSet();
                RenderCalendar();
                LoadOrdersForSelectedDate();
            }
            catch (Exception exception)
            {
                OrdersList.ItemsSource = null;
                EmptyOrdersText.Text = "Не вдалося завантажити замовлення.";
                EmptyOrdersText.Visibility = Visibility.Visible;
                ShowError("Не вдалося оновити розклад: " + exception.Message);
            }
        }

        private void RenderCalendar()
        {
            MonthTitle.Text = _displayedMonth.ToString("MMMM yyyy", UkrainianCulture);
            CalendarGrid.Children.Clear();

            var firstDayOffset = ((int)_displayedMonth.DayOfWeek + 6) % 7;
            var firstCellDate = _displayedMonth.AddDays(-firstDayOffset);
            for (var index = 0; index < 42; index++)
            {
                var date = firstCellDate.AddDays(index);
                var isCurrentMonth = date.Month == _displayedMonth.Month;
                var hasOrders = isCurrentMonth && _datesWithOrders.Contains(date.Date);
                var isToday = date.Date == DateTime.Today;
                var isSelected = date.Date == _selectedDate.Date;
                var button = new Button
                {
                    Padding = new Thickness(2),
                    Margin = new Thickness(2),
                    Background = isSelected ? new SolidColorBrush(Color.FromRgb(128, 97, 75)) : Brushes.Transparent,
                    Foreground = !isCurrentMonth ? new SolidColorBrush(Color.FromRgb(190, 185, 180))
                        : date.Date < DateTime.Today ? new SolidColorBrush(Color.FromRgb(160, 157, 153))
                        : isSelected ? Brushes.White : new SolidColorBrush(Color.FromRgb(71, 53, 43)),
                    BorderBrush = isToday ? new SolidColorBrush(Color.FromRgb(220, 112, 62)) : Brushes.Transparent,
                    BorderThickness = isToday ? new Thickness(2) : new Thickness(0),
                    IsEnabled = isCurrentMonth,
                    Tag = date.Date,
                    ToolTip = hasOrders ? "На цю дату є замовлення" : null
                };
                var content = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
                content.Children.Add(new TextBlock
                {
                    Text = date.Day.ToString(CultureInfo.InvariantCulture),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                content.Children.Add(new TextBlock
                {
                    Text = hasOrders ? "●" : " ",
                    Foreground = isSelected ? Brushes.White : new SolidColorBrush(Color.FromRgb(128, 97, 75)),
                    FontSize = 9,
                    Height = 11,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                button.Content = content;
                button.Click += CalendarDay_Click;
                CalendarGrid.Children.Add(button);
            }
        }

        private void LoadOrdersForSelectedDate()
        {
            var orders = _orderRepository.GetForDate(_selectedDate);
            OrdersList.ItemsSource = orders;
            SelectedDateTitle.Text = _selectedDate.ToString("dddd, d MMMM yyyy", UkrainianCulture);
            EmptyOrdersText.Text = "На цю дату замовлень немає";
            EmptyOrdersText.Visibility = orders.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CalendarDay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: DateTime date })
            {
                return;
            }

            _selectedDate = date;
            RefreshCalendarAndOrders();
        }

        private void PreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            _displayedMonth = _displayedMonth.AddMonths(-1);
            RefreshCalendarAndOrders();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            _displayedMonth = _displayedMonth.AddMonths(1);
            RefreshCalendarAndOrders();
        }

        private void Today_Click(object sender, RoutedEventArgs e)
        {
            _selectedDate = DateTime.Today;
            _displayedMonth = new DateTime(_selectedDate.Year, _selectedDate.Month, 1);
            RefreshCalendarAndOrders();
        }

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            OpenOrderEditor(null);
        }

        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            if (GetOrderFromMenu(sender) is SalonOrder order)
            {
                OpenOrderEditor(order);
            }
        }

        private SalonOrder? GetOrderFromMenu(object sender)
        {
            return sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu &&
                   contextMenu.PlacementTarget is Button button ? button.DataContext as SalonOrder : null;
        }

        private void OpenOrderEditor(SalonOrder? order)
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

                ClientComboBox.ItemsSource = clients;
                MasterComboBox.ItemsSource = masters;
                ServiceComboBox.ItemsSource = services;
                _editingOrder = order is null ? null : _orderRepository.GetById(order.OrderID);
                if (order is not null && _editingOrder is null)
                {
                    ShowError("Замовлення вже не існує. Оновіть розклад.");
                    RefreshCalendarAndOrders();
                    return;
                }

                ClientComboBox.SelectedValue = _editingOrder?.ClientID ?? clients[0].ClientID;
                MasterComboBox.SelectedValue = _editingOrder?.MasterID ?? masters[0].MasterID;
                ServiceComboBox.SelectedValue = _editingOrder?.ServiceID ?? services[0].ServiceID;
                OrderDateCalendar.SelectedDate = _editingOrder is null
                    ? _selectedDate.Date
                    : DateTime.ParseExact(_editingOrder.OrderDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                OrderTimeTextBox.Text = _editingOrder?.OrderTime ?? string.Empty;
                StatusComboBox.SelectedItem = _editingOrder?.Status is string status && OrderStatuses.Contains(status)
                    ? status : OrderStatuses[0];
                EditorTitle.Text = _editingOrder is null ? "Нове замовлення" : "Редагування замовлення";
                SaveButton.Content = _editingOrder is null ? "Створити" : "Зберегти";
                FormErrorText.Text = _editingOrder is not null && !OrderStatuses.Contains(_editingOrder.Status)
                    ? $"У замовлення збережений невідомий статус «{_editingOrder.Status}». Виберіть один із доступних статусів."
                    : string.Empty;
                OrderEditorOverlay.Visibility = Visibility.Visible;
            }
            catch (Exception exception)
            {
                ShowError("Не вдалося відкрити форму замовлення: " + exception.Message);
            }
        }

        private void ServiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrderPriceText is null)
            {
                return;
            }

            OrderPriceText.Text = ServiceComboBox.SelectedItem is SalonService service
                ? service.Price.ToString("N2", CultureInfo.CurrentCulture) + " ₴"
                : "Оберіть послугу";
        }

        private void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (ClientComboBox.SelectedValue is not int clientId ||
                MasterComboBox.SelectedValue is not int masterId ||
                ServiceComboBox.SelectedItem is not SalonService service)
            {
                FormErrorText.Text = "Оберіть клієнта, майстра та послугу.";
                return;
            }

            if (OrderDateCalendar.SelectedDate is not DateTime orderDate)
            {
                FormErrorText.Text = "Виберіть дату замовлення в календарі.";
                return;
            }

            if (!TimeOnly.TryParseExact(OrderTimeTextBox.Text.Trim(), "HH:mm", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var orderTime))
            {
                FormErrorText.Text = "Введіть час у форматі ГГ:ХХ, наприклад 14:30.";
                return;
            }

            if (service.Price <= 0 || StatusComboBox.SelectedItem is not string status || !OrderStatuses.Contains(status))
            {
                FormErrorText.Text = "Перевірте вартість послуги та виберіть коректний статус.";
                return;
            }

            var updatedOrder = new SalonOrder
            {
                OrderID = _editingOrder?.OrderID ?? 0,
                ClientID = clientId,
                MasterID = masterId,
                ServiceID = service.ServiceID,
                OrderDate = orderDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                OrderTime = orderTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                TotalPrice = service.Price,
                Status = status
            };

            try
            {
                if (_editingOrder is null)
                {
                    _orderRepository.Add(updatedOrder);
                }
                else
                {
                    _orderRepository.Update(updatedOrder);
                }

                OrderEditorOverlay.Visibility = Visibility.Collapsed;
                _editingOrder = null;
                _selectedDate = orderDate.Date;
                _displayedMonth = new DateTime(orderDate.Year, orderDate.Month, 1);
                RefreshCalendarAndOrders();
            }
            catch (Exception exception)
            {
                FormErrorText.Text = "Не вдалося зберегти замовлення: " + exception.Message;
            }
        }

        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            OrderEditorOverlay.Visibility = Visibility.Collapsed;
            FormErrorText.Text = string.Empty;
            _editingOrder = null;
        }

        private void OrderActionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu is ContextMenu contextMenu)
            {
                contextMenu.PlacementTarget = button;
                contextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        private void SetStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem { Tag: string status } menuItem || GetOrderFromMenu(menuItem) is not SalonOrder order)
            {
                return;
            }

            try
            {
                _orderRepository.UpdateStatus(order.OrderID, status);
                RefreshCalendarAndOrders();
            }
            catch (Exception exception)
            {
                ShowError("Не вдалося змінити статус замовлення: " + exception.Message);
            }
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (GetOrderFromMenu(sender) is not SalonOrder order)
            {
                return;
            }

            if (MessageBox.Show(Window.GetWindow(this),
                    $"Видалити замовлення для клієнта «{order.ClientName}» на {order.OrderDate} о {order.OrderTime}?",
                    "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _orderRepository.Delete(order.OrderID);
                RefreshCalendarAndOrders();
            }
            catch (Exception exception)
            {
                ShowError("Не вдалося видалити замовлення: " + exception.Message);
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(Window.GetWindow(this), message, "Beauty Salon",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

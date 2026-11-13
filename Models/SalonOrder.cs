namespace UsersApp.Models
{
    public class SalonOrder
    {
        public int OrderID { get; set; }
        public int ClientID { get; set; }
        public int MasterID { get; set; }
        public int ServiceID { get; set; }
        public string OrderDate { get; set; } = string.Empty;
        public string OrderTime { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string MasterName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
    }
}

namespace UsersApp.Models
{
    public class SalonService
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
    }
}

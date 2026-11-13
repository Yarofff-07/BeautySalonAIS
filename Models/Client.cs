namespace UsersApp.Models
{
    public class Client
    {
        public int ClientID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}

namespace UsersApp.Models
{
    public class Master
    {
        public int MasterID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Specialization { get; set; }
    }
}

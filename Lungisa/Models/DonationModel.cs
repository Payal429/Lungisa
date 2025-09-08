namespace Lungisa.Models
{
    public class DonationModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string PayFastPaymentId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Read-only computed property
        public string DonorName => $"{FirstName} {LastName}";
    }
}

namespace Lungisa.Models
{
    public class DonationModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DonorName => $"{FirstName} {LastName}";
        public string Email { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // PayFast tracking
        public string PayFastPaymentId { get; set; }
        public string Status { get; set; } = "Pending";
    }
}

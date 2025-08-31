namespace Lungisa.Models
{
    public class DonationModel
    {
        public string DonorName { get; set; }
        public string Email { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string PayFastPaymentId { get; set; } // Optional: Track PayFast reference
        public string Status { get; set; } // "Success" / "Failed"

        // For form binding
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}

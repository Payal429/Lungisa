namespace Lungisa.Models
{
    public class DonationModel
    {
        public string DonorName { get; set; }

        public string Email { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } // "Success" / "Failed"

        // For form binding
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // 🔑 Additions for PayFast integration
        public string MerchantId { get; set; }        // From PayFast config
        public string MerchantKey { get; set; }       // From PayFast config
        public string ReturnUrl { get; set; }         // Redirect after success
        public string CancelUrl { get; set; }         // Redirect after cancel
        public string NotifyUrl { get; set; }         // PayFast calls this to confirm payment

        public string PayFastPaymentId { get; set; } // Optional: Track PayFast reference

        public string M_PaymentId { get; set; } // Your merchant reference (unique per donation)
        public string PaymentReference { get; set; }  // Unique order/donation reference
    }
}

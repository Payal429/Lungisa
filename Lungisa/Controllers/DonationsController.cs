using Lungisa.Models;
using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Text;

using Lungisa.Models;
using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Lungisa.Controllers
{
    public class DonationsController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly FirebaseService _firebase;
        private readonly HttpClient _httpClient;

        public DonationsController(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
            _firebase = new FirebaseService(_config, _env);
            _httpClient = new HttpClient();
        }

        // GET: Donations page
        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Home/Donations.cshtml");
        }

        // POST: PayFast payment redirect
        [HttpPost]
        public async Task<IActionResult> PayFastPay(DonationModel model)
        {
            // Generate merchant reference
            var mPaymentId = Guid.NewGuid().ToString("N");

            // Save donation as Pending
            var pendingDonation = new DonationModel
            {
                DonorName = $"{model.FirstName} {model.LastName}",
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Amount = model.Amount,
                Status = "Pending",
                Timestamp = DateTime.UtcNow,
                M_PaymentId = mPaymentId
            };
            await _firebase.SaveDonation(pendingDonation);

            // PayFast parameters
            var pfData = new SortedDictionary<string, string>
            {
                ["merchant_id"] = _config["PayFastSettings:MerchantId"],
                ["merchant_key"] = _config["PayFastSettings:MerchantKey"],
                ["return_url"] = _config["PayFastSettings:ReturnUrl"],
                ["cancel_url"] = _config["PayFastSettings:CancelUrl"],
                ["notify_url"] = _config["PayFastSettings:NotifyUrl"],
                ["name_first"] = model.FirstName,
                ["name_last"] = model.LastName,
                ["email_address"] = model.Email,
                ["m_payment_id"] = mPaymentId,
                ["amount"] = model.Amount.ToString("F2", CultureInfo.InvariantCulture),
                ["item_name"] = "Donation to Lungisa NPO"
            };

            var signature = GeneratePayfastSignature(pfData, _config["PayFastSettings:Passphrase"]);
            pfData.Add("signature", signature);

            var processUrl = _config["PayFastSettings:ProcessUrl"];
            var queryString = string.Join("&", pfData.Select(kvp => $"{kvp.Key}={WebUtility.UrlEncode(kvp.Value)}"));

            // Redirect to PayFast sandbox
            return Redirect($"{processUrl}?{queryString}");
        }

        // ITN endpoint (PayFast notifies server of payment)
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> Notify()
        {
            if (!Request.HasFormContentType) return BadRequest("No form data");

            var form = Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString());
            var receivedSignature = form.GetValueOrDefault("signature", "");
            form.Remove("signature");

            var sortedData = new SortedDictionary<string, string>(form);
            var calculatedSignature = GeneratePayfastSignature(sortedData, _config["PayFastSettings:Passphrase"]);

            if (!string.Equals(receivedSignature, calculatedSignature, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid signature");

            // Map PayFast fields to DonationModel
            var donation = new DonationModel
            {
                DonorName = $"{form.GetValueOrDefault("name_first", "")} {form.GetValueOrDefault("name_last", "")}".Trim(),
                Email = form.GetValueOrDefault("email_address", ""),
                Amount = decimal.TryParse(form.GetValueOrDefault("amount_gross", form.GetValueOrDefault("amount", "0")), NumberStyles.Any, CultureInfo.InvariantCulture, out var amt) ? amt : 0,
                Status = MapPayFastStatusToLocalStatus(form.GetValueOrDefault("payment_status", "Failed")),
                PayFastPaymentId = form.GetValueOrDefault("pf_payment_id", ""),
                M_PaymentId = form.GetValueOrDefault("m_payment_id", ""),
                Timestamp = DateTime.UtcNow
            };

            try
            {
                await _firebase.SaveDonation(donation); // update existing or create new
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to save donation from ITN: " + ex.Message);
                return StatusCode(500, "Failed to save");
            }

            return Ok("ITN processed");
        }

        // Success / Cancel pages
        [HttpGet] public IActionResult Success() => View();
        [HttpGet] public IActionResult Cancel() => View();

        // ================= Helper Methods =================
        private string GeneratePayfastSignature(SortedDictionary<string, string> data, string passphrase)
        {
            var sb = new StringBuilder();
            foreach (var kv in data)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    sb.Append(kv.Key).Append('=').Append(WebUtility.UrlEncode(kv.Value).Replace("%20", "+")).Append('&');
                }
            }
            if (!string.IsNullOrEmpty(passphrase))
                sb.Append("passphrase=").Append(WebUtility.UrlEncode(passphrase).Replace("%20", "+"));
            else if (sb.Length > 0 && sb[sb.Length - 1] == '&') sb.Length--; // remove trailing &

            using var md5 = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var hash = md5.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private string MapPayFastStatusToLocalStatus(string pfStatus)
        {
            return pfStatus.ToUpper() switch
            {
                "COMPLETE" => "Success",
                "FAILED" => "Failed",
                "PENDING" => "Pending",
                _ => pfStatus
            };
        }
    }
}


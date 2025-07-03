namespace DemoEcentricPaymentSystems.Models
{
    public class CheckoutViewModel
    {
        public List<Product> Products { get; set; }
        public PosBuddyConfig Config { get; set; }
        public string? PaymentStatus { get; set; }
    }

}

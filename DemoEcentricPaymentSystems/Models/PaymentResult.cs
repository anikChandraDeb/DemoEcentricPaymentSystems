namespace DemoEcentricPaymentSystems.Models
{
    public class PaymentResult
    {
        public string ResultCode { get; set; }
        public string ResultDescription { get; set; }
        public string TransactionUuid { get; set; }
        public string InvoiceNumber { get; set; }
        public int Amount { get; set; }
    }
}

using DemoEcentricPaymentSystems.Models;
using Ecentric.PosBuddyClient.DTOs;
using Ecentric.PosBuddyClient;
using Ecentric.PosBuddyClient.Enums;
using System.Transactions;

namespace DemoEcentricPaymentSystems.Services
{
    public class PosBuddyService
    {
        private PosBuddy _client;
        private bool _isConnected = false;
        private bool _isAuthenticated = false;

        public PosBuddyService(PosBuddyConfig config)
        {
            _client = new PosBuddy((logType, message) =>
            {
                Console.WriteLine($"LogCallback {logType}, Message: {message}");
            });

            _client.SetMerchantId(config.MerchantId);
            _client.SetPosId(config.PosId);
        }

        public async Task<bool> ConnectAsync(string terminalCode)
        {
            var tcs = new TaskCompletionSource<bool>();
            _client.Connect(terminalCode, (status) =>
            {
                _isConnected = status == PbStatus.Connected;
                tcs.TrySetResult(_isConnected);
            });
            return await tcs.Task;
        }

        public async Task<bool> AuthenticateAsync(string secretKey, string Accesskey)
        {
            if (!_isConnected) return false;
            var tcs = new TaskCompletionSource<bool>();
            string? authResult = null;
            _client.DoPosAuth(secretKey, Accesskey, (result) =>
            {
                authResult = result["resultDescription"]?.ToString();
                
                if ((result["resultDescription"] as string) is "SUCCESS" or "COMPLETED")
                {
                    var authKey = result["authenticationKey"] as string;
                    _client.SetAuthenticationKey(authKey);
                    _isAuthenticated = true;
                    if (result.TryGetValue("applicationKey", out var appKeyObj))
                    {
                        string? applicationKey = appKeyObj as string;
                        Console.WriteLine("ApplicationKey: " + applicationKey);
                        _client.SetApplicationKey(applicationKey);

                    }
                }
                tcs.TrySetResult(_isConnected);
            });
            while (authResult == null) ;
            return await tcs.Task;
        }

        public void DisplayItems(List<Product> products)
        {
            var total = products.Sum(p => p.Price * p.Quantity).ToString();
            var items = products.Select(p => new PbItem
            {
                description = p.Description,
                quantity = p.Quantity.ToString(),
                price = p.Price.ToString()
            }).ToList();

            _client.DisplayItems(total, items);
        }

        public async Task<PaymentResult> DoSaleAsync(int total)
        {
            var tcs = new TaskCompletionSource<PaymentResult>();
            var customFields = new Dictionary<string, string>
            {
                { "invoiceNumber", "INV123" }
            };
            _client.DoSale(total, customFields, (result) =>
            {
                var paymentResult = new PaymentResult();

                if (result.TryGetValue("resultCode", out var resultCode))
                    paymentResult.ResultCode = resultCode?.ToString();

                if (result.TryGetValue("resultDescription", out var resultDescription))
                    paymentResult.ResultDescription = resultDescription?.ToString();

                if (result.TryGetValue("transactionUuid", out var uuid))
                    paymentResult.TransactionUuid = uuid?.ToString();
                else
                    paymentResult.TransactionUuid = null;

                if (result.TryGetValue("invoiceNumber", out var invoiceNumber))
                    paymentResult.InvoiceNumber = invoiceNumber?.ToString();

                if (result.TryGetValue("transactionAmount", out var amountObj) && int.TryParse(amountObj?.ToString(), out var amount))
                    paymentResult.Amount = amount;

                tcs.SetResult(paymentResult);
            });

            return await tcs.Task;
        }
    }

}
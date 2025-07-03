using DemoEcentricPaymentSystems.Models;
using DemoEcentricPaymentSystems.Services;
using Microsoft.AspNetCore.Mvc;

namespace DemoEcentricPaymentSystems.Controllers
{
    public class CheckoutController : Controller
    {
        private static List<Product> products = new()
        {
            new Product { Description = "Apple", Quantity = 15, Price = 350 },
            new Product { Description = "Orange", Quantity = 10, Price = 400 },
            new Product { Description = "Banana", Quantity = 3, Price = 100 },
        };

        public IActionResult Index()
        {
            return View(new CheckoutViewModel
            {
                Products = products,
                Config = new PosBuddyConfig()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            var posService = new PosBuddyService(model.Config);

            bool connected = await posService.ConnectAsync(model.Config.TerminalCode);
            if (!connected)
            {
                model.PaymentStatus = "Failed to connect to PosBuddy device.";
                return View(model);
            }

            bool authenticated = await posService.AuthenticateAsync(model.Config.SecretKey, model.Config.Accesskey);
            if (!authenticated)
            {
                model.PaymentStatus = "Authentication failed.";
                return View(model);
            }

            posService.DisplayItems(model.Products);
            int totalAmount = model.Products.Sum(p => p.Price * p.Quantity);
            var paymentResultModel = await posService.DoSaleAsync(totalAmount);

            return View("~/Views/Checkout/Result.cshtml", paymentResultModel);
        }
    }

}

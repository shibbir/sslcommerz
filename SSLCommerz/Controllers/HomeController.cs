using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static SSLCommerz.SSLCommerz;

namespace SSLCommerz.Controllers
{
    public class HomeController : Controller
    {
        private readonly string storeID = "<your_store_id>";
        private readonly string storePassword = "<your_store_password>";

        private readonly string totalAmount = "10200";

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AjaxCheckout(AjaxRequestModel model)
        {
            var data = JsonConvert.DeserializeObject<SSLCommerzInitRequest>(model.cart_json);

            NameValueCollection PostData = new NameValueCollection();

            foreach (var item in GetProperties(data))
            {
                PostData.Add(item.Key, item.Value);
            }

            var sslcz = new SSLCommerz(storeID, storePassword, true);

            var response = sslcz.InitiateTransaction(PostData);

            if (string.IsNullOrEmpty(response.GatewayPageURL))
            {
                return Ok(new
                {
                    status = "fail",
                    logo = response.storeLogo
                });
            }

            return Ok(new
            {
                status = "success",
                data = response.GatewayPageURL,
                logo = response.storeLogo
            });
        }

        public IActionResult HostedCheckout()
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";

            NameValueCollection PostData = new NameValueCollection
            {
                { "total_amount", totalAmount },
                { "currency", "BDT"},
                { "tran_id", GenerateUniqueId() },
                { "success_url", baseUrl + "Home/Success" },
                { "fail_url", baseUrl + "Home/Fail" },
                { "cancel_url", baseUrl + "Home/Cancel" },
                { "cus_name", "John Doe" },
                { "cus_email", "john.doe@mail.co" },
                { "cus_add1", "Address Line On" },
                { "cus_city", "Dhaka" },
                { "cus_postcode", "1219" },
                { "cus_country", "Bangladesh" },
                { "cus_phone", "8801XXXXXXXXX" },
                { "shipping_method", "NO" },
                { "product_name", "UD" },
                { "product_category", "Service" },
                { "product_profile", "general" }
            };

            var sslcz = new SSLCommerz(storeID, storePassword, true);

            var response = sslcz.InitiateTransaction(PostData);

            return Redirect(response.GatewayPageURL);
        }

        [HttpPost]
        public IActionResult Success(SSLCommerzValidatorResponse response)
        {
            var isValidOrder = false;

            if (!string.IsNullOrEmpty(Request.Form["status"]) && Request.Form["status"] == "VALID")
            {
                string transactionId = Request.Form["tran_id"];
                string currency = "BDT";

                SSLCommerz sslcz = new SSLCommerz(storeID, storePassword, true);
                isValidOrder = sslcz.OrderValidate(transactionId, totalAmount, currency, Request);
            }

            if (isValidOrder)
            {
                return View(GetProperties(response));
            }

            return View(GetProperties(response));
        }

        [HttpPost]
        public IActionResult Fail(SSLCommerzValidatorResponse response)
        {
            return View(GetProperties(response));
        }

        [HttpPost]
        public IActionResult Cancel(SSLCommerzValidatorResponse response)
        {
            return View(GetProperties(response));
        }

        private string GenerateUniqueId()
        {
            long i = 1;

            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= (b + 1);
            }

            return string.Format("{0:x}", i - DateTime.Now.Ticks).ToUpper();
        }

        private static Dictionary<string, string> GetProperties(object obj)
        {
            var props = new Dictionary<string, string>();
            if (obj == null)
                return props;

            var type = obj.GetType();
            foreach (var prop in type.GetProperties())
            {
                var val = prop.GetValue(obj, new object[] { });
                var valStr = val == null ? "" : val.ToString();
                props.Add(prop.Name, valStr);
            }

            return props;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using static SSLCommerz.SSLCommerz;

namespace SSLCommerz.Controllers
{
    public class HomeController : Controller
    {
        private readonly string storeID = string.Empty;
        private readonly string storePassword = string.Empty;
        private readonly EnvironmentConfig _configuration;

        private readonly string totalAmount = "10200";

        public HomeController(IOptions<EnvironmentConfig> configuration)
        {
            _configuration = configuration.Value;
            storeID = _configuration.StoreId;
            storePassword = _configuration.StorePassword;
        }

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
                { "success_url", baseUrl + "Home/Callback" },
                { "fail_url", baseUrl + "Home/Callback" },
                { "cancel_url", baseUrl + "Home/Callback" },
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
        public IActionResult Callback(SSLCommerzValidatorResponse response)
        {
            if (!string.IsNullOrEmpty(response.status) && response.status == "VALID")
            {
                SSLCommerz sslcz = new SSLCommerz(storeID, storePassword, true);

                if (sslcz.OrderValidate(response.tran_id, totalAmount, "BDT", Request))
                {
                    return View("Success", GetProperties(response));
                }
            }

            if (!string.IsNullOrEmpty(response.status) && response.status == "FAILED")
            {
                return View("Fail", GetProperties(response));
            }

            if (!string.IsNullOrEmpty(response.status) && response.status == "CANCELLED")
            {
                return View("Cancel", GetProperties(response));
            }

            return View("Error", GetProperties(response));
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

                if(val != null)
                {
                    props.Add(prop.Name, val.ToString());
                }
            }

            return props;
        }
    }
}

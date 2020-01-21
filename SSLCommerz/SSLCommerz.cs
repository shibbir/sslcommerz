﻿using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SSLCommerz
{
    public class SSLCommerz
    {
        protected List<string> key_list;
        protected string generated_hash;
        protected string error;

        protected string Store_ID;
        protected string Store_Pass;
        protected bool Store_Test_Mode;

        protected string SSLCz_URL = "https://securepay.sslcommerz.com/";
        protected string Submit_URL = "gwprocess/v4/api.php";
        protected string Validation_URL = "validator/api/validationserverAPI.php";
        protected string Checking_URL = "validator/api/merchantTransIDvalidationAPI.php";

        public SSLCommerz(string Store_ID, string Store_Pass, bool Store_Test_Mode = false)
        {
            System.Net.ServicePointManager.SecurityProtocol = (SecurityProtocolType)0x00000C00;

            if (Store_ID != "" && Store_Pass != "")
            {
                this.Store_ID = Store_ID;
                this.Store_Pass = Store_Pass;
                SetSSLCzTestMode(Store_Test_Mode);
            }
            else
            {
                throw new Exception("Please provide Store ID and Password to initialize SSLCommerz");
            }
        }

        public SSLCommerzInitResponse InitiateTransaction(NameValueCollection PostData, bool GetGateWayList = false)
        {
            PostData.Add("store_id", Store_ID);
            PostData.Add("store_passwd", Store_Pass);

            string response = SendPost(PostData);

            try
            {
                SSLCommerzInitResponse resp = JsonConvert.DeserializeObject<SSLCommerzInitResponse>(response);

                if (resp.status == "SUCCESS")
                {
                    return resp;
                }
                else
                {
                    throw new Exception("Unable to get data from SSLCommerz. Please contact your manager!");
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message.ToString());
            }
        }

        public bool OrderValidate(string MerchantTrxID, string MerchantTrxAmount, string MerchantTrxCurrency, HttpRequest req)
        {
            bool hash_verified = this.ipn_hash_verify(req);
            if (hash_verified)
            {
                string json = string.Empty;

                string EncodedValID = System.Web.HttpUtility.UrlEncode(req.Form["val_id"]);
                string EncodedStoreID = System.Web.HttpUtility.UrlEncode(Store_ID);
                string EncodedStorePassword = System.Web.HttpUtility.UrlEncode(Store_Pass);

                string validate_url = SSLCz_URL + Validation_URL + "?val_id=" + EncodedValID + "&store_id=" + EncodedStoreID + "&store_passwd=" + EncodedStorePassword + "&v=1&format=json";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(validate_url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream resStream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(resStream))
                {
                    json = reader.ReadToEnd();
                }
                if (json != "")
                {
                    SSLCommerzValidatorResponse resp = JsonConvert.DeserializeObject<SSLCommerzValidatorResponse>(json);

                    if (resp.status == "VALID" || resp.status == "VALIDATED")
                    {
                        if (MerchantTrxCurrency == "BDT")
                        {
                            if (MerchantTrxID == resp.tran_id && (Math.Abs(Convert.ToDecimal(MerchantTrxAmount) - Convert.ToDecimal(resp.amount)) < 1) && MerchantTrxCurrency == "BDT")
                            {
                                return true;
                            }
                            else
                            {
                                error = "Amount not matching";
                                return false;
                            }
                        }
                        else
                        {
                            if (MerchantTrxID == resp.tran_id && (Math.Abs(Convert.ToDecimal(MerchantTrxAmount) - Convert.ToDecimal(resp.currency_amount)) < 1) && MerchantTrxCurrency == resp.currency_type)
                            {
                                return true;
                            }
                            else
                            {
                                this.error = "Currency Amount not matching";
                                return false;
                            }

                        }
                    }
                    else
                    {
                        this.error = "This transaction is either expired or fails";
                        return false;
                    }
                }
                else
                {
                    this.error = "Unable to get Transaction JSON status";
                    return false;
                }
            }
            else
            {
                this.error = "Unable to verify hash";
                return false;
            }
        }

        protected void SetSSLCzTestMode(bool mode)
        {
            Store_Test_Mode = mode;

            if (mode)
            {
                SSLCz_URL = "https://sandbox.sslcommerz.com/";
            }
        }

        protected string SendPost(NameValueCollection PostData)
        {
            Console.WriteLine(this.SSLCz_URL + this.Submit_URL);
            string response = Post(this.SSLCz_URL + this.Submit_URL, PostData);
            return response;
        }

        public static string Post(string uri, NameValueCollection PostData)
        {
            byte[] response = null;
            using (WebClient client = new WebClient())
            {
                response = client.UploadValues(uri, PostData);
            }
            return System.Text.Encoding.UTF8.GetString(response);
        }

        public bool ipn_hash_verify(HttpRequest req)
        {
            // Check For verify_sign and verify_key parameters
            if (req.Form["verify_sign"] != "" && req.Form["verify_key"] != "")
            {
                // Get the verify key
                string verify_key = req.Form["verify_key"];
                if (verify_key != "")
                {

                    // Split key string by comma to make a list array
                    key_list = verify_key.Split(',').ToList<string>();

                    // Initiate a key value pair list array
                    List<KeyValuePair<string, string>> data_array = new List<KeyValuePair<string, string>>();

                    // Store key and value of post in a list
                    foreach (string k in key_list)
                    {
                        data_array.Add(new KeyValuePair<string, string>(k, req.Form[k]));
                    }

                    // Store Hashed Password in list
                    string hashed_pass = MD5(Store_Pass);
                    data_array.Add(new KeyValuePair<string, string>("store_passwd", hashed_pass));

                    // Sort Array
                    data_array.Sort(
                        delegate (KeyValuePair<string, string> pair1,
                        KeyValuePair<string, string> pair2)
                        {
                            return pair1.Key.CompareTo(pair2.Key);
                        }
                    );

                    // Concat and make String from array
                    string hash_string = "";
                    foreach (var kv in data_array)
                    {
                        hash_string += kv.Key + "=" + kv.Value + "&";
                    }
                    // Trim & from end of this string
                    hash_string = hash_string.TrimEnd('&');

                    // Make hash by hash_string and store
                    generated_hash = this.MD5(hash_string);

                    // Check if generated hash and verify_sign match or not
                    if (generated_hash == req.Form["verify_sign"])
                    {
                        return true; // Matched
                    }
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        public string MD5(string s)
        {
            byte[] asciiBytes = ASCIIEncoding.ASCII.GetBytes(s);
            byte[] hashedBytes = System.Security.Cryptography.MD5CryptoServiceProvider.Create().ComputeHash(asciiBytes);
            string hashedString = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            return hashedString;
        }

        public class Gw
        {
            public string visa { get; set; }
            public string master { get; set; }
            public string amex { get; set; }
            public string othercards { get; set; }
            public string internetbanking { get; set; }
            public string mobilebanking { get; set; }
        }

        public class Desc
        {
            public string name { get; set; }
            public string type { get; set; }
            public string logo { get; set; }
            public string gw { get; set; }
            public string redirectGatewayURL { get; set; }
        }

        public class SSLCommerzInitResponse
        {
            public string status { get; set; }
            public string failedreason { get; set; }
            public string sessionkey { get; set; }
            public Gw gw { get; set; }
            public string GatewayPageURL { get; set; }
            public string storeBanner { get; set; }
            public string storeLogo { get; set; }
            public List<Desc> desc { get; set; }
        }

        public class SSLCommerzValidatorResponse
        {
            public string status { get; set; }
            public string tran_date { get; set; }
            public string tran_id { get; set; }
            public string val_id { get; set; }
            public string amount { get; set; }
            public string store_amount { get; set; }
            public string currency { get; set; }
            public string bank_tran_id { get; set; }
            public string card_type { get; set; }
            public string card_no { get; set; }
            public string card_issuer { get; set; }
            public string card_brand { get; set; }
            public string card_issuer_country { get; set; }
            public string card_issuer_country_code { get; set; }
            public string currency_type { get; set; }
            public string currency_amount { get; set; }
            public string currency_rate { get; set; }
            public string base_fair { get; set; }
            public string value_a { get; set; }
            public string value_b { get; set; }
            public string value_c { get; set; }
            public string value_d { get; set; }
            public string emi_instalment { get; set; }
            public string emi_amount { get; set; }
            public string emi_description { get; set; }
            public string emi_issuer { get; set; }
            public string risk_title { get; set; }
            public string risk_level { get; set; }
            public string APIConnect { get; set; }
            public string validated_on { get; set; }
            public string gw_version { get; set; }
            public string discount_amount { get; set; }
            public string discount_percentage { get; set; }
            public string discount_remarks { get; set; }
        }

        public class SSLCommerzInitRequest
        {
            public string total_amount { get; set; }
            public string currency { get; set; }
            public string tran_id { get; set; }
            public string success_url { get; set; }
            public string fail_url { get; set; }
            public string cancel_url { get; set; }
            public string cus_name { get; set; }
            public string cus_email { get; set; }
            public string cus_add1 { get; set; }
            public string cus_city { get; set; }
            public string cus_postcode { get; set; }
            public string cus_country { get; set; }
            public string cus_phone { get; set; }
            public string shipping_method { get; set; }
            public string product_name { get; set; }
            public string product_category { get; set; }
            public string product_profile { get; set; }
        }

        public class AjaxRequestModel
        {
            public string order { get; set; }
            public string cart_json { get; set; }
        }
    }
}

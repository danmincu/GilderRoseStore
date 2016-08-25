using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Script.Serialization;
using System.Globalization;
using System.Net.Http.Headers;
using GilderRoseStore.Models;
using System.Linq;

namespace GilderRoseStore.Tests.Integration
{
    [TestClass]
    public class IntegrationTests
    {
        const int port = 19683;
        const string userName = "test@test.com";
        const string password = "GilderRose1@";
        
        private Token GetToken(string userName, string password)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(String.Format(CultureInfo.InvariantCulture, "http://localhost:{0}/Token", port));
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", userName),
                new KeyValuePair<string, string>("password", password)
            });

            var response = client.PostAsync(uri.ToString(), formContent).Result;
            return new JavaScriptSerializer().Deserialize<Token>(response.Content.ReadAsStringAsync().Result);
        }

        private string InsureCreateUser()
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(String.Format(CultureInfo.InvariantCulture, "http://localhost:{0}/api/Account/Register", port));

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Email", userName),
                new KeyValuePair<string, string>("Password", password),
                new KeyValuePair<string, string>("ConfirmPassword", password) });
            var response = client.PostAsync(uri.ToString(), formContent).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        [TestMethod]
        public void GetValuesWithNoAuthTest()
        {
            var result = GetInventory(null);
            Assert.AreEqual(result.Item2, System.Net.HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public void GetInventoryTest()
        {
            Assert.IsNotNull(InsureCreateUser());
            var token = GetToken(userName, password);
            Assert.IsNotNull(token);
            Assert.IsFalse(string.IsNullOrEmpty(token.access_token));
            var result = GetInventory(token);
            Assert.IsFalse(string.IsNullOrEmpty(result.Item1));
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
        }

        [TestMethod]
        public void BuyAnItemUntilEmptyStockTest()
        {
            Assert.IsNotNull(InsureCreateUser());
            var token = GetToken(userName, password);
            Assert.IsNotNull(token);
            Assert.IsFalse(string.IsNullOrEmpty(token.access_token));
            var result = GetInventory(token);
            Assert.IsFalse(string.IsNullOrEmpty(result.Item1));
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
            var firstItemId = items.First().Id;
            result = BuyItem(token, firstItemId);
            Assert.IsNotNull(result);
        }



        static Tuple<string, System.Net.HttpStatusCode> GetInventory(Token token)
        {

            var client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(String.Format(CultureInfo.InvariantCulture, "http://localhost:{0}/api/store", port)),
                Method = HttpMethod.Get,
            };
            if (token != null && !string.IsNullOrEmpty(token.access_token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
            var task = client.SendAsync(request)
                .ContinueWith((taskwithmsg) =>
                {
                    var response = taskwithmsg.Result;
                    return new Tuple<string, System.Net.HttpStatusCode>(response.Content.ReadAsStringAsync().Result, response.StatusCode);
                });
            return task.Result;
        }


        static Tuple<string, System.Net.HttpStatusCode> BuyItem(Token token, Guid id)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(String.Format(CultureInfo.InvariantCulture, "http://localhost:{0}/api/store/{1}", port, id)),
                Method = HttpMethod.Get,
            };
            if (token != null && !string.IsNullOrEmpty(token.access_token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
            var task = client.SendAsync(request)
                .ContinueWith((taskwithmsg) =>
                {
                    var response = taskwithmsg.Result;
                    return new Tuple<string, System.Net.HttpStatusCode>(response.Content.ReadAsStringAsync().Result, response.StatusCode);
                });
            return task.Result;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using GilderRoseStore.Models;
using GilderRoseStore.Providers;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GilderRoseStore.Tests.Integration
{

    //SELF HOSTED INTEGRATION TESTS
    //THEY ONLY RUN WHEN LOGGED IN AS ADMINISTRATOR

    [TestClass]
    public class Self_Hosted_IntegrationTests
    {
        const int port = 9113;
        const string userName = "test@test.com";
        const string password = "GilderRose1@";

        private static IDisposable _webApp;

        [AssemblyInitialize]
        public static void SetUp(TestContext context)
        {
            _webApp = WebApp.Start<Startup>(String.Format(CultureInfo.InvariantCulture,"http://*:{0}/",port));
           
        }

        [AssemblyCleanup]
        public static void TearDown()
        {
            _webApp.Dispose();
        }

        [TestInitialize]
        public void SetupTest()
        {
            InsureCreateUser();
        }

        [TestMethod]
        public void Test_BadPassword()
        {          
            using (var httpClient = new HttpClient())
            {
                var token = GetToken(userName, password + "blah", port);
                Assert.IsNull(token.access_token);
            }
        }

        [TestMethod]
        public void Test_GetInventory()
        {
            var token = GetToken(userName, password, port);
            var result = GetInventory(token);
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());          
        }

        [TestMethod]
        public void Test_GetInventoryWithNoAuth()
        {         
            var result = GetInventory(null);
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
        }

        [TestMethod]
        public void Test_BuyWithNoAuth()
        {
            var result = GetInventory(null);
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
            var firstItemWithStock = items.FirstOrDefault(itm => itm.Quantity > 0);
            Assert.IsNotNull(firstItemWithStock);
            var resultPurchaseAttempt = BuyItem(firstItemWithStock.Id, null);
            Assert.IsNotNull(resultPurchaseAttempt);
            Assert.AreEqual(resultPurchaseAttempt.Item2, HttpStatusCode.Unauthorized);
        }


        [TestMethod]
        public void Test_BuyItemToDepleteStock()
        {
            var result = GetInventory(null);
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
            var firstItemWithLargeStock = items.OrderByDescending(itm => itm.Quantity).FirstOrDefault(itm => itm.Quantity > 0 && itm.Quantity < 20);
            Assert.IsNotNull(firstItemWithLargeStock);
            var token = GetToken(userName, password, port);
            Assert.IsNotNull(token.access_token);
            for (int i = 0; i < firstItemWithLargeStock.Quantity; i++)
            {
                var resultPurchaseAttempt = BuyItem(firstItemWithLargeStock.Id, token);
                Assert.IsNotNull(resultPurchaseAttempt);
                Assert.AreEqual(resultPurchaseAttempt.Item2, HttpStatusCode.OK);
                Assert.IsTrue(resultPurchaseAttempt.Item1.Equals("true", StringComparison.OrdinalIgnoreCase));
            }
            var depletedStockPurchaseAttempt = BuyItem(firstItemWithLargeStock.Id, token);
            Assert.IsNotNull(depletedStockPurchaseAttempt);
            Assert.AreEqual(depletedStockPurchaseAttempt.Item2, HttpStatusCode.OK);
            Assert.IsTrue(depletedStockPurchaseAttempt.Item1.Equals("false", StringComparison.OrdinalIgnoreCase));
        }

        private Tuple<string, System.Net.HttpStatusCode> BuyItem(Guid itemId, Token token)
        {
            return ClientApiGetCall(token, "http://localhost:{0}/api/store/{1}", port, itemId);
        }
        
        private Tuple<string, System.Net.HttpStatusCode> GetInventory(Token token)
        {           
            return ClientApiGetCall(token, "http://localhost:{0}/api/store", port);
        }
        
        private Tuple<string, System.Net.HttpStatusCode> ClientApiGetCall(Token token, string uri, params object[] args)
        {
            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(String.Format(CultureInfo.InvariantCulture, uri, args)),
                    Method = HttpMethod.Get,
                };
                if (token != null && !string.IsNullOrEmpty(token.access_token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
                var task = httpClient.SendAsync(request)
                .ContinueWith((taskwithmsg) =>
                {
                    var response = taskwithmsg.Result;
                    return new Tuple<string, System.Net.HttpStatusCode>(response.Content.ReadAsStringAsync().Result, response.StatusCode);
                });
                return task.Result;
            }
        }

        private Token GetToken(string userName, string password, int port)
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


        private void InsureCreateUser()
        {
            var createUserResponse = CreateUser();
            Assert.IsTrue(createUserResponse.Item2 == HttpStatusCode.OK
                //the contains "already taken" is not necessarily a good test as the server response might change
                || (createUserResponse.Item2 == HttpStatusCode.BadRequest && createUserResponse.Item1.Contains("already taken")));
        }

        private Tuple<string, System.Net.HttpStatusCode> CreateUser()
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(String.Format(CultureInfo.InvariantCulture, "http://localhost:{0}/api/Account/Register", port));

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Email", userName),
                new KeyValuePair<string, string>("Password", password),
                new KeyValuePair<string, string>("ConfirmPassword", password) });
            var response = client.PostAsync(uri.ToString(), formContent).Result;
            return new Tuple<string, System.Net.HttpStatusCode>(response.Content.ReadAsStringAsync().Result, response.StatusCode);
        }
    }
}

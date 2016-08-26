using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Script.Serialization;
using System.Globalization;
using System.Net.Http.Headers;
using GilderRoseStore.Models;
using System.Linq;
using Owin;
using System.Web.Http;
using Microsoft.Owin.Testing;
using System.Web.Http.Dispatcher;
using Microsoft.Owin;
using System.Threading.Tasks;
using System.Diagnostics;

namespace GilderRoseStore.Tests.Integration
{

    //AGAINST A REAL HOST INTEGRATION TESTS
    //MAKE SURE YOU RUN WITH NO DEBUG (CTRL + F5) THE GilderRoseStore WEB APP BEFORE RUNNING THESE TESTS
    
    //Potential problem : running over and over agains the same host it will deplete item inventory and buy item tests will eventually fail
    //adding an option revert database is the solution however it exceeds the purpose of this exercise

    [TestClass]
    public class AgainstHostIntegrationTests
    {
        const int port = 19683;
        const string userName = "test@test.com";
        const string password = "GilderRose1@";
        Token token;
     
        [TestInitialize]
        public void SetupTest()
        {
            InsureCreateUser();
            this.token = GetToken(userName, password);
        }

        [TestMethod]
        public void TestGetValuesWithNoAuth()
        {
            //no auth
            var result = GetInventory(null);
            Assert.IsFalse(string.IsNullOrEmpty(result.Item1));
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
        }

        [TestMethod]
        public void TestBuyItemWithNoAuth()
        {
            //no auth
            var result = GetInventory(null);
            Assert.IsFalse(string.IsNullOrEmpty(result.Item1));
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
            var firstItem = items.OrderBy(itm => itm.Quantity).First();
            result = BuyItem(null, firstItem.Id);            
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Item2, System.Net.HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public void TestGetInventoryAuth()
        {
            Assert.IsNotNull(token);
            Assert.IsFalse(string.IsNullOrEmpty(token.access_token));
            var result = GetInventory(token);
            Assert.IsFalse(string.IsNullOrEmpty(result.Item1));
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
        }

        [TestMethod]
        public void TestBuyAnItemUntilTheStockIsEmpty()
        {
            Assert.IsNotNull(token);
            Assert.IsFalse(string.IsNullOrEmpty(token.access_token));
            var result = GetInventory(token);
            Assert.IsFalse(string.IsNullOrEmpty(result.Item1));
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
            var firstItem = items.OrderBy(itm => itm.Quantity).First();
            //buy all the stock
            for (int i = 0; i < firstItem.Quantity; i++)
            {
                result = BuyItem(token, firstItem.Id);
                Assert.IsTrue(result.Item1.Equals("true", StringComparison.OrdinalIgnoreCase));
            }
            result = BuyItem(token, firstItem.Id);
            //assert that you cannot buy this item
            Assert.IsTrue(result.Item1.Equals("false", StringComparison.OrdinalIgnoreCase));
        }


        private Tuple<string, System.Net.HttpStatusCode> GetInventory(Token token)
        {  
            return ClientApiGet(token, "http://localhost:{0}/api/store", port);
        }

        private Tuple<string, System.Net.HttpStatusCode> BuyItem(Token token, Guid id)
        {            
            return ClientApiGet(token, "http://localhost:{0}/api/store/{1}", port, id);
        }

        private Tuple<string, System.Net.HttpStatusCode> ClientApiGet(Token token, string uri, params object[] args)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(String.Format(CultureInfo.InvariantCulture, uri, args)),
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


    }
}

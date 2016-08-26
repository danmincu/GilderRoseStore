using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using GilderRoseStore.Models;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GilderRoseStore.Tests.Integration
{

    // SELF HOSTED INTEGRATION TESTS
    // THEY ONLY RUN WHEN LOGGED IN AS ADMINISTRATOR - Please run Visual Studio as Admin than attempt to run these tests

    [TestClass]
    public class Self_Hosted_IntegrationTests
    {
        const int port = 9113;
        const string userName = "test_self_hosted@test.com";
        const string password = "GilderRose1@";
        Token token;

        private static IDisposable webApp;

        [AssemblyInitialize]
        public static void SetUp(TestContext context)
        {
            //spins the self hosted Web app
            webApp = WebApp.Start<Startup>(String.Format(CultureInfo.InvariantCulture, "http://*:{0}/", port));

        }

        [AssemblyCleanup]
        public static void TearDown()
        {
            webApp.Dispose();
        }

        [TestInitialize]
        public void SetupTest()
        {
            //Arrange
            InsureCreateUser();
            this.token = GetToken(userName, password, port);
        }

        [TestMethod]
        [IntegrationTest]
        public void Test_BadPassword()
        {
            //Act
            var token_for_bad_password = GetToken(userName, password + "blah", port);
            //Assert
            Assert.IsNull(token_for_bad_password.access_token);
        }

        [TestMethod]
        [IntegrationTest]
        public void Test_GetInventory()
        {
            //Act
            var result = GetInventory(token);
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            //Assert that you get inventory items
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
        }

        [TestMethod]
        [IntegrationTest]
        public void Test_GetInventory_With_NoAuth()
        {
            //Act
            var result = GetInventory(null);
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            //Assert that you get inventory items
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
        }

        [TestMethod]
        [IntegrationTest]
        public void Test_Buy_Item_WithNoAuth()
        {
            //Arrange
            var result = GetInventory(null);
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
            var firstItemWithStock = items.FirstOrDefault(itm => itm.Quantity > 0);
            Assert.IsNotNull(firstItemWithStock);
            //Act
            var resultPurchaseAttempt = BuyItem(firstItemWithStock.Id, null);
            //Assert that the purchase failed because the lack of Authentication
            Assert.IsNotNull(resultPurchaseAttempt);
            Assert.AreEqual(resultPurchaseAttempt.Item2, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        [IntegrationTest]
        public void Test_Buy_Item_Until_Deplete_The_Stock()
        {
            var result = GetInventory(null);
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
            var firstItemWithLargeStock = items.OrderByDescending(itm => itm.Quantity).FirstOrDefault(itm => itm.Quantity > 0 && itm.Quantity < 20);
            Assert.IsNotNull(firstItemWithLargeStock);
            Assert.IsNotNull(token.access_token);
            //Act
            for (int i = 0; i < firstItemWithLargeStock.Quantity; i++)
            {
                var resultPurchaseAttempt = BuyItem(firstItemWithLargeStock.Id, token);
                //assert that the purchase succeeded 
                Assert.IsNotNull(resultPurchaseAttempt);
                Assert.AreEqual(resultPurchaseAttempt.Item2, HttpStatusCode.OK);
                Assert.IsTrue(resultPurchaseAttempt.Item1.Equals("true", StringComparison.OrdinalIgnoreCase));
            }
            //Act
            var depletedStockPurchaseAttempt = BuyItem(firstItemWithLargeStock.Id, token);
            //Assert that the last buy attempt failed due to stock depletion
            Assert.IsNotNull(depletedStockPurchaseAttempt);
            Assert.AreEqual(depletedStockPurchaseAttempt.Item2, HttpStatusCode.OK);
            Assert.IsTrue(depletedStockPurchaseAttempt.Item1.Equals("false", StringComparison.OrdinalIgnoreCase));
        }

        //using these "odd" tuples here rather than a poco because is all private method implementation - no-one outside this class cares
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

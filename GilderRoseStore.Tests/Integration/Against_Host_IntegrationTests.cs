using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Script.Serialization;
using System.Globalization;
using System.Net.Http.Headers;
using GilderRoseStore.Models;
using System.Linq;
using System.Net;

namespace GilderRoseStore.Tests.Integration
{

    //AGAINST A REAL HOST INTEGRATION TESTS
    //MAKE SURE YOU RUN WITH NO DEBUG (CTRL + F5) THE GilderRoseStore WEB APP BEFORE RUNNING THESE TESTS

    //Potential problem : running over and over agains the same host it will deplete item inventory and buy item tests will eventually fail
    //adding an option revert database is the solution however it exceeds the purpose of this exercise

    [TestClass]
    public class Against_Host_IntegrationTests
    {
        const int port = 19683;
        const string userName = "test_host@test.com";
        const string password = "GilderRose1@";
        Token token;

        [TestInitialize]
        public void SetupTest()
        {
            // Arrange            
            //add the user unless already in the database
            InsureCreateUser(userName, password);
            //obtain the token for the entire session
            this.token = GetToken(userName, password);
        }

        [TestMethod]
        public void Test_GetInventory_With_NoAuth()
        {
            //Act
            var result = GetInventory(null);//no auth
            //Assert
            Assert.IsFalse(string.IsNullOrEmpty(result.Item1));
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
        }

        [TestMethod]
        public void Test_Attempt_BuyItem_With_NoAuth()
        {
            //Act
            var result = GetInventory(null);//no auth
            //Assert
            Assert.IsFalse(string.IsNullOrEmpty(result.Item1));
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
            var firstItem = items.OrderBy(itm => itm.Quantity).First();
            //Act
            result = BuyItem(null, firstItem.Id);
            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Item2, System.Net.HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public void Test_GetInventory_With_Auth()
        {
            //Arrange
            Assert.IsNotNull(token);
            Assert.IsFalse(string.IsNullOrEmpty(token.access_token));
            //Act
            var result = GetInventory(token);
            //Assert
            Assert.IsFalse(string.IsNullOrEmpty(result.Item1));
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
        }

        [TestMethod]
        public void Test_Buy_An_Item_Until_The_Stock_Is_Depleted()
        {
            //Arrange
            Assert.IsNotNull(token);
            Assert.IsFalse(string.IsNullOrEmpty(token.access_token));
            var result = GetInventory(token);
            Assert.IsFalse(string.IsNullOrEmpty(result.Item1));
            var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Any());
            var firstItem = items.OrderBy(itm => itm.Quantity).First();
            //Act
            //buy all the stock
            for (int i = 0; i < firstItem.Quantity; i++)
            {
                result = BuyItem(token, firstItem.Id);
                //Assert that the item was bought
                Assert.IsTrue(result.Item1.Equals("true", StringComparison.OrdinalIgnoreCase));
            }
            result = BuyItem(token, firstItem.Id);
            //Assert that you cannot buy this item
            Assert.IsTrue(result.Item1.Equals("false", StringComparison.OrdinalIgnoreCase));
        }

        //using these "odd" tuples here rather than a poco because is all private method implementation - no-one outside this class cares
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

        private void InsureCreateUser(string userName, string password)
        {
            var createUserResponse = CreateUser(userName, password);
            Assert.IsTrue(createUserResponse.Item2 == HttpStatusCode.OK
                //the contains "already taken" is not necessarily a good test as the server response might change
                || (createUserResponse.Item2 == HttpStatusCode.BadRequest && createUserResponse.Item1.Contains("already taken")));
        }

        private Tuple<string, System.Net.HttpStatusCode> CreateUser(string userName, string password)
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

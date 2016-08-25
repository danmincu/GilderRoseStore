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
    public class SelfHostedIntegrationTests
    {

        const int port = 9443;
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

        [TestMethod]
        public void Test_BadPassword()
        {
            var insureCreateUserResponse = InsureCreateUser();
            Assert.IsTrue(insureCreateUserResponse.Item2 == HttpStatusCode.OK
                || (insureCreateUserResponse.Item2 == HttpStatusCode.BadRequest && insureCreateUserResponse.Item1.Contains("already taken")));
            using (var httpClient = new HttpClient())
            {
                var token = GetToken(userName, password + "blah", port);
                Assert.IsNull(token.access_token);
            }

        }

        [TestMethod]
        public void Test_GetInventory()
        {
            var insureCreateUserResponse = InsureCreateUser();
            Assert.IsTrue(insureCreateUserResponse.Item2 == HttpStatusCode.OK
                || (insureCreateUserResponse.Item2 == HttpStatusCode.BadRequest && insureCreateUserResponse.Item1.Contains("already taken")));
            using (var httpClient = new HttpClient())
            {
                var token = GetToken(userName, password, port);

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(String.Format(CultureInfo.InvariantCulture, "http://localhost:{0}/api/store", port)),
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
                var result = task.Result;

                var items = new JavaScriptSerializer().Deserialize<IEnumerable<Item>>(result.Item1);
                Assert.IsNotNull(items);
                Assert.IsTrue(items.Any());
            }
        }

        [TestMethod]
        public void Test_GetInventoryWithNoAuth()
        {
            //setup a call with no auth token
            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(String.Format(CultureInfo.InvariantCulture, "http://localhost:{0}/api/store", port)),
                    Method = HttpMethod.Get,
                };
             
                var task = httpClient.SendAsync(request)
                .ContinueWith((taskwithmsg) =>
                {
                    var response = taskwithmsg.Result;
                    return new Tuple<string, System.Net.HttpStatusCode>(response.Content.ReadAsStringAsync().Result, response.StatusCode);
                });
                var result = task.Result;
                //asert that the call received and Unauthorized status code
                Assert.AreEqual(result.Item2, System.Net.HttpStatusCode.Unauthorized);
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



        private Tuple<string, System.Net.HttpStatusCode> InsureCreateUser()
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

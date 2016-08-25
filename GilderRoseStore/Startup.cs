using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using GilderRoseStore.Controllers;
using GilderRoseStore.Models;
using GilderRoseStore.Providers;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security.OAuth;

[assembly: OwinStartup(typeof(GilderRoseStore.Startup))]

namespace GilderRoseStore
{
    public partial class Startup
    {
        public void Hosted_in_IIS_Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        //public void Configuration1(IAppBuilder builder)
        //{
        //    HttpConfiguration config = new HttpConfiguration();
        //    //config.Routes.MapHttpRoute("Default", "{controller}/{customerID}", new { controller = "Customer", customerID = RouteParameter.Optional });

        //    config.Routes.MapHttpRoute(
        //       name: "DefaultApi",
        //       routeTemplate: "api/{controller}/{id}",
        //       defaults: new { id = RouteParameter.Optional });


        //   builder.UseWebApi(config);
        //}

        //modified for self host and ability to integration test
        public void Configuration(IAppBuilder app)
        {


            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            
            //var oauthProvider = new OAuthAuthorizationServerProvider
            //{
            //    OnGrantResourceOwnerCredentials = async context =>
            //    {
            //        if (context.UserName == userName && context.Password == password)
            //        {
            //            var claimsIdentity = new ClaimsIdentity(context.Options.AuthenticationType);
            //            claimsIdentity.AddClaim(new Claim("user", context.UserName));
            //            context.Validated(claimsIdentity);
            //            return;
            //        }
            //        context.Rejected();
            //    },
            //    OnValidateClientAuthentication = async context =>
            //    {
            //        context.Validated();

            //        //string clientId;
            //        //string clientSecret;
            //        //if (context.TryGetBasicCredentials(out clientId, out clientSecret))
            //        //{
            //        //    if (clientId == "xyz" && clientSecret == "secretKey")
            //        //    {
            //        //        context.Validated();
            //        //    }
            //        //}
            //    }
            //};
            Startup.PublicClientId = "self";
            var oauthProvider = new ApplicationOAuthProvider(Startup.PublicClientId);

            var oauthOptions = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/Token"),
                Provider = oauthProvider,
                AuthorizationCodeExpireTimeSpan = TimeSpan.FromMinutes(10),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
                SystemClock = new SystemClock()

            };
            app.UseOAuthAuthorizationServer(oauthOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            var config = new HttpConfiguration();


            var builder = new ContainerBuilder();
            //    // Get your HttpConfiguration.
            //   var config1 = GlobalConfiguration.Configuration;
            builder.RegisterInstance(new InMemoryInventory()).As<IInventory>();
            builder.RegisterType<StoreController>();

            //    //Register all Web API controllers.
            //    //builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            //register individual controlllers manually.
            builder.RegisterType<StoreController>().InstancePerRequest();

            //    // OPTIONAL: Register the Autofac filter provider.
            builder.RegisterWebApiFilterProvider(config);
            //    // Set the dependency resolver to be Autofac.
            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);


            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
             name: "DefaultApi",
             routeTemplate: "api/{controller}/{id}",
             defaults: new { id = RouteParameter.Optional });

            app.UseWebApi(config);
        }


    }
}

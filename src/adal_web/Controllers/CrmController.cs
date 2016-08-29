using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using adal_web.Entities;
using adal_web.Helpers;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace adal_web.Controllers
{
    [Authorize]
    public class CrmController : Controller
    {
        public CrmController()
        {

        }
        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            AuthenticationResult result = null;
            string resourceId = "https://crmtechx.crm4.dynamics.com";
            try
            {

                string userObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
                string resourceUrl = "https://crmtechx.crm4.dynamics.com";
                //AuthenticationContext authContext = new AuthenticationContext(Startup.Authority, new NaiveSessionCache(userObjectID, HttpContext.Session));
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority,false, new NaiveSessionCache(userObjectID, HttpContext.Session));
                
                ClientCredential credential = new ClientCredential(Startup.ClientId, Startup.ClientSecret);

                

                result = await authContext.AcquireTokenSilentAsync(resourceUrl, Startup.ClientId);
                //result = await authContext.AcquireTokenSilentAsync("https://crmtechx.crm4.dynamics.com", credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
                
                //UserAssertion userAssertion = new UserAssertion(result.AccessToken, "urn:ietf:params:oauth:grant-type:jwt-bearer", "reece.campbell@crmtechx.onmicrosoft.com");
                // https://localhost:44316/signin-oidc
                
                using (HttpClient httpClient = new HttpClient())
                {
                    // Define the Web API address of the service and the period of time each request has to execute.
                    httpClient.BaseAddress = new Uri("https://crmtechx.crm4.dynamics.com");
                    httpClient.Timeout = new TimeSpan(0, 2, 0);  // 2 minutes
                    httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                    httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");

                    // Set the type of payload that will be accepted.
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Create an in-memory account using the early-bound Account class.

                    Account account = new Account();
                    account.name = "Contoso2";
                    account.telephone1 = "555-5555";

                    // It is a best practice to refresh the access token before every message request is sent. Doing so
                    // avoids having to check the expiration date/time of the token. This operation is quick.
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

                    // Send the request, and then check the response for success.
                    // POST api/data/accounts
                    HttpResponseMessage response =
                        await HttpClientExtensions.SendAsJsonAsync<Account>(httpClient, HttpMethod.Post, "api/data/v8.1/accounts", account);
                }
            }
            catch (Exception ee)
            {
                if (HttpContext.Request.Query["reauth"] == "True")
                {
                    //
                    // Send an OpenID Connect sign-in request to get a new set of tokens.
                    // If the user still has a valid session with Azure AD, they will not be prompted for their credentials.
                    // The OpenID Connect middleware will return to this controller after the sign-in response has been handled.
                    //
                    return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme);
                }
                
                throw;
            }

            return View();
        }
    }
}

using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndecopiVirtualAsistant.Services
{
    public class ExpedientRequestService
    {
        public async Task<Dictionary<string,string>> SearchExpedientState(string name, string document, string expedient)
        {
            try 
            {
                var options = new RestClientOptions("https://functionexpedientsearch.azurewebsites.net/api/functionexpedientsearch")
                {
                    ThrowOnAnyError = true,
                    Timeout = 60000
                };
                var client = new RestClient(options);
                var request = new RestRequest();
                request.AddHeader("Accept", "application/json");
                request.AddQueryParameter("name", name);
                request.AddQueryParameter("docNumber", document);
                request.AddQueryParameter("expNumber", expedient);
                var response = await client.GetAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK) { 
                    Dictionary<string, string> dResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content);
                    return dResponse;
                }

            } catch
            {
            }
            return null;
        }

        public async Task<List<Dictionary<string, string>>> SearchExpedientsByYear(string name, string document, string year)
        {
            try
            {
                var options = new RestClientOptions("https://functionexpedientelistsearch.azurewebsites.net/api/fucntionExpedienteListSearch")
                {
                    ThrowOnAnyError = true,
                    Timeout = 60000
                };
                var client = new RestClient(options);
                var request = new RestRequest();
                // request.AddHeader("Accept", "application/json");
                request.AddQueryParameter("name", name);
                request.AddQueryParameter("docNumber", document);
                request.AddQueryParameter("year", year);
                var response = await client.GetAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = response.Content;
                    List<Dictionary<string, string>> dResponse = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(response.Content);
                    return dResponse;
                }
                /*

                var client = new RestClient("https://functionexpedientelistsearch.azurewebsites.net/api/fucntionExpedienteListSearch?docNumber="+document+"&name=TITO&year="+year+"");
                var request = new RestRequest();
                request.AddHeader("postman-token", "8f7c1695-22c7-0089-a087-4f1d839e3657");
                request.AddHeader("cache-control", "no-cache");
                var response = await client.ExecuteGetAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    
                    var json = response.Content;
                    List<Dictionary<string, string>> dResponse = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(response.Content);
                    return dResponse;
                }
                */

            }
            catch
            {
            }
            return null;
        }
    }
}

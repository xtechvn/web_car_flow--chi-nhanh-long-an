using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;


namespace B2B.Utilities.Common
{
    public class ConnectApi
    {
        public string url_api { get; set; }   
        public string token { get; set; }

        public ConnectApi(string _url_api, string _token)
        {           
            url_api = _url_api; 
            token = _token;
        }

        public async Task<string> CreateHttpRequest()
        {
            try
            {
                string responseFromServer = string.Empty;
                string status = string.Empty;
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("token", token),
                    });

                    var response_api = await httpClient.PostAsync(url_api, content);
                    responseFromServer = response_api.Content.ReadAsStringAsync().Result;                    
                }

                return responseFromServer;
            }
            catch (Exception )
            {
              
                return string.Empty;
            }
        }
    }
}

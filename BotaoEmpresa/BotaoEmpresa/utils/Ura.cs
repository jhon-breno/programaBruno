using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace BotaoEmpresa.utils
{
    public class Ura
    {
        public static string PostWebApiInUraIV(object data, string metodo)
        {
            //Create a WebClient to POST the request
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;

            // Set the header so it knows we are sending JSON
            client.Headers[HttpRequestHeader.ContentType] = "application/json";
            client.Headers.Add("CanalAtendimento", ConfigurationManager.AppSettings["CanalAtendimentoWSUni"]);
            client.Headers.Add("Autenticacao", ConfigurationManager.AppSettings["AutenticacaoWSUni"]);

            // Serialise the data we are sending in to JSON
            string serialisedData = JsonConvert.SerializeObject(data);

            var url = ConfigurationManager.AppSettings["UrlWSUraIV"];

            // Make the request
            string response = client.UploadString(new Uri(url) + metodo, "POST", serialisedData);

            // Deserialise the response into a GUID
            return response;
        }
        public static string GetWebApiWithoutDeserializeObject(string empresa, IDictionary<string, object> data, string metodo)
        {
            // Create a WebClient to GET the request
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;

            // Set the header so it knows we are sending JSON
            client.Headers[HttpRequestHeader.ContentType] = "application/json";
            client.Headers.Add("CanalAtendimento", ConfigurationManager.AppSettings["CanalAtendimentoWSUni"]);
            client.Headers.Add("Autenticacao", ConfigurationManager.AppSettings["AutenticacaoWSUni"]);

            string queryString = "";

            if (data != null)
            {
                // Separate the KeyValuePairs in to a query string
                foreach (var pair in data)
                {
                    if (queryString.Length != 0)
                    {
                        queryString += "&";
                    }

                    queryString += pair.Key + "=" + pair.Value;
                }
            }

            var url = "2003".Equals(empresa) ? ConfigurationManager.AppSettings["UrlWSUraIV"] : ConfigurationManager.AppSettings["UrlWSUnificado"];
            // Make the request
            string response = client.DownloadString(new Uri(url) + metodo + "?empresa=" + empresa + "&" + queryString);

            // Deserialise the response into a GUID
            return response;
        }
    }
}
using ElectronicPatientCard.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ElectronicPatientCard.Services
{
    public class DataSourceService : IDataSourceService
    {
        public IEnumerable<T> GetListData<T>(string path)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create($"http://hapi.fhir.org/baseDstu3/{path}");
            webRequest.UserAgent = "userAgent";

            WebResponse webResp;

            try
            {
                webResp = webRequest.GetResponse();
            }
            catch (WebException ex)
            {
                throw ex;
            }
            var result = new List<T>();

            using (var reader = new StreamReader(webResp.GetResponseStream()))
            {
                string json = reader.ReadToEnd();
                var requestResult = JsonConvert.DeserializeObject<RequestBase>(json);
                foreach (var entry in requestResult.entry)
                {
                    var js = entry.resource.ToString();

                    result.Add(JsonConvert.DeserializeObject<T>(js));
                }

            }

            return result;
        }
    }
}

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
        public T GetData<T>(string path)
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
            T result;

            using (var reader = new StreamReader(webResp.GetResponseStream()))
            {
                string json = reader.ReadToEnd();
                result = JsonConvert.DeserializeObject<T>(json);

            }

            return result;
        }
    }
}

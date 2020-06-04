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
        public const string baseUrl = "http://hapi.fhir.org/baseDstu3";

        public T GetData<T>(string path) where T : new()
        {
            var webRequest = (HttpWebRequest)WebRequest.Create($"{baseUrl}/{path}");
            webRequest.UserAgent = "userAgent";

            WebResponse webResp;

            try
            {
                webResp = webRequest.GetResponse();
            }
            catch (WebException ex)
            {
                return new T();
            }
            T result = default;

            using (var reader = new StreamReader(webResp.GetResponseStream()))
            {
                string json = reader.ReadToEnd();
                result = JsonConvert.DeserializeObject<T>(json);

            }

            return result;
        }

        public IEnumerable<T> GetListData<T>(string path, int initCount)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create($"{baseUrl}/{path}");
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
            Link link = null;
            using (var reader = new StreamReader(webResp.GetResponseStream()))
            {
                string json = reader.ReadToEnd();
                var requestResult = JsonConvert.DeserializeObject<RequestBase>(json);

                foreach (var entry in requestResult.entry)
                {
                    var js = entry.resource.ToString();
                    try
                    {
                        result.Add(JsonConvert.DeserializeObject<T>(js));
                    } catch (Exception) { }
                }

                link = requestResult.link.FirstOrDefault(x => x.relation == "next");
            }
            var count = 0;
            while(link != null)
            {
                var webRequestNext = (HttpWebRequest)WebRequest.Create(link.url);
                webRequestNext.UserAgent = "userAgent";

                WebResponse webRespNext;

                try
                {
                    webRespNext = webRequestNext.GetResponse();
                }
                catch (WebException ex)
                {
                    throw ex;
                }
                link = null;
                using (var reader = new StreamReader(webRespNext.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    var requestResult = JsonConvert.DeserializeObject<RequestBase>(json);

                    foreach (var entry in requestResult.entry)
                    {
                        var js = entry.resource.ToString();
                        try
                        {
                            result.Add(JsonConvert.DeserializeObject<T>(js));
                        }
                        catch (Exception) { }
                    }

                    link = requestResult.link.FirstOrDefault(x => x.relation == "next");
                }
                count += 1;
                if (count >= initCount)
                {
                    break;
                }
            }


            return result;
        }
    }
}

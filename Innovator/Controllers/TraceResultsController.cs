using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Http;
using Innovator.Models;

namespace Innovator.Controllers
{
    public class TraceResultsController : ApiController
    {
        const string CERT_PATH = "C:\\Users\\halb01\\Desktop\\Innovation\\Innovator\\Certificates\\";
        const string CERT_PASS = "Ewk9s3Vt6zH3Q6lI1irw";

        public IHttpActionResult Post(TraceResultsRequestModel traceInfoModel)
        {
            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;

            var cookie = Login();
            var results = Query(cookie, traceInfoModel.TracingId);
            var calls = ParseResults(results);

            return Ok(calls);
        }

        private static Cookie Login()
        {
            string url = "https://kibana.ascendon.tv/api/shield/v1/login";
            string postData = "{\"username\":\"ascendon\",\"password\":\"k1b@n@\"}";
            string referrer = "https://kibana.ascendon.tv/login?next=%2Fapp%2Fkibana%3F";

            var response = PostData(url, postData, referrer);

            var cookie = response.Headers["Set-Cookie"];
            return new Cookie(cookie.Split('=')[0], cookie.Split('=')[1].Substring(0, cookie.Split('=')[1].IndexOf(";")));
        }

        private static HttpWebResponse PostData(string url, string postData, string referrer, Cookie cookie = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            var data = Encoding.ASCII.GetBytes(postData);

            // Create a collection object and populate it using the PFX file
            X509Certificate2Collection collection = new X509Certificate2Collection();
            collection.Import(CERT_PATH + "AscendonTools2016.pfx", CERT_PASS, X509KeyStorageFlags.PersistKeySet);
            collection.Add(X509Certificate.CreateFromCertFile(CERT_PATH + "CDNetworkCa.cer"));
            request.ClientCertificates = collection;

            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";
            request.ContentLength = data.Length;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
            request.Referer = referrer;

            if (cookie != null)
            {
                //if (request.CookieContainer == null)
                //    request.CookieContainer = new CookieContainer();
                //request.CookieContainer.Add(cookie);

                request.Headers.Add("Cookie", string.Format("{0}={1}", cookie.Name, cookie.Value));
            }

            request.Headers.Add("Origin", "https://kibana.ascendon.tv");
            //request.Headers.Add("X-DevTools-Emulate-Network-Conditions-Client-Id", "6F9A30CC-3951-4B3A-B9A8-B0504BEE6024");
            request.Headers.Add("kbn-version", "4.5.1");

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            return (HttpWebResponse)request.GetResponse();
        }

        private static string Query(Cookie cookie, string guid)
        {
            string url = "https://kibana.ascendon.tv/elasticsearch/_msearch?timeout=0&ignore_unavailable=true&preference=1469116518971";
            string postData =
@"{""index"":[""eventlog-trc-2016.07.21""],""ignore_unavailable"":true}
{""size"":500,""sort"":[{""TimeGenerated"":{""order"":""desc"",""unmapped_type"":""boolean""}}],""query"":{""filtered"":{""query"":{""query_string"":{""query"":""\""3ad3f713-8dd9-48ca-a6d3-0bbd66984385\"""",""analyze_wildcard"":true}},""filter"":{""bool"":{""must"":[{""range"":{""TimeGenerated"":{""gte"":1469129155433,""lte"":1469130055433,""format"":""epoch_millis""}}}],""must_not"":[]}}}},""highlight"":{""pre_tags"":[""@kibana-highlighted-field@""],""post_tags"":[""@/kibana-highlighted-field@""],""fields"":{""*"":{}},""require_field_match"":false,""fragment_size"":2147483647},""aggs"":{""2"":{""date_histogram"":{""field"":""TimeGenerated"",""interval"":""30s"",""time_zone"":""America/Chicago"",""min_doc_count"":0,""extended_bounds"":{""min"":1469129155432,""max"":1469130055432}}}},""fields"":[""*"",""_source""],""script_fields"":{},""fielddata_fields"":[""TimeWritten"",""CollectionDateTime"",""TimeGenerated""]}
";

            postData = postData.Replace("2016.07.21", DateTime.Now.ToString("yyyy.MM.dd"));
            postData = postData.Replace("1469130055433", (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds.ToString("F0"));
            postData = postData.Replace("1469129155433", (DateTime.UtcNow.AddHours(-4).Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds.ToString("F0"));
            postData = postData.Replace("3ad3f713-8dd9-48ca-a6d3-0bbd66984385", guid);

            string referrer = "https://kibana.ascendon.tv/app/kibana";

            var response = PostData(url, postData, referrer, cookie);

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            return responseString;
        }

        private static List<APICall> ParseResults(string results)
        {
            var kibanaResults = Newtonsoft.Json.JsonConvert.DeserializeObject<KibanaResults>(results);

            var messages = new List<string>();
            if (kibanaResults.responses == null || kibanaResults.responses.Count <= 0) return new List<APICall>();

            var kibanaHit = kibanaResults.responses.First().hits;
            if (kibanaHit != null && kibanaHit.hits != null && kibanaHit.hits.Count > 0)
            {
                messages.AddRange(kibanaHit.hits.Select(kibanaHitResult => kibanaHitResult._source.Message));
            }

            var apiCalls = new List<APICall>();
            foreach (var message in messages)
            {
                try
                {
                    string method = message.Substring(message.IndexOf("Method: ") + "Method: ".Length);
                    method = method.Substring(0, method.IndexOf("Status:"));

                    string request = message.Substring(message.LastIndexOf("Request:") + "Request:".Length);
                    request = request.Substring(0, request.IndexOf("Response:"));

                    string response = message.Substring(message.LastIndexOf("Response:") + "Response:".Length);

                    apiCalls.Add(new APICall(request, response, method));
                }
                catch (Exception e)
                {

                }
            }

            return apiCalls;
        }

        private static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }

    public class KibanaResults
    {
        public List<KibanaResponse> responses { get; set; }
    }

    public class KibanaResponse
    {
        public KibanaHit hits { get; set; }
    }

    public class KibanaHit
    {
        public List<KibanaHitResult> hits { get; set; }
    }

    public class KibanaHitResult
    {
        public KibanaHitResultSource _source { get; set; }
    }

    public class KibanaHitResultSource
    {
        public string Message { get; set; }
    }

    public class APICall
    {
        public string Request { get; set; }
        public string Response { get; set; }
        public string Method { get; set; }

        public APICall(string request, string response, string method)
        {
            Request = request;
            Response = response;
            Method = method;
        }
    }
}

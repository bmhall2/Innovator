using System.IO;
using System.Net;
using System.Text;
using Innovator.Controllers;
using Newtonsoft.Json;

namespace Innovator.Communication
{
    public interface IServiceRequestor
    {
        SessionModel CreateSession(string userName, string userPassword);
        TraceModel UpdateApiTracing(string apiToTrace, string userId, string sessionId);
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class ServiceRequestor : IServiceRequestor
    {
        public SessionModel CreateSession(string userName, string userPassword)
        {
            var postData = string.Format("{{\"Login\" : \"{0}\", \"Password\" : \"{1}\"}}", userName, userPassword);
            return MakeRequest<SessionModel>("https://services.qa7.cdops.net/Security/CreateSession", postData, null);
        }

        public TraceModel UpdateApiTracing(string apiToTrace, string userId, string sessionId)
        {
            var postData = string.Format("{{\"ApisToTrace\" : \"{0}\", \"Enable\" : true, \"UserId\" : \"{1}\"}}", apiToTrace, userId);
            return MakeRequest<TraceModel>("https://services.qa7.cdops.net/Security/UpdateApiTracing", postData, sessionId);
        }

        private T MakeRequest<T>(string uri, string postData, string sessionId)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.ContentLength = data.Length;
            request.Headers.Add("CD-SystemId", "85788485-e411-48a9-b478-610c1285dc1a");

            if (!string.IsNullOrEmpty(sessionId))
            {
                request.Headers.Add("CD-SessionId", sessionId);
            }

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            // ReSharper disable once AssignNullToNotNullAttribute
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            return JsonConvert.DeserializeObject<T>(responseString);
        }
    }
}
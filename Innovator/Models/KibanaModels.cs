using System.Collections.Generic;
using Innovator.Controllers;

namespace Innovator.Models
{
    public class UserLoginModel
    {
        public string UserName { get; set; }
        public string UserPassword { get; set; }
    }

    public class UserLoginSuccessViewModel
    {
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
    }

    public class TraceInfoModel
    {
        public string ApiToTrace { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }
    }

    public class TraceInfoSuccessViewModel
    {
        public string TracingId { get; set; }
    }

    public class TraceResultsRequestModel
    {
        public string TracingId { get; set; }
    }
}
using System.Web.Http;
using Innovator.Communication;
using Innovator.Models;

namespace Innovator.Controllers
{
    public class LoginController : ApiController
    {
        private readonly IServiceRequestor _serviceRequestor;

        public LoginController(IServiceRequestor serviceRequestor)
        {
            _serviceRequestor = serviceRequestor;
        }

        public IHttpActionResult Post(UserLoginModel categoryModel)
        {
            var sessionModel = _serviceRequestor.CreateSession(categoryModel.UserName, categoryModel.UserPassword);

            return Ok(new UserLoginSuccessViewModel
            {
                SessionId = sessionModel.SessionId,
                DisplayName = sessionModel.SessionSummary.DisplayName,
                UserId = sessionModel.SessionSummary.Id
            });
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class SessionModel
    {
        public string SessionId { get; set; }
        public SessionSummaryModel SessionSummary { get; set; }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class SessionSummaryModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
    }
}

using System.Web.Http;
using Innovator.Communication;
using Innovator.Models;

namespace Innovator.Controllers
{
    public class TraceController : ApiController
    {
        private readonly IServiceRequestor _serviceRequestor;

        public TraceController(IServiceRequestor serviceRequestor)
        {
            _serviceRequestor = serviceRequestor;
        }

        public IHttpActionResult Post(TraceInfoModel traceInfoModel)
        {
            var traceModel = _serviceRequestor.UpdateApiTracing(traceInfoModel.ApiToTrace, traceInfoModel.UserId, traceInfoModel.SessionId);

            return Ok(new TraceInfoSuccessViewModel
            {
                TracingId = traceModel.TracingId
            });
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class TraceModel
    {
        public string TracingId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;

namespace VotingService.Controllers
{
    public class VotesController : ApiController
    {
        // Used for health checks. 
        public static long _requestCount = 0L;

        // Holds the votes and counts. NOTE: THIS IS NOT THREAD SAFE FOR THE PURPOSES OF THE LAB. 
        static Dictionary<string, int> _counts = new Dictionary<string, int>();

        // GET api/votes 
        [HttpGet]
        [Route("api/votes")]
        public HttpResponseMessage Get()
        {
            string activityId = Guid.NewGuid().ToString();
            ServiceEventSource.Current.ServiceRequestStart("VotesController.Get", activityId);

            Interlocked.Increment(ref _requestCount);
            var votes = new List<KeyValuePair<string, int>>(_counts.Count);
            foreach (KeyValuePair<string, int> kvp in _counts)
            {
                votes.Add(kvp);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK, votes);

            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                NoCache = true,
                MustRevalidate = true
            };

            ServiceEventSource.Current.ServiceRequestStop("VotesController.Get", activityId);

            return response;
        }

        [HttpPost]
        [Route("api/{key}")]
        public HttpResponseMessage Post(string key)
        {
            string activityId = Guid.NewGuid().ToString();
            ServiceEventSource.Current.ServiceRequestStart("VotesController.Post", activityId);

            Interlocked.Increment(ref _requestCount);
            if (false == _counts.ContainsKey(key))
            {
                _counts.Add(key, 1);
            }
            else
            {
                _counts[key] = _counts[key] + 1;
            }
            ServiceEventSource.Current.ServiceRequestStop("VotesController.Post", activityId);
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Route("api/{key}")]
        public HttpResponseMessage Delete(string key)
        {
            string activityId = Guid.NewGuid().ToString();
            ServiceEventSource.Current.ServiceRequestStart("VotesController.Delete", activityId);

            Interlocked.Increment(ref _requestCount);
            if (true == _counts.ContainsKey(key))
            {
                if (_counts.Remove(key))
                    return Request.CreateResponse(HttpStatusCode.OK);
            }

            ServiceEventSource.Current.ServiceRequestStop("VotesController.Delete", activityId);
            return Request.CreateResponse(HttpStatusCode.NotFound);

        }

        [HttpGet]
        [Route("api/{file}")]
        public HttpResponseMessage GetFile(string file)
        {
            string activityId = Guid.NewGuid().ToString();
            ServiceEventSource.Current.ServiceRequestStart("VotesController.GetFile", activityId);

            string response = null;
            string responseType = "text/html";
            Interlocked.Increment(ref _requestCount);
            // Validate file name. 
            if ("index.html" == file)
            {
                string path = string.Format(@"..\VotingServicePkg.Code.1.0.0\{0}", file);
                response = File.ReadAllText(path);
            }

            ServiceEventSource.Current.ServiceRequestStop("VotesController.GetFile", activityId);

            if (null != response)
                return Request.CreateResponse(HttpStatusCode.OK, response, responseType);
            else
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "File");
        }


    }
}


using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("tsid/tsidsynservice/external/v1/")]
    public class HuaweiMockController : ControllerBase
    {
        private readonly ILogger<HuaweiMockController> _logger;
        public HuaweiMockController(ILogger<HuaweiMockController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("project/user/query")]
        public async Task<IActionResult> ProjectQuery(string account)
        {
            _logger.LogInformation("Query project, account = {}", account);
            if (string.IsNullOrWhiteSpace(account))
                return BadRequest(new { Error = 404, Msg = "account is empty" });

            using (var textReader = System.IO.File.OpenText("./TestData/project.json"))
            {
                using (var jsonReader = new JsonTextReader(textReader))
                {
                    return new JsonResult(await JToken.ReadFromAsync(jsonReader));
                }
            }
        }

        [HttpPost]
        [Route("site/task/query")]
        public async Task<IActionResult> TaskSiteQuery()
        {
            var headers = HttpContext.Request.Headers;
            var projectCode = headers["project-code"].ToString();
            var projectId = headers["project-id"].ToString();
            var userInfo = headers["user-info"].ToString();

            _logger.LogInformation("Query task site, projectCode = {}, projectId = {}, userInfo = {}",
            projectCode, projectId, userInfo);

            if (string.IsNullOrWhiteSpace(projectCode) ||
            string.IsNullOrWhiteSpace(projectId) ||
            string.IsNullOrWhiteSpace(userInfo) ||
            userInfo.Substring(1, 9) != "\"account\"")
                return BadRequest(new { ErrorCode = 404, Msg = "account error" });

            using (var textReader = System.IO.File.OpenText("./TestData/tasksite.json"))
            {
                using (var jsonReader = new JsonTextReader(textReader))
                {
                    return new JsonResult(await JToken.ReadFromAsync(jsonReader));
                }
            }
        }

        [HttpPost]
        [Route("cad/cad-data/report/zip")]
        public IActionResult TaskDataDownload(string siteId, string activityInstanceCode)
        {
            var headers = HttpContext.Request.Headers;
            var projectCode = headers["project-code"].ToString();
            var projectId = headers["project-id"].ToString();
            var userInfo = headers["user-info"].ToString();

            _logger.LogInformation(
@"Download task data, projectCode = {}, projectId = {}, userInfo = {},
siteId = {}, activityInstanceCode = {}",
            projectCode, projectId, userInfo, siteId, activityInstanceCode);

            if (string.IsNullOrWhiteSpace(projectCode) ||
            string.IsNullOrWhiteSpace(projectId) ||
            string.IsNullOrWhiteSpace(userInfo) ||
            userInfo.Substring(1, 9) != "\"account\"")
                return BadRequest(new { ErrorCode = 404, Msg = "account error" });

            if (string.IsNullOrWhiteSpace(siteId) ||
            string.IsNullOrWhiteSpace(activityInstanceCode))
                return BadRequest(new { ErrorCode = 404, Msg = "siteId or activityInstanceCode is empty" });

            try
            {
                var stream = Utils.Util.Compress("./TestData/taskdata_download",
                 "./TestData/taskdata_download/result.json",
                 "./TestData/taskdata_download/data");
                return File(stream, "application/zip", Guid.NewGuid() + ".zip");

            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("cad/download-cad-block")]
        public IActionResult CadDataDownload()
        {
            var headers = HttpContext.Request.Headers;
            var projectCode = headers["project-code"].ToString();
            var projectId = headers["project-id"].ToString();
            var userInfo = headers["user-info"].ToString();

            _logger.LogInformation("Download cad block data, projectCode = {}, projectId = {}, userInfo = {}",
            projectCode, projectId, userInfo);

            if (string.IsNullOrWhiteSpace(projectCode) ||
            string.IsNullOrWhiteSpace(projectId) ||
            string.IsNullOrWhiteSpace(userInfo) ||
            userInfo.Substring(1, 9) != "\"account\"")
                return BadRequest(new { ErrorCode = 404, Msg = "account error" });

            try
            {
                var stream = Utils.Util.Compress("./TestData/cadlibrary_download",
                "./TestData/cadlibrary_download/result.json",
                "./TestData/cadlibrary_download/data");
                return File(stream, "applicaton/zip", Guid.NewGuid() + ".zip");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
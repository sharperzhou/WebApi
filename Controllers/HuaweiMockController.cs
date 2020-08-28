
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text;

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
            _logger.LogInformation("Query project, account = {}, ip = {}",
            account, HttpContext.Connection.RemoteIpAddress.MapToIPv4());
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
                "Download task data, projectCode = {}, projectId = {}, userInfo = {}, siteId = {}, activityInstanceCode = {}",
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

        [HttpPost]
        [Route("cad/cad-data/deliverable/upload")]
        public IActionResult UploadData(string taskId, string deliverableType)
        {
            var headers = HttpContext.Request.Headers;
            var projectCode = headers["project-code"].ToString();
            var projectId = headers["project-id"].ToString();
            var userInfo = headers["user-info"].ToString();

            _logger.LogInformation(
                "Upload data, projectCode = {}, projectId = {}, userInfo = {}, taskId = {}, deliverableType = {}",
            projectCode, projectId, userInfo, taskId, deliverableType);

            if (string.IsNullOrWhiteSpace(projectCode) ||
                string.IsNullOrWhiteSpace(projectId) ||
                string.IsNullOrWhiteSpace(userInfo) ||
                userInfo.Substring(1, 9) != "\"account\"")
                return BadRequest(new { ErrorCode = 404, Msg = "account error" });

            if (string.IsNullOrWhiteSpace(taskId) ||
                (deliverableType != "CAD Drawing Report" &&
                deliverableType != "CAD Drawing"))
                return BadRequest(new { ErrorCode = 404, Msg = "taskId is empty or deliverableType is invalid" });

            var files = HttpContext.Request.Form.Files;
            if (files.Count <= 0)
                return BadRequest(new { ErrorCode = 404, Msg = "No files" });

            using (var stream = files[0].OpenReadStream())
            {
                var buffer = new byte[4096];
                while (stream.Read(buffer, 0, buffer.Length) > 0) ;
            }

            _logger.LogInformation("Content length: {}, File name: {}", HttpContext.Request.ContentLength, files[0].FileName);

            return Accepted(new { code = 1, msg = "upload ok", FileLength = files[0].Length });
        }

        [HttpPost]
        [Route("site/start-task")]
        public async Task<IActionResult> StartTask()
        {
            var headers = HttpContext.Request.Headers;
            var projectCode = headers["project-code"].ToString();
            var projectId = headers["project-id"].ToString();
            var userInfo = headers["user-info"].ToString();

            _logger.LogInformation("Start task, projectCode = {}, projectId = {}, userInfo = {}",
            projectCode, projectId, userInfo);

            if (string.IsNullOrWhiteSpace(projectCode) ||
                string.IsNullOrWhiteSpace(projectId) ||
                string.IsNullOrWhiteSpace(userInfo) ||
                userInfo.Substring(1, 9) != "\"account\"")
                return BadRequest(new { ErrorCode = 404, Msg = "account error" });

            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                return Ok(new { code = 1, msg = "start task ok", bodyContent = await reader.ReadToEndAsync() });
        }

        [HttpPost]
        [Route("cad/download-cad-block/check")]
        public async Task<IActionResult> CheckCadBlock()
        {
            var headers = HttpContext.Request.Headers;
            var projectCode = headers["project-code"].ToString();
            var projectId = headers["project-id"].ToString();
            var userInfo = headers["user-info"].ToString();

            _logger.LogInformation("Check CAD block data, projectCode = {}, projectId = {}, userInfo = {}",
            projectCode, projectId, userInfo);

            if (string.IsNullOrWhiteSpace(projectCode) ||
                string.IsNullOrWhiteSpace(projectId) ||
                string.IsNullOrWhiteSpace(userInfo) ||
                userInfo.Substring(1, 9) != "\"account\"")
                return BadRequest(new { ErrorCode = 404, Msg = "account error" });

            using (var textReader = System.IO.File.OpenText("./TestData/checkcad.json"))
            {
                using (var jsonReader = new JsonTextReader(textReader))
                {
                    return new JsonResult(await JToken.ReadFromAsync(jsonReader));
                }
            }
        }

        [HttpPost]
        [Route("site/submit-task")]
        public async Task<IActionResult> SubmitTask()
        {
            var headers = HttpContext.Request.Headers;
            var projectCode = headers["project-code"].ToString();
            var projectId = headers["project-id"].ToString();
            var userInfo = headers["user-info"].ToString();

            _logger.LogInformation("Submit task, projectCode = {}, projectId = {}, userInfo = {}",
            projectCode, projectId, userInfo);

            if (string.IsNullOrWhiteSpace(projectCode) ||
                string.IsNullOrWhiteSpace(projectId) ||
                string.IsNullOrWhiteSpace(userInfo) ||
                userInfo.Substring(1, 9) != "\"account\"")
                return BadRequest(new { ErrorCode = 404, Msg = "account error" });

            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var bodyContent = await reader.ReadToEndAsync();
                _logger.LogInformation("Body content: {}", bodyContent);
                return Ok(new { code = 1, msg = "submit task ok", bodyContent });
            }

        }
    }
}
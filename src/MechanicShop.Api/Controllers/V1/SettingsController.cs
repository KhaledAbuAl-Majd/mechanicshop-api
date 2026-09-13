using Asp.Versioning;
using MechanicShop.Api.Responses;
using MechanicShop.Application.Common.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers.V1
{
    [Route("settings")]
    [ApiVersionNeutral]
    [Tags("Settings")]
    public class SettingsController(AppSettings appSettings) : ApiController
    {

        [HttpGet("operating-hours")]
        [ProducesResponseType(typeof(OperatingHoursResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Gets the application's operating hours.")]
        [EndpointDescription("Returns the current configured opening and closing times.")]
        [EndpointName("GetOperatingHours")]
        [OutputCache(Duration = 3600)]
        public ActionResult<OperatingHoursResponse> GetOperatingHours()
        {
            var response = new OperatingHoursResponse(appSettings.OpeningTime, appSettings.ClosingTime);

            return Ok(response);
        }
    }
}

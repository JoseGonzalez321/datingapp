using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class HealthController : BaseApiController
{
    [HttpGet] // api/health
    public ActionResult<object> GetHealth()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
    }
}

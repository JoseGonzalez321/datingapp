using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
public class FallbackController : ControllerBase
{    
    public ActionResult Index()
    {
        return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(),
        "wwwroot", "index.html"), "text/html");
    }
}


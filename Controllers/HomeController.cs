using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("rfs-library/home")] // Base route
    public class HomeController : ControllerBase
    {
        [HttpGet]
        [Route("rfs-library/home")]
        public IActionResult Index()
        {
            return Content("RF-SMART Library Book System");
            // Render view or return HTML as needed
        }
    }
}

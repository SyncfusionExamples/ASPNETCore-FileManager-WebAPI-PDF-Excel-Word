using Microsoft.AspNetCore.Mvc;

namespace ASPNETCore_FileManager_PDF_Word_Excel.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

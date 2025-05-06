using Microsoft.AspNetCore.Mvc;

namespace GRUPO_01_SubirArchivosNube.AppWebMVC.Controllers
{
    public class ArchivosController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace State.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            HttpContext.Session.SetString("Test", "Nehme Roukos");
            return View();
            //string userName = Request.Cookies["UserName"];
            
            //return View("Index", userName);
        }

        [HttpPost]
        public IActionResult Index(IFormCollection form)
        {
            string userName = form["userName"].ToString();

            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddMinutes(10);
            Response.Cookies.Append("UserName", userName, option);

            return RedirectToAction(nameof(Index));

        }

        public IActionResult RemoveCookie()
        {
            Response.Cookies.Delete("UserName");
            return View("Index");
        }
    }
}

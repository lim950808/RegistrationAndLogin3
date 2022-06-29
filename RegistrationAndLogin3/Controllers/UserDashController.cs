using RegistrationAndLogin3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace RegistrationAndLogin3.Controllers
{
    public class UserDashController : Controller
    {
        [Authorize]
        // GET: UserDash
        public ActionResult Index()
        {
            return View();
        }
    }
}
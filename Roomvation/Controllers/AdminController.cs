using Microsoft.AspNet.Identity.Owin;
using Roomvation.Models;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Roomvation.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _context;

        public AdminController()
        {
            _context = new ApplicationDbContext();
        }

        public AdminController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Admin
        public ActionResult Index()
        {
            if (!User.IsInRole("Administrators"))
                return RedirectToAction("Index", "Home");

            var users = _context.Users.ToList();
            return View(users);
        }
    }
}
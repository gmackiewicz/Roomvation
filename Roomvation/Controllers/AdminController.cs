using Microsoft.AspNet.Identity.Owin;
using Roomvation.Models;
using Roomvation.Models.AdminViewModels;
using System;
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
        public ActionResult Users()
        {
            if (!User.IsInRole("Administrators"))
                return RedirectToAction("Index", "Reservations");

            var viewModel = new UsersViewModel();
            var users = _context.Users.OrderBy(u => u.LastName).ToList();
            viewModel.Users = users;
            return View(viewModel);
        }

        public ActionResult Unlock(string id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            user.LockoutEndDateUtc = null;
            _context.SaveChanges();

            return RedirectToAction(nameof(Users));

        }

        public ActionResult Lock(string id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            user.LockoutEndDateUtc = new DateTime(2999, 12, 31);
            _context.SaveChanges();

            return RedirectToAction(nameof(Users));
        }
    }
}
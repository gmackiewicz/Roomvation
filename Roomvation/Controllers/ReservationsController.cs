using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Roomvation.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Roomvation.Controllers
{
    public class ReservationsController : Controller
    {
        private ApplicationUserManager _userManager;
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public ReservationsController()
        {

        }

        public ReservationsController(ApplicationUserManager userManager)
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

        // GET: Reservations/MyList
        [Authorize]
        public ActionResult MyList()
        {
            var currentUserId = User.Identity.GetUserId();
            var reservations = _context.Reservations.Where(r => r.Creator.Id == currentUserId).Include(r => r.Creator);
            return View(reservations.ToList());
        }

        // GET: Reservations/Create
        [Authorize]
        public ActionResult Create()
        {
            var model = new Reservation { CreatorId = User.Identity.GetUserId() };
            return View(model);
        }
        
        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Reservation reservation)
        {
            if (!ModelState.IsValid)
            {
                return View(reservation);
            }

            reservation.CreationDate = DateTime.UtcNow;
            _context.Reservations.Add(reservation);
            _context.SaveChanges();
            return RedirectToAction("MyList", "Reservations");
        }
    }
}
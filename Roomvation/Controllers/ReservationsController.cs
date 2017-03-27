using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Roomvation.Models;
using Roomvation.Models.ReservationsViewModels;
using System;
using System.Collections.Generic;
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
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        // GET: Reservations/Index
        public ActionResult Index()
        {
            var now = DateTime.Now;
            var date = now.Date;

            var reservations = _context.Reservations
                .Where(r => r.Date >= date && r.StartTime >= now)
                .Include(r => r.Creator);

            var participations = _context.ReservationParticipants
                .Where(rp => reservations.Select(r => r.Id).Contains(rp.ReservationId))
                .Include(rp => rp.Participant);

            var model = new ReservationsListViewModel
            {
                Reservations = reservations.ToList(),
                Participations = participations.ToList()
            };

            return View(model);
        }

        // GET: Reservations/MyList
        [Authorize]
        public ActionResult MyList()
        {
            var currentUserId = User.Identity.GetUserId();
            var usersReservations = _context.Reservations
                .Where(r => r.CreatorId == currentUserId)
                .Include(r => r.Creator);

            var participations = _context.ReservationParticipants
                .Where(rp => usersReservations.Select(r => r.Id).Contains(rp.ReservationId))
                .Include(rp => rp.Participant);

            var model = new ReservationsListViewModel
            {
                Reservations = usersReservations.ToList().OrderBy(r => r.Date),
                Participations = participations.ToList()
            };

            return View(model);
        }

        // GET: Reservations/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.SelectedUser = new SelectList(_context.Users, "Id", "FullName");
            var model = new CreateReservationViewModel
            {
                Reservation = new Reservation
                {
                    CreatorId = User.Identity.GetUserId(),
                    Date = DateTime.Today
                },
                Participants = new List<ApplicationUser>()
            };
            return View(model);
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            model.Reservation.StartTime = model.Reservation.Date
                .AddHours(model.Reservation.StartTime.Hour)
                .AddMinutes(model.Reservation.StartTime.Minute);

            model.Reservation.EndTime = model.Reservation.Date
                .AddHours(model.Reservation.EndTime.Hour)
                .AddMinutes(model.Reservation.EndTime.Minute);

            var now = DateTime.Now;

            var collisions = _context.Reservations.Any(r => r.EndTime > model.Reservation.StartTime);

            if (model.Reservation.StartTime < now || collisions)
            {
                ViewBag.Error = "Your new reservation tries to be in the past, or collides with another reservation. Fix it!";
                ViewBag.SelectedUser = new SelectList(_context.Users, "Id", "FullName");
                return View(model);
            }

            model.Reservation.CreationDate = DateTime.Now;


            _context.Reservations.Add(model.Reservation);

            var users = model.ParticipantIds;
            if (users != null)
            {
                var split = users.Split('|');

                for (var i = 0; i < split.Length - 1; i++)
                {
                    var participation = new Participation
                    {
                        ReservationId = model.Reservation.Id,
                        ParticipantId = split[i]
                    };
                    _context.ReservationParticipants.Add(participation);
                }
            }

            _context.SaveChanges();
            return RedirectToAction("MyList", "Reservations");
        }

        public ActionResult Edit(int id)
        {
            var reservationToEdit = _context.Reservations.FirstOrDefault(r => r.Id == id);
            if (reservationToEdit == null || reservationToEdit.StartTime < DateTime.Now)
            {
                return RedirectToAction("MyList", "Reservations");
            }
            var participants = _context.ReservationParticipants.Where(rp => rp.ReservationId == id).Select(rp => rp.Participant).ToList();
            var model = new CreateReservationViewModel
            {
                Reservation = reservationToEdit,
                Participants = participants
            };

            model.Reservation.Date = new DateTime(model.Reservation.Date.Year, model.Reservation.Date.Month,
                model.Reservation.Date.Day);

            ViewBag.SelectedUser = new SelectList(_context.Users, "Id", "FullName");

            return View(model);
        }
    }
}
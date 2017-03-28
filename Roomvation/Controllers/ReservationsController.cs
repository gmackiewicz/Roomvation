using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Roomvation.Models;
using Roomvation.Models.ReservationsViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
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
            var dateError = CheckDateForErrors(0, model.Reservation.StartTime, model.Reservation.EndTime);
            if (dateError)
            {
                ViewBag.Error = "Your new reservation tries to be in the past, or collides with another reservation. Fix it!";
                ViewBag.SelectedUser = new SelectList(_context.Users, "Id", "FullName");
                return View(model);
            }
            model.Reservation.CreationDate = now;

            _context.Reservations.Add(model.Reservation);

            var users = model.ParticipantIds + User.Identity.GetUserId();
            AddParticipationsFor(model.Reservation.Id, users);

            _context.SaveChanges();
            return RedirectToAction("MyList", "Reservations");
        }

        private void AddParticipationsFor(int reservationId, string userIds)
        {
            if (userIds == null) return;

            var ids = userIds.Split('|');
            foreach (var id in ids)
            {
                var participation = new Participation
                {
                    ReservationId = reservationId,
                    ParticipantId = id
                };
                _context.ReservationParticipants.Add(participation);
            }
        }


        public ActionResult Details(int id = 0)
        {
            var userId = User.Identity.GetUserId();
            var reservation = _context.Reservations.FirstOrDefault(r => r.Id == id && r.Creator.Id == userId);
            if (reservation == null)
            {
                return RedirectToAction("MyList", "Reservations");
            }

            var participants = _context.ReservationParticipants.Where(rp => rp.ReservationId == id).Select(rp => rp.Participant).ToList();
            var model = new ReservationDetailsViewModel
            {
                Reservation = reservation,
                Participants = participants
            };

            return View(model);
        }

        public ActionResult Cancel(int id = 0)
        {
            var user = User.Identity.GetUserId();
            var reservation = _context.Reservations.Where(r => r.Id == id).Include(r => r.Creator).FirstOrDefault();
            if (reservation == null || reservation.Creator.Id != user)
            {
                return RedirectToAction("MyList", "Reservations");
            }

            _context.Reservations.Remove(reservation);
            _context.SaveChanges();

            return RedirectToAction("MyList", "Reservations");
        }

        [HttpPost]
        public ActionResult ChangeDate(int id, string newDate, string newStart, string newEnd)
        {
            var reservation = _context.Reservations.FirstOrDefault(r => r.Id == id);
            if (reservation == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Couldn't find that reservation.");
            }

            var date = DateTime.Parse(newDate).Date;
            var start = DateTime.Parse(newStart);
            var end = DateTime.Parse(newEnd);

            var startTime = date
                .AddHours(start.Hour)
                .AddMinutes(start.Minute);

            var endTime = date
                .AddHours(end.Hour)
                .AddMinutes(end.Minute);

            var dateError = CheckDateForErrors(id, startTime, endTime);
            if (dateError)
            {
                Response.StatusCode = (int)HttpStatusCode.Conflict;
                return Json("Provided date or time caused conlicts. Check if it is not the past date.");
            }

            reservation.Date = date;
            reservation.StartTime = startTime;
            reservation.EndTime = endTime;
            _context.Reservations.AddOrUpdate(reservation);
            _context.SaveChanges();

            Response.StatusCode = (int)HttpStatusCode.OK;
            return Json("Reservation date has been successfully changed.");
        }

        [HttpPost]
        public ActionResult ChangeDescription(int id, string description)
        {
            var reservation = _context.Reservations.FirstOrDefault(r => r.Id == id);
            if (reservation == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Couldn't find that reservation.");
            }
            try
            {
                reservation.MeetingDescription = description;
                _context.Reservations.AddOrUpdate(reservation);
                _context.SaveChanges();
            }
            catch (DbEntityValidationException exception)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json("An error occured. Provided description is too long.");
            }

            Response.StatusCode = (int)HttpStatusCode.OK;
            return Json("Meeting description has been successfully changed.");
        }

        private bool CheckDateForErrors(int id, DateTime startTime, DateTime endTime)
        {
            var now = DateTime.Now;
            if (startTime >= endTime || startTime < now)
            {
                return true;
            }

            var other = _context.Reservations.Where(r => r.Id != id).ToList();

            var result =
                other.Any(
                    r =>
                        (startTime >= r.StartTime && startTime < r.EndTime) ||
                        (startTime <= r.StartTime && endTime > r.StartTime));
            return result;
        }
    }
}
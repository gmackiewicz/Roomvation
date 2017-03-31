using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Roomvation.Models;
using Roomvation.Models.ReservationsViewModels;
using Roomvation.Utilities;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
                Reservations = reservations.OrderBy(r => r.StartTime).ToList(),
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
                Reservations = usersReservations.ToList().OrderBy(r => r.StartTime),
                Participations = participations.ToList()
            };

            return View(model);
        }

        // GET: Reservations/Create
        [Authorize]
        public ActionResult Create()
        {
            var model = new Reservation
            {
                CreatorId = User.Identity.GetUserId(),
                Date = DateTime.Today
            };

            return View(model);
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Reservation model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.StartTime = model.Date
                .AddHours(model.StartTime.Hour)
                .AddMinutes(model.StartTime.Minute);

            model.EndTime = model.Date
                .AddHours(model.EndTime.Hour)
                .AddMinutes(model.EndTime.Minute);

            var now = DateTime.Now;

            var error = VerifyDate(0, model.StartTime, model.EndTime);
            if (error != DateErrorStatus.Ok)
            {
                ViewBag.Error = ResolveErrorMessage(error);
                return View(model);
            }
            model.CreationDate = now;
            _context.Reservations.Add(model);

            var users = User.Identity.GetUserId();
            AddParticipationsFor(model.Id, users);

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Reservations", new { id = model.Id });
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

            var participants = _context.ReservationParticipants
                .Where(rp => rp.ReservationId == id)
                .Select(rp => rp.Participant)
                .ToList();

            var participantIds = participants.Select(p => p.Id).ToList();
            var available = _context.Users
                .Where(u => !participantIds.Contains(u.Id) && u.LockoutEndDateUtc == null)
                .OrderBy(u => u.LastName)
                .ToList();

            var model = new ReservationDetailsViewModel
            {
                Reservation = reservation,
                Participants = participants,
                AvailableUsers = available
            };

            return View(model);
        }

        public async Task<ActionResult> Cancel(int id = 0)
        {
            var user = User.Identity.GetUserId();
            var reservation = _context.Reservations.Where(r => r.Id == id).Include(r => r.Creator).FirstOrDefault();
            if (reservation == null || reservation.Creator.Id != user)
            {
                return RedirectToAction("MyList", "Reservations");
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyList", "Reservations");
        }

        [HttpPost]
        public async Task<JsonResult> ChangeDate(int id, string newDate, string newStart, string newEnd)
        {
            var reservation = _context.Reservations.FirstOrDefault(r => r.Id == id);
            if (reservation == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Couldn't find that reservation.");
            }
            DateTime date;
            DateTime start;
            DateTime end;

            try
            {
                date = DateTime.Parse(newDate).Date;
                start = DateTime.Parse(newStart);
                end = DateTime.Parse(newEnd);
            }
            catch (FormatException ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json("Input is incorrect.");
            }

            var startTime = date
                .AddHours(start.Hour)
                .AddMinutes(start.Minute);

            var endTime = date
                .AddHours(end.Hour)
                .AddMinutes(end.Minute);

            var error = VerifyDate(id, startTime, endTime);
            if (error != DateErrorStatus.Ok)
            {
                Response.StatusCode = (int)HttpStatusCode.Conflict;
                var message = ResolveErrorMessage(error);
                return Json(message);
            }

            reservation.Date = date;
            reservation.StartTime = startTime;
            reservation.EndTime = endTime;
            _context.Reservations.AddOrUpdate(reservation);
            await _context.SaveChangesAsync();

            Response.StatusCode = (int)HttpStatusCode.OK;
            return Json("Reservation date has been successfully changed.");
        }

        private string ResolveErrorMessage(DateErrorStatus error)
        {
            var result = string.Empty;
            switch (error)
            {
                case DateErrorStatus.Past:
                    result = "Reservation cannot start in the past!";
                    break;
                case DateErrorStatus.IncorrectTime:
                    result = "Meeting cannot end before it starts!";
                    break;
                case DateErrorStatus.Conflict:
                    result = "This date and time collides with another reservation!";
                    break;
            }
            return result;
        }

        [HttpPost]
        public async Task<JsonResult> ChangeDescription(int id, string description)
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
                await _context.SaveChangesAsync();
            }
            catch (DbEntityValidationException exception)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json("An error occured. Provided description is too long.");
            }

            Response.StatusCode = (int)HttpStatusCode.OK;
            return Json("Meeting description has been successfully changed.");
        }

        [HttpPost]
        public async Task<JsonResult> AddParticipant(int id, string participant)
        {
            var reservation = _context.Reservations.FirstOrDefault(r => r.Id == id);
            var user = _context.Users.FirstOrDefault(u => u.Id == participant);
            if (reservation == null || user == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Couldn't find that reservation or user.");
            }

            var participation = new Participation
            {
                ReservationId = id,
                ParticipantId = participant
            };

            _context.ReservationParticipants.AddOrUpdate(participation);
            await _context.SaveChangesAsync();

            Response.StatusCode = (int)HttpStatusCode.OK;
            return Json("User has been successfully added to the meeting.");
        }
        private DateErrorStatus VerifyDate(int id, DateTime startTime, DateTime endTime)
        {
            var now = DateTime.Now;
            if (startTime < now)
            {
                return DateErrorStatus.Past;
            }
            if (startTime >= endTime)
            {
                return DateErrorStatus.IncorrectTime;
            }

            var other = _context.Reservations.Where(r => r.Id != id).ToList();

            var conflict =
                other.Any(
                    r =>
                        (startTime >= r.StartTime && startTime < r.EndTime) ||
                        (startTime <= r.StartTime && endTime > r.StartTime));
            if (conflict)
            {
                return DateErrorStatus.Conflict;
            }
            return DateErrorStatus.Ok;
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

        public async Task<ActionResult> RemoveParticipant(int reservationId, string userId)
        {
            var participation = _context.ReservationParticipants
                .FirstOrDefault(rp => rp.ReservationId == reservationId && rp.ParticipantId == userId);
            if (participation != null)
            {
                _context.ReservationParticipants.Remove(participation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = reservationId });
        }
    }
}
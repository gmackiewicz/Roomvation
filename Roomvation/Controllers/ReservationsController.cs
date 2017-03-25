﻿using Microsoft.AspNet.Identity;
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
            ViewBag.SelectedUser = new SelectList(_context.Users, "Id", "FullName");
            var model = new CreateReservationViewModel
            {
                Reservation = new Reservation {CreatorId = User.Identity.GetUserId()},
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

            model.Reservation.CreationDate = DateTime.UtcNow;
            _context.Reservations.Add(model.Reservation);

            var users = model.ParticipantIds;
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

            _context.SaveChanges();
            return RedirectToAction("MyList", "Reservations");
        }
    }
}
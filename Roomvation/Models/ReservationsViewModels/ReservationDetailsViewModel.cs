using System.Collections.Generic;

namespace Roomvation.Models.ReservationsViewModels
{
    public class ReservationDetailsViewModel
    {
        public Reservation Reservation { get; set; }
        public List<ApplicationUser> Participants { get; set; }
        public List<ApplicationUser> AvailableUsers { get; set; }
    }
}
using System.Collections.Generic;

namespace Roomvation.Models.ReservationsViewModels
{
    public class CreateReservationViewModel
    {
        public Reservation Reservation { get; set; }
        public List<ApplicationUser> Participants { get; set; }
        public string SelectedUser { get; set; }
        public string ParticipantIds { get; set; }
    }
}
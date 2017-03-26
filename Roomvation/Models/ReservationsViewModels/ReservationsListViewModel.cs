using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Roomvation.Models.ReservationsViewModels
{
    public class ReservationsListViewModel
    {
        public IEnumerable<Reservation> Reservations { get; set; }

        [Display(Name = "Participants")]
        public IEnumerable<Participation> Participations { get; set; }
    }
}
namespace Roomvation.Models
{
    public class Participation
    {
        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }

        public string ParticipantId { get; set; }
        public ApplicationUser Participant { get; set; }
    }
}
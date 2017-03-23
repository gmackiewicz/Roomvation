using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Roomvation.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        [Required]
        [StringLength(250)]
        public string MeetingDescription { get; set; }
        public DateTime CreationDate { get; set; }

        [Required]
        public string CreatorId { get; set; }
        public ApplicationUser Creator { get; set; }
        public IEnumerable<ApplicationUser> Participants { get; set; }
    }
}
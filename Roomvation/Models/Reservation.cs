using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Roomvation.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Display(Name = "Meeting date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Display(Name = "Meeting starts")]
        [DataType(DataType.Time)]
        public DateTime StartTime { get; set; }

        [Display(Name = "Meeting ends")]
        [DataType(DataType.Time)]
        public DateTime EndTime { get; set; }

        [Required]
        [StringLength(250)]
        [Display(Name = "Description")]
        public string MeetingDescription { get; set; }

        [Display(Name = "Created")]
        public DateTime CreationDate { get; set; }

        [Required]
        public string CreatorId { get; set; }
        public ApplicationUser Creator { get; set; }
        public IEnumerable<ApplicationUser> Participants { get; set; }
    }
}
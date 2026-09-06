using System;
using System.ComponentModel.DataAnnotations;

namespace CommunitySportsFacilitiesBookingSystem.ViewModels
{
    public class BookingCreateViewModel
    {
        [Required]
        public int FacilityID { get; set; }

        [Display(Name = "Facility")]
        public string FacilityName { get; set; }
        public decimal HourlyRate { get; set; }
        public int Capacity { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "Number of Participants")]
        public int NumberOfParticipants { get; set; }

        [StringLength(250)]
        public string Purpose { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace CommunitySportsFacilitiesBookingSystem.ViewModels
{
    public class ReviewCreateViewModel
    {
        [Required]
        public int BookingID { get; set; }

        [Display(Name = "Facility")]
        public string FacilityName { get; set; }

        [Required]
        [Range(1, 5)]
        [Display(Name = "Overall Rating")]
        public int OverallRating { get; set; }

        [Required]
        [Range(1, 5)]
        [Display(Name = "Facility Condition")]
        public int ConditionRating { get; set; }

        [Required]
        [Range(1, 5)]
        [Display(Name = "Amenities")]
        public int AmenitiesRating { get; set; }

        [StringLength(100)]
        [Display(Name = "Review Title")]
        public string ReviewTitle { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Your Review")]
        public string ReviewComment { get; set; }
    }
}
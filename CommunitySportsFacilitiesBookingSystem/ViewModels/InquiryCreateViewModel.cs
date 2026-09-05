using System.ComponentModel.DataAnnotations;

namespace CommunitySportsFacilitiesBookingSystem.ViewModels
{
    public class InquiryCreateViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Your Name")]
        public string SenderName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Address")]
        public string SenderEmail { get; set; }

        [Phone]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string SenderPhone { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Subject")]
        public string InquirySubject { get; set; }

        [Required]
        [StringLength(1000)]
        [Display(Name = "Message")]
        public string InquiryMessage { get; set; }
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunitySportsFacilitiesBookingSystem.ViewModels
{
    public class RegisterViewModel
    {
        [Required, StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [Required, Phone, StringLength(20)]
        public string Phone { get; set; }

        [Required, StringLength(150)]
        [Display(Name = "Address")]
        public string AddressLine { get; set; }

        [Required, StringLength(60)]
        public string City { get; set; }

        [StringLength(15)]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Required, StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Preferred Sports")]
        public List<int> SelectedSportIDs { get; set; } = new List<int>();
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
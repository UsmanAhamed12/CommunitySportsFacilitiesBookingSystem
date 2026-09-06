using System; using System.Collections.Generic; using System.ComponentModel.DataAnnotations;
namespace CommunitySportsFacilitiesBookingSystem.Models {
[MetadataType(typeof(BookingMetadata))] public partial class Booking {}
public class BookingMetadata {
[Range(1, int.MaxValue)] public object NumberOfParticipants { get; set; }
[StringLength(250)] public object Purpose { get; set; }
[Required, StringLength(20), RegularExpression("Pending|Confirmed|Completed|Cancelled|Rejected")] public object BookingStatus { get; set; }
[Range(typeof(decimal), "0", "99999999.99")] public object TotalAmount { get; set; }
}
[MetadataType(typeof(FacilityMetadata))] public partial class Facility {}
public class FacilityMetadata {
[Required, StringLength(100)] public object FacilityName { get; set; }
[StringLength(500)] public object Description { get; set; }
[Required, StringLength(150)] public object AddressLine { get; set; }
[Required, StringLength(60)] public object City { get; set; }
[StringLength(15)] public object PostalCode { get; set; }
[Range(1, int.MaxValue)] public object Capacity { get; set; }
[Range(typeof(decimal), "0", "99999999.99")] public object HourlyRate { get; set; }
[StringLength(20)] public object ContactPhone { get; set; }
[StringLength(255)] public object ImagePath { get; set; }
}
[MetadataType(typeof(FacilityScheduleMetadata))] public partial class FacilitySchedule {}
public class FacilityScheduleMetadata {
[Range(1, 7)] public object DayOfWeek { get; set; }
}
[MetadataType(typeof(FacilitySportMetadata))] public partial class FacilitySport {}
public class FacilitySportMetadata {
}
[MetadataType(typeof(FacilityTypeMetadata))] public partial class FacilityType {}
public class FacilityTypeMetadata {
[Required, StringLength(60)] public object TypeName { get; set; }
[StringLength(250)] public object Description { get; set; }
}
[MetadataType(typeof(InquiryMetadata))] public partial class Inquiry {}
public class InquiryMetadata {
[Required, StringLength(100)] public object SenderName { get; set; }
[Required, StringLength(100), EmailAddress] public object SenderEmail { get; set; }
[StringLength(20)] public object SenderPhone { get; set; }
[Required, StringLength(150)] public object InquirySubject { get; set; }
[Required, StringLength(1000)] public object InquiryMessage { get; set; }
[Required, StringLength(20), RegularExpression("New|InProgress|Resolved|Closed")] public object InquiryStatus { get; set; }
[StringLength(1000)] public object ResponseMessage { get; set; }
}
[MetadataType(typeof(MemberMetadata))] public partial class Member {}
public class MemberMetadata {
[Required, StringLength(50)] public object FirstName { get; set; }
[Required, StringLength(50)] public object LastName { get; set; }
[Required, StringLength(100), EmailAddress] public object Email { get; set; }
[Required, StringLength(255)] public object PasswordHash { get; set; }
[Required, StringLength(20)] public object Phone { get; set; }
[Required, StringLength(150)] public object AddressLine { get; set; }
[Required, StringLength(60)] public object City { get; set; }
[StringLength(15)] public object PostalCode { get; set; }
[Required, StringLength(20), RegularExpression("Member|Admin")] public object UserRole { get; set; }
}
[MetadataType(typeof(MemberSportMetadata))] public partial class MemberSport {}
public class MemberSportMetadata {
}
[MetadataType(typeof(ReviewMetadata))] public partial class Review {}
public class ReviewMetadata {
[Range(1, 5)] public object OverallRating { get; set; }
[Range(1, 5)] public object ConditionRating { get; set; }
[Range(1, 5)] public object AmenitiesRating { get; set; }
[StringLength(100)] public object ReviewTitle { get; set; }
[Required, StringLength(500)] public object ReviewComment { get; set; }
}
[MetadataType(typeof(SportMetadata))] public partial class Sport {}
public class SportMetadata {
[Required, StringLength(50)] public object SportName { get; set; }
[StringLength(250)] public object Description { get; set; }
}
public partial class FacilitySchedule : IValidatableObject {
 public IEnumerable<ValidationResult> Validate(ValidationContext context) {
  if (OpeningTime < TimeSpan.Zero || ClosingTime >= TimeSpan.FromDays(1) || ClosingTime <= OpeningTime)
   yield return new ValidationResult("Closing time must be after opening time on the same day.", new[] { "ClosingTime" });
 }
}
}



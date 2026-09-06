# Community Sports Facilities Booking System Database Design

## Entity relationship diagram

```mermaid
erDiagram
    FACILITY_TYPE ||--o{ FACILITY : categorises
    FACILITY ||--o{ FACILITY_SPORT : supports
    SPORT ||--o{ FACILITY_SPORT : is_offered_at
    FACILITY ||--o{ FACILITY_SCHEDULE : has
    MEMBER ||--o{ MEMBER_SPORT : prefers
    SPORT ||--o{ MEMBER_SPORT : is_selected_by
    MEMBER ||--o{ BOOKING : makes
    FACILITY ||--o{ BOOKING : receives
    BOOKING ||--o| REVIEW : may_have

    FACILITY_TYPE {
        int FacilityTypeID PK
        nvarchar TypeName UK
        nvarchar Description
        bit IsActive
    }
    SPORT {
        int SportID PK
        nvarchar SportName UK
        nvarchar Description
        bit IsActive
    }
    MEMBER {
        int MemberID PK
        nvarchar FirstName
        nvarchar LastName
        nvarchar Email UK
        nvarchar PasswordHash
        nvarchar Phone
        nvarchar AddressLine
        nvarchar City
        nvarchar PostalCode
        datetime2 RegisteredAt
        bit IsActive
        nvarchar UserRole
    }
    MEMBER_SPORT {
        int MemberSportID PK
        int MemberID FK
        int SportID FK
        bit IsPrimaryPreference
        datetime2 SelectedAt
    }
    FACILITY {
        int FacilityID PK
        int FacilityTypeID FK
        nvarchar FacilityName
        nvarchar Description
        nvarchar AddressLine
        nvarchar City
        nvarchar PostalCode
        int Capacity
        decimal HourlyRate
        nvarchar ContactPhone
        nvarchar ImagePath
        bit IsActive
        datetime2 CreatedAt
    }
    FACILITY_SPORT {
        int FacilitySportID PK
        int FacilityID FK
        int SportID FK
        bit IsPrimarySport
        datetime2 AddedAt
    }
    FACILITY_SCHEDULE {
        int ScheduleID PK
        int FacilityID FK
        tinyint DayOfWeek
        time OpeningTime
        time ClosingTime
        bit IsAvailable
    }
    BOOKING {
        int BookingID PK
        int MemberID FK
        int FacilityID FK
        date BookingDate
        time StartTime
        time EndTime
        int NumberOfParticipants
        nvarchar Purpose
        nvarchar BookingStatus
        decimal TotalAmount
        datetime2 CreatedAt
        datetime2 CancelledAt
    }
    REVIEW {
        int ReviewID PK
        int BookingID FK_UK
        tinyint OverallRating
        tinyint ConditionRating
        tinyint AmenitiesRating
        nvarchar ReviewTitle
        nvarchar ReviewComment
        datetime2 SubmittedAt
        bit IsApproved
    }
    INQUIRY {
        int InquiryID PK
        nvarchar SenderName
        nvarchar SenderEmail
        nvarchar SenderPhone
        nvarchar InquirySubject
        nvarchar InquiryMessage
        datetime2 SubmittedAt
        nvarchar InquiryStatus
        nvarchar ResponseMessage
        datetime2 RespondedAt
    }
```

The Inquiry entity is independent because the coursework permits both anonymous guests and signed-in members to send a message. A Booking belongs to exactly one Member and one Facility. A Booking can have no review or one review because a member may review a completed booking once.

## Data dictionary

### FacilityType

| Attribute | Data type | Null | Key or constraint | Description |
|---|---|---:|---|---|
| FacilityTypeID | INT | No | Primary key, identity | Identifies the facility type. |
| TypeName | NVARCHAR(60) | No | Unique | Type name such as Court or Sports Hall. |
| Description | NVARCHAR(250) | Yes |  | Description of the type. |
| IsActive | BIT | No | Default 1 | Controls whether the type is available. |

### Sport

| Attribute | Data type | Null | Key or constraint | Description |
|---|---|---:|---|---|
| SportID | INT | No | Primary key, identity | Identifies the sport. |
| SportName | NVARCHAR(50) | No | Unique | Name of the sport. |
| Description | NVARCHAR(250) | Yes |  | Description of the sport. |
| IsActive | BIT | No | Default 1 | Controls whether the sport is selectable. |

### Member

| Attribute | Data type | Null | Key or constraint | Description |
|---|---|---:|---|---|
| MemberID | INT | No | Primary key, identity | Identifies a registered member. |
| FirstName | NVARCHAR(50) | No |  | Member's first name. |
| LastName | NVARCHAR(50) | No |  | Member's last name. |
| Email | NVARCHAR(100) | No | Unique | Login and contact email. |
| PasswordHash | NVARCHAR(255) | No |  | Secure password hash, never plain text. |
| Phone | NVARCHAR(20) | No |  | Member contact number. |
| AddressLine | NVARCHAR(150) | No |  | Member street address. |
| City | NVARCHAR(60) | No |  | Member city. |
| PostalCode | NVARCHAR(15) | Yes |  | Postal code. |
| RegisteredAt | DATETIME2(0) | No | Default current date/time | Registration timestamp. |
| IsActive | BIT | No | Default 1 | Controls login access. |
| UserRole | NVARCHAR(20) | No | Member or Admin | Stores the system role. Normal registration creates Member. |

### MemberSport

| Attribute | Data type | Null | Key or constraint | Description |
|---|---|---:|---|---|
| MemberSportID | INT | No | Primary key, identity | Identifies a preference record. |
| MemberID | INT | No | Foreign key to Member | Member selecting the sport. |
| SportID | INT | No | Foreign key to Sport | Selected sport. |
| IsPrimaryPreference | BIT | No | Default 0 | Marks the member's first preference. |
| SelectedAt | DATETIME2(0) | No | Default current date/time | Selection timestamp. |

The pair MemberID and SportID is unique.

### Facility

| Attribute | Data type | Null | Key or constraint | Description |
|---|---|---:|---|---|
| FacilityID | INT | No | Primary key, identity | Identifies a sports facility. |
| FacilityTypeID | INT | No | Foreign key to FacilityType | Categorises the facility. |
| FacilityName | NVARCHAR(100) | No |  | Public facility name. |
| Description | NVARCHAR(500) | Yes |  | Public description. |
| AddressLine | NVARCHAR(150) | No |  | Full address for signed-in members. |
| City | NVARCHAR(60) | No |  | Broad location used by guest and member search. |
| PostalCode | NVARCHAR(15) | Yes |  | Postal code. |
| Capacity | INT | No | Greater than 0 | Maximum participants. |
| HourlyRate | DECIMAL(10,2) | No | At least 0 | Booking price per hour. |
| ContactPhone | NVARCHAR(20) | Yes |  | Facility contact number. |
| ImagePath | NVARCHAR(255) | Yes |  | Optional facility image path. |
| IsActive | BIT | No | Default 1 | Controls public visibility and booking. |
| CreatedAt | DATETIME2(0) | No | Default current date/time | Creation timestamp. |

### FacilitySport

| Attribute | Data type | Null | Key or constraint | Description |
|---|---|---:|---|---|
| FacilitySportID | INT | No | Primary key, identity | Identifies a facility-sport assignment. |
| FacilityID | INT | No | Foreign key to Facility | Assigned facility. |
| SportID | INT | No | Foreign key to Sport | Supported sport. |
| IsPrimarySport | BIT | No | Default 0 | Marks the main sport. |
| AddedAt | DATETIME2(0) | No | Default current date/time | Assignment timestamp. |

The pair FacilityID and SportID is unique.

### FacilitySchedule

| Attribute | Data type | Null | Key or constraint | Description |
|---|---|---:|---|---|
| ScheduleID | INT | No | Primary key, identity | Identifies an opening schedule. |
| FacilityID | INT | No | Foreign key to Facility | Scheduled facility. |
| DayOfWeek | TINYINT | No | 1 to 7 | Monday is 1 and Sunday is 7. |
| OpeningTime | TIME(0) | No | Before ClosingTime | Opening time. |
| ClosingTime | TIME(0) | No | After OpeningTime | Closing time. |
| IsAvailable | BIT | No | Default 1 | Indicates whether bookings are accepted. |

The pair FacilityID and DayOfWeek is unique.

### Booking

| Attribute | Data type | Null | Key or constraint | Description |
|---|---|---:|---|---|
| BookingID | INT | No | Primary key, identity | Identifies a booking request. |
| MemberID | INT | No | Foreign key to Member | Member making the booking. |
| FacilityID | INT | No | Foreign key to Facility | Requested facility. |
| BookingDate | DATE | No |  | Requested date. |
| StartTime | TIME(0) | No | Before EndTime | Start of requested period. |
| EndTime | TIME(0) | No | After StartTime | End of requested period. |
| NumberOfParticipants | INT | No | Greater than 0 | Planned attendance. |
| Purpose | NVARCHAR(250) | Yes |  | Optional purpose. |
| BookingStatus | NVARCHAR(20) | No | Defined status list | Pending, Confirmed, Completed, Cancelled or Rejected. |
| TotalAmount | DECIMAL(10,2) | No | At least 0 | Duration multiplied by hourly rate. |
| CreatedAt | DATETIME2(0) | No | Default current date/time | Submission timestamp. |
| CancelledAt | DATETIME2(0) | Yes |  | Cancellation timestamp. |

### Review

| Attribute | Data type | Null | Key or constraint | Description |
|---|---|---:|---|---|
| ReviewID | INT | No | Primary key, identity | Identifies a review. |
| BookingID | INT | No | Foreign key, unique | Completed booking being reviewed. |
| OverallRating | TINYINT | No | 1 to 5 | Overall rating. |
| ConditionRating | TINYINT | No | 1 to 5 | Facility-condition rating. |
| AmenitiesRating | TINYINT | No | 1 to 5 | Amenities rating. |
| ReviewTitle | NVARCHAR(100) | Yes |  | Optional review heading. |
| ReviewComment | NVARCHAR(500) | No |  | Member's review text. |
| SubmittedAt | DATETIME2(0) | No | Default current date/time | Submission timestamp. |
| IsApproved | BIT | No | Default 1 | Controls display in public review search. |

### Inquiry

| Attribute | Data type | Null | Key or constraint | Description |
|---|---|---:|---|---|
| InquiryID | INT | No | Primary key, identity | Identifies an inquiry. |
| SenderName | NVARCHAR(100) | No |  | Guest or member name. |
| SenderEmail | NVARCHAR(100) | No |  | Reply email address. |
| SenderPhone | NVARCHAR(20) | Yes |  | Optional phone number. |
| InquirySubject | NVARCHAR(150) | No |  | Inquiry subject. |
| InquiryMessage | NVARCHAR(1000) | No |  | Inquiry content. |
| SubmittedAt | DATETIME2(0) | No | Default current date/time | Submission timestamp. |
| InquiryStatus | NVARCHAR(20) | No | Defined status list | New, InProgress, Resolved or Closed. |
| ResponseMessage | NVARCHAR(1000) | Yes |  | Optional council response. |
| RespondedAt | DATETIME2(0) | Yes |  | Response timestamp. |


USE CommunitySportsFacilitiesBookingDB;
GO

INSERT INTO Sport (SportName, Description, IsActive) VALUES
(N'Tennis', N'Racquet sport for singles and doubles.', 1),
(N'Football', N'Team field sport.', 1),
(N'Basketball', N'Indoor or outdoor court sport.', 1),
(N'Badminton', N'Indoor racquet sport.', 1);

INSERT INTO FacilityType (TypeName, Description, IsActive) VALUES
(N'Court', N'Indoor or outdoor marked court.', 1),
(N'Playing Field', N'Outdoor grass or artificial playing field.', 1),
(N'Sports Hall', N'Multi-purpose indoor sports venue.', 1);

DECLARE @CourtTypeID INT = (SELECT FacilityTypeID FROM FacilityType WHERE TypeName = N'Court');
DECLARE @FieldTypeID INT = (SELECT FacilityTypeID FROM FacilityType WHERE TypeName = N'Playing Field');
DECLARE @HallTypeID INT = (SELECT FacilityTypeID FROM FacilityType WHERE TypeName = N'Sports Hall');

INSERT INTO Facility
    (FacilityTypeID, FacilityName, Description, AddressLine, City, PostalCode,
     Capacity, HourlyRate, ContactPhone, ImagePath, IsActive, CreatedAt)
VALUES
(@CourtTypeID, N'Central Tennis Court', N'Outdoor community tennis court.',
 N'10 Park Road', N'Colombo', N'00700', 8, 2500.00, N'0112001001', NULL, 1, SYSDATETIME()),
(@FieldTypeID, N'Riverside Football Field', N'Full-size community football field.',
 N'25 Riverside Avenue', N'Colombo', N'00500', 30, 5000.00, N'0112001002', NULL, 1, SYSDATETIME()),
(@HallTypeID, N'Community Indoor Hall', N'Indoor hall suitable for basketball and badminton.',
 N'8 Council Lane', N'Kandy', N'20000', 40, 4000.00, N'0812001003', NULL, 1, SYSDATETIME());

DECLARE @TennisID INT = (SELECT SportID FROM Sport WHERE SportName = N'Tennis');
DECLARE @FootballID INT = (SELECT SportID FROM Sport WHERE SportName = N'Football');
DECLARE @BasketballID INT = (SELECT SportID FROM Sport WHERE SportName = N'Basketball');
DECLARE @BadmintonID INT = (SELECT SportID FROM Sport WHERE SportName = N'Badminton');
DECLARE @TennisCourtID INT = (SELECT FacilityID FROM Facility WHERE FacilityName = N'Central Tennis Court');
DECLARE @FootballFieldID INT = (SELECT FacilityID FROM Facility WHERE FacilityName = N'Riverside Football Field');
DECLARE @IndoorHallID INT = (SELECT FacilityID FROM Facility WHERE FacilityName = N'Community Indoor Hall');

INSERT INTO FacilitySport (FacilityID, SportID, IsPrimarySport, AddedAt) VALUES
(@TennisCourtID, @TennisID, 1, SYSDATETIME()),
(@FootballFieldID, @FootballID, 1, SYSDATETIME()),
(@IndoorHallID, @BasketballID, 1, SYSDATETIME()),
(@IndoorHallID, @BadmintonID, 0, SYSDATETIME());

INSERT INTO FacilitySchedule (FacilityID, DayOfWeek, OpeningTime, ClosingTime, IsAvailable)
SELECT f.FacilityID, d.DayNumber, CAST('08:00' AS TIME), CAST('20:00' AS TIME), 1
FROM Facility AS f
CROSS JOIN (VALUES (1), (2), (3), (4), (5), (6), (7)) AS d(DayNumber);

-- Register normal members through the web application so PasswordHash is generated securely.
-- The following inquiry demonstrates guest input data.
INSERT INTO Inquiry
    (SenderName, SenderEmail, SenderPhone, InquirySubject, InquiryMessage,
     SubmittedAt, InquiryStatus, ResponseMessage, RespondedAt)
VALUES
(N'Sample Guest', N'guest@example.com', N'0771234567',
 N'Membership information', N'Please send information about becoming a member.',
 SYSDATETIME(), N'New', NULL, NULL);
GO


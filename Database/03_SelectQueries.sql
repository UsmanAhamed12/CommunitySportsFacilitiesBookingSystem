USE CommunitySportsFacilitiesBookingDB;
GO

-- 1. Display active facilities with their type.
SELECT f.FacilityID, f.FacilityName, ft.TypeName, f.City, f.Capacity, f.HourlyRate
FROM Facility AS f
INNER JOIN FacilityType AS ft ON ft.FacilityTypeID = f.FacilityTypeID
WHERE f.IsActive = 1
ORDER BY f.FacilityName;

-- 2. Search facilities by sport and location.
SELECT DISTINCT f.FacilityName, f.City, s.SportName
FROM Facility AS f
INNER JOIN FacilitySport AS fs ON fs.FacilityID = f.FacilityID
INNER JOIN Sport AS s ON s.SportID = fs.SportID
WHERE f.IsActive = 1
  AND s.SportName = N'Tennis'
  AND f.City = N'Colombo';

-- 3. Display bookings with member and facility details.
SELECT b.BookingID,
       CONCAT(m.FirstName, N' ', m.LastName) AS MemberName,
       f.FacilityName, b.BookingDate, b.StartTime, b.EndTime,
       b.BookingStatus, b.TotalAmount
FROM Booking AS b
INNER JOIN Member AS m ON m.MemberID = b.MemberID
INNER JOIN Facility AS f ON f.FacilityID = b.FacilityID
ORDER BY b.BookingDate DESC, b.StartTime DESC;

-- 4. Check facilities available for a requested period.
DECLARE @BookingDate DATE = DATEADD(DAY, 1, CAST(GETDATE() AS DATE));
DECLARE @StartTime TIME(0) = '09:00';
DECLARE @EndTime TIME(0) = '10:00';
DECLARE @DayOfWeek TINYINT = CASE DATEPART(WEEKDAY, @BookingDate)
    WHEN 1 THEN 7 ELSE DATEPART(WEEKDAY, @BookingDate) - 1 END;

SELECT f.FacilityID, f.FacilityName, f.City, f.HourlyRate
FROM Facility AS f
WHERE f.IsActive = 1
  AND EXISTS (
      SELECT 1
      FROM FacilitySchedule AS sch
      WHERE sch.FacilityID = f.FacilityID
        AND sch.DayOfWeek = @DayOfWeek
        AND sch.IsAvailable = 1
        AND sch.OpeningTime <= @StartTime
        AND sch.ClosingTime >= @EndTime
  )
  AND NOT EXISTS (
      SELECT 1
      FROM Booking AS b
      WHERE b.FacilityID = f.FacilityID
        AND b.BookingDate = @BookingDate
        AND b.BookingStatus IN (N'Pending', N'Confirmed')
        AND @StartTime < b.EndTime
        AND @EndTime > b.StartTime
  );

-- 5. Display approved facility reviews.
SELECT f.FacilityName,
       r.OverallRating, r.ConditionRating, r.AmenitiesRating,
       r.ReviewTitle, r.ReviewComment, r.SubmittedAt
FROM Review AS r
INNER JOIN Booking AS b ON b.BookingID = r.BookingID
INNER JOIN Facility AS f ON f.FacilityID = b.FacilityID
WHERE r.IsApproved = 1
ORDER BY r.SubmittedAt DESC;

-- 6. Display each member's preferred sports.
SELECT CONCAT(m.FirstName, N' ', m.LastName) AS MemberName,
       s.SportName, ms.IsPrimaryPreference
FROM MemberSport AS ms
INNER JOIN Member AS m ON m.MemberID = ms.MemberID
INNER JOIN Sport AS s ON s.SportID = ms.SportID
ORDER BY MemberName, ms.IsPrimaryPreference DESC, s.SportName;

-- 7. Display inquiries submitted by guests or members.
SELECT InquiryID, SenderName, SenderEmail, InquirySubject,
       SubmittedAt, InquiryStatus
FROM Inquiry
ORDER BY SubmittedAt DESC;
GO


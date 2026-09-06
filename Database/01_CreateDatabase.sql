IF DB_ID(N'CommunitySportsFacilitiesBookingDB') IS NULL
    CREATE DATABASE CommunitySportsFacilitiesBookingDB;
GO

USE CommunitySportsFacilitiesBookingDB;
GO

CREATE TABLE FacilityType (
    FacilityTypeID INT IDENTITY(1,1) PRIMARY KEY,
    TypeName NVARCHAR(60) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_FacilityType_IsActive DEFAULT (1)
);

CREATE TABLE Sport (
    SportID INT IDENTITY(1,1) PRIMARY KEY,
    SportName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Sport_IsActive DEFAULT (1)
);

CREATE TABLE Member (
    MemberID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    AddressLine NVARCHAR(150) NOT NULL,
    City NVARCHAR(60) NOT NULL,
    PostalCode NVARCHAR(15) NULL,
    RegisteredAt DATETIME2(0) NOT NULL CONSTRAINT DF_Member_RegisteredAt DEFAULT (SYSDATETIME()),
    IsActive BIT NOT NULL CONSTRAINT DF_Member_IsActive DEFAULT (1),
    UserRole NVARCHAR(20) NOT NULL CONSTRAINT DF_Member_UserRole DEFAULT (N'Member'),
    CONSTRAINT CK_Member_UserRole CHECK (UserRole IN (N'Member', N'Admin'))
);

CREATE TABLE Facility (
    FacilityID INT IDENTITY(1,1) PRIMARY KEY,
    FacilityTypeID INT NOT NULL,
    FacilityName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    AddressLine NVARCHAR(150) NOT NULL,
    City NVARCHAR(60) NOT NULL,
    PostalCode NVARCHAR(15) NULL,
    Capacity INT NOT NULL,
    HourlyRate DECIMAL(10,2) NOT NULL,
    ContactPhone NVARCHAR(20) NULL,
    ImagePath NVARCHAR(255) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Facility_IsActive DEFAULT (1),
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Facility_CreatedAt DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_Facility_FacilityType FOREIGN KEY (FacilityTypeID)
        REFERENCES FacilityType(FacilityTypeID),
    CONSTRAINT CK_Facility_Capacity CHECK (Capacity > 0),
    CONSTRAINT CK_Facility_HourlyRate CHECK (HourlyRate >= 0)
);

CREATE TABLE MemberSport (
    MemberSportID INT IDENTITY(1,1) PRIMARY KEY,
    MemberID INT NOT NULL,
    SportID INT NOT NULL,
    IsPrimaryPreference BIT NOT NULL CONSTRAINT DF_MemberSport_Primary DEFAULT (0),
    SelectedAt DATETIME2(0) NOT NULL CONSTRAINT DF_MemberSport_SelectedAt DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_MemberSport_Member FOREIGN KEY (MemberID) REFERENCES Member(MemberID),
    CONSTRAINT FK_MemberSport_Sport FOREIGN KEY (SportID) REFERENCES Sport(SportID),
    CONSTRAINT UQ_MemberSport UNIQUE (MemberID, SportID)
);

CREATE TABLE FacilitySport (
    FacilitySportID INT IDENTITY(1,1) PRIMARY KEY,
    FacilityID INT NOT NULL,
    SportID INT NOT NULL,
    IsPrimarySport BIT NOT NULL CONSTRAINT DF_FacilitySport_Primary DEFAULT (0),
    AddedAt DATETIME2(0) NOT NULL CONSTRAINT DF_FacilitySport_AddedAt DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_FacilitySport_Facility FOREIGN KEY (FacilityID) REFERENCES Facility(FacilityID),
    CONSTRAINT FK_FacilitySport_Sport FOREIGN KEY (SportID) REFERENCES Sport(SportID),
    CONSTRAINT UQ_FacilitySport UNIQUE (FacilityID, SportID)
);

CREATE TABLE FacilitySchedule (
    ScheduleID INT IDENTITY(1,1) PRIMARY KEY,
    FacilityID INT NOT NULL,
    DayOfWeek TINYINT NOT NULL,
    OpeningTime TIME(0) NOT NULL,
    ClosingTime TIME(0) NOT NULL,
    IsAvailable BIT NOT NULL CONSTRAINT DF_FacilitySchedule_Available DEFAULT (1),
    CONSTRAINT FK_FacilitySchedule_Facility FOREIGN KEY (FacilityID) REFERENCES Facility(FacilityID),
    CONSTRAINT CK_FacilitySchedule_Day CHECK (DayOfWeek BETWEEN 1 AND 7),
    CONSTRAINT CK_FacilitySchedule_Time CHECK (ClosingTime > OpeningTime),
    CONSTRAINT UQ_FacilitySchedule UNIQUE (FacilityID, DayOfWeek)
);

CREATE TABLE Booking (
    BookingID INT IDENTITY(1,1) PRIMARY KEY,
    MemberID INT NOT NULL,
    FacilityID INT NOT NULL,
    BookingDate DATE NOT NULL,
    StartTime TIME(0) NOT NULL,
    EndTime TIME(0) NOT NULL,
    NumberOfParticipants INT NOT NULL,
    Purpose NVARCHAR(250) NULL,
    BookingStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_Booking_Status DEFAULT (N'Pending'),
    TotalAmount DECIMAL(10,2) NOT NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Booking_CreatedAt DEFAULT (SYSDATETIME()),
    CancelledAt DATETIME2(0) NULL,
    CONSTRAINT FK_Booking_Member FOREIGN KEY (MemberID) REFERENCES Member(MemberID),
    CONSTRAINT FK_Booking_Facility FOREIGN KEY (FacilityID) REFERENCES Facility(FacilityID),
    CONSTRAINT CK_Booking_Time CHECK (EndTime > StartTime),
    CONSTRAINT CK_Booking_Participants CHECK (NumberOfParticipants > 0),
    CONSTRAINT CK_Booking_Total CHECK (TotalAmount >= 0),
    CONSTRAINT CK_Booking_Status CHECK
        (BookingStatus IN (N'Pending', N'Confirmed', N'Completed', N'Cancelled', N'Rejected'))
);

CREATE TABLE Review (
    ReviewID INT IDENTITY(1,1) PRIMARY KEY,
    BookingID INT NOT NULL UNIQUE,
    OverallRating TINYINT NOT NULL,
    ConditionRating TINYINT NOT NULL,
    AmenitiesRating TINYINT NOT NULL,
    ReviewTitle NVARCHAR(100) NULL,
    ReviewComment NVARCHAR(500) NOT NULL,
    SubmittedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Review_SubmittedAt DEFAULT (SYSDATETIME()),
    IsApproved BIT NOT NULL CONSTRAINT DF_Review_IsApproved DEFAULT (1),
    CONSTRAINT FK_Review_Booking FOREIGN KEY (BookingID) REFERENCES Booking(BookingID),
    CONSTRAINT CK_Review_Overall CHECK (OverallRating BETWEEN 1 AND 5),
    CONSTRAINT CK_Review_Condition CHECK (ConditionRating BETWEEN 1 AND 5),
    CONSTRAINT CK_Review_Amenities CHECK (AmenitiesRating BETWEEN 1 AND 5)
);

CREATE TABLE Inquiry (
    InquiryID INT IDENTITY(1,1) PRIMARY KEY,
    SenderName NVARCHAR(100) NOT NULL,
    SenderEmail NVARCHAR(100) NOT NULL,
    SenderPhone NVARCHAR(20) NULL,
    InquirySubject NVARCHAR(150) NOT NULL,
    InquiryMessage NVARCHAR(1000) NOT NULL,
    SubmittedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Inquiry_SubmittedAt DEFAULT (SYSDATETIME()),
    InquiryStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_Inquiry_Status DEFAULT (N'New'),
    ResponseMessage NVARCHAR(1000) NULL,
    RespondedAt DATETIME2(0) NULL,
    CONSTRAINT CK_Inquiry_Status CHECK (InquiryStatus IN (N'New', N'InProgress', N'Resolved', N'Closed'))
);
GO


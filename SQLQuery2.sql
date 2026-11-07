IF EXISTS (SELECT * FROM sys.databases WHERE name = 'RaceDay')
BEGIN
    ALTER DATABASE RaceDay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDay;
END
GO

CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO

-- Create Roles table
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(255)
);

-- Create Users table
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    RoleID INT NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName VARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

-- Create OrganiserProfiles table
CREATE TABLE OrganiserProfiles (
    ProfileID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    CompanyName VARCHAR(100),
    Website VARCHAR(255),
    ContactPhone VARCHAR(20),
    CONSTRAINT FK_OrganiserProfiles_Users FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);

-- Create ParticipantProfiles table
CREATE TABLE ParticipantProfiles (
    ProfileID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    DateOfBirth DATE,
    Gender VARCHAR(20),
    EmergencyContact VARCHAR(100),
    MedicalNotes VARCHAR(MAX),
    CONSTRAINT FK_ParticipantProfiles_Users FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);

-- Create Events table
CREATE TABLE Events (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    Location VARCHAR(100) NOT NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    Status VARCHAR(20) DEFAULT 'draft',
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserID) REFERENCES Users(UserID)
);

-- Create Categories table
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    Name VARCHAR(50) NOT NULL,
    Description VARCHAR(255),
    Distance DECIMAL(5,2) NOT NULL,
    MinAge INT,
    MaxAge INT,
    MaxCapacity INT NOT NULL,
    CurrentEnrolments INT DEFAULT 0,
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventID) REFERENCES Events(EventID) ON DELETE CASCADE
);

-- Create EventEnrolments table
CREATE TABLE EventEnrolments (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryID INT NOT NULL,
    ParticipantID INT NOT NULL,
    EnrolmentDate DATETIME2 DEFAULT GETDATE(),
    Status VARCHAR(20) DEFAULT 'pending',
    BibNumber VARCHAR(20) UNIQUE,
    CONSTRAINT FK_EventEnrolments_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    CONSTRAINT FK_EventEnrolments_Users FOREIGN KEY (ParticipantID) REFERENCES Users(UserID),
    CONSTRAINT UQ_Enrolment UNIQUE (CategoryID, ParticipantID)
);

-- Create Results table
CREATE TABLE Results (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    FinishTime TIME(3),
    Position INT,
    Status VARCHAR(20) DEFAULT 'entered',
    Notes VARCHAR(500),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentID) REFERENCES EventEnrolments(EnrolmentID)
);

-- ============================================================
-- INSERT SAMPLE DATA
-- ============================================================

-- Insert Roles
INSERT INTO Roles (RoleName, Description) VALUES 
('Organiser', 'Can create and manage events'),
('Participant', 'Can enrol in events and view results');

-- Insert Users (2 Organisers, 2 Participants)
INSERT INTO Users (RoleID, Email, PasswordHash, FullName) VALUES
(1, 'john@raceday.com', 'hashed_password_123', 'John Smith'),
(1, 'sarah@raceday.com', 'hashed_password_456', 'Sarah Johnson'),
(2, 'mike@email.com', 'hashed_password_789', 'Mike Thompson'),
(2, 'emma@email.com', 'hashed_password_101', 'Emma Davis');

-- Insert Organiser Profiles
INSERT INTO OrganiserProfiles (UserID, CompanyName, Website, ContactPhone) VALUES
(1, 'Fast Track Events', 'www.fasttrackevents.com', '555-0101'),
(2, 'Coastal Run Ltd', 'www.coastalrun.com', '555-0102');

-- Insert Participant Profiles
INSERT INTO ParticipantProfiles (UserID, DateOfBirth, Gender, EmergencyContact, MedicalNotes) VALUES
(3, '1990-05-15', 'Male', 'Jane - 555-0103', 'No allergies'),
(4, '1985-12-22', 'Female', 'Robert - 555-0104', 'Mild asthma');

-- Insert Events (3 events as required)
INSERT INTO Events (OrganiserID, Name, Description, Location, StartDate, EndDate, Status) VALUES
(1, 'City Marathon 2026', 'Annual city marathon', 'Downtown City Centre', '2026-11-15 07:00', '2026-11-15 18:00', 'open'),
(1, 'Summer Fun Run', 'Family friendly 5km', 'Central Park', '2026-12-01 08:00', '2026-12-01 12:00', 'draft'),
(2, 'Coastal Trail Run', 'Scenic trail run', 'Coastal Reserve', '2026-12-05 08:00', '2026-12-05 16:00', 'open');

-- Insert Categories for each event
INSERT INTO Categories (EventID, Name, Description, Distance, MinAge, MaxAge, MaxCapacity) VALUES
(1, 'Full Marathon', '42.2km full marathon', 42.20, 18, 65, 200),
(1, 'Half Marathon', '21.1km half marathon', 21.10, 16, 70, 300),
(1, '5km Fun Run', '5km charity run', 5.00, 8, 99, 150),
(2, '5km Walk', '5km leisure walk', 5.00, 5, 99, 100),
(2, '10km Run', '10km competitive run', 10.00, 12, 70, 80),
(3, '15km Trail', '15km challenging trail', 15.00, 18, 60, 100),
(3, '25km Endurance', '25km endurance trail', 25.00, 21, 55, 75);

-- Insert Enrolments
INSERT INTO EventEnrolments (CategoryID, ParticipantID, Status, BibNumber) VALUES
(2, 3, 'approved', 'B001'),
(1, 4, 'pending', 'B002'),
(4, 3, 'approved', 'B003'),
(5, 4, 'pending', 'B004');

-- Insert Results
INSERT INTO Results (EnrolmentID, FinishTime, Position, Status) VALUES
(1, '01:45:30', 25, 'completed'),
(3, '00:28:45', 12, 'completed');

-- Update Current Enrolments count
UPDATE Categories SET CurrentEnrolments = 1 WHERE CategoryID = 1;
UPDATE Categories SET CurrentEnrolments = 1 WHERE CategoryID = 2;
UPDATE Categories SET CurrentEnrolments = 1 WHERE CategoryID = 4;
UPDATE Categories SET CurrentEnrolments = 1 WHERE CategoryID = 5;

-- VERIFICATION
PRINT '=== RACEDAY DATABASE CREATED SUCCESSFULLY! ===';
SELECT COUNT(*) AS TotalRoles FROM Roles;
SELECT COUNT(*) AS TotalUsers FROM Users;
SELECT COUNT(*) AS TotalEvents FROM Events;
SELECT COUNT(*) AS TotalCategories FROM Categories;
SELECT COUNT(*) AS TotalEnrolments FROM EventEnrolments;
SELECT COUNT(*) AS TotalResults FROM Results;

GO
CREATE VIEW vw_EventEnrolments AS
SELECT 
    e.EnrolmentID,
    ev.Name AS EventName,
    c.Name AS CategoryName,
    u.FullName AS ParticipantName,
    e.Status AS EnrolmentStatus,
    e.BibNumber,
    e.EnrolmentDate
FROM EventEnrolments e
JOIN Categories c ON e.CategoryID = c.CategoryID
JOIN Events ev ON c.EventID = ev.EventID
JOIN Users u ON e.ParticipantID = u.UserID;

GO
CREATE VIEW vw_EventResults AS
SELECT 
    r.ResultID,
    u.FullName AS ParticipantName,
    ev.Name AS EventName,
    c.Name AS CategoryName,
    r.FinishTime,
    r.Position,
    r.Status AS ResultStatus
FROM Results r
JOIN EventEnrolments e ON r.EnrolmentID = e.EnrolmentID
JOIN Users u ON e.ParticipantID = u.UserID
JOIN Categories c ON e.CategoryID = c.CategoryID
JOIN Events ev ON c.EventID = ev.EventID;
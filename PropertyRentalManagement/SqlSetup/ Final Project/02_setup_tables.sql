-- ============================================================
-- STEP 2 of 2: Run this connected to PropertyRentalDB
-- Creates all tables and inserts test data
-- NO "GO" statements - works perfectly in DBeaver
-- ============================================================

-- TABLE 1: Users
CREATE TABLE Users (
    UserId       INT IDENTITY(1,1) PRIMARY KEY,
    FirstName    NVARCHAR(50)  NOT NULL,
    LastName     NVARCHAR(50)  NOT NULL,
    Email        NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Role         NVARCHAR(20)  NOT NULL CHECK (Role IN ('Owner', 'Manager', 'Tenant')),
    DateCreated  DATETIME      DEFAULT GETDATE(),
    IsActive     BIT           DEFAULT 1
);

-- TABLE 2: Buildings
CREATE TABLE Buildings (
    BuildingId  INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    Address     NVARCHAR(200) NOT NULL,
    City        NVARCHAR(50)  NOT NULL,
    PostalCode  NVARCHAR(20)  NOT NULL,
    Description NVARCHAR(MAX),
    ManagerId   INT NOT NULL,
    FOREIGN KEY (ManagerId) REFERENCES Users(UserId)
);

-- TABLE 3: Apartments
CREATE TABLE Apartments (
    ApartmentId     INT IDENTITY(1,1) PRIMARY KEY,
    BuildingId      INT           NOT NULL,
    ApartmentNumber NVARCHAR(20)  NOT NULL,
    Rooms           INT           NOT NULL,
    Bathrooms       DECIMAL(3,1)  NOT NULL,
    RentAmount      DECIMAL(10,2) NOT NULL,
    Status          NVARCHAR(20)  NOT NULL CHECK (Status IN ('Available', 'Rented', 'Under Maintenance')),
    FOREIGN KEY (BuildingId) REFERENCES Buildings(BuildingId)
);

-- TABLE 4: Appointments
CREATE TABLE Appointments (
    AppointmentId   INT IDENTITY(1,1) PRIMARY KEY,
    TenantId        INT          NOT NULL,
    ManagerId       INT          NOT NULL,
    ApartmentId     INT          NOT NULL,
    AppointmentDate DATETIME     NOT NULL,
    Status          NVARCHAR(20) NOT NULL CHECK (Status IN ('Scheduled', 'Completed', 'Cancelled')),
    Notes           NVARCHAR(MAX),
    FOREIGN KEY (TenantId)    REFERENCES Users(UserId),
    FOREIGN KEY (ManagerId)   REFERENCES Users(UserId),
    FOREIGN KEY (ApartmentId) REFERENCES Apartments(ApartmentId)
);

-- TABLE 5: Messages
CREATE TABLE Messages (
    MessageId  INT IDENTITY(1,1) PRIMARY KEY,
    SenderId   INT           NOT NULL,
    ReceiverId INT           NOT NULL,
    Subject    NVARCHAR(100) NOT NULL,
    Body       NVARCHAR(MAX) NOT NULL,
    SentDate   DATETIME      DEFAULT GETDATE(),
    IsRead     BIT           DEFAULT 0,
    FOREIGN KEY (SenderId)   REFERENCES Users(UserId),
    FOREIGN KEY (ReceiverId) REFERENCES Users(UserId)
);

-- ============================================================
-- INSERT TEST DATA
-- ============================================================

-- Users: 1 Owner, 2 Managers, 2 Tenants
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role) VALUES
('Admin', 'Owner',   'admin@propertyrental.com',       'hashed_password_placeholder', 'Owner'),
('John',  'Smith',   'john.manager@propertyrental.com', 'hashed_password_placeholder', 'Manager'),
('Sarah', 'Johnson', 'sarah.manager@propertyrental.com','hashed_password_placeholder', 'Manager'),
('Mike',  'Tenant',  'mike.tenant@gmail.com',           'hashed_password_placeholder', 'Tenant'),
('Emma',  'Davis',   'emma.tenant@yahoo.com',           'hashed_password_placeholder', 'Tenant');

-- Buildings (John=UserId 2, Sarah=UserId 3)
INSERT INTO Buildings (Name, Address, City, PostalCode, Description, ManagerId) VALUES
('Sunset Apartments', '123 Sunset Blvd', 'Montreal', 'H3Z 2Y7', 'Luxury apartments downtown',  2),
('Maple View',        '456 Maple St',    'Montreal', 'H2X 1Y6', 'Quiet neighborhood building', 3);

-- Apartments
INSERT INTO Apartments (BuildingId, ApartmentNumber, Rooms, Bathrooms, RentAmount, Status) VALUES
(1, '101', 3, 1.0, 1500.00, 'Available'),
(1, '102', 4, 2.0, 2000.00, 'Rented'),
(1, '201', 2, 1.0, 1200.00, 'Under Maintenance'),
(2, 'A1',  3, 1.0, 1400.00, 'Available'),
(2, 'B2',  5, 2.5, 2500.00, 'Rented');

-- Appointments (Mike=4, Emma=5 are Tenants)
INSERT INTO Appointments (TenantId, ManagerId, ApartmentId, AppointmentDate, Status, Notes) VALUES
(4, 2, 1, DATEADD(day,  2, GETDATE()), 'Scheduled', 'Interested in 3 rooms'),
(5, 3, 4, DATEADD(day, -1, GETDATE()), 'Completed',  'Liked the apartment, will apply');

-- Messages
INSERT INTO Messages (SenderId, ReceiverId, Subject, Body) VALUES
(4, 2, 'Question about parking',    'Is there indoor parking available for apt 101?'),
(2, 4, 'RE: Question about parking','Yes, indoor parking is an extra $100/month.');

PRINT 'All tables created and test data inserted successfully!';

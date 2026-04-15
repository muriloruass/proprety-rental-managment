-- Run this connected to PropertyRentalDB in DBeaver

-- Add Reports table (Manager reports events to Owner)
CREATE TABLE Reports (
    ReportId    INT IDENTITY(1,1) PRIMARY KEY,
    ManagerId   INT           NOT NULL,
    Title       NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    ReportDate  DATETIME      DEFAULT GETDATE(),
    FOREIGN KEY (ManagerId) REFERENCES Users(UserId)
);

-- Insert test data
INSERT INTO Reports (ManagerId, Title, Description) VALUES
(2, 'Water leak in unit 201', 'There is a water leak in apartment 201. Maintenance has been notified and the unit is under repair.'),
(3, 'Noise complaint in building B2', 'Tenant in B2 reported excessive noise from neighbors. Situation was resolved after a warning was issued.');

-- Verify
SELECT r.ReportId, r.Title, r.Description, r.ReportDate,
       u.FirstName + ' ' + u.LastName AS ManagerName
FROM Reports r
JOIN Users u ON r.ManagerId = u.UserId;

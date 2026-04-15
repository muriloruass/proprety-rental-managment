
USE PropertyRentalDB;

-- 1. Show all Users with their Roles
SELECT UserId,
       CONCAT(FirstName, ' ', LastName) AS FullName,
       Email,
       Role AS UserRole
FROM Users;

-- 2. Show Buildings and their assigned Property Managers
SELECT b.Name AS BuildingName, b.Address, b.Description,
       CONCAT(u.FirstName, ' ', u.LastName) AS ManagerName,
       u.Email AS ManagerEmail
FROM Buildings b
JOIN Users u ON b.ManagerId = u.UserId;

-- 3. Show all Apartments with their current Status
SELECT a.ApartmentNumber, b.Name AS BuildingName, a.Status
FROM Apartments a
JOIN Buildings b ON a.BuildingId = b.BuildingId;

-- 4. Show Appointments between Tenants and Managers
SELECT appt.AppointmentDate, appt.Status, a.ApartmentNumber,
       CONCAT(t.FirstName, ' ', t.LastName) AS TenantName,
       CONCAT(m.FirstName, ' ', m.LastName) AS ManagerName
FROM Appointments appt
JOIN Users t ON appt.TenantId = t.UserId
JOIN Users m ON appt.ManagerId = m.UserId
JOIN Apartments a ON appt.ApartmentId = a.ApartmentId;

-- 5. Show Messages sent between users
SELECT msg.SentDate, msg.Subject, msg.Body AS Content,
       CONCAT(s.FirstName, ' ', s.LastName) AS Sender,
       CONCAT(r.FirstName, ' ', r.LastName) AS Receiver
FROM Messages msg
JOIN Users s ON msg.SenderId = s.UserId
JOIN Users r ON msg.ReceiverId = r.UserId;

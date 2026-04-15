
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'PropertyRentalDB')
BEGIN
    ALTER DATABASE PropertyRentalDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PropertyRentalDB;
END

CREATE DATABASE PropertyRentalDB;

PRINT 'Database PropertyRentalDB created! Now run 02_setup_tables.sql';

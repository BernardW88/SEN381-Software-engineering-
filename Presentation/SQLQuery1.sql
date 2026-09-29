-- Drop old tables if re-creating
IF OBJECT_ID('dbo.ServiceRequests', 'U') IS NOT NULL DROP TABLE dbo.ServiceRequests;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;

-- Users Table
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL -- Citizen, FieldStaff, Admin
);

-- Service Requests Table
CREATE TABLE ServiceRequests (
    RequestID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50) NOT NULL, -- Pothole, Water Outage, Electricity, Waste
    Location NVARCHAR(150) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Submitted', -- Submitted, In Progress, Resolved, Closed
    Priority NVARCHAR(20) NOT NULL DEFAULT 'Medium', -- Low, Medium, High, Critical
    DateCreated DATETIME NOT NULL DEFAULT GETDATE(),
    LastModifiedBy NVARCHAR(50) NULL
);

-- Seed Initial Users
INSERT INTO Users (Username, Password, Role) VALUES 
('admin', 'admin123', 'Admin'),
('citizen1', 'pass123', 'Citizen'),
('staff_water', 'staff123', 'FieldStaff');

-- Seed Initial Service Requests
INSERT INTO ServiceRequests (Title, Category, Location, Description, Status, Priority, LastModifiedBy) VALUES 
('Burst Pipe in Sector 4', 'Water Outage', '123 Main Street, Pretoria', 'Major water leak on the sidewalk flooding road.', 'In Progress', 'High', 'admin'),
('Pothole on Old Johannesburg Rd', 'Pothole', 'Corner 5th Ave and Old Joburg Rd', 'Deep pothole causing vehicle tire damage.', 'Submitted', 'Medium', 'citizen1'),
('Streetlight Failure', 'Electricity', 'Block B Park Road', 'All streetlights out on main access road.', 'Resolved', 'Low', 'staff_water');
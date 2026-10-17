-- Run this migration once against your CivicConnect database.
-- It enables optimistic concurrency and transactional status audit persistence.

IF COL_LENGTH('dbo.ServiceRequests', 'UpdatedAt') IS NULL
BEGIN
	ALTER TABLE dbo.ServiceRequests
	ADD UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_ServiceRequests_UpdatedAt DEFAULT SYSUTCDATETIME();
END
GO

IF OBJECT_ID('dbo.ServiceRequestStatusAudit', 'U') IS NULL
BEGIN
	CREATE TABLE dbo.ServiceRequestStatusAudit
	(
		AuditId INT IDENTITY(1,1) PRIMARY KEY,
		RequestID INT NOT NULL,
		PreviousStatus NVARCHAR(100) NOT NULL,
		NewStatus NVARCHAR(100) NOT NULL,
		ChangedBy NVARCHAR(100) NOT NULL,
		ChangedAtUtc DATETIME2 NOT NULL,
		CONSTRAINT FK_ServiceRequestStatusAudit_ServiceRequests
			FOREIGN KEY (RequestID) REFERENCES dbo.ServiceRequests(RequestID)
	);
END
GO

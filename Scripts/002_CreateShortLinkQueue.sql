/*
    جدول صف عمومی برای تولید لینک کوتاه
    هر سیستمی که لینک طولانی دارد می‌تواند از این صف استفاده کند.
*/
USE [apiweb-locationsmap];
GO

IF OBJECT_ID('dbo.ShortLinkQueue', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ShortLinkQueue
    (
        Id              BIGINT IDENTITY(1,1) NOT NULL,
        SourceSchema    NVARCHAR(128) NOT NULL CONSTRAINT DF_ShortLinkQueue_SourceSchema DEFAULT (N'dbo'),
        SourceTable     NVARCHAR(128) NOT NULL,
        SourceKeyColumn NVARCHAR(128) NOT NULL,
        SourceKeyValue  NVARCHAR(200) NOT NULL,
        LongUrl         NVARCHAR(2000) NOT NULL,
        TargetColumn    NVARCHAR(128) NOT NULL,
        ShortUrl        NVARCHAR(500) NULL,
        Status          TINYINT NOT NULL CONSTRAINT DF_ShortLinkQueue_Status DEFAULT (0),
        -- 0=Pending, 1=Processing, 2=Done, 3=Failed
        AttemptCount    INT NOT NULL CONSTRAINT DF_ShortLinkQueue_AttemptCount DEFAULT (0),
        LastError       NVARCHAR(MAX) NULL,
        CreatedAt       DATETIME2(3) NOT NULL CONSTRAINT DF_ShortLinkQueue_CreatedAt DEFAULT (SYSUTCDATETIME()),
        ProcessedAt     DATETIME2(3) NULL,
        CONSTRAINT PK_ShortLinkQueue PRIMARY KEY CLUSTERED (Id)
    );

    CREATE NONCLUSTERED INDEX IX_ShortLinkQueue_Status_CreatedAt
        ON dbo.ShortLinkQueue (Status, CreatedAt)
        INCLUDE (SourceTable, SourceKeyValue, LongUrl, TargetColumn, AttemptCount);

    CREATE NONCLUSTERED INDEX IX_ShortLinkQueue_Source
        ON dbo.ShortLinkQueue (SourceTable, SourceKeyValue, Status);
END
GO

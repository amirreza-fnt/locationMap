/*
    جداول و فیلدهای لینک بازدید + صف لینک کوتاه
    دیتابیس: apiweb-locationsmap
    اجرا با sqlcmd یا SSMS

    این اسکریپت:
      1) ستون VisitLink و ShortVisitLink روی MapPoints
      2) جدول ShortLinkQueue
      3) تریگر صف‌گذاری خودکار
      4) صف‌گذاری نقاط موجود بدون لینک کوتاه
      5) دسترسی کاربر سرویس
*/
USE [apiweb-locationsmap];
GO

IF COL_LENGTH('dbo.MapPoints', 'ShortVisitLink') IS NULL
BEGIN
    ALTER TABLE dbo.MapPoints
        ADD ShortVisitLink NVARCHAR(500) NULL;
END
GO

IF COL_LENGTH('dbo.MapPoints', 'VisitLink') IS NULL
BEGIN
    ALTER TABLE dbo.MapPoints
        ADD VisitLink AS (
            N'https://map.sabzevar.ir/?layers='
            + CAST(CategoryId AS NVARCHAR(36))
            + N'&id='
            + CAST(Id AS NVARCHAR(36))
        ) PERSISTED;
END
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

CREATE OR ALTER TRIGGER dbo.tr_MapPoints_EnqueueShortLink
ON dbo.MapPoints
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE mp
    SET mp.ShortVisitLink = NULL
    FROM dbo.MapPoints AS mp
    INNER JOIN inserted AS i ON mp.Id = i.Id
    INNER JOIN deleted AS d ON d.Id = i.Id
    WHERE i.CategoryId <> d.CategoryId;

    INSERT INTO dbo.ShortLinkQueue (
        SourceSchema,
        SourceTable,
        SourceKeyColumn,
        SourceKeyValue,
        LongUrl,
        TargetColumn,
        Status,
        CreatedAt
    )
    SELECT
        N'dbo',
        N'MapPoints',
        N'Id',
        CAST(i.Id AS NVARCHAR(36)),
        N'https://map.sabzevar.ir/?layers='
            + CAST(i.CategoryId AS NVARCHAR(36))
            + N'&id='
            + CAST(i.Id AS NVARCHAR(36)),
        N'ShortVisitLink',
        0,
        SYSUTCDATETIME()
    FROM inserted AS i
    LEFT JOIN deleted AS d ON i.Id = d.Id
    WHERE
        (d.Id IS NULL OR i.CategoryId <> d.CategoryId)
        AND i.ShortVisitLink IS NULL
        AND NOT EXISTS (
            SELECT 1
            FROM dbo.ShortLinkQueue AS q
            WHERE q.SourceTable = N'MapPoints'
              AND q.SourceKeyValue = CAST(i.Id AS NVARCHAR(36))
              AND q.Status IN (0, 1)
        );
END
GO

INSERT INTO dbo.ShortLinkQueue (
    SourceSchema,
    SourceTable,
    SourceKeyColumn,
    SourceKeyValue,
    LongUrl,
    TargetColumn,
    Status,
    CreatedAt
)
SELECT
    N'dbo',
    N'MapPoints',
    N'Id',
    CAST(mp.Id AS NVARCHAR(36)),
    N'https://map.sabzevar.ir/?layers='
        + CAST(mp.CategoryId AS NVARCHAR(36))
        + N'&id='
        + CAST(mp.Id AS NVARCHAR(36)),
    N'ShortVisitLink',
    0,
    SYSUTCDATETIME()
FROM dbo.MapPoints AS mp
WHERE mp.ShortVisitLink IS NULL
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.ShortLinkQueue AS q
        WHERE q.SourceTable = N'MapPoints'
          AND q.SourceKeyValue = CAST(mp.Id AS NVARCHAR(36))
          AND q.Status IN (0, 1)
    );
GO

IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'apiweblocationsmapuser')
BEGIN
    GRANT SELECT, INSERT, UPDATE ON dbo.ShortLinkQueue TO [apiweblocationsmapuser];
    GRANT SELECT, UPDATE ON dbo.MapPoints TO [apiweblocationsmapuser];
END
GO

SELECT
    N'MapPoints.VisitLink' AS ObjectName,
    CASE WHEN COL_LENGTH('dbo.MapPoints', 'VisitLink') IS NULL THEN N'MISSING' ELSE N'OK' END AS Status
UNION ALL
SELECT
    N'MapPoints.ShortVisitLink',
    CASE WHEN COL_LENGTH('dbo.MapPoints', 'ShortVisitLink') IS NULL THEN N'MISSING' ELSE N'OK' END
UNION ALL
SELECT
    N'ShortLinkQueue',
    CASE WHEN OBJECT_ID('dbo.ShortLinkQueue', 'U') IS NULL THEN N'MISSING' ELSE N'OK' END
UNION ALL
SELECT
    N'tr_MapPoints_EnqueueShortLink',
    CASE WHEN OBJECT_ID('dbo.tr_MapPoints_EnqueueShortLink', 'TR') IS NULL THEN N'MISSING' ELSE N'OK' END;
GO

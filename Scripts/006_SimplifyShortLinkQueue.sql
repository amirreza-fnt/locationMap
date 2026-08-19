/*
    ساده‌سازی صف لینک کوتاه طبق نظر کارفرما:
    فقط PointId در صف می‌ماند.
    لینک کوتاه موفق روی MapPoints.ShortVisitLink ذخیره می‌شود.
    خطا ذخیره نمی‌شود؛ سیکل بعد دوباره تلاش می‌شود.
*/
USE [apiweb-locationsmap];
GO

IF OBJECT_ID('dbo.tr_MapPoints_EnqueueShortLink', 'TR') IS NOT NULL
    DROP TRIGGER dbo.tr_MapPoints_EnqueueShortLink;
GO

IF OBJECT_ID('dbo.ShortLinkQueue', 'U') IS NOT NULL
    DROP TABLE dbo.ShortLinkQueue;
GO

CREATE TABLE dbo.ShortLinkQueue
(
    PointId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_ShortLinkQueue PRIMARY KEY CLUSTERED (PointId)
);
GO

CREATE TRIGGER dbo.tr_MapPoints_EnqueueShortLink
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

    INSERT INTO dbo.ShortLinkQueue (PointId)
    SELECT i.Id
    FROM inserted AS i
    LEFT JOIN deleted AS d ON i.Id = d.Id
    WHERE
        (d.Id IS NULL OR i.CategoryId <> d.CategoryId)
        AND i.ShortVisitLink IS NULL
        AND NOT EXISTS (
            SELECT 1 FROM dbo.ShortLinkQueue AS q WHERE q.PointId = i.Id
        );
END
GO

INSERT INTO dbo.ShortLinkQueue (PointId)
SELECT mp.Id
FROM dbo.MapPoints AS mp
WHERE mp.ShortVisitLink IS NULL
  AND NOT EXISTS (
        SELECT 1 FROM dbo.ShortLinkQueue AS q WHERE q.PointId = mp.Id
    );
GO

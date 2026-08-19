/*
    تریگر صف لینک کوتاه برای MapPoints
    فقط PointId را در صف می‌گذارد. اگر لینک کوتاه از قبل باشد، وارد صف نمی‌شود.
*/
USE [apiweb-locationsmap];
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

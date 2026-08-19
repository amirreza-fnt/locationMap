/*
    صف‌گذاری نقاط موجود که هنوز لینک کوتاه ندارند.
*/
USE [apiweb-locationsmap];
GO

INSERT INTO dbo.ShortLinkQueue (PointId)
SELECT mp.Id
FROM dbo.MapPoints AS mp
WHERE mp.ShortVisitLink IS NULL
  AND NOT EXISTS (
        SELECT 1 FROM dbo.ShortLinkQueue AS q WHERE q.PointId = mp.Id
    );
GO

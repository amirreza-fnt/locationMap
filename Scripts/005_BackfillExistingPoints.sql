/*
    صف‌گذاری نقاط موجود که هنوز لینک کوتاه ندارند.
    بعد از 001 تا 003 اجرا شود.
*/
USE [apiweb-locationsmap];
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

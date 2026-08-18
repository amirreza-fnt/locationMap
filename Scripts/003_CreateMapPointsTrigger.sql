/*
    تریگر صف لینک کوتاه برای MapPoints
    فقط رکورد را در صف می‌گذارد — هیچ فراخوانی HTTP از داخل تریگر انجام نمی‌شود.
*/
USE [apiweb-locationsmap];
GO

CREATE OR ALTER TRIGGER dbo.tr_MapPoints_EnqueueShortLink
ON dbo.MapPoints
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- اگر دسته‌بندی عوض شد، لینک کوتاه قبلی باطل می‌شود
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

/*
    جدول صف لینک کوتاه — فقط شناسه نقطه.
    لینک کوتاه موفق روی MapPoints.ShortVisitLink ذخیره می‌شود.
*/
USE [apiweb-locationsmap];
GO

IF OBJECT_ID('dbo.ShortLinkQueue', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ShortLinkQueue
    (
        PointId UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT PK_ShortLinkQueue PRIMARY KEY CLUSTERED (PointId)
    );
END
GO

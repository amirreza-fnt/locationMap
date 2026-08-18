/*
    فیلد VisitLink (محاسباتی) و ShortVisitLink روی جدول MapPoints
    دیتابیس: apiweb-locationsmap
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

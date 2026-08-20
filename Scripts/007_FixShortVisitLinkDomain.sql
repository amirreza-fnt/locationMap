/*
    لینک‌های کوتاه ذخیره‌شده با IP را به دامنه sbzl.ir تبدیل می‌کند.
*/
USE [apiweb-locationsmap];
GO

UPDATE dbo.MapPoints
SET ShortVisitLink = REPLACE(
        REPLACE(ShortVisitLink, N'http://185.255.91.242:5013', N'https://sbzl.ir'),
        N'https://185.255.91.242:5013', N'https://sbzl.ir')
WHERE ShortVisitLink LIKE N'%185.255.91.242%';
GO

SELECT TOP 10 Id, ShortVisitLink, VisitLink
FROM dbo.MapPoints
WHERE ShortVisitLink IS NOT NULL;
GO

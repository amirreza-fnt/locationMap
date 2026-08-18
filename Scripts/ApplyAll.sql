/*
    اجرای همه اسکریپت‌های لینک بازدید و صف لینک کوتاه
    ترتیب اجرا مهم است.
*/
:r 001_AddVisitLinkColumns.sql
GO
:r 002_CreateShortLinkQueue.sql
GO
:r 003_CreateMapPointsTrigger.sql
GO

-- اسکریپت 004 (SQL Agent Job) را جداگانه و با مقادیر محیط تولید اجرا کنید:
-- :r 004_CreateSqlAgentJob.sql

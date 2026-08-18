/*
    SQL Agent Job برای پردازش صف لینک کوتاه
    این جاب هر ۱۰ ثانیه میکروسرویس Bridge را صدا می‌زند.

    پیش‌نیاز:
    - SQL Server Agent فعال باشد
    - PowerShell execution policy مناسب باشد
    - سرویس ShortLinkBridge روی پورت 5014 در حال اجرا باشد
    - ApiKey در appsettings Bridge با مقدار زیر یکسان باشد
*/
USE msdb;
GO

DECLARE @jobName SYSNAME = N'ShortLinkQueue_Processor';
DECLARE @bridgeUrl NVARCHAR(500) = N'http://127.0.0.1:5014/api/queue/process?batchSize=50';
DECLARE @apiKey NVARCHAR(200) = N'CHANGE_ME_BRIDGE_API_KEY';

IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = @jobName)
BEGIN
    EXEC msdb.dbo.sp_delete_job @job_name = @jobName, @delete_unused_schedule = 1;
END
GO

DECLARE @jobId UNIQUEIDENTIFIER;
EXEC msdb.dbo.sp_add_job
    @job_name = N'ShortLinkQueue_Processor',
    @enabled = 1,
    @description = N'پردازش صف لینک کوتاه — فراخوانی ShortLinkBridge',
    @job_id = @jobId OUTPUT;

DECLARE @command NVARCHAR(MAX) = N'
$headers = @{ "X-Api-Key" = "CHANGE_ME_BRIDGE_API_KEY" }
try {
    Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:5014/api/queue/process?batchSize=50" -Headers $headers -TimeoutSec 30 | Out-Null
} catch {
    Write-Error $_.Exception.Message
    exit 1
}
';

EXEC msdb.dbo.sp_add_jobstep
    @job_id = @jobId,
    @step_name = N'Call ShortLinkBridge',
    @subsystem = N'PowerShell',
    @command = @command,
    @retry_attempts = 3,
    @retry_interval = 1;

EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'Every_10_Seconds',
    @freq_type = 4,
    @freq_interval = 1,
    @freq_subday_type = 2,
    @freq_subday_interval = 10,
    @active_start_time = 0;

EXEC msdb.dbo.sp_attach_schedule
    @job_name = N'ShortLinkQueue_Processor',
    @schedule_name = N'Every_10_Seconds';

EXEC msdb.dbo.sp_add_jobserver
    @job_name = N'ShortLinkQueue_Processor';

GO

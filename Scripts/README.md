# لینک بازدید و صف لینک کوتاه

## فیلدهای جدید روی `MapPoints`

| فیلد | نوع | توضیح |
|---|---|---|
| `VisitLink` | Computed (PERSISTED) | `https://map.sabzevar.ir/?layers={CategoryId}&id={Id}` |
| `ShortVisitLink` | NVARCHAR(500) | لینک کوتاه — توسط جاب صف پر می‌شود |

## نصب دیتابیس

اسکریپت‌ها را به ترتیب روی دیتابیس `apiweb-locationsmap` اجرا کنید:

1. `001_AddVisitLinkColumns.sql`
2. `002_CreateShortLinkQueue.sql`
3. `003_CreateMapPointsTrigger.sql`
4. `005_BackfillExistingPoints.sql`
5. `004_CreateSqlAgentJob.sql` (با آدرس Bridge و ApiKey واقعی — بعد از بالا آمدن سرویس Bridge)

## معماری

```
ثبت نقطه (API یا مستقیم در DB)
        │
        ▼
VisitLink خودکار محاسبه می‌شود
        │
        ▼
Trigger → ShortLinkQueue
        │
        ▼
SQL Agent (هر ۱۰ ثانیه) → ShortLinkBridge → ShortLinks API
        │
        ▼
ShortVisitLink ذخیره می‌شود
```

## سرویس‌های مرتبط

- **LocationMap API** — فیلدها در DTOها برگردانده می‌شوند
- **ShortLinkBridge** — `short-link-bridge/` — پردازش صف
- **ShortLinks API** — `short-links/` — ساخت لینک کوتاه (`POST /api/links/batch`)

## نکته

ثبت نقطه از API سریع تمام می‌شود؛ `ShortVisitLink` چند ثانیه بعد پر می‌شود. اگر سرویس Bridge قطع باشد، جاب بعداً دوباره تلاش می‌کند.

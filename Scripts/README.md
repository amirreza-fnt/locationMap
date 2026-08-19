# لینک بازدید و صف لینک کوتاه

## فیلدهای جدید روی `MapPoints`

| فیلد | نوع | توضیح |
|---|---|---|
| `VisitLink` | Computed (PERSISTED) | `https://map.sabzevar.ir/?layers={CategoryId}&id={Id}` |
| `ShortVisitLink` | NVARCHAR(500) | لینک کوتاه — توسط جاب صف پر می‌شود |

## نصب دیتابیس (برای کارفرما)

یک فایل برای اجرا کافی است:

- `ApplyForEmployer.sql` — جداول، ستون‌ها، تریگر، صف نقاط موجود
- `ApplyForEmployer.cmd` — اجرای همان فایل با `sqlcmd`

SQL Agent لازم نیست. جاب داخل سرویس `short-link-bridge` است.

اسکریپت‌های شماره‌دار (`001` تا `005`) همان مراحل جداگانه هستند.

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
ShortLinkBridge (هر ۱۰ ثانیه) → ShortLinks API
        │
        ▼
ShortVisitLink ذخیره می‌شود
```

## سرویس‌های مرتبط

- **LocationMap API** — فیلدها در DTOها برگردانده می‌شوند
- **ShortLinkBridge** — `short-link-bridge/` — پردازش صف
- **ShortLinks API** — `short-links/` — ساخت لینک کوتاه (`POST /api/links/batch`)

# ActiveCommunity

A community sports booking application built with ASP.NET MVC 5, Entity Framework 6, .NET Framework 4.7.2 and SQL Server.

## Run on this computer

Double-click **Start.cmd**, or open the solution in Visual Studio and press **F5**.

The launcher builds the application and opens **http://localhost:5088**. It checks the database first and reuses an existing preview when available. The application uses Windows authentication to **.\SQLEXPRESS**, database **CommunitySportsFacilitiesBookingDB**.

The previous LocalDB database was copied into SQL Express because LocalDB's startup API was failing even though its SQL process was running. The original database was retained. A copy-only backup is stored in `.local/CommunitySportsFacilitiesBookingDB.bak`; keep this private. SQL Express is now the application's database, so later changes are not synchronised back to LocalDB.

## New installation

1. Install Visual Studio with **ASP.NET and web development**, the **.NET Framework 4.7.2 targeting pack**, IIS Express, and SQL Server Express. Restore NuGet packages if the `packages` directory is missing.
2. In Windows PowerShell, run `powershell -NoProfile -File .\Scripts\Setup-Database.ps1`. Use `-Server 'your-instance'` for a different SQL Server.
3. Open **Start.cmd**.

Setup creates the schema and sample facilities only when the database does not exist. It never drops or overwrites an existing database. The numbered SQL files are also available for manual execution on a fresh database; do not rerun CREATE/INSERT files on populated tables.

## Administrator access

Existing account credentials were preserved. There is no shared default administrator password.

To create a new administrator, run `powershell -NoProfile -File .\Scripts\Setup-Database.ps1 -AdminEmail 'your-email@example.com'`. The script prompts privately for a password and refuses to change an existing account. Sign in, then select **Admin** in the navigation.

Members register through the website and select their preferred sports. Administrators manage requests, member access, facility types, sports, facilities, schedules, preferences, review visibility and inquiries.

## Booking behaviour

- Guests browse by sport, facility type and city. Members also see prices, opening hours and availability.
- Booking requests must be in the future, within opening hours and capacity. Overlapping requests are rejected under a database transaction, including competing requests from separate sessions.
- Prices are calculated on the server and rounded to two decimal places. The displayed estimate is informational.
- Requests start **Pending**. Administrators confirm or reject them; confirmed sessions can be completed after their end time.
- Members may cancel their own upcoming pending/confirmed bookings. Finalised bookings cannot be reopened.
- Members review only their own completed bookings, once per booking.
- Deactivated accounts lose access on their next request. Password hashes are never exposed through member-management pages.
- Facilities, sports and types are deactivated instead of deleting their history. Linked records and duplicate assignments are validated.

## Validation

Build, including all Razor views:

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe' .\CommunitySportsFacilitiesBookingSystem.sln /p:MvcBuildViews=true /verbosity:minimal
```

With the application running, use **PowerShell 7** for integration checks:

```powershell
pwsh -NoProfile -File .\Tests\Integration.ps1
```

The tests use uniquely named temporary records in this local development database, verify member and administrator flows, and clean up their own records. Results are written to `.local/integration-results.json`. See `Documentation/Verification.md` for the latest actual results.

## Scope

This is a local full-stack booking system. Booking fees are recorded, but no online payments are collected. Inquiry responses are recorded for council staff; saving one does not send email. Public hosting, email delivery, password recovery and payment integration are not configured. Deploying under IIS requires a suitable SQL login for the application's Windows identity, HTTPS and production configuration.

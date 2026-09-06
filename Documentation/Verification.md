# Verification results

## Outcome

The project builds successfully with Razor view compilation enabled. All 57 end-to-end checks passed against the real SQL Express database. Temporary test records were removed after the run.

## Database repair

The LocalDB startup API reported the instance stopped while an existing SQL process could still serve its named-pipe connection. The original database was backed up with COPY_ONLY and CHECKSUM, then restored into the existing running `.\SQLEXPRESS` server. The restore refused to overwrite an existing destination database. The application connection now targets SQL Express. The original LocalDB database was preserved; the databases are separate copies, not synchronised.

The existing inquiry constraint uses New, InProgress, Resolved and Closed. Application validation, admin controls and the fresh-install SQL script now use the same values.

## Executed checks

- Debug build with `/p:MvcBuildViews=true`: passed, including all Razor views.
- Local launcher under Windows PowerShell with `-NoBrowser`: passed.
- Setup against the existing database: passed; existing tables/data retained.
- All three numbered SQL scripts executed successfully in a uniquely named fresh test database; three sample facilities created. That test database was removed afterward.
- Desktop homepage and facility browser inspected visually.
- Mobile homepage navigation expanded correctly. Facility search checked at 390 px viewport width; document width was 375 px, with no horizontal overflow.
- Browser error/warning log on the facility page: empty.
- Integration suite: 57 passed, listed below.

## Scope and limits

These are development-environment checks, not a claim of zero possible defects. Public hosting, online payment collection, automatic email delivery and password recovery are not configured. Inquiry replies are stored as council records, not sent as email. The coursework's personal reflection, final report and required modelling-tool screenshots still need the student's own work.

## Integration results

| Check | Result |
|---|---|
| Public page / | PASS |
| Public page /Facilities | PASS |
| Public page /Reviews | PASS |
| Public page /Account/Login | PASS |
| Public page /Account/Register | PASS |
| Public page /Home/About | PASS |
| Public page /Inquiries/Create | PASS |
| Empty search state | PASS |
| Guest admin redirect | PASS |
| Missing anti-forgery token rejected | PASS |
| Registration succeeds | PASS |
| Registration saved with member role and hashed password | PASS |
| Duplicate registration is rejected | PASS |
| Incorrect password rejected | PASS |
| Login returns to selected booking | PASS |
| Booking price and capacity rendered | PASS |
| Member cannot access admin dashboard | PASS |
| Booking request succeeds | PASS |
| Server calculates amount and pending status | PASS |
| Member booking receipt shows correct navigation | PASS |
| Malformed availability date gives validation feedback | PASS |
| Overlapping booking rejected | PASS |
| Reversed time rejected | PASS |
| Capacity exceeded rejected | PASS |
| Past booking rejected | PASS |
| Closed hours rejected | PASS |
| Availability search excludes reserved facility | PASS |
| Review before completion rejected | PASS |
| Other member cannot read booking | PASS |
| Other member cannot cancel booking | PASS |
| Owner cancellation succeeds | PASS |
| Admin page /Home/Dashboard | PASS |
| Admin page /Bookings | PASS |
| Admin page /Members | PASS |
| Admin page /Facilities/Create | PASS |
| Admin page /FacilitySchedules | PASS |
| Admin page /FacilitySchedules/Create | PASS |
| Admin page /FacilitySports | PASS |
| Admin page /FacilityTypes | PASS |
| Admin page /MemberSports | PASS |
| Admin page /Sports | PASS |
| Admin page /Inquiries | PASS |
| Admin confirms booking | PASS |
| Future completion rejected | PASS |
| Admin completes past session | PASS |
| Completed booking review saved | PASS |
| Duplicate review rejected | PASS |
| Guest inquiry saved | PASS |
| Admin inquiry response saved | PASS |
| Concurrent requests create exactly one reservation | PASS |
| Concurrent loser receives availability feedback | PASS |
| Duplicate sport produces validation feedback | PASS |
| Duplicate opening schedule produces validation feedback | PASS |
| Invalid facility capacity and price rejected | PASS |
| Member management preserves credentials and role | PASS |
| Deactivated member session revoked | PASS |
| Sign out succeeds | PASS |

Recorded locally: 2026-09-05 23:39:06 +05:30.


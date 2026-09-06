Actual automated results are recorded in [Verification.md](Verification.md): 57 integration checks passed. The cases below remain useful for manual coursework evidence.

# Community Sports Facilities Booking System Test Plan

The following tests directly cover the member and guest functionality in the coursework. Record the actual result and add a screenshot after executing each test in the running application.

| ID | User | Test and input | Expected result | Actual result | Status |
|---|---|---|---|---|---|
| T01 | Guest | Open the home page. | Sports Council information and links to register, sign in, facilities, reviews and contact are visible. | To execute | Not run |
| T02 | Guest | Browse facilities without signing in. | Facility type, sport and city search are available; exact date/time availability and pricing are restricted. | To execute | Not run |
| T03 | Guest | Open a facility details page. | Broad type, city, sport and description appear; address, price, schedule and booking action require sign-in. | To execute | Not run |
| T04 | Guest | Search approved reviews by facility and keyword. | Matching approved reviews appear. | To execute | Not run |
| T05 | Guest | Submit a valid inquiry. | Inquiry is saved and a success message appears. | To execute | Not run |
| T06 | Guest | Submit an inquiry with invalid email or missing message. | Validation errors appear and nothing is saved. | To execute | Not run |
| T07 | Guest | Register with valid personal details and preferred sports. | Member and selected sport preferences are saved; login page appears. | To execute | Not run |
| T08 | Guest | Register using an existing email address. | Duplicate registration is rejected. | To execute | Not run |
| T09 | Member | Sign in with correct email and password. | Session starts and the member's name appears in navigation. | To execute | Not run |
| T10 | Member | Sign in with an incorrect password. | Generic invalid-credentials error appears. | To execute | Not run |
| T11 | Member | Search using a valid future date and available time. | Only active facilities open and unbooked for the full period appear. | To execute | Not run |
| T12 | Member | Search using a past date. | Past-date validation message appears. | To execute | Not run |
| T13 | Member | Search with end time before start time. | Time-range validation message appears. | To execute | Not run |
| T14 | Member | Book an available facility within capacity and opening hours. | Booking is saved as Pending and total amount is calculated. | To execute | Not run |
| T15 | Member | Book a period overlapping a Pending or Confirmed booking. | Booking is rejected as a conflict. | To execute | Not run |
| T16 | Member | Book outside the facility schedule. | Booking is rejected as unavailable. | To execute | Not run |
| T17 | Member | Enter participants above facility capacity. | Capacity validation error appears. | To execute | Not run |
| T18 | Member | Open My Bookings. | Only the signed-in member's bookings appear. | To execute | Not run |
| T19 | Member | Attempt to open another member's booking ID. | Record is not displayed. | To execute | Not run |
| T20 | Member | Cancel a Pending or Confirmed booking. | Status becomes Cancelled and cancellation time is recorded. | To execute | Not run |
| T21 | Member | Submit a review for a Completed booking. | Ratings and comment are saved and shown in approved review search. | To execute | Not run |
| T22 | Member | Submit a review for a non-completed booking. | Review submission is rejected. | To execute | Not run |
| T23 | Member | Submit a second review for the same booking. | Duplicate review is rejected. | To execute | Not run |
| T24 | Member | Sign out. | Session ends and guest navigation appears. | To execute | Not run |

## Build verification

The Visual Studio 2019 MSBuild Debug build completed successfully on 5 September 2026 with zero warnings and zero errors after the guest/member access and availability-search changes.

## Executed smoke-test results

| Check | Actual result | Status |
|---|---|---|
| Home page request | HTTP 200; page rendered successfully. | Pass |
| Guest facility-search request | HTTP 200; page rendered successfully. | Pass |
| Public review-search request | HTTP 200; page rendered successfully. | Pass |
| Guest inquiry-form request | HTTP 200; page rendered successfully. | Pass |
| Registration-form request | HTTP 200; page rendered successfully. | Pass |
| Login-form request | HTTP 200; page rendered successfully. | Pass |
| Guest facility-search restrictions | Date/time controls and hourly price were absent. | Pass |
| Guest facility-detail restrictions | Full address and hourly rate were absent. | Pass |
| Protected member and management URLs | Anonymous requests were redirected to login or denied. | Pass |
| LocalDB representative data check | Database contained 5 members, 5 facilities, 6 sports, 7 bookings, 3 reviews and 5 inquiries. | Pass |

These smoke tests confirm page availability and access restrictions. They do not replace the 24 functional test cases above, which should be executed through the user interface and supported by screenshots for the final report.

## Evidence to capture

For the submitted report, add screenshots of representative inputs and outputs, including registration, login, guest search, member availability search, successful booking, conflict rejection, review submission, inquiry submission and relevant SQL query results. Do not mark a test as passed until its expected result has been observed.


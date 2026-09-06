param([string]$BaseUrl = 'http://localhost:5088', [string]$Server = '.\SQLEXPRESS')
$ErrorActionPreference = 'Stop'
$runId = [Guid]::NewGuid().ToString('N').Substring(0,12)
$email = 'qa-' + $runId + '@example.invalid'
$email2 = 'qa-other-' + $runId + '@example.invalid'
$password = 'Qa!' + [Guid]::NewGuid().ToString('N')
$script:results = [System.Collections.Generic.List[object]]::new()
$connection = [System.Data.SqlClient.SqlConnection]::new("Server=$Server;Database=CommunitySportsFacilitiesBookingDB;Integrated Security=True;Connect Timeout=10")
$connection.Open()
function Sql($query, $parameters = @{}) {
 $command = $connection.CreateCommand(); $command.CommandText=$query
 foreach($key in $parameters.Keys) { [void]$command.Parameters.AddWithValue('@'+$key, $parameters[$key]) }
 try { return $command.ExecuteScalar() } finally {$command.Dispose()}
}
function Check($name, $condition) {
 $script:results.Add([pscustomobject]@{Test=$name;Passed=[bool]$condition})
 if (!$condition) { throw "FAILED: $name" }
 Write-Output "PASS: $name"
}
function GetPage($path, $session) { Invoke-WebRequest ($BaseUrl+$path) -WebSession $session -SkipHttpErrorCheck -TimeoutSec 45 }
function PostForm($path, $data, $session, $getPath = $path) {
 $page=GetPage $getPath $session
 $token=[regex]::Match($page.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value
 if(!$token) { throw "No anti-forgery token on $getPath (HTTP $($page.StatusCode))" }
 $data['__RequestVerificationToken']=[System.Net.WebUtility]::HtmlDecode($token)
 Invoke-WebRequest ($BaseUrl+$path) -Method Post -Body $data -WebSession $session -SkipHttpErrorCheck -TimeoutSec 45
}
$guest=[Microsoft.PowerShell.Commands.WebRequestSession]::new()
$member=[Microsoft.PowerShell.Commands.WebRequestSession]::new()
$other=[Microsoft.PowerShell.Commands.WebRequestSession]::new()
$admin=[Microsoft.PowerShell.Commands.WebRequestSession]::new()
$facilityId=$null;$sportId=$null;$typeId=$null;$memberId=$null;$otherId=$null
try {
 foreach($path in @('/','/Facilities','/Reviews','/Account/Login','/Account/Register','/Home/About','/Inquiries/Create')) { $r=GetPage $path $guest; Check "Public page $path" ($r.StatusCode -eq 200 -and $r.Content -notmatch 'Server Error') }
 $r=GetPage '/Facilities?city=NoSuchCity_QA' $guest; Check 'Empty search state' ($r.Content -match 'No spaces match')
 $r=GetPage '/Home/Dashboard' $guest; Check 'Guest admin redirect' ($r.Content -match 'Welcome back')
 $r=Invoke-WebRequest ($BaseUrl+'/Inquiries/Create') -Method Post -Body @{SenderName='QA'} -WebSession $guest -SkipHttpErrorCheck
 Check 'Missing anti-forgery token rejected' ($r.StatusCode -ge 400)
 $sportId=Sql "INSERT dbo.Sport(SportName,Description,IsActive) OUTPUT INSERTED.SportID VALUES(@name,'Integration test',1)" @{name="QA $runId"}
 $typeId=Sql "INSERT dbo.FacilityType(TypeName,Description,IsActive) OUTPUT INSERTED.FacilityTypeID VALUES(@name,'Integration test',1)" @{name="QA $runId"}
 $facilityId=Sql "INSERT dbo.Facility(FacilityTypeID,FacilityName,Description,AddressLine,City,Capacity,HourlyRate,IsActive,CreatedAt) OUTPUT INSERTED.FacilityID VALUES(@type,@name,'Integration test','QA road','QA City',12,1500,1,GETDATE())" @{type=$typeId;name="QA $runId"}
 [void](Sql 'INSERT dbo.FacilitySchedule(FacilityID,DayOfWeek,OpeningTime,ClosingTime,IsAvailable) SELECT @id,n,''08:00'',''20:00'',1 FROM (VALUES(1),(2),(3),(4),(5),(6),(7)) d(n)' @{id=$facilityId})
 $registration=@{FirstName='QA';LastName=$runId;Email=$email;Phone='0771234567';AddressLine='QA road';City='QA City';Password=$password;ConfirmPassword=$password;SelectedSportIDs=$sportId}
 $r=PostForm '/Account/Register' $registration $member
 Check 'Registration succeeds' ($r.StatusCode -eq 200 -and $r.Content -match 'Registration successful')
 $memberId=Sql 'SELECT MemberID FROM dbo.Member WHERE Email=@email' @{email=$email}
 Check 'Registration saved with member role and hashed password' ((Sql 'SELECT COUNT(*) FROM dbo.Member WHERE MemberID=@id AND UserRole=''Member'' AND PasswordHash<>@password' @{id=$memberId;password=$password}) -eq 1)
 $r=PostForm '/Account/Register' $registration $guest; Check 'Duplicate registration is rejected' ($r.Content -match 'already exists')
 $r=PostForm '/Account/Login' @{Email=$email;Password='incorrect'} $member; Check 'Incorrect password rejected' ($r.Content -match 'Invalid email address or password')
 $r=PostForm '/Account/Login' @{Email=$email;Password=$password;returnUrl="/Bookings/Create?facilityId=$facilityId"} $member
 Check 'Login returns to selected booking' ($r.Content -match 'Book your space')
 Check 'Booking price and capacity rendered' ($r.Content -match '1500' -and $r.Content -match '12 people')
 $r=GetPage '/Home/Dashboard' $member; Check 'Member cannot access admin dashboard' ($r.StatusCode -eq 403)
 $tomorrow=(Get-Date).Date.AddDays(2).ToString('yyyy-MM-dd')
 $booking=@{FacilityID=$facilityId;BookingDate=$tomorrow;StartTime='09:00';EndTime='10:00';NumberOfParticipants=2;Purpose="QA $runId"}
 $r=PostForm '/Bookings/Create' ($booking.Clone()) $member "/Bookings/Create?facilityId=$facilityId"; Check 'Booking request succeeds' ($r.Content -match 'Booking requested')
 $bookingId=Sql 'SELECT BookingID FROM dbo.Booking WHERE MemberID=@id AND FacilityID=@facility' @{id=$memberId;facility=$facilityId}
 Check 'Server calculates amount and pending status' ((Sql 'SELECT COUNT(*) FROM dbo.Booking WHERE BookingID=@id AND TotalAmount=1500 AND BookingStatus=''Pending''' @{id=$bookingId}) -eq 1)
 $r=GetPage "/Bookings/Details/$bookingId" $member; Check 'Member booking receipt shows correct navigation' ($r.StatusCode -eq 200 -and $r.Content -match 'Back to my bookings' -and $r.Content -notmatch 'Manage status')
 $r=GetPage '/Facilities?bookingDate=invalid&startTime=09:00&endTime=10:00' $member; Check 'Malformed availability date gives validation feedback' ($r.Content -match 'Enter a valid date and time')
 $r=PostForm '/Bookings/Create' ($booking.Clone()) $member "/Bookings/Create?facilityId=$facilityId"; Check 'Overlapping booking rejected' ($r.Content -match 'already been reserved')
 $invalid=$booking.Clone();$invalid.StartTime='11:00';$invalid.EndTime='10:00';$r=PostForm '/Bookings/Create' $invalid $member "/Bookings/Create?facilityId=$facilityId"; Check 'Reversed time rejected' ($r.Content -match 'valid end time')
 $invalid=$booking.Clone();$invalid.NumberOfParticipants=13;$r=PostForm '/Bookings/Create' $invalid $member "/Bookings/Create?facilityId=$facilityId"; Check 'Capacity exceeded rejected' ($r.Content -match 'up to 12')
 $invalid=$booking.Clone();$invalid.BookingDate='2020-01-01';$r=PostForm '/Bookings/Create' $invalid $member "/Bookings/Create?facilityId=$facilityId"; Check 'Past booking rejected' ($r.Content -match 'in the future')
 $invalid=$booking.Clone();$invalid.StartTime='06:00';$invalid.EndTime='07:00';$r=PostForm '/Bookings/Create' $invalid $member "/Bookings/Create?facilityId=$facilityId"; Check 'Closed hours rejected' ($r.Content -match 'outside this facility')
 $r=GetPage "/Facilities?city=QA%20City&bookingDate=$tomorrow&startTime=09:00&endTime=10:00" $member; Check 'Availability search excludes reserved facility' ($r.Content -match 'No spaces match')
 $r=GetPage "/Reviews/Create?bookingId=$bookingId" $member; Check 'Review before completion rejected' ($r.Content -match 'only be submitted for completed')
 $registration.Email=$email2;$r=PostForm '/Account/Register' $registration $other
 $otherId=Sql 'SELECT MemberID FROM dbo.Member WHERE Email=@email' @{email=$email2}
 $r=PostForm '/Account/Login' @{Email=$email2;Password=$password} $other
 $r=GetPage "/Bookings/Details/$bookingId" $other; Check 'Other member cannot read booking' ($r.StatusCode -eq 404)
 $r=PostForm "/Bookings/Cancel/$bookingId" @{} $other '/Bookings/MyBookings'; Check 'Other member cannot cancel booking' ($r.StatusCode -eq 404)
 $r=PostForm "/Bookings/Cancel/$bookingId" @{} $member '/Bookings/MyBookings'; Check 'Owner cancellation succeeds' ($r.Content -match 'Your booking has been cancelled')
 [void](Sql 'UPDATE dbo.Member SET UserRole=''Admin'' WHERE MemberID=@id' @{id=$otherId})
 $r=PostForm '/Account/Login' @{Email=$email2;Password=$password} $admin
 foreach($path in @('/Home/Dashboard','/Bookings','/Members','/Facilities/Create','/FacilitySchedules','/FacilitySchedules/Create','/FacilitySports','/FacilityTypes','/MemberSports','/Sports','/Inquiries')) { $r=GetPage $path $admin; Check "Admin page $path" ($r.StatusCode -eq 200) }
 $booking.StartTime='11:00';$booking.EndTime='12:00';$r=PostForm '/Bookings/Create' ($booking.Clone()) $member "/Bookings/Create?facilityId=$facilityId"
 $pendingId=Sql 'SELECT MAX(BookingID) FROM dbo.Booking WHERE MemberID=@id' @{id=$memberId}
 $r=PostForm "/Bookings/Edit/$pendingId" @{BookingID=$pendingId;BookingStatus='Confirmed'} $admin; Check 'Admin confirms booking' ((Sql 'SELECT BookingStatus FROM dbo.Booking WHERE BookingID=@id' @{id=$pendingId}) -eq 'Confirmed')
 $r=PostForm "/Bookings/Edit/$pendingId" @{BookingID=$pendingId;BookingStatus='Completed'} $admin; Check 'Future completion rejected' ($r.Content -match 'only be completed after')
 [void](Sql 'UPDATE dbo.Booking SET BookingDate=DATEADD(day,-2,CAST(GETDATE() AS date)) WHERE BookingID=@id' @{id=$pendingId})
 $r=PostForm "/Bookings/Edit/$pendingId" @{BookingID=$pendingId;BookingStatus='Completed'} $admin; Check 'Admin completes past session' ((Sql 'SELECT BookingStatus FROM dbo.Booking WHERE BookingID=@id' @{id=$pendingId}) -eq 'Completed')
 $r=PostForm '/Reviews/Create' @{BookingID=$pendingId;OverallRating=5;ConditionRating=4;AmenitiesRating=4;ReviewTitle="QA $runId";ReviewComment='Test session review.'} $member "/Reviews/Create?bookingId=$pendingId"; Check 'Completed booking review saved' ($r.Content -match 'review was submitted successfully')
 $r=GetPage "/Reviews/Create?bookingId=$pendingId" $member; Check 'Duplicate review rejected' ($r.Content -match 'already been submitted')
 $r=PostForm '/Inquiries/Create' @{SenderName='QA';SenderEmail=$email;InquirySubject="QA $runId";InquiryMessage='Integration test inquiry.'} $guest; Check 'Guest inquiry saved' ($r.Content -match 'inquiry was submitted successfully')
 $inquiryId=Sql 'SELECT InquiryID FROM dbo.Inquiry WHERE SenderEmail=@email' @{email=$email}
 $r=PostForm "/Inquiries/Edit/$inquiryId" @{InquiryID=$inquiryId;InquiryStatus='Resolved';ResponseMessage='Test response record.'} $admin
 Check 'Admin inquiry response saved' ((Sql 'SELECT InquiryStatus FROM dbo.Inquiry WHERE InquiryID=@id' @{id=$inquiryId}) -eq 'Resolved')
 # Two independently authenticated sessions submit the same slot concurrently.
 $page1=GetPage "/Bookings/Create?facilityId=$facilityId" $member
 $page2=GetPage "/Bookings/Create?facilityId=$facilityId" $other
 $http=[System.Net.Http.HttpClient]::new()
 try {
  $requests=@();$jobs=@();$sessions=@($member,$other);$pages=@($page1,$page2)
  for($i=0;$i -lt 2;$i++) {
   $fields=[System.Collections.Generic.Dictionary[string,string]]::new()
   $fields.Add('FacilityID',[string]$facilityId);$fields.Add('BookingDate',$tomorrow);$fields.Add('StartTime','13:00');$fields.Add('EndTime','14:00');$fields.Add('NumberOfParticipants','2');$fields.Add('Purpose',"QA $runId concurrency")
   $token=[regex]::Match($pages[$i].Content,'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value
   $fields.Add('__RequestVerificationToken',[System.Net.WebUtility]::HtmlDecode($token))
   $request=[System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Post,($BaseUrl+'/Bookings/Create'))
   $request.Content=[System.Net.Http.FormUrlEncodedContent]::new($fields)
   [void]$request.Headers.TryAddWithoutValidation('Cookie',$sessions[$i].Cookies.GetCookieHeader([uri]$BaseUrl))
   $requests += $request; $jobs += $http.SendAsync($request)
  }
  $responses=@();foreach($job in $jobs){$response=$job.GetAwaiter().GetResult();$responses += $response.Content.ReadAsStringAsync().GetAwaiter().GetResult();$response.Dispose()}
  Check 'Concurrent requests create exactly one reservation' ((Sql 'SELECT COUNT(*) FROM dbo.Booking WHERE FacilityID=@id AND BookingDate=@day AND StartTime=''13:00'' AND BookingStatus=''Pending''' @{id=$facilityId;day=$tomorrow}) -eq 1)
  Check 'Concurrent loser receives availability feedback' (($responses -join ' ') -match 'already been reserved')
 } finally {foreach($request in $requests){$request.Dispose()};$http.Dispose()}
 $r=PostForm '/Sports/Create' @{SportName="QA $runId";Description='duplicate';IsActive='true'} $admin
 Check 'Duplicate sport produces validation feedback' ($r.Content -match 'already exists')
 $r=PostForm '/FacilitySchedules/Create' @{FacilityID=$facilityId;DayOfWeek=1;OpeningTime='08:00';ClosingTime='20:00';IsAvailable='true'} $admin
 Check 'Duplicate opening schedule produces validation feedback' ($r.Content -match 'Opening hours already exist')
 $r=PostForm '/Facilities/Create' @{FacilityTypeID=$typeId;FacilityName='Invalid QA';AddressLine='QA';City='QA';Capacity=0;HourlyRate=-1;IsActive='true'} $admin
 Check 'Invalid facility capacity and price rejected' ($r.StatusCode -eq 200 -and $r.Content -match 'field-validation-error')
 $r=PostForm "/Members/Edit/$memberId" @{MemberID=$memberId;IsActive='false';PasswordHash='tampered';UserRole='Admin'} $admin
 Check 'Member management preserves credentials and role' ((Sql 'SELECT COUNT(*) FROM dbo.Member WHERE MemberID=@id AND UserRole=''Member'' AND PasswordHash<>''tampered'' AND IsActive=0' @{id=$memberId}) -eq 1)
 $r=GetPage '/Bookings/MyBookings' $member; Check 'Deactivated member session revoked' ($r.Content -match 'Welcome back')
 $r=PostForm '/Account/Logout' @{} $admin '/Home/Dashboard'; Check 'Sign out succeeds' ($r.Content -match 'Join the community')
} finally {
 # Remove only records created by this uniquely identified test run.
 if ($memberId) { [void](Sql 'DELETE FROM dbo.Review WHERE BookingID IN (SELECT BookingID FROM dbo.Booking WHERE MemberID=@id); DELETE FROM dbo.Booking WHERE MemberID=@id; DELETE FROM dbo.MemberSport WHERE MemberID=@id; DELETE FROM dbo.Member WHERE MemberID=@id AND Email=@email' @{id=$memberId;email=$email}) }
 if ($otherId) { [void](Sql 'DELETE FROM dbo.Booking WHERE MemberID=@id; DELETE FROM dbo.MemberSport WHERE MemberID=@id; DELETE FROM dbo.Member WHERE MemberID=@id AND Email=@email' @{id=$otherId;email=$email2}) }
 [void](Sql 'DELETE FROM dbo.Inquiry WHERE SenderEmail=@email AND InquirySubject=@subject' @{email=$email;subject="QA $runId"})
 if ($facilityId) { [void](Sql 'DELETE FROM dbo.FacilitySchedule WHERE FacilityID=@id; DELETE FROM dbo.Facility WHERE FacilityID=@id AND FacilityName=@name' @{id=$facilityId;name="QA $runId"}) }
 if ($typeId) { [void](Sql 'DELETE FROM dbo.FacilityType WHERE FacilityTypeID=@id AND TypeName=@name' @{id=$typeId;name="QA $runId"}) }
 if ($sportId) { [void](Sql 'DELETE FROM dbo.Sport WHERE SportID=@id AND SportName=@name' @{id=$sportId;name="QA $runId"}) }
 $connection.Dispose()
 $script:results | ConvertTo-Json | Set-Content (Join-Path $PSScriptRoot '../.local/integration-results.json')
}
Write-Output ("Passed " + $script:results.Count + " integration checks; test records removed.")




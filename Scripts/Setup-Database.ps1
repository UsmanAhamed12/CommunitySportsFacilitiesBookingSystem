param([string]$Server = '.\SQLEXPRESS', [string]$AdminEmail)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$connection = New-Object System.Data.SqlClient.SqlConnection "Server=$Server;Database=master;Integrated Security=True;Connect Timeout=10"
$connection.Open()
try {
 $command=$connection.CreateCommand();$command.CommandText="SELECT DB_ID(N'CommunitySportsFacilitiesBookingDB')";$exists=$command.ExecuteScalar() -isnot [DBNull]
 if (!$exists) {
  foreach($file in @('01_CreateDatabase.sql','02_InsertSampleData.sql')) {
   $script=Get-Content (Join-Path $root ('Database\'+$file)) -Raw
   foreach($batch in [regex]::Split($script,'(?im)^\s*GO\s*$')) {
    if (![string]::IsNullOrWhiteSpace($batch)) { $command.CommandText=$batch; $command.CommandTimeout=60; [void]$command.ExecuteNonQuery() }
   }
  }
  Write-Host 'Database and sample facilities created.'
 } else { Write-Host 'Existing database retained; schema and sample data were not reapplied.' }
 $connection.ChangeDatabase('CommunitySportsFacilitiesBookingDB')
 $command.CommandText='SELECT COUNT(*) FROM sys.tables WHERE name IN (''Member'',''Sport'',''FacilityType'',''Facility'',''FacilitySport'',''FacilitySchedule'',''MemberSport'',''Booking'',''Review'',''Inquiry'')'
 if ($command.ExecuteScalar() -ne 10) { throw 'The database has an incomplete schema. Review it before applying any changes; no tables were dropped.' }
 if ($AdminEmail) {
  if ($AdminEmail -notmatch '^[^\s@]+@[^\s@]+\.[^\s@]+$' -or $AdminEmail.Length -gt 100) { throw 'Enter a valid administrator email.' }
  $command.Parameters.Clear();[void]$command.Parameters.AddWithValue('@email',$AdminEmail.Trim().ToLowerInvariant());$command.CommandText='SELECT COUNT(*) FROM dbo.Member WHERE Email=@email'
  if ($command.ExecuteScalar() -gt 0) { throw 'That email already exists. No account or password was changed.' }
  $secure=Read-Host 'Choose an administrator password (at least 12 characters)' -AsSecureString
  $pointer=[Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
  try { $plain=[Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer); if ($plain.Length -lt 12) { throw 'Use a password of at least 12 characters.' }
   # ASP.NET Web Helpers password format: version 0, 16-byte salt, 32-byte PBKDF2 subkey.
   $salt=New-Object byte[] 16;$rng=[Security.Cryptography.RandomNumberGenerator]::Create();$rng.GetBytes($salt);$rng.Dispose()
   $derive=New-Object Security.Cryptography.Rfc2898DeriveBytes($plain,$salt,1000);$key=$derive.GetBytes(32);$derive.Dispose()
   $bytes=New-Object byte[] 49;[Array]::Copy($salt,0,$bytes,1,16);[Array]::Copy($key,0,$bytes,17,32);$hash=[Convert]::ToBase64String($bytes)
  } finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer);$plain=$null }
  [void]$command.Parameters.AddWithValue('@hash',$hash)
  $command.CommandText="INSERT dbo.Member(FirstName,LastName,Email,PasswordHash,Phone,AddressLine,City,RegisteredAt,IsActive,UserRole) VALUES('Council','Administrator',@email,@hash,'Not provided','Council office','Not provided',GETDATE(),1,'Admin')"
  [void]$command.ExecuteNonQuery(); Write-Host 'Administrator created. Sign in with the email and password you chose.'
 }
 [xml]$config=Get-Content (Join-Path $root 'CommunitySportsFacilitiesBookingSystem\Web.config')
 $builder=New-Object System.Data.Common.DbConnectionStringBuilder;$builder.set_ConnectionString([string]$config.configuration.connectionStrings.add.connectionString)
 $sql=New-Object System.Data.SqlClient.SqlConnectionStringBuilder ([string]$builder.get_Item('provider connection string'));$sql.set_DataSource($Server)
 $builder.set_Item('provider connection string',$sql.get_ConnectionString());$config.configuration.connectionStrings.add.connectionString=$builder.get_ConnectionString()
 $config.Save((Join-Path $root 'CommunitySportsFacilitiesBookingSystem\Web.config'))
 Write-Host 'Database connection verified. Open Start.cmd to launch ActiveCommunity.'
} finally {$connection.Dispose()}


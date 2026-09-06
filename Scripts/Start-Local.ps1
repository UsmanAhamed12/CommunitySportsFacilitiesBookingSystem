param([int]$Port = 5088, [switch]$NoBrowser)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root 'CommunitySportsFacilitiesBookingSystem'
$local = Join-Path $root '.local'
New-Item -ItemType Directory -Force $local | Out-Null
[xml]$configuration = Get-Content (Join-Path $project 'Web.config')
$entity = New-Object System.Data.Common.DbConnectionStringBuilder
$entity.set_ConnectionString([string]$configuration.configuration.connectionStrings.add.connectionString)
$connection = New-Object System.Data.SqlClient.SqlConnection ([string]$entity.get_Item('provider connection string'))
try { $connection.Open() } catch { throw ('Cannot connect to the project database. Run Scripts\Setup-Database.ps1 for a new installation, or check SQL Server is running. ' + $_.Exception.Message) } finally { $connection.Dispose() }
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (!(Test-Path $vswhere)) { throw 'Install Visual Studio with the ASP.NET and web development workload.' }
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (!$msbuild) { throw 'MSBuild was not found. Install the ASP.NET and web development workload.' }
& $msbuild (Join-Path $root 'CommunitySportsFacilitiesBookingSystem.sln') /p:Configuration=Debug /verbosity:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Build failed. Resolve the reported build errors before starting.' }
$url = "http://localhost:$Port/"
$running = $false
try { $response = Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 3; $running = $response.Content -match 'ActiveCommunity' } catch { }
if (!$running) {
 $iis = Join-Path $env:ProgramFiles 'IIS Express\iisexpress.exe'
 if (!(Test-Path $iis)) { throw 'IIS Express was not found. Install it through Visual Studio Installer.' }
 $process = Start-Process $iis -ArgumentList ('/path:"'+$project+'"'),('/port:'+$Port),'/systray:false' -WindowStyle Hidden -RedirectStandardOutput (Join-Path $local 'iis-launch.log') -RedirectStandardError (Join-Path $local 'iis-launch-error.log') -PassThru
 $process.Id | Set-Content (Join-Path $local 'iis.pid')
}
Write-Host "ActiveCommunity: $url"
if (!$NoBrowser) { Start-Process $url }


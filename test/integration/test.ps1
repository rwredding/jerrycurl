#Requires -Version 5.0

[CmdletBinding(PositionalBinding=$false)]
param(
    [String] $PackageSource,
    [String] $BuildPath,
    [String] $Verbosity = "minimal"
)

. (Join-Path $PSScriptRoot ..\docker\dotfile.ps1)
. (Join-Path $PSScriptRoot .\dotfile.ps1)

if (-not $PackageSource) { $PackageSource = Join-Path $PSScriptRoot "..\..\artifacts\packages" }
if (-not $BuildPath)     { $BuildPath     = Join-Path $PSScriptRoot "..\..\artifacts\integration" }

Set-Live-Connection "sqlite" ("FILENAME=" + (Join-Path $BuildPath "sqlite\int.db"))

foreach ($vendor in Get-All-Vendors)
{
    $connectionString = Get-Live-Connection -Vendor $vendor
    $userConnectionString = Get-Connection-String -Vendor $vendor -User
    
    if ($env:CI_WINDOWS -eq "true" -and $vendor -eq "mysql") { $connectionString = $null }
    
    Write-Host ""
    Write-Host "   Testing '$vendor'..."
    
    if ($connectionString)
    {
        Test-Integration -Vendor $vendor -Connection $connectionString -PackageSource $PackageSource -Verbosity $Verbosity -TempPath $BuildPath -UserConnectionString $userConnectionString
        
        if (Verify-Integration -Vendor $vendor -TempPath $BuildPath)
        {
            Write-Host "   Integration success." -ForegroundColor Green
        }
        else
        {
            Write-Host "   Result could not be verified." -ForegroundColor Red
            Exit -1
        }
    }
    else
    {
        Write-Host "   Skipped. Connection not configured." -ForegroundColor Yellow
    }
}

Set-Live-Connection "sqlite" $null

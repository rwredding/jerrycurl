[CmdletBinding(PositionalBinding=$false)]
param(
    [string] $Verbosity = "minimal"
)

. (Join-Path $PSScriptRoot ..\docker\dotfile.ps1)
. (Join-Path $PSScriptRoot .\dotfile.ps1)

$tempPath = Join-Path $PSScriptRoot "..\..\artifacts\integration"
$packageSource = Join-Path $PSScriptRoot "..\..\artifacts\packages"

Set-Connection-String "sqlite" ("FILENAME=" + (Join-Path $tempPath "sqlite\int.db"))

foreach ($vendor in Get-All-Vendors)
{
    Write-Host "APPVEYOR"
    Write-Host $env:APPVEYOR
    
    Write-Host "CI"
    Write-Host $env:CI
    
    Write-Host "CI2"
    Write-Host "$env:CI"
    Write-Host ($env:CI -eq "True")
    
    $connectionString = Get-Connection-String -Vendor $vendor
    
    if ($env:APPVEYOR -eq "True" -and $vendor -eq "mysql") { $connectionSring = $null }
    
    Write-Host ""
    Write-Host "   Testing '$vendor'..."
    
    if ($connectionString)
    {
        Test-Integration -Vendor $vendor -Connection $connectionString -PackageSource $packageSource -Verbosity $Verbosity -TempPath $tempPath
        
        if (Verify-Integration -Vendor $vendor -TempPath $tempPath)
        {
            Write-Host "   Integration success." -ForegroundColor Green
        }
        else { Exit -1 }
    }
    else
    {
        Write-Host "   Skipped. Database server is not running." -ForegroundColor Yellow
    }
}
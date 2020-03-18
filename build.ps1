#Requires -Version 5.0

[CmdletBinding(PositionalBinding=$false)]
param(
    [switch] $NoTest,
    [switch] $NoPack,
    [switch] $NoIntegrate,
    [switch] $PublicRelease,
    [String] $Configuration = "Release",
    [String] $Verbosity = "minimal"
)

$ErrorActionPreference = "Stop"

$sln = Join-Path $PSScriptRoot jerrycurl.sln
$packageSource = Join-Path $PSScriptRoot "artifacts\packages"
$timer = [Diagnostics.Stopwatch]::StartNew()
$props = @(
    "/verbosity:$Verbosity"
)

if ($PublicRelease)
{
    $props += "/property:PublicRelease=$PublicRelease"
}

# Restore
Write-Host "Restoring NuGet packages..." -ForegroundColor Magenta
dotnet restore /property:Configuration=$Configuration $sln @props
if ($LastExitCode -ne 0)
{
    Write-Host "Restore failed, aborting." -ForegroundColor Red
    Exit -1
}
Write-Host "Done restoring." -ForegroundColor Green

# Clean
Write-Host "Cleaning..." -ForegroundColor Magenta
dotnet clean --configuration $Configuration $sln @props
if ($LastExitCode -ne 0)
{
    Write-Host "Clean failed, aborting." -ForegroundColor Red
    Exit -1
}
if (-not $NoPack)
{
    Remove-Item $packageSource\* -Force -ErrorAction Ignore
}
Write-Host "Done cleaning." -ForegroundColor Green

# Build
Write-Host "Building..." -ForegroundColor Magenta
dotnet build --configuration $Configuration --no-restore $sln @props
if ($LastExitCode -ne 0)
{
    Write-Host "Build failed, aborting." -ForegroundColor Red
    Exit -1
}
Write-Host "Done building." -ForegroundColor Green

# Test
if (-not $NoTest)
{
	Write-Host "Testing..." -ForegroundColor Magenta

    .\test\unit\test.ps1 -NoBuild -Configuration $Configuration

    if ($LastExitCode -ne 0)
    {
		Write-Host "Testing failed, aborting." -ForegroundColor Red
		Exit -1
	}
	
    Write-Host "Done testing." -ForegroundColor Green
}
else
{
    Write-Host "Testing skipped." -ForegroundColor Yellow
}

# Pack
if (-not $NoPack)
{
	Write-Host "Packing..." -ForegroundColor Magenta
	dotnet pack $sln --configuration $Configuration --no-build --no-restore @props
	
    if ($LastExitCode -ne 0)
    {
		Write-Host "Packing failed, aborting." -ForegroundColor Red
		Exit -1
	}
	
	Write-Host "Done packing." -ForegroundColor Green
}
else
{
    Write-Host "Packing skipped." -ForegroundColor Yellow
}

# Integration test
if (-not $NoPack -and (-not $NoIntegrate))
{
    Write-Host "Integrating..." -ForegroundColor Magenta

    .\test\integration\test.ps1 -Verbosity $Verbosity

    if ($LastExitCode -ne 0)
    {
        Write-Host "Integration failed, aborting." -ForegroundColor Red
		Exit -1
    }
    else
    {
        Write-Host "Done integrating." -ForegroundColor Green
    }
}
else
{
    Write-Host "Integration skipped." -ForegroundColor Yellow
}

Write-Host "All done. Took $("{0:0.00}" -f $timer.Elapsed.TotalSeconds) seconds" -ForegroundColor Blue
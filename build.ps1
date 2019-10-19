#Requires -Version 5.0

[CmdletBinding(PositionalBinding=$false)]
Param(
    [switch]$NoTest,
    [switch]$NoPack,
    [switch]$PublicRelease,
    [string]$Config = "Release"
)

$ErrorActionPreference = "Stop"

$sln = Join-Path $PSScriptRoot jerrycurl.sln
$verbosity = "minimal"
$sw = [Diagnostics.Stopwatch]::StartNew()

Write-Host "Restoring NuGet packages..." -ForegroundColor "Magenta"
dotnet restore /property:Configuration=$Config $sln /verbosity:$verbosity
if ($LastExitCode -ne 0) {
    Write-Host "Restore failed, aborting." -Foreground "Red"
    exit -1
}
Write-Host "Done restoring." -ForegroundColor "Green"

Write-Host "Cleaning..." -ForegroundColor "Magenta"
dotnet clean -c $Config $sln /verbosity:$verbosity
if ($LastExitCode -ne 0) {
    Write-Host "Clean failed, aborting." -Foreground "Red"
    exit -1
}
Write-Host "Done cleaning." -ForegroundColor "Green"

Write-Host "Building..." -ForegroundColor "Magenta"
dotnet build -c $Config --no-restore $sln /verbosity:$verbosity
if ($LastExitCode -ne 0) {
    Write-Host "Build failed, aborting." -Foreground "Red"
    exit -1
}
Write-Host "Done building." -ForegroundColor "Green"

if (-Not $NoTest) {
    $testProj = "Mvc\Jerrycurl.Data.Test"
	Write-Host "Testing..." -ForegroundColor "Magenta"
	
	foreach ($csproj in (dir .\test\src -recurse -filter *.Test.csproj)) {
        pushd (Split-Path $csproj.FullName -Parent)
        dotnet fixie --no-build --configuration $Config
        
        if ($LastExitCode -ne 0) {            
            Write-Host "Testing failed, aborting." -Foreground "Red"
            popd
            exit -1
        }
        popd
    }
	
    Write-Host "Done testing." -ForegroundColor "Green"
} else {
    Write-Host "Testing skipped." -ForegroundColor "Yellow"
}

if (-Not $NoPack) {
	Write-Host "Packing..." -ForegroundColor "Magenta"
	dotnet pack $sln -c $Config --no-build --no-restore /verbosity:$verbosity /property:PublicRelease=$PublicRelease
	
    if ($LastExitCode -ne 0) {
		Write-Host "Packing failed, aborting." -Foreground "Red"
		exit -1
	}
	
	Write-Host "Done packing." -ForegroundColor "Green"
} else {
    Write-Host "Packing skipped." -ForegroundColor "Yellow"
}

Write-Host "All done. Took $("{0:0.00}" -f $sw.Elapsed.TotalSeconds) seconds" -ForegroundColor "Blue"
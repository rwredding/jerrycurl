#Requires -Version 5.0

[CmdletBinding(PositionalBinding=$false)]
param(
    [Switch] $NoBuild,
    [String] $Configuration = "Release"
)

. (Join-Path $PSScriptRoot .\dotfile.ps1)

$args = @(
    "--configuration",
    $Configuration
)

if ($NoBuild) { $args += "--no-build" }

foreach ($path in Get-Test-Projects)
{
    Push-Location $path
    dotnet fixie @args
    
    if ($LastExitCode -ne 0)
    {            
        Pop-Location
        Exit -1
    }
    
    Pop-Location
}
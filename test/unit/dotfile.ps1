function Get-Test-Projects
{
    $source = Join-Path $PSScriptRoot .\src
    
    Get-ChildItem $source -Recurse -Filter *.Test.csproj | % { Split-Path $_.FullName -Parent }
}
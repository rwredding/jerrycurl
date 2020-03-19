param(
    [String]   $Direction = "toggle",
    [String[]] $Vendors = @("*")
)

. (Join-Path $PSScriptRoot .\dotfile.ps1)

if ($Direction -eq "toggle")
{
    if (Is-Docker-Live) { $Direction = "down" }
    else                { $Direction = "up"   }
}

if ($Vendors.Length -eq 1 -and $Vendors[0] -eq "*")
{
    $Vendors = "sqlserver", "oracle", "mysql", "postgres"
}

$args = @()
$filtered = @()

foreach ($vendor in $Vendors)
{
    $variable = Get-Connection-Variable -Vendor $vendor
    $yml = Get-Yml-Path -Vendor $vendor
    $connectionString = Get-Connection-String -Vendor $vendor
    
    if ($variable -and $yml -and $connectionString)
    {
        $args += "-f", $yml
        $filtered += $vendor
        
        if ($Direction -eq "up")
        {
            Set-Live-Connection $Vendor $connectionString
        }
        else
        {
            Set-Live-Connection $Vendor $null
        }
    }
    else
    {
        Write-Host "Skipping '$Vendor'. No configuration found." -ForegroundColor Yellow
    }
}

if (-not $filtered)
{
    Write-Host "No valid servers specified. Specify any combination of 'sqlserver', 'postgres', 'mysql' and 'oracle'." -ForegroundColor Red
    Write-Host "If testing Oracle you will need to associate a license with your Docker login at: https://hub.docker.com/_/oracle-database-enterprise-edition" -ForegroundColor Yellow
    Exit -1
}

if ($Direction -eq "up") { Set-Docker-Live }
else { Set-Docker-Live -IsOffline }

try
{
    docker-compose @args $Direction
}
finally
{
    foreach ($vendor in $filtered)
    {
        Set-Live-Connection $vendor -ConnectionString $null
    }
}
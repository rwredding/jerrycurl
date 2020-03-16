param(
    [String]$Direction = "up",
    [String[]]$Vendors = @("sqlserver", "mysql", "postgres", "oracle")
)

. (Join-Path $PSScriptRoot .\dotfile.ps1)

$JERRY_SQLSERVER_CONN = "SERVER=localhost,11433;DATABASE=tempdb;USER ID=sa;Password=Password12!"
$JERRY_POSTGRES_CONN = "SERVER=localhost;PORT=5432;DATABASE=jerry_testdb;USER ID=jerry;PASSWORD=Password12!;ENLIST=true"
$JERRY_MYSQL_CONN = "SERVER=localhost;DATABASE=jerry_testdb;UID=jerry;PWD=Password12!"
$JERRY_ORACLE_CONN = "DATA SOURCE=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))" +
                     "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ORCLCDB.localdomain)));USER ID=sys;PASSWORD=Oradoc_db1;DBA Privilege=SYSDBA"

if (-not $Vendors)
{
    Write-Host "No servers specified. Specify any combination of 'sqlserver', 'postgres', 'mysql' and 'oracle'." -ForegroundColor Red
    Write-Host "If testing Oracle you will need to associate a license with your Docker login at: https://hub.docker.com/_/oracle-database-enterprise-edition" -ForegroundColor Yellow
    Exit -1
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
    
    if (-not $variable) { continue }
    
    $connectionString = Get-Variable "$variable" -ValueOnly -ErrorAction Ignore
    $yml = Get-Yml-Path -Vendor $vendor
    
    if ($connectionString -and $yml)
    {
        Set-Connection-String -Vendor $vendor -ConnectionString $connectionString
        $args += "-f", $yml
        $filtered += $vendor
    }
}

try
{
    docker-compose @args $Direction
}
finally
{
    foreach ($vendor in $filtered)
    {
        Set-Connection-String -Vendor $vendor -ConnectionString $null
    }
}
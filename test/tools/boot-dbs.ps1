param(
    [String]$Direction = "up",
    [String[]]$Vendors
)

$JERRY_SQLSERVER_CONN = "SERVER=localhost,11433;DATABASE=tempdb;USER ID=sa;Password=Password12!"
$JERRY_POSTGRES_CONN = "SERVER=localhost;PORT=5432;DATABASE=jerry_testdb;USER ID=jerry;PASSWORD=Password12!;ENLIST=true"
$JERRY_MYSQL_CONN = "SERVER=localhost;DATABASE=jerry_testdb;UID=jerry;PWD=Password12!"
$JERRY_ORACLE_CONN = "DATA SOURCE=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ORCLCDB.localdomain)));USER ID=sys;PASSWORD=Oradoc_db1;DBA Privilege=SYSDBA"

if ($Vendors -eq $null) {
    Write-Host "No servers specified. Specify any combination of 'sqlserver', 'postgres', 'mysql' and 'oracle'." -ForegroundColor Red
    Write-Host "If testing Oracle you will need to associate a license with your Docker login at: https://hub.docker.com/_/oracle-database-enterprise-edition" -ForegroundColor Yellow
    exit -1
}

if ($Vendors.Length -eq 1 -and $Vendors[0] -eq "*") {
    $Vendors = "sqlserver", "oracle", "mysql", "postgres"
}

$args = @()

if ($Vendors.Contains("sqlserver")) {
    [Environment]::SetEnvironmentVariable("JERRY_SQLSERVER_CONN", $JERRY_SQLSERVER_CONN, "User")
    $args += "-f", (Join-Path $PSScriptRoot "compose/sqlserver.yml")
}

if ($Vendors.Contains("postgres")) {
    [Environment]::SetEnvironmentVariable("JERRY_POSTGRES_CONN", $JERRY_POSTGRES_CONN, "User")
    $args += "-f", (Join-Path $PSScriptRoot "compose/postgres.yml")
}

if ($Vendors.Contains("mysql")) {
    [Environment]::SetEnvironmentVariable("JERRY_MYSQL_CONN", $JERRY_MYSQL_CONN, "User")
    $args += "-f", (Join-Path $PSScriptRoot "compose/mysql.yml")
}

if ($Vendors.Contains("oracle")) {
    [Environment]::SetEnvironmentVariable("JERRY_ORACLE_CONN", $JERRY_ORACLE_CONN, "User")
    $args += "-f", (Join-Path $PSScriptRoot "compose/oracle.yml")
}

try {
    docker-compose @args $Direction
} finally {
    if ($Vendors.Contains("sqlserver")) {
        [Environment]::SetEnvironmentVariable("JERRY_SQLSERVER_CONN", $null, "User")
    }

    if ($Vendors.Contains("postgres")) {
        [Environment]::SetEnvironmentVariable("JERRY_POSTGRES_CONN", $null, "User")
    }

    if ($Vendors.Contains("mysql")) {
        [Environment]::SetEnvironmentVariable("JERRY_MYSQL_CONN", $null, "User")
    }

    if ($Vendors.Contains("oracle")) {
        [Environment]::SetEnvironmentVariable("JERRY_ORACLE_CONN", $null, "User")
    }
}
function Get-Connection-Variable
{
    param(
        [String] $Vendor
    )
    
    if ($Vendor -eq "sqlserver") { "JERRY_SQLSERVER_CONN" }
    if ($Vendor -eq "postgres")  { "JERRY_POSTGRES_CONN" }
    if ($Vendor -eq "oracle")    { "JERRY_ORACLE_CONN" }
    if ($Vendor -eq "mysql")     { "JERRY_MYSQL_CONN" }
    if ($Vendor -eq "sqlite")    { "JERRY_SQLITE_CONN" }
}

function Get-All-Vendors
{
    [Array]"sqlite" + (Get-Live-Vendors)
}

function Get-Live-Vendors
{
    @("sqlserver", "postgres", "oracle", "mysql")
}

function Set-Docker-Live
{
    param(
        [Switch] $IsOffline
    )
    
    if ($IsOffline)
    {
        [Environment]::SetEnvironmentVariable("JERRY_DOCKER_LIVE", $null, "User")
        [Environment]::SetEnvironmentVariable("JERRY_DOCKER_LIVE", $null)
    }
    else
    {
        [Environment]::SetEnvironmentVariable("JERRY_DOCKER_LIVE", "TRUE", "User")
        [Environment]::SetEnvironmentVariable("JERRY_DOCKER_LIVE", "TRUE")
    }
}

function Is-Docker-Live
{
    $value1 = [Environment]::GetEnvironmentVariable("JERRY_DOCKER_LIVE", "User")
    $value2 = [Environment]::GetEnvironmentVariable("JERRY_DOCKER_LIVE")
    
    ($value1 -eq "TRUE" -or $value2 -eq "TRUE")
}

function Set-Live-Connection
{
    param(
        [Parameter(Mandatory=$true)]
        [String] $Vendor,
        [String] $ConnectionString
    )
    
    $variable = Get-Connection-Variable -Vendor $Vendor
    
    if ($variable)
    {
        [Environment]::SetEnvironmentVariable($variable, $ConnectionString, "User")
        [Environment]::SetEnvironmentVariable($variable, $ConnectionString)
    }
}

function Get-Live-Connection
{
    param(
        [Parameter(Mandatory=$true)]
        [String] $Vendor
    )
    
    $variable = Get-Connection-Variable -Vendor $Vendor
    
    if ($variable)
    {
        $value = [Environment]::GetEnvironmentVariable($variable, "Machine")
        
        if (-not $value) { $value = [Environment]::GetEnvironmentVariable($variable, "User") }
        if (-not $value) { $value = [Environment]::GetEnvironmentVariable($variable) }
        
        $value
    }
}

function Get-Connection-String
{
    param(
        [Parameter(Mandatory=$true)]
        [String] $Vendor,
        [Switch] $User
    )
    
    $data = Import-PowerShellDataFile -Path (Join-Path $PSScriptRoot connection.psd1)
    $connectionString = $data[$Vendor]
    
    if ($User -and $data.User) { $connectionString = $data.User[$Vendor] }
    elseif ($User) { $connectionString = $null }
    
    $connectionString
}

function Get-Yml-Path
{
    param(
        [Parameter(Mandatory=$true)]
        [String] $Vendor
    )

    $path = Join-Path $PSScriptRoot "yml\$Vendor.yml"
    
    if (Test-Path $path) { $path }
    else { $null }
}
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
    @("sqlserver", "mysql", "postgres", "oracle")
}

function Set-Connection-String
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
    }
}

function Get-Connection-String
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
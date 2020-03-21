function Test-Integration
{
    param(
        [Parameter(Mandatory=$true)]
        [String] $Vendor,
        [Parameter(Mandatory=$true)]
        [String] $ConnectionString,
        [String] $UserConnectionString,
        [String] $Version,
        [String] $TargetFramework = "netcoreapp3.0",
        [Parameter(Mandatory=$true)]
        [String] $PackageSource,
        [String] $Verbosity = "minimal",
        [Parameter(Mandatory=$true)]
        [String] $TempPath
    )
    
    $allFrameworks = Get-Target-Frameworks
    
    if (-not ($allFrameworks.Contains($TargetFramework)))
    {
        Write-Host "Invalid target framework '$TargetFramework'." -ForegroundColor Red
        Exit -1
    }
    
    if (-not $Version)
    {
        [String[]]$nuget = Get-NuGet-Versions -PackageSource $PackageSource
        
        if ($nuget.Length -eq 0)
        {
            Write-Host "No packages found in '$PackagesPath'" -ForegroundColor Red
            Exit -1
        }
        elseif ($nuget.Length -gt 1)
        {
            Write-Host "Multiple packages found in '$PackagesPath'." -ForegroundColor Red
            Write-Host "Please choose one with the -Version argument:" -ForegroundColor Red
            Write-Host $nuget
            Exit -1
        }
        else
        {
            $Version = $nuget[0]
        }
    }
    
    if (-not [System.IO.Path]::IsPathRooted($PackageSource))
    {
        $PackageSource = Resolve-Path $PackageSource
    }
    
    $package = Get-Vendor-Package $Vendor
    
    if (-not $package)
    {
        Write-Host "Unknown vendor '$Vendor'"
        Exit -1
    }
    
    foreach ($targetFramework in $allFrameworks)
    {
        Clean-Source $Vendor $targetFramework $TempPath
        Prepare-Source $Vendor $TargetFramework $TempPath $PackageSource
        
        Install-Cli $Vendor $Version $TargetFramework $Verbosity $TempPath $PackageSource
        
        $integrateConnection = $ConnectionString
        
        if ($UserConnectionString)
        {
            Create-Database-User $Vendor $integrateConnection $TargetFramework $TempPath
            
            $integrateConnection = $UserConnectionString
        }
        
        Prepare-Database $Vendor $integrateConnection $TargetFramework $TempPath
        Run-Project-Test $Vendor $Version $integrateConnection $PackageSource $targetFramework $Verbosity $TempPath
        
        $success = Verify-Integration $Vendor $targetFramework $TempPath
        
        if (-not $success) { break }
    }
}

function Get-Target-Frameworks { @("netcoreapp2.2", "netcoreapp3.0") }

function Verify-Integration
{
    param(
        [Parameter(Mandatory=$true)]
        [String] $Vendor,
        [String] $TargetFramework,
        [String] $TempPath
    )
    
    if ($TargetFramework)
    {
        $path = Join-Path (Get-Temp-Path $Vendor $TargetFramework $TempPath) "results.txt"
    
        if (Test-Path $path) { ((Get-Content $path) -eq "OK") }
        else { $false }
    }
    else
    {
        foreach ($targetFramework in Get-Target-Frameworks)
        {
            $result = (Verify-Integration $Vendor $targetFramework $TempPath)
            
            if (-not $result) { return $false }
        }
        
        return $true
    }
}

function Clean-Source
{
    param(
        [String] $Vendor,
        [String] $TargetFramework,
        [String] $TempPath
    )
    
    Write-Host "  Cleaning source ($TargetFramework)..." -ForegroundColor Cyan
    
    $path = Get-Temp-Path $Vendor $TargetFramework $TempPath
    
    if (Test-Path $path)
    {
        Remove-Item $path -Force -Recurse
    }
}

function Prepare-Source
{
    param(
        [String] $Vendor,
        [String] $TargetFramework,
        [String] $TempPath,
        [String] $PackageSource
    )
    
    Write-Host "  Preparing source ($TargetFramework)..." -ForegroundColor Cyan
    
    $source = Join-Path $PSScriptRoot ".\src"
    $temp = Join-Path $TempPath "$Vendor\$TargetFramework"
    $configFile = Join-Path $temp "nuget.config"

    New-Item $temp -ItemType Directory | Out-Null
    Copy-Item "$source\*" $temp -Recurse | Out-Null
    
    ((Get-Content -Path $configFile -Raw) -Replace '%PackageSource%', "$PackageSource") | Set-Content -Path $configFile
}

function Get-Temp-Path
{
    param(
        [String] $Vendor,
        [String] $TargetFramework,
        [String] $TempPath
    )
    
    Join-Path $TempPath "$Vendor\$TargetFramework"
}

function Install-Cli
{
    param(
        [String] $Vendor,
        [String] $Version,
        [String] $TargetFramework,
        [String] $Verbosity,
        [String] $TempPath,
        [String] $PackageSource
    )
    
    Write-Host "  Installing CLI ($TargetFramework)..." -ForegroundColor Cyan
    
    $toolPath = Get-Temp-Path $Vendor $TargetFramework $TempPath
    
    Push-Location $toolPath
    dotnet tool install --tool-path . dotnet-jerry --version $Version --verbosity $Verbosity --add-source "$PackageSource"
    Pop-Location
}

function Create-Database-User
{
    param(
        [String] $Vendor,
        [String] $ConnectionString,
        [String] $TargetFramework,
        [String] $TempPath
    )

    Write-Host "  Creating database user ($TargetFramework)..." -ForegroundColor Cyan
    
    $sql = Join-Path $PSScriptRoot "sql\user.$Vendor.sql"
    $toolPath = Get-Temp-Path $Vendor $TargetFramework $TempPath
    
    Push-Location $toolPath
    .\jerry run -v "$Vendor" -c "$ConnectionString" --file "$sql"
    if ($LastExitCode -ne 0) { Pop-Location; throw "Error running 'jerry run'." }
    Pop-Location
}

function Prepare-Database
{
    param(
        [String] $Vendor,
        [String] $ConnectionString,
        [String] $TargetFramework,
        [String] $TempPath
    )
    
    Write-Host "  Preparing database ($TargetFramework)..." -ForegroundColor Cyan

    $sql = Join-Path $PSScriptRoot "sql\prepare.$Vendor.sql"
    $toolPath = Get-Temp-Path $Vendor $TargetFramework $TempPath

    Push-Location $toolPath
    .\jerry run -v "$Vendor" -c "$ConnectionString" --file "$sql"
    if ($LastExitCode -ne 0) { Pop-Location; throw "Error running 'jerry run'." }
    Pop-Location
}

function Run-Project-Test
{
    param(
        [String] $Vendor,
        [String] $Version,
        [String] $ConnectionString,
        [String] $PackageSource,
        [String] $TargetFramework,
        [String] $Verbosity,
        [String] $TempPath,
        [Switch] $TranspileWithCli
    )
    
    $projectPath = Join-Path (Get-Temp-Path $Vendor $TargetFramework $TempPath) "Jerrycurl.Test.Integration"
    $resultsPath = Join-Path (Get-Temp-Path $Vendor $TargetFramework $TempPath) "results.txt"
    $package = Get-Vendor-Package $Vendor
    $constant = Get-Vendor-Constant $Vendor
    $buildArgs = @(
        "--framework",
        "$TargetFramework",
        "--verbosity",
        "$Verbosity",
        "--configuration",
        "Release",
        "-p:DefineConstants=$constant",
        "-p:DatabaseVendor=$Vendor"
    )
    
    if ($TranspileWithCli)
    {
        $buildArgs += "-p:JerrycurlUseCli=true", "-p:JerrycurlCliPath=..\jerry"
    }
    
    Push-Location $projectPath
    Write-Host "  Building project ($TargetFramework)..." -ForegroundColor Cyan
    dotnet add package Jerrycurl --version $Version --source "$PackageSource"
    dotnet add package $package --version $Version --source "$PackageSource"
    ..\jerry scaffold -v $Vendor -c $ConnectionString -ns "Jerrycurl.Test.Integration.Database" --verbose
    if ($LastExitCode -eq 0)
    {
        dotnet build @buildArgs
    }
    Write-Host "  Running code..." -ForegroundColor Cyan
    if ($LastExitCode -eq 0)
    {
        dotnet run --no-build --framework "$TargetFramework" --verbosity "$Verbosity" --configuration Release "$ConnectionString" "$resultsPath"
    }
    Pop-Location
}

function Get-NuGet-Versions
{
    param(
        [String] $PackageSource
    )
    
    $versions = @()
    
    if (Test-Path $PackageSource)
    {
        foreach ($file in (Get-ChildItem "$PackageSource\*.nupkg"))
        {
            $nuget = Parse-NuGet-String $file.Name
            
            if ($nuget.Package -eq "Jerrycurl")
            {
                $versions += $nuget.Version
            }
        }
    }

    $versions
}

function Get-Vendor-Constant
{
    param(
        [String] $Vendor
    )
    
    if ($Vendor -eq "sqlserver") { $constant = "VENDOR_SQLSERVER" }
    if ($Vendor -eq "postgres")  { $constant = "VENDOR_POSTGRES" }
    if ($Vendor -eq "oracle")    { $constant = "VENDOR_ORACLE" }
    if ($Vendor -eq "mysql")     { $constant = "VENDOR_MYSQL" }
    if ($Vendor -eq "sqlite")    { $constant = "VENDOR_SQLITE" }
    
    $constant
}

function Get-Vendor-Package
{
    param(
        [String] $Vendor
    )
    
    if ($Vendor -eq "sqlserver") { $package = "Jerrycurl.Vendors.SqlServer" }
    if ($Vendor -eq "postgres")  { $package = "Jerrycurl.Vendors.Postgres" }
    if ($Vendor -eq "oracle")    { $package = "Jerrycurl.Vendors.Oracle" }
    if ($Vendor -eq "mysql")     { $package = "Jerrycurl.Vendors.MySql" }
    if ($Vendor -eq "sqlite")    { $package = "Jerrycurl.Vendors.Sqlite" }
    
    $package
}

function Parse-NuGet-String
{
    param(
        [Parameter(Mandatory=$true)]
        [string] $InputString
    )
    

    $match = [Regex]::Match($InputString, '^([^\d]+)\.(\d+\.\d+\.\d+.*?)(\.nupkg)?$')
    
    if ($match.Success)
    {
        $package = $match.Groups[1].Value
        $version = $match.Groups[2].Value
        
        @{
            Package = $package
            Version = $version
        }
    }
}
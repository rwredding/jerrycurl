function Invoke-Jerry
{
	param(
        [Parameter(Mandatory=$false)] $Command,
		[Parameter(Mandatory=$false, ValueFromRemainingArguments=$true)] $Args
	)

    if (-not (Prepare-Jerry))
    {
        return;
    }

    if (-not $Command -and (Has-Database-Cli))
    {
        Push-Project-Dir

        jerry -- "scaffold" "@Database.cli"
      
        Pop-Location
    }
    elseif (-not $Command)
    {
        jerry
    }
	elseif (Is-Project-Missing)
	{
        jerry $Command -- $Args
	}
	else
	{
        Push-Project-Dir

        jerry $Command -- $Args
      
        Pop-Location
	}
}

function Has-Database-Cli
{
    $project = Get-Project

    Test-Path (Join-Path (Split-Path $project.FileName) "Database.cli") -PathType Leaf
}

function Push-Project-Dir
{
    $project = Get-Project
    
    Push-Location (Split-Path $project.FileName)
}

function Prepare-Jerry
{
	if (Is-DotNet-Missing)
	{
        Write-Host ".NET Core CLI not found. Make sure .NET Core SDK >= 2.2 is installed and in your PATH."
      
        return $false
	}
	
	if (Is-Jerry-Missing)
	{
        Write-Host "Jerrycurl CLI not found. Installing latest version..."
      
        dotnet tool install -g "dotnet-jerry"

        return $false
	}

    return $true
}

function Is-DotNet-Missing
{
    $cmd = (Get-Command "dotnet" -ErrorAction SilentlyContinue)
  
    ($cmd -eq $null)
}

function Is-Project-Missing
{
    $cmd = (Get-Command "Get-Project" -ErrorAction SilentlyContinue)
  
    ($cmd -eq $null)
}

function Is-Jerry-Missing
{
    $cmd = (Get-Command "jerry" -ErrorAction SilentlyContinue)
  
    ($cmd -eq $null)
}

Export-ModuleMember -Function Invoke-Jerry, Install-Jerry
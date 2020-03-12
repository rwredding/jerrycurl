function Jerry-Scaffold
{
	param(
        [Parameter(Mandatory=$false)] $Command,
		[Parameter(Mandatory=$false, ValueFromRemainingArguments=$true)] $Args
	)

    Prepare-Jerry

	if (Is-Project-Missing)
	{
        jerry scaffold -- $Args
	}
	else
	{
        $projectArgs = Get-Scaffold-Arguments
      
        Push-Project-Dir
      
        jerry scaffold -- $Args @projectArgs
      
        Pop-Location
	}
}

function Push-Project-Dir
{
    $project = Get-Project
    
    Push-Location (Split-Path $project.FileName)
}

function Get-Project-Arguments
{
    $project = Get-Project

    $namespace1 = $project.Properties.Item("JerrycurlCliNamespace").Value
    $namespace2 = $project.Properties.Item("RootNamespace").Value
    $vendor = $project.Properties.Item("JerrycurlCliVendor").Value
    $connection = $project.Properties.Item("JerrycurlCliConnection")
    $output = $project.Properties.Item("JerrycurlCliOutput").Value

    $args = @()

    if ($namespace1 -neq "")
    {
        args += "--namespace", $namespace1
    }
    elseif ($namespace2 -neq "")
    {
        args += "--namespace", $namespace2
    }

    if ($vendor -neq "")
    {
        args += "--vendor", $vendor
    }

    if ($connection -neq "")
    {
        args += "--connection", $connection
    }

    if ($output -neq "")
    {
        args += "--output", $output
    }

    return $args
}

function Prepare-Jerry
{
	if (Is-DotNet-Missing)
	{
        Write-Host ".NET Core CLI not found. Make sure .NET Core SDK >= 2.2 is installed and in your PATH."
      
        return;
	}
	
	if (Is-Jerry-Missing)
	{
        Write-Host "Jerrycurl CLI not found. Installing latest version..."
      
        dotnet tool install -g "dotnet-jerry"
	}
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

Export-ModuleMember -Function Jerry-Scaffold
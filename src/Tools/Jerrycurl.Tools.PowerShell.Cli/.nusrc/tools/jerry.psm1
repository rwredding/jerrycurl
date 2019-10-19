function Invoke-Jerry {
	param(
        [Parameter(Mandatory=$false)] $Command,
		[Parameter(Mandatory=$false)] [switch]$Reinstall,
		[Parameter(Mandatory=$false, ValueFromRemainingArguments=$true)] $Args
	)
	
	if (Is-DotNet-Missing)
	{
        Write-Host ".NET Core CLI not found. Make sure .NET Core SDK >= 2.0 is installed and in your PATH."
      
        return;
	}
	
	if (Is-Jerry-Missing)
	{
        Write-Host "Jerrycurl CLI not found. Installing latest version..."
      
        dotnet tool install -g "dotnet-jerry"
	}
	elseif ($Reinstall)
	{
		Write-Host "Reinstalling Jerrycurl CLI..."
		
		dotnet tool uninstall -g "dotnet-jerry"
		dotnet tool install -g "dotnet-jerry"
	}
	
	if (Is-Project-Missing)
	{
        jerry $Command -- $Args
	}
	else
	{
        $proj = Get-Project
        $projectDir = Split-Path $proj.FileName
        $rootNamespace = $proj.Properties.Item("RootNamespace").Value
      
        Push-Location $projectDir
      
        jerry $Command -- $Args --namespace $rootNamespace
      
        Pop-Location
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

Export-ModuleMember Invoke-Jerry
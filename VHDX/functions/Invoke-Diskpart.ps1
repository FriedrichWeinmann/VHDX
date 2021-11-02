function Invoke-Diskpart {
<#
	.SYNOPSIS
		Execute diskpart with the commands specified.
	
	.DESCRIPTION
		Execute diskpart with the commands specified.
		Commands are run sequentially, as if manually entered into the diskpart console.
		A final "exit" is implicitly ran and needs not be specified.
	
	.PARAMETER ArgumentList
		Commands to execute, in the order they should be executed.
	
	.EXAMPLE
		PS C:\> 'select vdisk file="C:\disks\w10.vhdx"', 'attach vdisk' | Invoke-Diskpart
	
		Mounts the C:\disks\w10.vhdx file
#>
	[CmdletBinding()]
	param (
		[Parameter(Mandatory = $true, ValueFromPipeline = $true)]
		[string[]]
		$ArgumentList
	)
	
	begin {
		Assert-Elevation -Cmdlet $PSCmdlet
		
		$startInfo = [System.Diagnostics.ProcessStartInfo]::new()
		$startInfo.UseShellExecute = $false
		$startInfo.RedirectStandardError = $true
		$startInfo.RedirectStandardInput = $true
		$startInfo.RedirectStandardOutput = $true
		$startInfo.WindowStyle = 'hidden'
		$startInfo.FileName = 'diskpart.exe'
		$process = [System.Diagnostics.Process]::new()
		$process.StartInfo = $startInfo
		
		$null = $process.Start()
	}
	process {
		foreach ($line in $ArgumentList) {
			$process.StandardInput.WriteLine($line)
		}
	}
	end {
		$process.StandardInput.WriteLine('exit')
		
		while (-not $process.HasExited) { Start-Sleep -Milliseconds 100 }
		
		$messages = $process.StandardOutput.ReadToEnd() -split 'DISKPART>' | ForEach-Object Trim | Microsoft.PowerShell.Utility\Select-Object -Skip 1
		foreach ($message in $messages) {
			Write-PSFMessage -String 'Invoke-Diskpart.Message' -StringValues $message
		}
		[PSCustomObject]@{
			Success = $process.ExitCode -eq 0
			Message = $messages
			Errors  = $process.StandardError.ReadToEnd()
			ExitCode = $process.ExitCode
		}
		$process.Dispose()
	}
}
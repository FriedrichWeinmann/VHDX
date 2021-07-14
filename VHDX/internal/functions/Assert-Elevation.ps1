function Assert-Elevation
{
<#
	.SYNOPSIS
		Asserts the current console is running "As Administrator"
	
	.DESCRIPTION
		Asserts the current console is running "As Administrator"
	
	.PARAMETER Cmdlet
		The $PSCmdlet variable of the caller, enabling errors to look as generated from that, rather than this internal helper.
	
	.EXAMPLE
		PS C:\> Assert-Elevation -Cmdlet $PSCmdlet
	
		Asserts the current console is running "As Administrator"
#>
	[CmdletBinding()]
	param (
		[Parameter(Mandatory = $true)]
		$Cmdlet
	)
	
	process
	{
		if (Test-PSFPowerShell -Elevated) { return }
		
		$exception = [System.InvalidOperationException]::new('Insufficient privileges, this command requires elevation and must be run "As Administrator"')
		$record = [System.Management.Automation.ErrorRecord]::new($exception, 'NotElevated', [System.Management.Automation.ErrorCategory]::SecurityError, $null)
		$Cmdlet.ThrowTerminatingError($record)
	}
}
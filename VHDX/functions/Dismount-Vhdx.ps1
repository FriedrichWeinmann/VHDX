function Dismount-Vhdx {
<#
	.SYNOPSIS
		Dismounts an already mounted VHDX file.
	
	.DESCRIPTION
		Dismounts an already mounted VHDX file.
		Use 'Mount-Vhdx' to mount one.
	
	.PARAMETER Path
		Path to the VHDX file to dismount.
	
	.PARAMETER EnableException
		This parameters disables user-friendly warnings and enables the throwing of exceptions.
		This is less user friendly, but allows catching exceptions in calling scripts.
	
	.EXAMPLE
		PS C:\> Dismount-Vhdx -Path 'c:\disks\profile.vhdx'
	
		Dismounts the file 'c:\disks\profile.vhdx'
#>
	[CmdletBinding()]
	param (
		[Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
		[PsfValidateScript('PSFramework.Validate.FSPath.File', ErrorString = 'PSFramework.Validate.FSPath.File')]
		[Alias('FullName')]
		[string[]]
		$Path,
		
		[switch]
		$EnableException
	)
	
	begin {
		Assert-Elevation -Cmdlet $PSCmdlet
	}
	process {
		foreach ($filePath in $Path) {
			Write-PSFMessage -String 'Dismount-Vhdx.Unmounting' -StringValues $filePath
			$resolvedPath = Resolve-PSFPath -Path $filePath -Provider FileSystem -SingleItem
			$result = Invoke-Diskpart -ArgumentList @(
				('select vdisk file="{0}"' -f $resolvedPath)
				'detach vdisk'
			)
			if (-not $result.Success) {
				Stop-PSFFunction -String 'Dismount-Vhdx.Failed' -StringValues $resolvedPath, $result.ExitCode, ($result.Message -join "`n"), $result.Errors -EnableException $EnableException -Target $filePath -Cmdlet $PSCmdlet -Continue
			}
		}
	}
}
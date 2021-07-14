function Mount-Vhdx {
<#
	.SYNOPSIS
		Mounts VHDX files as drives.
	
	.DESCRIPTION
		Mounts VHDX files as drives.
		Use Dismount-Vhdx to undo this once done with the disk.
	
	.PARAMETER Path
		Path to the file to mount
	
	.PARAMETER EnableException
		This parameters disables user-friendly warnings and enables the throwing of exceptions.
		This is less user friendly, but allows catching exceptions in calling scripts.
	
	.EXAMPLE
		PS C:\> Mount-Vhdx -Path 'c:\disks\profile.vhdx'
	
		Mounts the file 'c:\disks\profile.vhdx'
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
			Write-PSFMessage -String 'Mount-Vhdx.Mounting' -StringValues $filePath
			$resolvedPath = Resolve-PSFPath -Path $filePath -Provider FileSystem -SingleItem
			$result = Invoke-Diskpart -ArgumentList @(
				('select vdisk file="{0}"' -f $resolvedPath)
				'attach vdisk'
			)
			if (-not $result.Success) {
				Stop-PSFFunction -String 'Mount-Vhdx.Failed' -StringValues $resolvedPath, $result.ExitCode, ($result.Message -join "`n"), $result.Errors -EnableException $EnableException -Target $filePath -Cmdlet $PSCmdlet -Continue
			}
		}
	}
}
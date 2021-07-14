function Remove-VhdxContent {
<#
	.SYNOPSIS
		Removes files or folders from the specified vhdx file.
	
	.DESCRIPTION
		Removes files or folders from the specified vhdx file.
		If the VHDX hasn't been mounted yet, it will be mounted before removing content and dismounted once done.
		
		Note: Only supports single-volume VHDX files.
	
	.PARAMETER Path
		Path to the VHDX file.
	
	.PARAMETER SubPath
		Relative path _within_ the volume of the VHDX file, where to remove the files & folders specified.
		Defaults to the volume root.
	
	.PARAMETER Content
		Relative path to files or folders to remove.
	
	.PARAMETER EnableException
		This parameters disables user-friendly warnings and enables the throwing of exceptions.
		This is less user friendly, but allows catching exceptions in calling scripts.
	
	.PARAMETER Confirm
		If this switch is enabled, you will be prompted for confirmation before executing any operations that change state.
	
	.PARAMETER WhatIf
		If this switch is enabled, no actions are performed but informational messages will be displayed that explain what would happen if the command were to run.
	
	.EXAMPLE
		PS C:\> Remove-VhdxContent -Path 'c:\disks\data.vhdx' -Content 'secret.txt'
	
		Removes the file "secret.txt" from the volume root path of the specified disk.
	
	.EXAMPLE
		PS C:\> Remove-VhdxContent -Path 'c:\disks\data.vhdx' -SubPath config\secrets -Content '*.txt'
	
		Removes all txt files from the subfolder config\secrets of the volume of the specified disk.
		Assuming that volume, once mounted, has the drive letter "T", every text file in "T:\config\secrets" would be deleted.
#>
	[CmdletBinding(SupportsShouldProcess = $true)]
	param (
		[Parameter(Mandatory = $true)]
		[PsfValidateScript('PSFramework.Validate.FSPath.File', ErrorString = 'PSFramework.Validate.FSPath.File')]
		[string]
		$Path,
		
		[string]
		$SubPath,
		
		[Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
		[string[]]
		$Content,
		
		[switch]
		$EnableException
	)
	
	begin {
		Assert-Elevation -Cmdlet $PSCmdlet
		
		$resolvedPath = Resolve-PSFPath -Path $Path -Provider FileSystem -SingleItem
		$wasMounted = $false
		$disk = Get-Disk | Where-Object Location -EQ $resolvedPath
		if (-not $disk) {
			Mount-Vhdx -Path $resolvedPath
			$disk = Get-Disk | Where-Object Location -EQ $resolvedPath
			if ($disk) { $wasMounted = $true }
		}
		if (-not $disk) {
			Stop-PSFFunction -String 'Remove-VhdxContent.Mount.Failed' -StringValues $resolvedPath -EnableException $EnableException -Cmdlet $PSCmdlet -Target $resolvedPath
			return
		}
		$volume = $disk | Get-Partition | Get-Volume
		if (@($volume).Count -gt 1) {
			Stop-PSFFunction -String 'Remove-VhdxContent.Volumes.Multiple.Error' -StringValues $resolvedPath -EnableException $EnableException -Cmdlet $PSCmdlet -Target $resolvedPath
			return
		}
		$rootPath = '{0}:\' -f $volume.DriveLetter
		if ($SubPath) {
			$rootPath = Join-Path -Path $rootPath -ChildPath $SubPath
		}
	}
	process {
		if (Test-PSFFunctionInterrupt) { return }
		
		foreach ($item in $Content) {
			$itemPath = Join-Path -Path $rootPath -ChildPath $item
			Invoke-PSFProtectedCommand -ActionString 'Remove-VhdxContent.Removing.Item' -ActionStringValues $itemPath -Target $resolvedPath -ScriptBlock {
				Remove-Item -Path $itemPath -Recurse -Force -Confirm:$false -ErrorAction Stop
			} -EnableException $EnableException -PSCmdlet $PSCmdlet -Continue
		}
	}
	end {
		if ($wasMounted) {
			Dismount-Vhdx -Path $resolvedPath
		}
	}
}
function Add-VhdxContent {
<#
	.SYNOPSIS
		Adds files and folders to a target VHDX file.
	
	.DESCRIPTION
		Adds files and folders to a target VHDX file.
		If the VHDX hasn't been mounted yet, it will be mounted before adding content and dismounted once done.
		
		Note: Only supports single-volume VHDX files.
	
	.PARAMETER Path
		Path to the VHDX file.
	
	.PARAMETER SubPath
		Relative path _within_ the volume of the VHDX file, where to add the new files & folders.
		Defaults to the volume root.
	
	.PARAMETER Content
		Files and folders to add to the disk.
	
	.PARAMETER EnableException
		This parameters disables user-friendly warnings and enables the throwing of exceptions.
		This is less user friendly, but allows catching exceptions in calling scripts.
	
	.EXAMPLE
		PS C:\> Get-ChildItem C:\Data | Add-VhdxContent -Path 'c:\disks\data.vhdx'
	
		Adds everything under C:\Data to the root level of the volume in the disk 'c:\disks\data.vhdx'
	
	.EXAMPLE
		PS C:\> Get-ChildItem C:\Documents | Add-VhdxContent -Path 'c:\disks\data.vhdx' -SubPath "$env:COMPUTERNAME\documents"
	
		Adds everything under C:\Documents to the subfolder "$env:COMPUTERNAME\documents" of the volume in the disk 'c:\disks\data.vhdx'
#>
	[CmdletBinding()]
	param (
		[Parameter(Mandatory = $true)]
		[PsfValidateScript('PSFramework.Validate.FSPath.File', ErrorString = 'PSFramework.Validate.FSPath.File')]
		[string]
		$Path,
		
		[string]
		$SubPath,
		
		[Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
		[Alias('FullName')]
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
			Stop-PSFFunction -String 'Add-VhdxContent.Mount.Failed' -StringValues $resolvedPath -EnableException $EnableException -Cmdlet $PSCmdlet -Target $resolvedPath
			return
		}
		$volume = $disk | Get-Partition | Get-Volume
		if (@($volume).Count -gt 1) {
			Stop-PSFFunction -String 'Add-VhdxContent.Volumes.Multiple.Error' -StringValues $resolvedPath -EnableException $EnableException -Cmdlet $PSCmdlet -Target $resolvedPath
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
			Write-PSFMessage -String 'Add-VhdxContent.Adding.Item' -StringValues $item -Target $resolvedPath
			Copy-Item -Path $item -Destination $rootPath -Recurse -Force
		}
	}
	end {
		if ($wasMounted) {
			Dismount-Vhdx -Path $resolvedPath
		}
	}
}
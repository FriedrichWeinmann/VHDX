﻿function New-Vhdx {
<#
	.SYNOPSIS
		Creates a new vdhx file.
	
	.DESCRIPTION
		Creates a new vdhx file.
		It comes preconfigured with a single volume and can have files and folders pre-assigned to it.
	
	.PARAMETER Path
		Path to the VHDX file to create.
		Folder must exist, file should not.
	
	.PARAMETER Type
		How should the disk be provisioned.
		- Dynamic: File grows with its content (less consumption)
		- Fixed: File size equals size limit (slightly better performance)
		Defaults to: Dynamic
	
	.PARAMETER Size
		How large should the disk be?
		Defaults to 5GB
		Note: With "dynamic" disk type, the file backing it grows as its content is added, so this size limit need not be the actual storage consumed.
	
	.PARAMETER Label
		What label to add to the volume created in this disk.
		Defaults to: "Data"
	
	.PARAMETER Content
		Any files or folders to add to the disk.
	
	.EXAMPLE
		PS C:\> Get-ChildItem C:\install\sccm\ | New-Vhdx -Path 'C:\disks\sccm-content.vhdx'
	
		Creates a new vhdx file as 'C:\disks\sccm-content.vhdx', adding all files and folders from 'C:\install\sccm\' to it.
#>
	
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [PsfValidateScript('PSFramework.Validate.FSPath.FileOrParent', ErrorString = 'PSFramework.Validate.FSPath.FileOrParent')]
        [string]
        $Path,

        [ValidateSet('Dynamic', 'Fixed')]
        [string]
        $Type = 'Dynamic',

        [PSFSize]
        $Size = 5GB,

        [string]
        $Label = 'Data',

        [Parameter(ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
        [Alias('FullName')]
        [string[]]
        $Content
    )
	
	begin {
		Assert-Elevation -Cmdlet $PSCmdlet
		
        $typeMapping = @{
            Dynamic = 'expandable'
            Fixed   = 'fixed'
        }

        $resolvedPath = Resolve-PSFPath -Path $Path -Provider FileSystem -SingleItem -NewChild
		$diskPartCommand = 'create vdisk file="{0}" maximum={1} type={2}' -f $resolvedPath, $Size.Megabyte, $typeMapping[$Type]
		$result = Invoke-Diskpart -ArgumentList $diskPartCommand
		
		if (-not $result.Success) {
			Stop-PSFFunction -String 'New-Vhdx.CreateDisk.Failed' -StringValues $resolvedPath, $result.Errors -EnableException $true
        }
        

        $result = Invoke-Diskpart -ArgumentList @(
            ('select vdisk file="{0}"' -f $resolvedPath)
            'attach vdisk'
            'create partition primary'
            ('format fs=NTFS label="{0}" quick' -f $Label)
            'assign'
        )
		if (-not $result.Success) {
			Stop-PSFFunction -String 'New-Vhdx.PrepareDisk.Failed' -StringValues $resolvedPath, $result.Errors -EnableException $true
        }

        $disk = Get-Disk | Where-Object Location -EQ $resolvedPath
        $volume = $disk | Get-Partition | Get-Volume
        $rootPath = '{0}:\' -f $volume.DriveLetter
    }
    process {
		foreach ($inputItem in $Content) {
			Write-PSFMessage -String 'New-Vhdx.Copying' -StringValues $inputItem
            Copy-Item -Path $inputItem -Destination $rootPath -Force -Recurse
        }
    }
	end {
		Dismount-Vhdx -Path $resolvedPath -EnableException
    }
}
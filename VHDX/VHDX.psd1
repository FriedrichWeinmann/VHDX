@{
	# Script module or binary module file associated with this manifest
	RootModule = 'VHDX.psm1'
	
	# Version number of this module.
	ModuleVersion = '1.0.1'
	
	# ID used to uniquely identify this module
	GUID = 'a59182b4-2f77-44f2-bc12-323b53bc82ba'
	
	# Author of this module
	Author = 'Friedrich Weinmann'
	
	# Company or vendor of this module
	CompanyName = 'Microsoft'
	
	# Copyright statement for this module
	Copyright = 'Copyright (c) 2021 Friedrich Weinmann'
	
	# Description of the functionality provided by this module
	Description = 'Create and maintain VHDX files'
	
	# Minimum version of the Windows PowerShell engine required by this module
	PowerShellVersion = '5.0'
	
	# Modules that must be imported into the global environment prior to importing
	# this module
	RequiredModules = @(
		@{ ModuleName='PSFramework'; ModuleVersion='1.6.198' }
	)
	
	# Assemblies that must be loaded prior to importing this module
	# RequiredAssemblies = @('bin\VHDX.dll')
	
	# Type files (.ps1xml) to be loaded when importing this module
	# TypesToProcess = @('xml\VHDX.Types.ps1xml')
	
	# Format files (.ps1xml) to be loaded when importing this module
	# FormatsToProcess = @('xml\VHDX.Format.ps1xml')
	
	# Functions to export from this module
	FunctionsToExport = @(
		'Add-VhdxContent'
		'Dismount-Vhdx'
		'Invoke-Diskpart'
		'Mount-Vhdx'
		'New-Vhdx'
		'Remove-VhdxContent'
	)
	
	# Private data to pass to the module specified in ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
	PrivateData = @{
		
		#Support for PowerShellGet galleries.
		PSData = @{
			
			# Tags applied to this module. These help with module discovery in online galleries.
			Tags = @('VHDX')
			
			# A URL to the license for this module.
			LicenseUri = 'https://github.com/FriedrichWeinmann/VHDX/blob/master/LICENSE'
			
			# A URL to the main website for this project.
			ProjectUri = 'https://github.com/FriedrichWeinmann/VHDX'
			
			# A URL to an icon representing this module.
			# IconUri = ''
			
			# ReleaseNotes of this module
			ReleaseNotes = 'https://github.com/FriedrichWeinmann/VHDX/blob/master/VHDX/changelog.md'
			
		} # End of PSData hashtable
		
	} # End of PrivateData hashtable
}
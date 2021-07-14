# This is where the strings go, that are written by
# Write-PSFMessage, Stop-PSFFunction or the PSFramework validation scriptblocks
@{
	'Add-VhdxContent.Adding.Item'			    = 'Adding file or folder: {0}' # $item
	'Add-VhdxContent.Mount.Failed'			    = 'Failed to mount disk: {0}' # $resolvedPath
	'Add-VhdxContent.Volumes.Multiple.Error'    = 'More than one volume found on {0}. Currently, only disks with a single volume are supported.' # $resolvedPath
	
	'Dismount-Vhdx.Failed'					    = "Failed to mount disk '{0}' (Exit code: {1}).`nMessages:`n{2}`nErrors:`n{3}" # $resolvedPath, $result.ExitCode, ($result.Message -join "`n"), $result.Errors
	'Dismount-Vhdx.Unmounting'				    = 'Dismounting disk: {0}' # $filePath
	
	'Invoke-Diskpart.Message'				    = ' {0}' # $message
	
	'Mount-Vhdx.Failed'						    = "Failed to mount disk '{0}' (Exit code: {1}).`nMessages:`n{2}`nErrors:`n{3}" # $resolvedPath, $result.ExitCode, ($result.Message -join "`n"), $result.Errors
	'Mount-Vhdx.Mounting'					    = 'Mounting disk: {0}' # $filePath
	
	'New-Vhdx.Copying'						    = 'Adding content to new disk: {0}' # $inputItem
	'New-Vhdx.CreateDisk.Failed'			    = 'Failed to create disk {0}: {1}' # $resolvedPath, $result.Errors
	'New-Vhdx.PrepareDisk.Failed'			    = 'Failed to prepare & configure disk {0}: {1}' # $resolvedPath, $result.Errors
	
	'Remove-VhdxContent.Mount.Failed'		    = 'Failed to mount disk: {0}' # $resolvedPath
	'Remove-VhdxContent.Removing.Item'		    = 'Removing file or folder: {0}' # $itemPath
	'Remove-VhdxContent.Volumes.Multiple.Error' = 'More than one volume found on {0}. Currently, only disks with a single volume are supported.' # $resolvedPath
}
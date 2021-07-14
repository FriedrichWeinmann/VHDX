# VHDX

## Synopsis

This PowerShell module is designed to help creating and managing simple data VHDX files.

## Installation

```powershell
Install-Module VHDX
```

## Creating a new disk

```powershell
# Simple disk, no content
New-Vhdx -Path C:\temp\empty.vhdx

# Disk with content
Get-ChildItem C:\install\sccm\ | New-Vhdx -Path 'C:\disks\sccm-content.vhdx'
```

## Adding content to existing disk

```powershell
# Add to root path of volume
Get-ChildItem C:\Data | Add-VhdxContent -Path 'c:\disks\data.vhdx'

# Add to a child folder
Get-ChildItem C:\Documents | Add-VhdxContent -Path 'c:\disks\data.vhdx' -SubPath "$env:COMPUTERNAME\documents"
```

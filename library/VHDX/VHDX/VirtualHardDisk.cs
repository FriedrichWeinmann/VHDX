using System;
using System.ComponentModel;
using System.IO;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace Vhdx
{
    // Based on code written by the Hyper-V Test team.
    /// <summary>
    /// The Virtual Hard Disk class provides methods for creating and manipulating Virtual Hard Disk files.
    /// </summary>
    public class VirtualHardDisk
    {
        #region Sparse Disks

        /// <summary>
        /// Abbreviated signature of CreateSparseDisk so it's easier to use from WIM2VHD.
        /// </summary>
        /// <param name="virtualStorageDeviceType">The type of disk to create, VHD or VHDX.</param>
        /// <param name="path">The path of the disk to create.</param>
        /// <param name="size">The maximum size of the disk to create.</param>
        /// <param name="overwrite">Overwrite the VHD if it already exists.</param>
        public static void
        CreateSparseDisk(
            NativeMethods.VirtualStorageDeviceType virtualStorageDeviceType,
            string path,
            ulong size,
            bool overwrite)
        {

            CreateSparseDisk(
                path,
                size,
                overwrite,
                null,
                IntPtr.Zero,
                (virtualStorageDeviceType == NativeMethods.VirtualStorageDeviceType.VHD)
                    ? NativeMethods.DEFAULT_BLOCK_SIZE
                    : 0,
                virtualStorageDeviceType,
                NativeMethods.DISK_SECTOR_SIZE);
        }

        /// <summary>
        /// Creates a new sparse (dynamically expanding) virtual hard disk (.vhd). Supports both sync and async modes.
        /// The VHD image file uses only as much space on the backing store as needed to store the actual data the VHD currently contains.
        /// </summary>
        /// <param name="path">The path and name of the VHD to create.</param>
        /// <param name="size">The size of the VHD to create in bytes.
        /// When creating this type of VHD, the VHD API does not test for free space on the physical backing store based on the maximum size requested,
        /// therefore it is possible to successfully create a dynamic VHD with a maximum size larger than the available physical disk free space.
        /// The maximum size of a dynamic VHD is 2,040 GB. The minimum size is 3 MB.</param>
        /// <param name="source">Optional path to pre-populate the new virtual disk object with block data from an existing disk
        /// This path may refer to a VHD or a physical disk. Use NULL if you don't want a source.</param>
        /// <param name="overwrite">If the VHD exists, setting this parameter to 'True' will delete it and create a new one.</param>
        /// <param name="overlapped">If not null, the operation runs in async mode</param>
        /// <param name="blockSizeInBytes">Block size for the VHD.</param>
        /// <param name="virtualStorageDeviceType">VHD format version (VHD1 or VHD2)</param>
        /// <param name="sectorSizeInBytes">Sector size for the VHD.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid size is specified</exception>
        /// <exception cref="FileNotFoundException">Thrown when source VHD is not found.</exception>
        /// <exception cref="SecurityException">Thrown when there was an error while creating the default security descriptor.</exception>
        /// <exception cref="Win32Exception">Thrown when an error occurred while creating the VHD.</exception>
        public static void
        CreateSparseDisk(
            string path,
            ulong size,
            bool overwrite,
            string source,
            IntPtr overlapped,
            uint blockSizeInBytes,
            NativeMethods.VirtualStorageDeviceType virtualStorageDeviceType,
            uint sectorSizeInBytes)
        {

            // Validate the virtualStorageDeviceType
            if (virtualStorageDeviceType != NativeMethods.VirtualStorageDeviceType.VHD && virtualStorageDeviceType != NativeMethods.VirtualStorageDeviceType.VHDX)
            {

                throw (
                    new ArgumentOutOfRangeException(
                        "virtualStorageDeviceType",
                        virtualStorageDeviceType,
                        "VirtualStorageDeviceType must be VHD or VHDX."
                ));
            }

            // Validate size. It needs to be a multiple of DISK_SECTOR_SIZE (512)...
            if ((size % NativeMethods.DISK_SECTOR_SIZE) != 0)
            {

                throw (
                    new ArgumentOutOfRangeException(
                        "size",
                        size,
                        "The size of the virtual disk must be a multiple of 512."
                ));
            }

            if ((!String.IsNullOrEmpty(source)) && (!System.IO.File.Exists(source)))
            {

                throw (
                    new System.IO.FileNotFoundException(
                        "Unable to find the source file.",
                        source
                ));
            }

            if ((overwrite) && (System.IO.File.Exists(path)))
            {

                System.IO.File.Delete(path);
            }

            NativeMethods.CreateVirtualDiskParameters createParams = new NativeMethods.CreateVirtualDiskParameters();

            // Select the correct version.
            createParams.Version = (virtualStorageDeviceType == NativeMethods.VirtualStorageDeviceType.VHD)
                ? NativeMethods.CreateVirtualDiskVersion.Version1
                : NativeMethods.CreateVirtualDiskVersion.Version2;

            createParams.UniqueId = Guid.NewGuid();
            createParams.MaximumSize = size;
            createParams.BlockSizeInBytes = blockSizeInBytes;
            createParams.SectorSizeInBytes = sectorSizeInBytes;
            createParams.ParentPath = null;
            createParams.SourcePath = source;
            createParams.OpenFlags = NativeMethods.OpenVirtualDiskFlags.None;
            createParams.GetInfoOnly = false;
            createParams.ParentVirtualStorageType = new NativeMethods.VirtualStorageType();
            createParams.SourceVirtualStorageType = new NativeMethods.VirtualStorageType();

            //
            // Create and init a security descriptor.
            // Since we're creating an essentially blank SD to use with CreateVirtualDisk
            // the VHD will take on the security values from the parent directory.
            //

            NativeMethods.SecurityDescriptor securityDescriptor;
            if (!NativeMethods.InitializeSecurityDescriptor(out securityDescriptor, 1))
            {

                throw (
                    new SecurityException(
                        "Unable to initialize the security descriptor for the virtual disk."
                ));
            }

            NativeMethods.VirtualStorageType virtualStorageType = new NativeMethods.VirtualStorageType();
            virtualStorageType.DeviceId = virtualStorageDeviceType;
            virtualStorageType.VendorId = NativeMethods.VirtualStorageTypeVendorMicrosoft;

            SafeFileHandle vhdHandle;

            uint returnCode = NativeMethods.CreateVirtualDisk(
                ref virtualStorageType,
                    path,
                    (virtualStorageDeviceType == NativeMethods.VirtualStorageDeviceType.VHD)
                        ? NativeMethods.VirtualDiskAccessMask.All
                        : NativeMethods.VirtualDiskAccessMask.None,
                ref securityDescriptor,
                    NativeMethods.CreateVirtualDiskFlags.None,
                    0,
                ref createParams,
                    overlapped,
                out vhdHandle);

            vhdHandle.Close();

            if (NativeMethods.ERROR_SUCCESS != returnCode && NativeMethods.ERROR_IO_PENDING != returnCode)
            {

                throw (
                    new Win32Exception(
                        (int)returnCode
                ));
            }
        }

        #endregion Sparse Disks

        #region Fixed Disks

        /// <summary>
        /// Abbreviated signature of CreateFixedDisk so it's easier to use from WIM2VHD.
        /// </summary>
        /// <param name="virtualStorageDeviceType">The type of disk to create, VHD or VHDX.</param>
        /// <param name="path">The path of the disk to create.</param>
        /// <param name="size">The maximum size of the disk to create.</param>
        /// <param name="overwrite">Overwrite the VHD if it already exists.</param>
        public static void CreateFixedDisk(
            NativeMethods.VirtualStorageDeviceType virtualStorageDeviceType,
            string path,
            ulong size,
            bool overwrite)
        {

            CreateFixedDisk(
                path,
                size,
                overwrite,
                null,
                IntPtr.Zero,
                0,
                virtualStorageDeviceType,
                NativeMethods.DISK_SECTOR_SIZE);
        }

        /// <summary>
        /// Creates a fixed-size Virtual Hard Disk. Supports both sync and async modes. This methods always calls the V2 version of the
        /// CreateVirtualDisk API, and creates VHD2.
        /// </summary>
        /// <param name="path">The path and name of the VHD to create.</param>
        /// <param name="size">The size of the VHD to create in bytes.
        /// The VHD image file is pre-allocated on the backing store for the maximum size requested.
        /// The maximum size of a dynamic VHD is 2,040 GB. The minimum size is 3 MB.</param>
        /// <param name="source">Optional path to pre-populate the new virtual disk object with block data from an existing disk
        /// This path may refer to a VHD or a physical disk. Use NULL if you don't want a source.</param>
        /// <param name="overwrite">If the VHD exists, setting this parameter to 'True' will delete it and create a new one.</param>
        /// <param name="overlapped">If not null, the operation runs in async mode</param>
        /// <param name="blockSizeInBytes">Block size for the VHD.</param>
        /// <param name="virtualStorageDeviceType">Virtual storage device type: VHD1 or VHD2.</param>
        /// <param name="sectorSizeInBytes">Sector size for the VHD.</param>
        /// <remarks>Creating a fixed disk can be a time consuming process!</remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid size or wrong virtual storage device type is specified.</exception>
        /// <exception cref="FileNotFoundException">Thrown when source VHD is not found.</exception>
        /// <exception cref="SecurityException">Thrown when there was an error while creating the default security descriptor.</exception>
        /// <exception cref="Win32Exception">Thrown when an error occurred while creating the VHD.</exception>
        public static void CreateFixedDisk(
            string path,
            ulong size,
            bool overwrite,
            string source,
            IntPtr overlapped,
            uint blockSizeInBytes,
            NativeMethods.VirtualStorageDeviceType virtualStorageDeviceType,
            uint sectorSizeInBytes)
        {

            // Validate the virtualStorageDeviceType
            if (virtualStorageDeviceType != NativeMethods.VirtualStorageDeviceType.VHD && virtualStorageDeviceType != NativeMethods.VirtualStorageDeviceType.VHDX)
            {

                throw (
                    new ArgumentOutOfRangeException(
                        "virtualStorageDeviceType",
                        virtualStorageDeviceType,
                        "VirtualStorageDeviceType must be VHD or VHDX."
                ));
            }

            // Validate size. It needs to be a multiple of DISK_SECTOR_SIZE (512)...
            if ((size % NativeMethods.DISK_SECTOR_SIZE) != 0)
            {

                throw (
                    new ArgumentOutOfRangeException(
                        "size",
                        size,
                        "The size of the virtual disk must be a multiple of 512."
                ));
            }

            if ((!String.IsNullOrEmpty(source)) && (!System.IO.File.Exists(source)))
            {

                throw (
                    new System.IO.FileNotFoundException(
                        "Unable to find the source file.",
                        source
                ));
            }

            if ((overwrite) && (System.IO.File.Exists(path)))
            {

                System.IO.File.Delete(path);
            }

            NativeMethods.CreateVirtualDiskParameters createParams = new NativeMethods.CreateVirtualDiskParameters();

            // Select the correct version.
            createParams.Version = (virtualStorageDeviceType == NativeMethods.VirtualStorageDeviceType.VHD)
                ? NativeMethods.CreateVirtualDiskVersion.Version1
                : NativeMethods.CreateVirtualDiskVersion.Version2;

            createParams.UniqueId = Guid.NewGuid();
            createParams.MaximumSize = size;
            createParams.BlockSizeInBytes = blockSizeInBytes;
            createParams.SectorSizeInBytes = sectorSizeInBytes;
            createParams.ParentPath = null;
            createParams.SourcePath = source;
            createParams.OpenFlags = NativeMethods.OpenVirtualDiskFlags.None;
            createParams.GetInfoOnly = false;
            createParams.ParentVirtualStorageType = new NativeMethods.VirtualStorageType();
            createParams.SourceVirtualStorageType = new NativeMethods.VirtualStorageType();

            //
            // Create and init a security descriptor.
            // Since we're creating an essentially blank SD to use with CreateVirtualDisk
            // the VHD will take on the security values from the parent directory.
            //

            NativeMethods.SecurityDescriptor securityDescriptor;
            if (!NativeMethods.InitializeSecurityDescriptor(out securityDescriptor, 1))
            {
                throw (
                    new SecurityException(
                        "Unable to initialize the security descriptor for the virtual disk."
                ));
            }

            NativeMethods.VirtualStorageType virtualStorageType = new NativeMethods.VirtualStorageType();
            virtualStorageType.DeviceId = virtualStorageDeviceType;
            virtualStorageType.VendorId = NativeMethods.VirtualStorageTypeVendorMicrosoft;

            SafeFileHandle vhdHandle;

            uint returnCode = NativeMethods.CreateVirtualDisk(
                ref virtualStorageType,
                    path,
                    (virtualStorageDeviceType == NativeMethods.VirtualStorageDeviceType.VHD)
                        ? NativeMethods.VirtualDiskAccessMask.All
                        : NativeMethods.VirtualDiskAccessMask.None,
                ref securityDescriptor,
                    NativeMethods.CreateVirtualDiskFlags.FullPhysicalAllocation,
                    0,
                ref createParams,
                    overlapped,
                out vhdHandle);

            vhdHandle.Close();

            if (NativeMethods.ERROR_SUCCESS != returnCode && NativeMethods.ERROR_IO_PENDING != returnCode)
            {

                throw (
                    new Win32Exception(
                        (int)returnCode
                ));
            }
        }

        #endregion Fixed Disks
    }
}

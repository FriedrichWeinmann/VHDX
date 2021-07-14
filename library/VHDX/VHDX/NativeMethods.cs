using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Vhdx
{
    /// <summary>
    /// P/Invoke methods and associated enums, flags, and structs.
    /// </summary>
    public class NativeMethods
    {

        #region Delegates and Callbacks
        #region WIMGAPI

        ///<summary>
        ///User-defined function used with the RegisterMessageCallback or UnregisterMessageCallback function.
        ///</summary>
        ///<param name="MessageId">Specifies the message being sent.</param>
        ///<param name="wParam">Specifies additional message information. The contents of this parameter depend on the value of the
        ///MessageId parameter.</param>
        ///<param name="lParam">Specifies additional message information. The contents of this parameter depend on the value of the
        ///MessageId parameter.</param>
        ///<param name="UserData">Specifies the user-defined value passed to RegisterCallback.</param>
        ///<returns>
        ///To indicate success and to enable other subscribers to process the message return WIM_MSG_SUCCESS.
        ///To prevent other subscribers from receiving the message, return WIM_MSG_DONE.
        ///To cancel an image apply or capture, return WIM_MSG_ABORT_IMAGE when handling the WIM_MSG_PROCESS message.
        ///</returns>
        public delegate uint
        WimMessageCallback(
            uint MessageId,
            IntPtr wParam,
            IntPtr lParam,
            IntPtr UserData
        );

        public static void
        RegisterMessageCallback(
            WimFileHandle hWim,
            WimMessageCallback callback)
        {

            uint _callback = NativeMethods.WimRegisterMessageCallback(hWim, callback, IntPtr.Zero);
            int rc = Marshal.GetLastWin32Error();
            if (0 != rc)
            {
                // Throw an exception if something bad happened on the Win32 end.
                throw
                    new InvalidOperationException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Unable to register message callback."
                ));
            }
        }

        public static void
        UnregisterMessageCallback(
            WimFileHandle hWim,
            WimMessageCallback registeredCallback)
        {

            bool status = NativeMethods.WimUnregisterMessageCallback(hWim, registeredCallback);
            int rc = Marshal.GetLastWin32Error();
            if (!status)
            {
                throw
                    new InvalidOperationException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Unable to unregister message callback."
                ));
            }
        }

        #endregion WIMGAPI
        #endregion Delegates and Callbacks

        #region Constants

        #region VDiskInterop

        /// <summary>
        /// The default depth in a VHD parent chain that this library will search through.
        /// If you want to go more than one disk deep into the parent chain, provide a different value.
        /// </summary>
        public const uint OPEN_VIRTUAL_DISK_RW_DEFAULT_DEPTH = 0x00000001;

        public const uint DEFAULT_BLOCK_SIZE = 0x00080000;
        public const uint DISK_SECTOR_SIZE = 0x00000200;

        internal const uint ERROR_VIRTDISK_NOT_VIRTUAL_DISK = 0xC03A0015;
        internal const uint ERROR_NOT_FOUND = 0x00000490;
        internal const uint ERROR_IO_PENDING = 0x000003E5;
        internal const uint ERROR_INSUFFICIENT_BUFFER = 0x0000007A;
        internal const uint ERROR_ERROR_DEV_NOT_EXIST = 0x00000037;
        internal const uint ERROR_BAD_COMMAND = 0x00000016;
        internal const uint ERROR_SUCCESS = 0x00000000;

        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const short FILE_ATTRIBUTE_NORMAL = 0x00000080;
        public const uint CREATE_NEW = 0x00000001;
        public const uint CREATE_ALWAYS = 0x00000002;
        public const uint OPEN_EXISTING = 0x00000003;
        public const short INVALID_HANDLE_VALUE = -1;

        internal static Guid VirtualStorageTypeVendorUnknown = new Guid("00000000-0000-0000-0000-000000000000");
        internal static Guid VirtualStorageTypeVendorMicrosoft = new Guid("EC984AEC-A0F9-47e9-901F-71415A66345B");

        #endregion VDiskInterop

        #region WIMGAPI

        public const uint WIM_FLAG_VERIFY = 0x00000002;
        public const uint WIM_FLAG_INDEX = 0x00000004;

        public const uint WM_APP = 0x00008000;

        #endregion WIMGAPI

        #endregion Constants

        #region Enums and Flags

        #region VDiskInterop

        /// <summary>
        /// Indicates the version of the virtual disk to create.
        /// </summary>
        public enum CreateVirtualDiskVersion : int
        {
            VersionUnspecified = 0x00000000,
            Version1 = 0x00000001,
            Version2 = 0x00000002
        }

        public enum OpenVirtualDiskVersion : int
        {
            VersionUnspecified = 0x00000000,
            Version1 = 0x00000001,
            Version2 = 0x00000002
        }

        /// <summary>
        /// Contains the version of the virtual hard disk (VHD) ATTACH_VIRTUAL_DISK_PARAMETERS structure to use in calls to VHD functions.
        /// </summary>
        public enum AttachVirtualDiskVersion : int
        {
            VersionUnspecified = 0x00000000,
            Version1 = 0x00000001,
            Version2 = 0x00000002
        }

        public enum CompactVirtualDiskVersion : int
        {
            VersionUnspecified = 0x00000000,
            Version1 = 0x00000001
        }

        /// <summary>
        /// Contains the type and provider (vendor) of the virtual storage device.
        /// </summary>
        public enum VirtualStorageDeviceType : int
        {
            /// <summary>
            /// The storage type is unknown or not valid.
            /// </summary>
            Unknown = 0x00000000,
            /// <summary>
            /// For internal use only. This type is not supported.
            /// </summary>
            ISO = 0x00000001,
            /// <summary>
            /// Virtual Hard Disk device type.
            /// </summary>
            VHD = 0x00000002,
            /// <summary>
            /// Virtual Hard Disk v2 device type.
            /// </summary>
            VHDX = 0x00000003
        }

        /// <summary>
        /// Contains virtual hard disk (VHD) open request flags.
        /// </summary>
        [Flags]
        public enum OpenVirtualDiskFlags
        {
            /// <summary>
            /// No flags. Use system defaults.
            /// </summary>
            None = 0x00000000,
            /// <summary>
            /// Open the VHD file (backing store) without opening any differencing-chain parents. Used to correct broken parent links.
            /// </summary>
            NoParents = 0x00000001,
            /// <summary>
            /// Reserved.
            /// </summary>
            BlankFile = 0x00000002,
            /// <summary>
            /// Reserved.
            /// </summary>
            BootDrive = 0x00000004,
        }

        /// <summary>
        /// Contains the bit mask for specifying access rights to a virtual hard disk (VHD).
        /// </summary>
        [Flags]
        public enum VirtualDiskAccessMask
        {
            /// <summary>
            /// Only Version2 of OpenVirtualDisk API accepts this parameter
            /// </summary>
            None = 0x00000000,
            /// <summary>
            /// Open the virtual disk for read-only attach access. The caller must have READ access to the virtual disk image file.
            /// </summary>
            /// <remarks>
            /// If used in a request to open a virtual disk that is already open, the other handles must be limited to either
            /// VIRTUAL_DISK_ACCESS_DETACH or VIRTUAL_DISK_ACCESS_GET_INFO access, otherwise the open request with this flag will fail.
            /// </remarks>
            AttachReadOnly = 0x00010000,
            /// <summary>
            /// Open the virtual disk for read-write attaching access. The caller must have (READ | WRITE) access to the virtual disk image file.
            /// </summary>
            /// <remarks>
            /// If used in a request to open a virtual disk that is already open, the other handles must be limited to either
            /// VIRTUAL_DISK_ACCESS_DETACH or VIRTUAL_DISK_ACCESS_GET_INFO access, otherwise the open request with this flag will fail.
            /// If the virtual disk is part of a differencing chain, the disk for this request cannot be less than the readWriteDepth specified
            /// during the prior open request for that differencing chain.
            /// </remarks>
            AttachReadWrite = 0x00020000,
            /// <summary>
            /// Open the virtual disk to allow detaching of an attached virtual disk. The caller must have
            /// (FILE_READ_ATTRIBUTES | FILE_READ_DATA) access to the virtual disk image file.
            /// </summary>
            Detach = 0x00040000,
            /// <summary>
            /// Information retrieval access to the virtual disk. The caller must have READ access to the virtual disk image file.
            /// </summary>
            GetInfo = 0x00080000,
            /// <summary>
            /// Virtual disk creation access.
            /// </summary>
            Create = 0x00100000,
            /// <summary>
            /// Open the virtual disk to perform offline meta-operations. The caller must have (READ | WRITE) access to the virtual
            /// disk image file, up to readWriteDepth if working with a differencing chain.
            /// </summary>
            /// <remarks>
            /// If the virtual disk is part of a differencing chain, the backing store (host volume) is opened in RW exclusive mode up to readWriteDepth.
            /// </remarks>
            MetaOperations = 0x00200000,
            /// <summary>
            /// Reserved.
            /// </summary>
            Read = 0x000D0000,
            /// <summary>
            /// Allows unrestricted access to the virtual disk. The caller must have unrestricted access rights to the virtual disk image file.
            /// </summary>
            All = 0x003F0000,
            /// <summary>
            /// Reserved.
            /// </summary>
            Writable = 0x00320000
        }

        /// <summary>
        /// Contains virtual hard disk (VHD) creation flags.
        /// </summary>
        [Flags]
        public enum CreateVirtualDiskFlags
        {
            /// <summary>
            /// Contains virtual hard disk (VHD) creation flags.
            /// </summary>
            None = 0x00000000,
            /// <summary>
            /// Pre-allocate all physical space necessary for the size of the virtual disk.
            /// </summary>
            /// <remarks>
            /// The CREATE_VIRTUAL_DISK_FLAG_FULL_PHYSICAL_ALLOCATION flag is used for the creation of a fixed VHD.
            /// </remarks>
            FullPhysicalAllocation = 0x00000001
        }

        /// <summary>
        /// Contains virtual disk attach request flags.
        /// </summary>
        [Flags]
        public enum AttachVirtualDiskFlags
        {
            /// <summary>
            /// No flags. Use system defaults.
            /// </summary>
            None = 0x00000000,
            /// <summary>
            /// Attach the virtual disk as read-only.
            /// </summary>
            ReadOnly = 0x00000001,
            /// <summary>
            /// No drive letters are assigned to the disk's volumes.
            /// </summary>
            /// <remarks>Oddly enough, this doesn't apply to NTFS mount points.</remarks>
            NoDriveLetter = 0x00000002,
            /// <summary>
            /// Will decouple the virtual disk lifetime from that of the VirtualDiskHandle.
            /// The virtual disk will be attached until the Detach() function is called, even if all open handles to the virtual disk are closed.
            /// </summary>
            PermanentLifetime = 0x00000004,
            /// <summary>
            /// Reserved.
            /// </summary>
            NoLocalHost = 0x00000008
        }

        [Flags]
        public enum DetachVirtualDiskFlag
        {
            None = 0x00000000
        }

        [Flags]
        public enum CompactVirtualDiskFlags
        {
            None = 0x00000000,
            NoZeroScan = 0x00000001,
            NoBlockMoves = 0x00000002
        }

        #endregion VDiskInterop

        #region WIMGAPI

        [FlagsAttribute]
        internal enum
        WimCreateFileDesiredAccess : uint
        {
            WimQuery = 0x00000000,
            WimGenericRead = 0x80000000
        }

        public enum WimMessage : uint
        {
            WIM_MSG = WM_APP + 0x1476,
            WIM_MSG_TEXT,
            ///<summary>
            ///Indicates an update in the progress of an image application.
            ///</summary>
            WIM_MSG_PROGRESS,
            ///<summary>
            ///Enables the caller to prevent a file or a directory from being captured or applied.
            ///</summary>
            WIM_MSG_PROCESS,
            ///<summary>
            ///Indicates that volume information is being gathered during an image capture.
            ///</summary>
            WIM_MSG_SCANNING,
            ///<summary>
            ///Indicates the number of files that will be captured or applied.
            ///</summary>
            WIM_MSG_SETRANGE,
            ///<summary>
            ///Indicates the number of files that have been captured or applied.
            ///</summary>
            WIM_MSG_SETPOS,
            ///<summary>
            ///Indicates that a file has been either captured or applied.
            ///</summary>
            WIM_MSG_STEPIT,
            ///<summary>
            ///Enables the caller to prevent a file resource from being compressed during a capture.
            ///</summary>
            WIM_MSG_COMPRESS,
            ///<summary>
            ///Alerts the caller that an error has occurred while capturing or applying an image.
            ///</summary>
            WIM_MSG_ERROR,
            ///<summary>
            ///Enables the caller to align a file resource on a particular alignment boundary.
            ///</summary>
            WIM_MSG_ALIGNMENT,
            WIM_MSG_RETRY,
            ///<summary>
            ///Enables the caller to align a file resource on a particular alignment boundary.
            ///</summary>
            WIM_MSG_SPLIT,
            WIM_MSG_SUCCESS = 0x00000000,
            WIM_MSG_ABORT_IMAGE = 0xFFFFFFFF
        }

        internal enum
        WimCreationDisposition : uint
        {
            WimOpenExisting = 0x00000003,
        }

        internal enum
        WimActionFlags : uint
        {
            WimIgnored = 0x00000000
        }

        internal enum
        WimCompressionType : uint
        {
            WimIgnored = 0x00000000
        }

        internal enum
        WimCreationResult : uint
        {
            WimCreatedNew = 0x00000000,
            WimOpenedExisting = 0x00000001
        }

        #endregion WIMGAPI

        #endregion Enums and Flags

        #region Structs

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CreateVirtualDiskParameters
        {
            /// <summary>
            /// A CREATE_VIRTUAL_DISK_VERSION enumeration that specifies the version of the CREATE_VIRTUAL_DISK_PARAMETERS structure being passed to or from the virtual hard disk (VHD) functions.
            /// </summary>
            public CreateVirtualDiskVersion Version;

            /// <summary>
            /// Unique identifier to assign to the virtual disk object. If this member is set to zero, a unique identifier is created by the system.
            /// </summary>
            public Guid UniqueId;

            /// <summary>
            /// The maximum virtual size of the virtual disk object. Must be a multiple of 512.
            /// If a ParentPath is specified, this value must be zero.
            /// If a SourcePath is specified, this value can be zero to specify the size of the source VHD to be used, otherwise the size specified must be greater than or equal to the size of the source disk.
            /// </summary>
            public ulong MaximumSize;

            /// <summary>
            /// Internal size of the virtual disk object blocks.
            /// The following are predefined block sizes and their behaviors. For a fixed VHD type, this parameter must be zero.
            /// </summary>
            public uint BlockSizeInBytes;

            /// <summary>
            /// Internal size of the virtual disk object sectors. Must be set to 512.
            /// </summary>
            public uint SectorSizeInBytes;

            /// <summary>
            /// Optional path to a parent virtual disk object. Associates the new virtual disk with an existing virtual disk.
            /// If this parameter is not NULL, SourcePath must be NULL.
            /// </summary>
            public string ParentPath;

            /// <summary>
            /// Optional path to pre-populate the new virtual disk object with block data from an existing disk. This path may refer to a VHD or a physical disk.
            /// If this parameter is not NULL, ParentPath must be NULL.
            /// </summary>
            public string SourcePath;

            /// <summary>
            /// Flags for opening the VHD
            /// </summary>
            public OpenVirtualDiskFlags OpenFlags;

            /// <summary>
            /// GetInfoOnly flag for V2 handles
            /// </summary>
            public bool GetInfoOnly;

            /// <summary>
            /// Virtual Storage Type of the parent disk
            /// </summary>
            public VirtualStorageType ParentVirtualStorageType;

            /// <summary>
            /// Virtual Storage Type of the source disk
            /// </summary>
            public VirtualStorageType SourceVirtualStorageType;

            /// <summary>
            /// A GUID to use for fallback resiliency over SMB.
            /// </summary>
            public Guid ResiliencyGuid;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct VirtualStorageType
        {
            public VirtualStorageDeviceType DeviceId;
            public Guid VendorId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SecurityDescriptor
        {
            public byte revision;
            public byte size;
            public short control;
            public IntPtr owner;
            public IntPtr group;
            public IntPtr sacl;
            public IntPtr dacl;
        }

        #endregion Structs

        #region VirtDisk.DLL P/Invoke

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern uint
        CreateVirtualDisk(
            [In, Out] ref VirtualStorageType VirtualStorageType,
            [In] string Path,
            [In] VirtualDiskAccessMask VirtualDiskAccessMask,
            [In, Out] ref SecurityDescriptor SecurityDescriptor,
            [In] CreateVirtualDiskFlags Flags,
            [In] uint ProviderSpecificFlags,
            [In, Out] ref CreateVirtualDiskParameters Parameters,
            [In] IntPtr Overlapped,
            [Out] out SafeFileHandle Handle);

        #endregion VirtDisk.DLL P/Invoke

        #region Win32 P/Invoke

        [DllImport("advapi32", SetLastError = true)]
        public static extern bool InitializeSecurityDescriptor(
            [Out] out SecurityDescriptor pSecurityDescriptor,
            [In] uint dwRevision);

        #endregion Win32 P/Invoke

        #region WIMGAPI P/Invoke

        #region SafeHandle wrappers for WimFileHandle and WimImageHandle

        public sealed class WimFileHandle : SafeHandle
        {

            public WimFileHandle(
                string wimPath)
                : base(IntPtr.Zero, true)
            {

                if (String.IsNullOrEmpty(wimPath))
                {
                    throw new ArgumentNullException("wimPath");
                }

                if (!File.Exists(Path.GetFullPath(wimPath)))
                {
                    throw new FileNotFoundException((new FileNotFoundException()).Message, wimPath);
                }

                NativeMethods.WimCreationResult creationResult;

                this.handle = NativeMethods.WimCreateFile(
                    wimPath,
                    NativeMethods.WimCreateFileDesiredAccess.WimGenericRead,
                    NativeMethods.WimCreationDisposition.WimOpenExisting,
                    NativeMethods.WimActionFlags.WimIgnored,
                    NativeMethods.WimCompressionType.WimIgnored,
                    out creationResult
                );

                // Check results.
                if (creationResult != NativeMethods.WimCreationResult.WimOpenedExisting)
                {
                    throw new Win32Exception();
                }

                if (this.handle == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                // Set the temporary path.
                NativeMethods.WimSetTemporaryPath(
                    this,
                    Environment.ExpandEnvironmentVariables("%TEMP%")
                );
            }

            protected override bool ReleaseHandle()
            {
                return NativeMethods.WimCloseHandle(this.handle);
            }

            public override bool IsInvalid
            {
                get { return this.handle == IntPtr.Zero; }
            }
        }

        public sealed class WimImageHandle : SafeHandle
        {
            public WimImageHandle(
                WimFile Container,
                uint ImageIndex)
                : base(IntPtr.Zero, true)
            {

                if (null == Container)
                {
                    throw new ArgumentNullException("Container");
                }

                if ((Container.Handle.IsClosed) || (Container.Handle.IsInvalid))
                {
                    throw new ArgumentNullException("The handle to the WIM file has already been closed, or is invalid.", "Container");
                }

                if (ImageIndex > Container.ImageCount)
                {
                    throw new ArgumentOutOfRangeException("ImageIndex", "The index does not exist in the specified WIM file.");
                }

                this.handle = NativeMethods.WimLoadImage(
                    Container.Handle.DangerousGetHandle(),
                    ImageIndex);
            }

            protected override bool ReleaseHandle()
            {
                return NativeMethods.WimCloseHandle(this.handle);
            }

            public override bool IsInvalid
            {
                get { return this.handle == IntPtr.Zero; }
            }
        }

        #endregion SafeHandle wrappers for WimFileHandle and WimImageHandle

        [DllImport("Wimgapi.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WIMCreateFile")]
        internal static extern IntPtr
        WimCreateFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string WimPath,
            [In] WimCreateFileDesiredAccess DesiredAccess,
            [In] WimCreationDisposition CreationDisposition,
            [In] WimActionFlags FlagsAndAttributes,
            [In] WimCompressionType CompressionType,
            [Out, Optional] out WimCreationResult CreationResult
        );

        [DllImport("Wimgapi.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WIMCloseHandle")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool
        WimCloseHandle(
            [In] IntPtr Handle
        );

        [DllImport("Wimgapi.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WIMLoadImage")]
        internal static extern IntPtr
        WimLoadImage(
            [In] IntPtr Handle,
            [In] uint ImageIndex
        );

        [DllImport("Wimgapi.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WIMGetImageCount")]
        internal static extern uint
        WimGetImageCount(
            [In] WimFileHandle Handle
        );

        [DllImport("Wimgapi.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WIMGetImageInformation")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool
        WimGetImageInformation(
            [In] SafeHandle Handle,
            [Out] out StringBuilder ImageInfo,
            [Out] out uint SizeOfImageInfo
        );

        [DllImport("Wimgapi.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WIMSetTemporaryPath")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool
        WimSetTemporaryPath(
            [In] WimFileHandle Handle,
            [In] string TempPath
        );

        [DllImport("Wimgapi.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WIMRegisterMessageCallback", CallingConvention = CallingConvention.StdCall)]
        internal static extern uint
        WimRegisterMessageCallback(
            [In, Optional] WimFileHandle hWim,
            [In] WimMessageCallback MessageProc,
            [In, Optional] IntPtr ImageInfo
        );

        [DllImport("Wimgapi.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WIMUnregisterMessageCallback", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool
        WimUnregisterMessageCallback(
            [In, Optional] WimFileHandle hWim,
            [In] WimMessageCallback MessageProc
        );


        #endregion WIMGAPI P/Invoke
    }
}
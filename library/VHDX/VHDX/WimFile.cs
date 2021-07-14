using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;


namespace Vhdx
{
    public class WimFile
    {

        internal XDocument m_xmlInfo;
        internal List<WimImage> m_imageList;

        private static NativeMethods.WimMessageCallback wimMessageCallback;

        #region Events

        /// <summary>
        /// DefaultImageEvent handler
        /// </summary>
        public delegate void DefaultImageEventHandler(object sender, DefaultImageEventArgs e);

        ///<summary>
        ///ProcessFileEvent handler
        ///</summary>
        public delegate void ProcessFileEventHandler(object sender, ProcessFileEventArgs e);

        ///<summary>
        ///Enable the caller to prevent a file resource from being compressed during a capture.
        ///</summary>
        public event ProcessFileEventHandler ProcessFileEvent;

        ///<summary>
        ///Indicate an update in the progress of an image application.
        ///</summary>
        public event DefaultImageEventHandler ProgressEvent;

        ///<summary>
        ///Alert the caller that an error has occurred while capturing or applying an image.
        ///</summary>
        public event DefaultImageEventHandler ErrorEvent;

        ///<summary>
        ///Indicate that a file has been either captured or applied.
        ///</summary>
        public event DefaultImageEventHandler StepItEvent;

        ///<summary>
        ///Indicate the number of files that will be captured or applied.
        ///</summary>
        public event DefaultImageEventHandler SetRangeEvent;

        ///<summary>
        ///Indicate the number of files that have been captured or applied.
        ///</summary>
        public event DefaultImageEventHandler SetPosEvent;

        #endregion Events

        private
        enum
        ImageEventMessage : uint
        {
            ///<summary>
            ///Enables the caller to prevent a file or a directory from being captured or applied.
            ///</summary>
            Progress = NativeMethods.WimMessage.WIM_MSG_PROGRESS,
            ///<summary>
            ///Notification sent to enable the caller to prevent a file or a directory from being captured or applied.
            ///To prevent a file or a directory from being captured or applied, call WindowsImageContainer.SkipFile().
            ///</summary>
            Process = NativeMethods.WimMessage.WIM_MSG_PROCESS,
            ///<summary>
            ///Enables the caller to prevent a file resource from being compressed during a capture.
            ///</summary>
            Compress = NativeMethods.WimMessage.WIM_MSG_COMPRESS,
            ///<summary>
            ///Alerts the caller that an error has occurred while capturing or applying an image.
            ///</summary>
            Error = NativeMethods.WimMessage.WIM_MSG_ERROR,
            ///<summary>
            ///Enables the caller to align a file resource on a particular alignment boundary.
            ///</summary>
            Alignment = NativeMethods.WimMessage.WIM_MSG_ALIGNMENT,
            ///<summary>
            ///Enables the caller to align a file resource on a particular alignment boundary.
            ///</summary>
            Split = NativeMethods.WimMessage.WIM_MSG_SPLIT,
            ///<summary>
            ///Indicates that volume information is being gathered during an image capture.
            ///</summary>
            Scanning = NativeMethods.WimMessage.WIM_MSG_SCANNING,
            ///<summary>
            ///Indicates the number of files that will be captured or applied.
            ///</summary>
            SetRange = NativeMethods.WimMessage.WIM_MSG_SETRANGE,
            ///<summary>
            ///Indicates the number of files that have been captured or applied.
            /// </summary>
            SetPos = NativeMethods.WimMessage.WIM_MSG_SETPOS,
            ///<summary>
            ///Indicates that a file has been either captured or applied.
            ///</summary>
            StepIt = NativeMethods.WimMessage.WIM_MSG_STEPIT,
            ///<summary>
            ///Success.
            ///</summary>
            Success = NativeMethods.WimMessage.WIM_MSG_SUCCESS,
            ///<summary>
            ///Abort.
            ///</summary>
            Abort = NativeMethods.WimMessage.WIM_MSG_ABORT_IMAGE
        }

        ///<summary>
        ///Event callback to the Wimgapi events
        ///</summary>
        private
        uint
        ImageEventMessagePump(
            uint MessageId,
            IntPtr wParam,
            IntPtr lParam,
            IntPtr UserData)
        {

            uint status = (uint)NativeMethods.WimMessage.WIM_MSG_SUCCESS;

            DefaultImageEventArgs eventArgs = new DefaultImageEventArgs(wParam, lParam, UserData);

            switch ((ImageEventMessage)MessageId)
            {

                case ImageEventMessage.Progress:
                    ProgressEvent(this, eventArgs);
                    break;

                case ImageEventMessage.Process:
                    if (null != ProcessFileEvent)
                    {
                        string fileToImage = Marshal.PtrToStringUni(wParam);
                        ProcessFileEventArgs fileToProcess = new ProcessFileEventArgs(fileToImage, lParam);
                        ProcessFileEvent(this, fileToProcess);

                        if (fileToProcess.Abort == true)
                        {
                            status = (uint)ImageEventMessage.Abort;
                        }
                    }
                    break;

                case ImageEventMessage.Error:
                    if (null != ErrorEvent)
                    {
                        ErrorEvent(this, eventArgs);
                    }
                    break;

                case ImageEventMessage.SetRange:
                    if (null != SetRangeEvent)
                    {
                        SetRangeEvent(this, eventArgs);
                    }
                    break;

                case ImageEventMessage.SetPos:
                    if (null != SetPosEvent)
                    {
                        SetPosEvent(this, eventArgs);
                    }
                    break;

                case ImageEventMessage.StepIt:
                    if (null != StepItEvent)
                    {
                        StepItEvent(this, eventArgs);
                    }
                    break;

                default:
                    break;
            }
            return status;

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="wimPath">Path to the WIM container.</param>
        public
        WimFile(string wimPath)
        {
            if (string.IsNullOrEmpty(wimPath))
            {
                throw new ArgumentNullException("wimPath");
            }

            if (!File.Exists(Path.GetFullPath(wimPath)))
            {
                throw new FileNotFoundException((new FileNotFoundException()).Message, wimPath);
            }

            Handle = new NativeMethods.WimFileHandle(wimPath);

            // Hook up the events before we return.
            //wimMessageCallback = new NativeMethods.WimMessageCallback(ImageEventMessagePump);
            //NativeMethods.RegisterMessageCallback(this.Handle, wimMessageCallback);
        }

        /// <summary>
        /// Closes the WIM file.
        /// </summary>
        public void
        Close()
        {
            foreach (WimImage image in Images)
            {
                image.Close();
            }

            if (null != wimMessageCallback)
            {
                NativeMethods.UnregisterMessageCallback(this.Handle, wimMessageCallback);
                wimMessageCallback = null;
            }

            if ((!Handle.IsClosed) && (!Handle.IsInvalid))
            {
                Handle.Close();
            }
        }

        /// <summary>
        /// Provides a list of WimImage objects, representing the images in the WIM container file.
        /// </summary>
        public List<WimImage>
        Images
        {
            get
            {
                if (null == m_imageList)
                {

                    int imageCount = (int)ImageCount;
                    m_imageList = new List<WimImage>(imageCount);
                    for (int i = 0; i < imageCount; i++)
                    {

                        // Load up each image so it's ready for us.
                        m_imageList.Add(
                            new WimImage(this, (uint)i + 1));
                    }
                }

                return m_imageList;
            }
        }

        /// <summary>
        /// Provides a list of names of the images in the specified WIM container file.
        /// </summary>
        public List<string>
        ImageNames
        {
            get
            {
                List<string> nameList = new List<string>();
                foreach (WimImage image in Images)
                {
                    nameList.Add(image.ImageName);
                }
                return nameList;
            }
        }

        /// <summary>
        /// Indexer for WIM images inside the WIM container, indexed by the image number.
        /// The list of Images is 0-based, but the WIM container is 1-based, so we automatically compensate for that.
        /// this[1] returns the 0th image in the WIM container.
        /// </summary>
        /// <param name="ImageIndex">The 1-based index of the image to retrieve.</param>
        /// <returns>WinImage object.</returns>
        public WimImage
        this[int ImageIndex]
        {
            get { return Images[ImageIndex - 1]; }
        }

        /// <summary>
        /// Indexer for WIM images inside the WIM container, indexed by the image name.
        /// WIMs created by different processes sometimes contain different information - including the name.
        /// Some images have their name stored in the Name field, some in the Flags field, and some in the EditionID field.
        /// We take all of those into account in while searching the WIM.
        /// </summary>
        /// <param name="ImageName"></param>
        /// <returns></returns>
        public WimImage
        this[string ImageName]
        {
            get
            {
                return
                    Images.Where(i => (
                        i.ImageName.ToUpper() == ImageName.ToUpper() ||
                        i.ImageFlags.ToUpper() == ImageName.ToUpper()))
                    .DefaultIfEmpty(null)
                        .FirstOrDefault<WimImage>();
            }
        }

        /// <summary>
        /// Returns the number of images in the WIM container.
        /// </summary>
        internal uint
        ImageCount
        {
            get { return NativeMethods.WimGetImageCount(Handle); }
        }

        /// <summary>
        /// Returns an XDocument representation of the XML metadata for the WIM container and associated images.
        /// </summary>
        internal XDocument
        XmlInfo
        {
            get
            {

                if (null == m_xmlInfo)
                {
                    StringBuilder builder;
                    uint bytes;
                    if (!NativeMethods.WimGetImageInformation(Handle, out builder, out bytes))
                    {
                        throw new Win32Exception();
                    }

                    // Ensure the length of the returned bytes to avoid garbage characters at the end.
                    int charCount = (int)bytes / sizeof(char);
                    if (null != builder)
                    {
                        // Get rid of the unicode file marker at the beginning of the XML.
                        builder.Remove(0, 1);
                        builder.EnsureCapacity(charCount - 1);
                        builder.Length = charCount - 1;

                        // This isn't likely to change while we have the image open, so cache it.
                        m_xmlInfo = XDocument.Parse(builder.ToString().Trim());
                    }
                    else
                    {
                        m_xmlInfo = null;
                    }
                }

                return m_xmlInfo;
            }
        }

        public NativeMethods.WimFileHandle Handle
        {
            get;
            private set;
        }
    }

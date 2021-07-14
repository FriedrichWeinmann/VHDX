using System;
using System.ComponentModel;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Vhdx
{
    public class WimImage
    {
        internal XDocument m_xmlInfo;

        public WimImage(WimFile Container, uint ImageIndex)
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

            Handle = new NativeMethods.WimImageHandle(Container, ImageIndex);
        }

        public enum Architectures : uint
        {
            x86 = 0x0,
            ARM = 0x5,
            IA64 = 0x6,
            AMD64 = 0x9,
            ARM64 = 0xC
        }

        public void Close()
        {
            if ((!Handle.IsClosed) && (!Handle.IsInvalid))
            {
                Handle.Close();
            }
        }

        public NativeMethods.WimImageHandle Handle
        {
            get;
            private set;
        }

        internal XDocument XmlInfo
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

        public string ImageIndex
        {
            get { return XmlInfo.Element("IMAGE").Attribute("INDEX").Value; }
        }

        public string ImageName
        {
            get { return XmlInfo.XPathSelectElement("/IMAGE/NAME").Value; }
        }

        public string ImageEditionId
        {
            get { return XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/EDITIONID").Value; }
        }

        public string ImageFlags
        {
            get
            {
                string flagValue = String.Empty;

                try
                {
                    flagValue = XmlInfo.XPathSelectElement("/IMAGE/FLAGS").Value;
                }
                catch
                {

                    // Some WIM files don't contain a FLAGS element in the metadata.
                    // In an effort to support those WIMs too, inherit the EditionId if there
                    // are no Flags.

                    if (String.IsNullOrEmpty(flagValue))
                    {
                        flagValue = this.ImageEditionId;

                        // Check to see if the EditionId is "ServerHyper". If so,
                        // tweak it to be "ServerHyperCore" instead.

                        if (0 == String.Compare("serverhyper", flagValue, true))
                        {
                            flagValue = "ServerHyperCore";
                        }
                    }

                }

                return flagValue;
            }
        }

        public string ImageProductType
        {
            get { return XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/PRODUCTTYPE").Value; }
        }

        public string ImageInstallationType
        {
            get { return XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/INSTALLATIONTYPE").Value; }
        }

        public string ImageDescription
        {
            get { return XmlInfo.XPathSelectElement("/IMAGE/DESCRIPTION").Value; }
        }

        public ulong ImageSize
        {
            get { return ulong.Parse(XmlInfo.XPathSelectElement("/IMAGE/TOTALBYTES").Value); }
        }

        public Architectures ImageArchitecture
        {
            get
            {
                int arch = -1;
                try
                {
                    arch = int.Parse(XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/ARCH").Value);
                }
                catch { }

                return (Architectures)arch;
            }
        }

        public string ImageDefaultLanguage
        {
            get
            {
                string lang = null;
                try
                {
                    lang = XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/LANGUAGES/DEFAULT").Value;
                }
                catch { }

                return lang;
            }
        }

        public Version ImageVersion
        {
            get
            {
                int major = 0;
                int minor = 0;
                int build = 0;
                int revision = 0;

                try
                {
                    major = int.Parse(XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/VERSION/MAJOR").Value);
                    minor = int.Parse(XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/VERSION/MINOR").Value);
                    build = int.Parse(XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/VERSION/BUILD").Value);
                    revision = int.Parse(XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/VERSION/SPBUILD").Value);
                }
                catch { }

                return (new Version(major, minor, build, revision));
            }
        }

        public string ImageDisplayName
        {
            get { return XmlInfo.XPathSelectElement("/IMAGE/DISPLAYNAME").Value; }
        }

        public string ImageDisplayDescription
        {
            get { return XmlInfo.XPathSelectElement("/IMAGE/DISPLAYDESCRIPTION").Value; }
        }
    }
}

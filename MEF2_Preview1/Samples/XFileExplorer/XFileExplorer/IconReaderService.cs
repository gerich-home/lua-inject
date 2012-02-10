//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Samples.XFileExplorer
{
    [Export(typeof(IIconReaderService))]
    public class IconReaderService : IIconReaderService
    {
       
        #region IIconReaderService Members

        public ImageSource GetFileIconImage(string filePath, bool small)
        {
            return LoadBitmap(IconReader.GetFileIcon(
                filePath,
                (small) ? IconReader.IconSize.Small : IconReader.IconSize.Large,
                false));
        }

        public ImageSource GetFolderIconImage(bool closed, bool small)
        {
            return LoadBitmap(IconReader.GetFolderIcon(
                (small) ? IconReader.IconSize.Small : IconReader.IconSize.Large,
                (closed) ? IconReader.FolderType.Closed : IconReader.FolderType.Opened));
        }

        public ImageSource GetFolderIconImage(string folderPath, bool closed, bool small)
        {
            return LoadBitmap(IconReader.GetFolderIcon(
                folderPath,
                (small) ? IconReader.IconSize.Small : IconReader.IconSize.Large,
                (closed) ? IconReader.FolderType.Closed : IconReader.FolderType.Opened));
        }

        #endregion

        #region Convert System.Drawing.Icon to System.Windows.Media.Imaging.BitmapSource

        private static BitmapSource LoadBitmap(Icon icon)
        {
            return (icon == null) ? null :
                System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    icon.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        #endregion

        #region Retrieve file icon

        /// <summary>
        /// Retrieve icons from the windows shell
        /// </summary>
        private static class IconReader
        {
            private const int MAX_PATH = 260;
            private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
            private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

            /// <summary>
            /// File/folder information
            /// </summary>
            [System.FlagsAttribute()]
            private enum FileInfoFlags : uint
            {
                /// get large icon
                SHGFI_LARGEICON = 0x000000000,
                /// get small icon
                SHGFI_SMALLICON = 0x000000001,
                /// get open icon
                SHGFI_OPENICON = 0x000000002,
                /// get shell size icon
                SHGFI_SHELLICONSIZE = 0x000000004,
                /// pszPath is a pidl
                SHGFI_PIDL = 0x000000008,
                /// use passed dwFileAttribute
                SHGFI_USEFILEATTRIBUTES = 0x000000010,
                /// apply the appropriate overlays
                SHGFI_ADDOVERLAYS = 0x000000020,
                /// get the index of the overlay
                SHGFI_OVERLAYINDEX = 0x000000040,
                /// get icon
                SHGFI_ICON = 0x000000100,
                /// get display name
                SHGFI_DISPLAYNAME = 0x000000200,
                /// get type name
                SHGFI_TYPENAME = 0x000000400,
                /// get attributes
                SHGFI_ATTRIBUTES = 0x000000800,
                /// get icon location
                SHGFI_ICONLOCATION = 0x000001000,
                /// return exe type
                SHGFI_EXETYPE = 0x000002000,
                /// get system icon index
                SHGFI_SYSICONINDEX = 0x000004000,
                /// put a link overlay on icon
                SHGFI_LINKOVERLAY = 0x000008000,
                /// show icon in selected state
                SHGFI_SELECTED = 0x000010000,
                /// get only specified attributes
                SHGFI_ATTR_SPECIFIED = 0x000020000
            }

            /// <summary>
            /// State of the folder
            /// </summary>
            public enum FolderType
            {
                Opened = 0,
                Closed = 1
            }

            /// <summary>
            /// Size of the icon
            /// </summary>
            public enum IconSize
            {
                Large = 0,// 32x32
                Small = 1 // 16x16
            }

            [DllImport("User32.dll")]
            private static extern int DestroyIcon(System.IntPtr hIcon);

            [DllImport("Shell32.dll")]
            private static extern System.IntPtr SHGetFileInfo(
              string pszPath,
              uint dwFileAttributes,
              ref ShellFileInfo psfi,
              uint cbFileInfo,
              uint uFlags
            );

            [StructLayout(LayoutKind.Sequential)]
            private struct ShellFileInfo
            {
                public const int nameSize = 80;
                public IntPtr hIcon;
                public uint dwAttributes;
                public int index;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
                public string szDisplayName;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = nameSize)]
                public string szTypeName;
            };

            /// <summary>
            /// Retrieve a file icon with specific path name
            /// </summary>
            public static Icon GetFileIcon(string filePath, IconSize size, bool addLinkOverlay)
            {
                FileInfoFlags flags = FileInfoFlags.SHGFI_ICON | FileInfoFlags.SHGFI_USEFILEATTRIBUTES;

                flags |= (size == IconSize.Small) ? FileInfoFlags.SHGFI_SMALLICON : FileInfoFlags.SHGFI_LARGEICON;

                if (addLinkOverlay)
                {
                    flags |= FileInfoFlags.SHGFI_LINKOVERLAY;
                }

                ShellFileInfo shellFileInfo = new ShellFileInfo();
                SHGetFileInfo(filePath, FILE_ATTRIBUTE_NORMAL, ref shellFileInfo, (uint)Marshal.SizeOf(shellFileInfo), (uint)flags);

                // Deep copy 
                Icon icon = (Icon)Icon.FromHandle(shellFileInfo.hIcon).Clone();

                // Release icon handle 
                DestroyIcon(shellFileInfo.hIcon);

                return icon;
            }

            /// <summary>
            /// Retrieve a folder icon with specific path
            /// </summary>
            public static Icon GetFolderIcon(string folderPath, IconSize size, FolderType folderType)
            {
                FileInfoFlags flags = FileInfoFlags.SHGFI_ICON | FileInfoFlags.SHGFI_USEFILEATTRIBUTES;

                flags |= (size == IconSize.Small) ? FileInfoFlags.SHGFI_SMALLICON : FileInfoFlags.SHGFI_LARGEICON;

                if (folderType == FolderType.Opened)
                {
                    flags |= FileInfoFlags.SHGFI_OPENICON;
                }

                ShellFileInfo shellFileInfo = new ShellFileInfo();
                SHGetFileInfo(folderPath, FILE_ATTRIBUTE_DIRECTORY, ref shellFileInfo, (uint)Marshal.SizeOf(shellFileInfo), (uint)flags);

                // Deep copy 
                Icon icon = (Icon)Icon.FromHandle(shellFileInfo.hIcon).Clone();

                // Release icon handle
                DestroyIcon(shellFileInfo.hIcon);

                return icon;
            }

            /// <summary>
            /// Return a normal folder icon
            /// </summary>
            public static Icon GetFolderIcon(IconSize size, FolderType folderType)
            {
                return GetFolderIcon(null, size, folderType);
            }
        }
        #endregion  
    }
}

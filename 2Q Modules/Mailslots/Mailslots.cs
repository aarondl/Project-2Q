using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Mailslots {

    /// <summary>
    /// An enumeration to get file access in CreateFile.
    /// </summary>
    [Flags]
    public enum FileAccess : uint {
        /// <summary>
        /// Generic Write access to the file.
        /// </summary>
        Generic_Write = 40000000,
        /// <summary>
        /// Generic Reading access to the file.
        /// </summary>
        Generic_Read = 0x80000000,
    };

    /// <summary>
    /// The requested sharing mode of the file or device, which can be read, write, both, delete, all of these, or none.
    /// </summary>
    [Flags]
    public enum ShareMode : uint {
        /// <summary>
        /// Disables subsequent open operations on a file or device to request any type of access to that file or device.
        /// </summary>
        Disable = 0x0,
        /// <summary>
        /// Enables subsequent open operations on a file or device to request delete access.
        /// Otherwise, other processes cannot open the file or device if they request delete access.
        /// If this flag is not specified, but the file or device has been opened for delete access, the function fails.
        /// Note  Delete access allows both delete and rename operations.
        /// </summary>
        FileShareDelete = 0x00000004,
        /// <summary>
        /// Enables subsequent open operations on a file or device to request read access.
        /// Otherwise, other processes cannot open the file or device if they request read access.
        /// If this flag is not specified, but the file or device has been opened for read access, the function fails.
        /// </summary>
        FileShareRead = 0x00000001,
        /// <summary>
        /// Enables subsequent open operations on a file or device to request write access.
        /// Otherwise, other processes cannot open the file or device if they request write access.
        /// If this flag is not specified, but the file or device has been opened for write access or has a file mapping with write access, the function fails.
        /// </summary>
        FileShareWrite = 0x00000002,
    };

    /// <summary>
    /// A single value to tell CreateFile how to deal with the file's state.
    /// </summary>
    public enum CreationDisposition : uint {
        /// <summary>
        /// Creates a new file, always.
        /// </summary>
        CreateAlways = 0x2,
        /// <summary>
        /// Creates a new file, only if it does not already exist
        /// </summary>
        CreateNew = 0x1,
        /// <summary>
        /// Opens a file, always.
        /// </summary>
        OpenAlways = 0x4,
        /// <summary>
        /// Opens a file or device, only if it exists.
        /// </summary>
        OpenExisting = 0x3,
        /// <summary>
        /// Opens a file and truncates it so that its size is zero bytes, only if it exists.
        /// </summary>
        TruncateExisting = 0x5,
    }

    /// <summary>
    /// Used with CreateFile to determine file attributes.
    /// </summary>
    [Flags]
    public enum FileAttributes : uint {
        /// <summary>
        /// The file should be archived. Applications use this attribute to mark files for backup or removal.
        /// </summary>
        Archive = 0x20,
        /// <summary>
        /// The file or directory is encrypted. For a file, this means that all data in the file is encrypted. For a directory, this means that encryption is the default for newly created files and subdirectories. For more information, see File Encryption.
        /// </summary>
        Encrypted = 0x4000,
        /// <summary>
        /// The file is hidden. Do not include it in an ordinary directory listing.
        /// </summary>
        Hidden = 0x2,
        /// <summary>
        /// The file does not have other attributes set. This attribute is valid only if used alone.
        /// </summary>
        Normal = 0x80,
        /// <summary>
        /// The data of a file is not immediately available. This attribute indicates that file data is physically moved to offline storage. This attribute is used by Remote Storage.
        /// </summary>
        Offline = 0x1000,
        /// <summary>
        /// The file is read only. Applications can read the file, but cannot write to or delete it.
        /// </summary>
        ReadOnly = 0x1,
        /// <summary>
        /// The file is part of or used exclusively by an operating system.
        /// </summary>
        System = 0x4,
        /// <summary>
        /// The file is being used for temporary storage.
        /// </summary>
        Temporary = 0x100,
    }

    /// <summary>
    /// Used with CreateFile to determine file flags.
    /// </summary>
    [Flags]
    public enum FileFlags : uint {
        /// <summary>
        /// The file is being opened or created for a backup or restore operation. The system ensures that the calling process overrides file security checks when the process has SE_BACKUP_NAME and SE_RESTORE_NAME privileges.
        /// </summary>
        BackupSemantics = 0x02000000,
        /// <summary>
        /// The file is to be deleted immediately after all of its handles are closed, which includes the specified handle and any other open or duplicated handles.
        /// </summary>
        DeleteOnClose = 0x04000000,
        /// <summary>
        /// The file or device is being opened with no system caching for data reads and writes. This flag does not affect hard disk caching or memory mapped files.
        /// </summary>
        NoBuffering = 0x20000000,
        /// <summary>
        /// The file data is requested, but it should continue to be located in remote storage. It should not be transported back to local storage.
        /// </summary>
        OpenNoRecall = 0x00100000,
        /// <summary>
        /// Normal reparse point processing will not occur; CreateFile will attempt to open the reparse point. When a file is opened, a file handle is returned, whether or not the filter that controls the reparse point is operational.
        /// This flag cannot be used with the CREATE_ALWAYS flag.
        /// If the file is not a reparse point, then this flag is ignored.
        /// </summary>
        OpenReparsePoint = 0x00200000,
        /// <summary>
        /// The file or device is being opened or created for asynchronous I/O.
        /// </summary>
        Overlapped = 0x40000000,
        /// <summary>
        /// Access will occur according to POSIX rules. This includes allowing multiple files with names, differing only in case, for file systems that support that naming. Use care when using this option, because files created with this flag may not be accessible by applications that are written for MS-DOS or 16-bit Windows.
        /// </summary>
        PosixSemantics = 0x0100000,
        /// <summary>
        /// Access is intended to be random. The system can use this as a hint to optimize file caching.
        /// </summary>
        RandomAccess = 0x10000000,
        /// <summary>
        /// Access is intended to be sequential from beginning to end. The system can use this as a hint to optimize file caching.
        /// This flag should not be used if read-behind (that is, backwards scans) will be used.
        /// </summary>
        SequentialScan = 0x08000000,
        /// <summary>
        /// Write operations will not go through any intermediate cache, they will go directly to disk.
        /// </summary>
        WriteThrough = 0x80000000,
    }

    public enum MailslotTimeout : uint {
        ReturnImmediately = 0x0,
        WaitForever = UInt32.MaxValue-1,
    }

    public class Mailslots {

        #region WinAPI Structs

        /*typedef struct _SECURITY_ATTRIBUTES {
          DWORD nLength;
          LPVOID lpSecurityDescriptor;
        BOOL bInheritHandle;
        } SECURITY_ATTRIBUTES*/
        [StructLayout(LayoutKind.Sequential)]
        public struct Security_Attributes {
            UInt32 nLength;
            IntPtr securityDescriptor;
            bool inheritHandle;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct Overlapped {
            IntPtr Internal;
            IntPtr InternalHigh;
            public struct pointer {
                IntPtr Offset;
                IntPtr OffsetHigh;
                };
            IntPtr hEvent;
        };

        #endregion

        #region General WinAPI File Functions

        /*HANDLE WINAPI CreateFile(
          __in      LPCTSTR lpFileName,
          __in      DWORD dwDesiredAccess,
          __in      DWORD dwShareMode,
          __in_opt  LPSECURITY_ATTRIBUTES lpSecurityAttributes,
          __in      DWORD dwCreationDisposition,
          __in      DWORD dwFlagsAndAttributes,
          __in_opt  HANDLE hTemplateFile
        );*/
        [DllImport( "kernel32.dll" )]
        public static extern IntPtr CreateFile(
            [MarshalAs( UnmanagedType.LPWStr )] string fileName,
            UInt32 desiredAccess,
            UInt32 shareMode,
            IntPtr securityAttributes,
            UInt32 creationDisposition,
            UInt32 flagsAndAttribs,
            IntPtr templateFile);

        [DllImport( "kernel32.dll" )]
        public static extern bool CloseHandle(IntPtr handle);

        /*BOOL WINAPI WriteFile(
          __in         HANDLE hFile,
          __in         LPCVOID lpBuffer,
          __in         DWORD nNumberOfBytesToWrite,
          __out_opt    LPDWORD lpNumberOfBytesWritten,
          __inout_opt  LPOVERLAPPED lpOverlapped
        );*/
        [DllImport( "kernel32.dll" )]
        public static extern bool WriteFile(
            IntPtr handle,
            IntPtr toWrite,
            UInt32 nBytesToWrite,
            IntPtr out_opt_numberOfBytesWritten,
            IntPtr inout_opt_overlappedStruct);
            

        /*BOOL WINAPI ReadFile(
          __in         HANDLE hFile,
          __out        LPVOID lpBuffer,
          __in         DWORD nNumberOfBytesToRead,
          __out_opt    LPDWORD lpNumberOfBytesRead,
          __inout_opt  LPOVERLAPPED lpOverlapped
        );*/
        [DllImport( "kernel32.dll" )]
        public static extern bool ReadFile(
            IntPtr handle,
            IntPtr buffer,
            UInt32 bytesToRead,
            IntPtr out_opt_numberOfBytesRead,
            IntPtr inout_opt_overlappedStruct);

        #endregion

        #region Mailslot Functions

        /*HANDLE WINAPI CreateMailslot(
          __in      LPCTSTR lpName,
          __in      DWORD nMaxMessageSize,
          __in      DWORD lReadTimeout,
          __in_opt  LPSECURITY_ATTRIBUTES lpSecurityAttributes
        );*/
        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateMailSlot(
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            UInt32 maxMessageSize,
            UInt32 readTimeout,
            Security_Attributes securityAttribs);

        [DllImport("kernel32.dll")]
        public static extern bool GetMailSlotInfo(
            IntPtr handle,          //HANDLE hMailSlot
            IntPtr maxMessageSize,  //out optional: LPDWORD lpMaxMessageSize
            IntPtr nextSize,        //out optional: LPDWORD lpNextSize
            IntPtr messageCount,    //out optional: LPDWORD lpMessageCount
            IntPtr readTimeout);    //out optional: LPDWORD readTimeout

        /*BOOL WINAPI SetMailslotInfo(
          __in  HANDLE hMailslot,
          __in  DWORD lReadTimeout
        );*/
        [DllImport( "kernel32.dll" )]
        public static extern bool SetMailSlotInfo(
            IntPtr handle,
            UInt32 readTimeout);

        #endregion


    }

}

using System;

namespace Docms.Domain.Documents
{
    [Flags]
    public enum StMode
    {
        /// <summary>
        /// bit mask for the file type bit fields
        /// </summary>
        S_IFMT = 0b1111000000000000,
        /// <summary>
        /// socket
        /// </summary>
        S_IFSOCK = 0b1100000000000000,
        /// <summary>
        /// symbolic link
        /// </summary>
        S_IFLNK = 0b1010000000000000,
        /// <summary>
        /// regular file
        /// </summary>
        S_IFREG = 0b1000000000000000,
        /// <summary>
        /// block device
        /// </summary>
        S_IFBLK = 0b110000000000000,
        /// <summary>
        /// directory
        /// </summary>
        S_IFDIR = 0b010000000000000,
        /// <summary>
        /// character device
        /// </summary>
        S_IFCHR = 0b001000000000000,
        /// <summary>
        /// FIFO
        /// </summary>
        S_IFIFO = 0b1000000000000,
        /// <summary>
        /// set UID bit
        /// </summary>
        S_ISUID = 0b100000000000,
        /// <summary>
        /// set-group-ID bit (see below)
        /// </summary>
        S_ISGID = 0b10000000000,
        /// <summary>
        /// sticky bit (see below)
        /// </summary>
        S_ISVTX = 0b1000000000,
        /// <summary>
        /// mask for file owner permissions
        /// </summary>
        S_IRWXU = 0b111000000,
        /// <summary>
        /// owner has read permission
        /// </summary>
        S_IRUSR = 0b100000000,
        /// <summary>
        /// owner has write permission
        /// </summary>
        S_IWUSR = 0b10000000,
        /// <summary>
        /// owner has execute permission
        /// </summary>
        S_IXUSR = 0b1000000,
        /// <summary>
        /// mask for group permissions
        /// </summary>
        S_IRWXG = 0b111000,
        /// <summary>
        /// group has read permission
        /// </summary>
        S_IRGRP = 0b100000,
        /// <summary>
        /// group has write permission
        /// </summary>
        S_IWGRP = 0b10000,
        /// <summary>
        /// group has execute permission
        /// </summary>
        S_IXGRP = 0b1000,
        /// <summary>
        /// mask for permissions for others (not in group)
        /// </summary>
        S_IRWXO = 0b111,
        /// <summary>
        /// others have read permission
        /// </summary>
        S_IROTH = 0b100,
        /// <summary>
        /// others have write permission
        /// </summary>
        S_IWOTH = 0b10,
        /// <summary>
        /// others have execute permission
        /// </summary>
        S_IXOTH = 0b1
    }
}

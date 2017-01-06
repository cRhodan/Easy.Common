﻿namespace Easy.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Provides a set of useful methods for working with <see cref="FileInfo"/> and <see cref="DirectoryInfo"/>.
    /// </summary>
    public static class FileAndDirectoryInfoExtensions
    {
        /// <summary>
        /// Returns the size in bytes of the <paramref name="directoryInfo"/> represented by the <paramref name="directoryInfo"/> instance.
        /// </summary>
        /// <param name="directoryInfo">The <paramref name="directoryInfo"/> to get the size of.</param>
        /// <returns>The size of <paramref name="directoryInfo"/> in bytes.</returns>
        public static long GetSizeInByte(this DirectoryInfo directoryInfo)
        {
            long length = 0;

            foreach (var nextfile in directoryInfo.GetFiles())
            {
                length += nextfile.Exists ? nextfile.Length : 0;
            }

            foreach (var nextdir in directoryInfo.GetDirectories())
            {
                length += nextdir.Exists ? nextdir.GetSizeInByte() : 0;
            }
            return length;
        }

        /// <summary>
        /// Indicates if a given <paramref name="directoryInfo"/> is hidden.
        /// </summary>
        /// <param name="directoryInfo">The <paramref name="directoryInfo"/> to check.</param>
        /// <returns>Boolean indicating if the <paramref name="directoryInfo"/> is hidden.</returns>
        public static bool IsHidden(this DirectoryInfo directoryInfo)
        {
            return (directoryInfo.Attributes & FileAttributes.Hidden) != 0;
        }

        /// <summary>
        /// Indicates if a given <paramref name="fileInfo"/> is hidden.
        /// </summary>
        /// <param name="fileInfo">The <paramref name="fileInfo"/> to check.</param>
        /// <returns>
        /// Boolean indicating if the <paramref name="fileInfo"/> is hidden.
        /// </returns>
        public static bool IsHidden(this FileInfo fileInfo)
        {
            return (fileInfo.Attributes & FileAttributes.Hidden) != 0;
        }

        /// <summary>
        /// Renames the given <paramref name="fileInfo"/> to <paramref name="newName"/>.
        /// </summary>
        public static void Rename(this FileInfo fileInfo, string newName)
        {
            Ensure.NotNull(fileInfo, nameof(fileInfo));
            Ensure.NotNullOrEmptyOrWhiteSpace(newName);
            Ensure.That(PathHelper.IsValidFilename(newName), "Invalid file name: " + newName);

            fileInfo.Refresh();
            if (fileInfo.DirectoryName != null && fileInfo.Directory != null
                && fileInfo.Directory.Exists && fileInfo.Exists)
            {
                fileInfo.MoveTo(Path.Combine(fileInfo.DirectoryName, newName));
            }
            else
            {
                var errMsg = "Unable to rename the file: {0} to: {1}".FormatWith(fileInfo.FullName, newName);
                throw new InvalidOperationException(errMsg);
            }
        }

        /// <summary>
        /// Lazily reads all the lines in the <paramref name="fileInfo"/> without requiring a file lock.
        /// <remarks>
        /// This method is preferred over the <see cref="File.ReadAllLines(string)"/> which requires a file lock
        /// which may result <see cref="IOException"/> if the file is opened exclusively by another process such as <c>Excel</c>.
        /// </remarks>
        /// </summary>
        public static IEnumerable<string> ReadAllLines(this FileInfo fileInfo)
        {
            return fileInfo.ReadAllLines(Encoding.UTF8);
        }

        /// <summary>
        /// Lazily reads all the lines in the <paramref name="fileInfo"/> without requiring a file lock.
        /// <remarks>
        /// This method is preferred over the <see cref="File.ReadAllLines(string)"/> which requires a file lock
        /// which may result <see cref="IOException"/> if the file is opened exclusively by another process such as <c>Excel</c>.
        /// </remarks>
        /// </summary>
        public static IEnumerable<string> ReadAllLines(this FileInfo fileInfo, Encoding encoding)
        {
            Ensure.NotNull(fileInfo, nameof(fileInfo));
            Ensure.NotNull(encoding, nameof(encoding));

            using (var stream = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, encoding))
            {
                while (!reader.EndOfStream)
                {
                    yield return reader.ReadLine();
                }
            }
        }
    }
}
using System;
using System.IO;

namespace Jerrycurl.IO
{
    internal static class PathHelper
    {
        public static bool IsRelativeTo(string path, string basePath) => (MakeRelativePath(basePath, path) != null);

        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            else if (Path.IsPathRooted(path))
                return Path.GetFullPath(path);
            else
            {
                string dotPath = Path.GetFullPath(".");

                if (path.StartsWith(dotPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    return path.Remove(0, dotPath.Length + 1);

                return path;
            }
        }

        public static string MakeAbsolutePath(string basePath, string path)
        {
            MakeRelativeAndAbsolutePath(basePath, path, out string absolutePath, out _);

            return absolutePath;
        }

        public static string MakeRelativePath(string basePath, string path)
        {
            MakeRelativeAndAbsolutePath(basePath, path, out _, out string relativePath);

            return relativePath;
        }
        public static string MakeRelativeOrAbsolutePath(string basePath, string path)
        {
            MakeRelativeAndAbsolutePath(basePath, path, out string absolutePath, out string relativePath);

            return relativePath ?? absolutePath;
        }

        public static void MakeRelativeAndAbsolutePath(string basePath, string path, out string absolutePath, out string relativePath)
        {
            if (basePath == null)
                throw new ArgumentNullException(nameof(basePath));

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be empty.", nameof(path));

            string fullBasePath = Path.GetFullPath(string.IsNullOrEmpty(basePath) ? "." : basePath);
            string fullPath = Path.GetFullPath(path);

            fullBasePath = fullBasePath.TrimEnd(Path.DirectorySeparatorChar);

            if (fullPath.StartsWith(fullBasePath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = fullPath.Remove(0, fullBasePath.Length + 1);
                absolutePath = fullPath;
            }
            else
            {
                relativePath = null;
                absolutePath = fullPath;
            }
        }
    }
}

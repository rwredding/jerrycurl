using System.Reflection;

namespace Jerrycurl.Reflection
{
    internal static class NuGetExtensions
    {
        public static string GetNuGetPackageVersion(this Assembly assembly)
        {
            string infoVersion = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            if (string.IsNullOrWhiteSpace(infoVersion))
                return null;
            else if (infoVersion.Contains("+"))
                return infoVersion.Substring(0, infoVersion.IndexOf('+'));
            else
                return infoVersion;
        }

        public static string GetNuGetPackageHash(this Assembly assembly)
        {
            string infoVersion = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            if (infoVersion != null && infoVersion.Contains("+"))
                return infoVersion.Remove(0, infoVersion.IndexOf('+') + 1);

            return null;
        }
    }
}

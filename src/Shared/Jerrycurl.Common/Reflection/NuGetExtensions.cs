using System.Reflection;

namespace Jerrycurl.Reflection
{
    internal static class NuGetExtensions
    {
        public static string GetNuGetPackageVersion(this Assembly assembly)
        {
            string infoVersion = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            if (infoVersion != null && infoVersion.Contains("+"))
                return infoVersion.Substring(0, infoVersion.IndexOf('+'));

            return null;
        }
    }
}

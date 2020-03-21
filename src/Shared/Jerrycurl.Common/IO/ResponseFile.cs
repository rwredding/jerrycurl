using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Jerrycurl.IO
{
    internal class ResponseFile
    {
        public string WorkingDirectory { get; set; }
        public string InputPath { get; set; }
        public string FullPath { get; set; }
        public string Value { get; set; }
        public bool Ignore { get; set; }
        public bool IsPath { get; set; }

        public override string ToString() => this.Value;

        public static bool HasCommentSyntax(string input) => (input != null && input.StartsWith("#"));
        public static bool HasPathSyntax(string input, out string path)
        {
            if (input == null || !input.StartsWith("@"))
            {
                path = null;

                return false;
            }

            path = input.Substring(1);

            return true;
        }

        public static ResponseFile Parse(string input) => Parse(input, null);
        public static ResponseFile Parse(string input, ResponseSettings settings)
        {
            ResponseFile responseFile = new ResponseFile()
            {
                WorkingDirectory = !string.IsNullOrEmpty(settings?.WorkingDirectory) ? settings.WorkingDirectory : Environment.CurrentDirectory,
                Value = input,
            };

            settings = settings ?? ResponseSettings.Default;

            if (!Path.IsPathRooted(responseFile.WorkingDirectory))
                responseFile.WorkingDirectory = Path.GetFullPath(responseFile.WorkingDirectory);

            if (settings.IgnoreWhitespace && string.IsNullOrWhiteSpace(input))
                responseFile.Ignore = true;
            else if (settings.IgnoreComments && HasCommentSyntax(input))
                responseFile.Ignore = true;
            else if (HasPathSyntax(input, out string inputPath))
            {
                responseFile.InputPath = inputPath;
                responseFile.IsPath = true;

                if (string.IsNullOrWhiteSpace(inputPath))
                    responseFile.Ignore = true;
                else
                {
                    string fullPath = Path.IsPathRooted(inputPath) ? inputPath : Path.Combine(responseFile.WorkingDirectory, inputPath);
                    string defaultPath = fullPath + '.' + settings.DefaultExtension?.TrimStart('.');

                    if (File.Exists(fullPath))
                        responseFile.FullPath = fullPath;
                    else if (!string.IsNullOrWhiteSpace(settings.DefaultExtension) && File.Exists(defaultPath))
                        responseFile.FullPath = defaultPath;
                    else if (settings.IgnoreMissingFiles)
                        responseFile.Ignore = true;
                    else
                        responseFile.FullPath = fullPath;
                }
            }

            return responseFile;
        }

        public static IEnumerable<string> ExpandFiles(IEnumerable<string> files, ResponseSettings settings = null)
            => ExpandStrings(files.Select(s => HasPathSyntax(s, out _) ? s : '@' + s), settings);

        public static IEnumerable<string> ExpandStrings(IEnumerable<string> inputs, ResponseSettings settings = null)
            => inputs.SelectMany(s => ExpandStrings(s, settings));

        public static IEnumerable<string> ExpandStrings(string input, ResponseSettings settings = null)
            => Expand(input, settings).Select(f => f.Value);

        public static IEnumerable<string> ExpandStrings(ResponseFile file) => Expand(file, null).Select(f => f.Value);

        public static IEnumerable<ResponseFile> Expand(ResponseFile file, ResponseSettings settings)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            HashSet<string> fileSet = new HashSet<string>();
            Func<ResponseFile, IEnumerable<string>> fileReader = settings?.Reader ?? ResponseSettings.Default.Reader;
            Func<string, ResponseSettings, ResponseFile> inputParser = settings?.Parser ?? ResponseSettings.Default.Parser;

            return ExpandFiles(file, settings);

            IEnumerable<ResponseFile> ExpandFiles(ResponseFile file2, ResponseSettings settings2)
            {
                if (file2.Ignore)
                    yield break;
                else if (file2.IsPath)
                {
                    if (fileSet.Contains(file2.FullPath))
                        yield break;

                    fileSet.Add(file2.FullPath);

                    ResponseSettings newSettings = new ResponseSettings()
                    {
                        WorkingDirectory = Path.GetDirectoryName(file2.FullPath),
                        IgnoreComments = settings2.IgnoreComments,
                        IgnoreMissingFiles = settings2.IgnoreMissingFiles,
                        IgnoreWhitespace = settings2.IgnoreWhitespace,
                        DefaultExtension = settings2.DefaultExtension,
                        Reader = settings2.Reader,
                        Parser = settings2.Parser,
                    };

                    foreach (ResponseFile f in File.ReadAllLines(file2.FullPath).Select(s => inputParser(s, newSettings)))
                        foreach (ResponseFile f2 in ExpandFiles(f, newSettings))
                            yield return f2;
                }
                else
                    yield return file2;
            }
        }
        public static IEnumerable<ResponseFile> Expand(string input, ResponseSettings settings)
            => Expand(Parse(input, settings), settings);
    }
}

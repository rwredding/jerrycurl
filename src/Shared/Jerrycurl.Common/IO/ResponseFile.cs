using System;
using System.Collections.Generic;
using System.Text;
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
        public static ResponseFile Parse(string input, string workingDirectory)
        {
            ResponseFile responseFile = new ResponseFile()
            {
                WorkingDirectory = !string.IsNullOrEmpty(workingDirectory) ? workingDirectory : Environment.CurrentDirectory,
                Value = input,
            };

            if (!Path.IsPathRooted(responseFile.WorkingDirectory))
                responseFile.WorkingDirectory = Path.GetFullPath(responseFile.WorkingDirectory);

            if (string.IsNullOrWhiteSpace(input) || HasCommentSyntax(input))
                responseFile.Ignore = true;
            else if (HasPathSyntax(input, out string inputPath))
            {
                responseFile.InputPath = inputPath;
                responseFile.IsPath = true;

                if (string.IsNullOrWhiteSpace(inputPath))
                    responseFile.Ignore = true;
                else if (Path.IsPathRooted(inputPath))
                    responseFile.FullPath = inputPath;
                else
                    responseFile.FullPath = Path.Combine(responseFile.WorkingDirectory, inputPath);

                if (responseFile.FullPath != null && !File.Exists(responseFile.FullPath))
                    responseFile.Ignore = true;
            }

            return responseFile;
        }

        public static IEnumerable<string> ExpandStrings(IEnumerable<string> inputs, string workingDirectory = null)
            => inputs.SelectMany(s => ExpandStrings(s, workingDirectory));

        public static IEnumerable<string> ExpandStrings(string input, string workingDirectory = null)
            => Expand(input, workingDirectory).Select(f => f.Value);

        public static IEnumerable<string> ExpandStrings(ResponseFile file) => Expand(file).Select(f => f.Value);

        public static IEnumerable<ResponseFile> Expand(ResponseFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            HashSet<string> fileSet = new HashSet<string>();

            return ExpandFiles(file);

            IEnumerable<ResponseFile> ExpandFiles(ResponseFile file2)
            {
                if (file2.Ignore)
                    yield break;
                else if (file2.IsPath)
                {
                    if (fileSet.Contains(file2.FullPath))
                        yield break;

                    fileSet.Add(file2.FullPath);

                    string nextWorkingDir = Path.GetDirectoryName(file2.FullPath);

                    foreach (ResponseFile f in File.ReadAllLines(file2.FullPath).Select(s => Parse(s, nextWorkingDir)))
                        foreach (ResponseFile f2 in ExpandFiles(f))
                            yield return f2;
                }
                else
                    yield return file2;
            }
        }
        public static IEnumerable<ResponseFile> Expand(string input, string workingDirectory = null)
            => Expand(Parse(input, workingDirectory));
    }
}

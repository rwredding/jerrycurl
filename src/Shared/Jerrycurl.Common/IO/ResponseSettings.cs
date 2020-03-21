using System;
using System.Collections.Generic;
using System.IO;

namespace Jerrycurl.IO
{
    internal class ResponseSettings
    {
        public static ResponseSettings Default { get; } = new ResponseSettings();

        public bool IgnoreWhitespace { get; set; }
        public bool IgnoreMissingFiles { get; set; }
        public bool IgnoreComments { get; set; }
        public string DefaultExtension { get; set; }
        public string WorkingDirectory { get; set; }
        public Func<ResponseFile, IEnumerable<string>> Reader { get; set; }
        public Func<string, ResponseSettings, ResponseFile> Parser { get; set; }

        public ResponseSettings()
        {
            this.IgnoreWhitespace = false;
            this.IgnoreMissingFiles = true;
            this.IgnoreComments = true;
            this.Reader = f => File.ReadAllLines(f.FullPath);
            this.Parser = ResponseFile.Parse;
        }

        public ResponseSettings(string workingDirectory)
            : this()
        {
            this.WorkingDirectory = workingDirectory;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Jerrycurl.CodeAnalysis.Lexing;
using Jerrycurl.CodeAnalysis.Razor.Lexing;
using Jerrycurl.CodeAnalysis.Razor.Parsing.Fragments;
using Jerrycurl.CodeAnalysis.Razor.Parsing.Directives;
using Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp;
using Jerrycurl.CodeAnalysis.Razor.Lexing.Razor;
using Jerrycurl.CodeAnalysis.Razor.Lexing.Sql;
using Jerrycurl.IO;
using Jerrycurl.Collections;
using System.Security.Cryptography;
using Jerrycurl.CodeAnalysis.Razor.ProjectSystem;
using Jerrycurl.CodeAnalysis.Razor.ProjectSystem.Conventions;
using Jerrycurl.Diagnostics;

namespace Jerrycurl.CodeAnalysis.Razor.Parsing
{
    public class RazorParser
    {
        public IList<RazorPage> Parse(RazorProject project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            IList<RazorPage> pages = this.ParsePages(project).ToList();

            foreach (IRazorProjectConvention convention in project.Conventions ?? Array.Empty<IRazorProjectConvention>())
                convention.Apply(project, pages);

            return pages;
        }

        private IEnumerable<RazorPage> ParsePages(RazorProject project)
        {
            if (project.Items == null || project.Items.Count == 0)
                yield break;

            HashSet<string> intermediatePaths = new HashSet<string>();

            bool hasProjectDirectory = !string.IsNullOrWhiteSpace(project.ProjectDirectory);

            foreach (RazorProjectItem item in project.Items.NotNull())
            {
                bool hasProjectPath = !string.IsNullOrWhiteSpace(item.ProjectPath);

                if (string.IsNullOrWhiteSpace(item.FullPath))
                    throw new InvalidOperationException($"Error parsing file at index {project.Items.IndexOf(item)}. Path cannot be empty.");
                else if (!Path.IsPathRooted(item.FullPath))
                    throw new InvalidOperationException(Error("Path must be rooted."));
                else if (!File.Exists(item.FullPath))
                    throw new FileNotFoundException(Error("File not found."));
                else if (!hasProjectDirectory && !hasProjectPath)
                    throw new InvalidOperationException(Error($"ProjectPath must be specified when no ProjectDirectory is present."));

                string projectPath = hasProjectPath ? item.ProjectPath : null;

                if (hasProjectDirectory)
                    projectPath = PathHelper.MakeRelativePath(project.ProjectDirectory, projectPath ?? item.FullPath);

                if (projectPath == null)
                    throw new InvalidOperationException(Error($"ProjectPath '{item.ProjectPath}' must be relative to to ProjectDirectory '{project.ProjectDirectory}'."));

                RazorPageData pageData = this.Parse(item.FullPath);

                yield return new RazorPage()
                {
                    Data = pageData,
                    Path = Path.GetFullPath(item.FullPath),
                    ProjectPath = projectPath,
                    IntermediatePath = this.MakeIntermediatePath(project, item, pageData, intermediatePaths),
                };

                string Error(string s) => $"Error parsing file '{item.FullPath}'. {s}";
            }
        }

        private string MakeIntermediatePath(RazorProject project, RazorProjectItem item, RazorPageData pageData, HashSet<string> currentPaths)
        {
            if (string.IsNullOrWhiteSpace(project.IntermediateDirectory))
                return null;

            int hashCode = (item.FullPath + "|" + item.ProjectPath + "|" + pageData.SourceChecksum).GetStableHashCode();
            string baseFileName = Path.GetFileNameWithoutExtension(item.ProjectPath);
            string fileName = $"{baseFileName}.{hashCode:x2}.g.cssql.cs";
            string fullName = Path.Combine(project.IntermediateDirectory, fileName);

            int n = 0;

            while (currentPaths.Contains(fullName, StringComparer.OrdinalIgnoreCase))
            {
                fileName = $"{baseFileName}.{hashCode:x2}.g{n++}.cssql.cs";

                fullName = Path.Combine(project.IntermediateDirectory, fileName);
            }

            currentPaths.Add(fullName);

            return fullName;
        }

        public RazorPageData Parse(string fullPath)
        {
            byte[] fileData = File.ReadAllBytes(fullPath);

            string sourceString = this.GetPageStringData(fileData);
            string sourceChecksum = this.GetPageChecksum(fileData);

            return this.Parse(new StringSource(sourceString), fullPath, sourceChecksum);
        }

        public RazorPageData Parse(ISource source, string sourceName, string sourceChecksum)
        {
            Lexer lexer = new Lexer(source);

            lexer.Yield(new RazorDocument());

            if (!lexer.Eof)
                throw new InvalidOperationException("Cannot parse to EOF.");

            return this.Parse(source, lexer, sourceName, sourceChecksum);
        }

        private RazorPageData Parse(ISource source, IEnumerable<Lexeme> lexemes, string sourceName, string sourceChecksum)
        {
            RazorPageData pageData = new RazorPageData()
            {
                SourceName = sourceName,
                SourceChecksum = sourceChecksum,
            };

            foreach (Lexeme lex in lexemes)
            {
                switch (lex.Rule)
                {
                    case CSharpBlock cs when cs.Type == CSharpType.Expression || cs.Type == CSharpType.Statement || cs.Type == CSharpType.Comment:
                        pageData.Content.Add(this.CreateCodeFragment(source, lex.Span, sourceName, cs.Type));
                        break;
                    case CSharpBlock cs when cs.RazorType == RazorType.Model:
                        pageData.Model = this.CreateFragment(source, lex.Span, sourceName);
                        break;
                    case CSharpBlock cs when cs.RazorType == RazorType.Result:
                        pageData.Result = this.CreateFragment(source, lex.Span, sourceName);
                        break;
                    case CSharpBlock cs when cs.RazorType == RazorType.Import:
                        pageData.Imports.Add(this.CreateFragment(source, lex.Span, sourceName));
                        break;
                    case CSharpBlock cs when cs.RazorType == RazorType.Injection || cs.RazorType == RazorType.Projection:
                        {
                            Token[] arguments = lex.Tokens.Where(t => t.Symbol is Argument).ToArray();

                            InjectDirective directive = new InjectDirective();

                            if (arguments.Length == 1)
                                directive.Type = this.CreateFragment(source, arguments[0].Span, sourceName);
                            else if (arguments.Length > 1)
                            {
                                directive.Type = this.CreateFragment(source, arguments[0].Span, sourceName);
                                directive.Variable = this.CreateFragment(source, arguments[1].Span, sourceName);
                            }

                            if (cs.RazorType == RazorType.Injection)
                                pageData.Injections.Add(directive);
                            else
                                pageData.Projections.Add(directive);
                        }
                        break;
                    case CSharpBlock cs when cs.RazorType == RazorType.Template:
                        pageData.Template = this.CreateFragment(source, lex.Span, sourceName);
                        break;
                    case CSharpBlock cs when cs.RazorType == RazorType.Namespace:
                        pageData.Namespace = this.CreateFragment(source, lex.Span, sourceName);
                        break;
                    case CSharpBlock cs when cs.RazorType == RazorType.Class:
                        pageData.Class = this.CreateFragment(source, lex.Span, sourceName);
                        break;
                    case SqlBlock sql:
                        pageData.Content.Add(this.CreateSqlFragment(source, lex, sourceName));
                        break;
                }
            }

            return pageData;

        }

        private SqlFragment CreateSqlFragment(ISource source, Lexeme lex, string sourceName)
        {
            return new SqlFragment()
            {
                Span = lex.Span,
                Text = this.GetUnescapedSqlText(source, lex),
                SourceName = sourceName,
            };
        }

        private string GetPageStringData(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (StreamReader sr = new StreamReader(ms, detectEncodingFromByteOrderMarks: true))
                return sr.ReadToEnd();
        }

        private string GetPageChecksum(byte[] data)
        {
            using (SHA1CryptoServiceProvider hash = new SHA1CryptoServiceProvider())
                return string.Join("", hash.ComputeHash(data).Select(b => b.ToString("x2")));
        }

        private string GetUnescapedSqlText(ISource source, Lexeme lex)
        {
            StringBuilder sql = new StringBuilder();

            foreach (Token token in lex.Tokens)
            {
                if (token.Symbol is SqlEscape e)
                    sql.Append(e.Char);
                else
                    sql.Append(source.GetText(token.Span));
            }

            return sql.ToString();
        }

        private CodeFragment CreateCodeFragment(ISource source, SourceSpan span, string sourceName, CSharpType codeType)
        {
            return new CodeFragment()
            {
                Span = span,
                Text = source.GetText(span),
                SourceName = sourceName,
                CodeType = codeType,
            };
        }

        private RazorFragment CreateFragment(SourceSpan span, string text, string sourceName)
        {
            return new RazorFragment()
            {
                Span = span,
                Text = text,
                SourceName = sourceName,
            };
        }

        private RazorFragment CreateFragment(ISource source, SourceSpan span, string sourceName) => this.CreateFragment(span, source.GetText(span), sourceName);
    }
}

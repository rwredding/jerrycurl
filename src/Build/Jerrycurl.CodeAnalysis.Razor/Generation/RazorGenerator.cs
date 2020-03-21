using System;
using System.Collections.Generic;
using System.Linq;
using Jerrycurl.CodeAnalysis.Lexing;
using Jerrycurl.CodeAnalysis.Projection;
using Jerrycurl.CodeAnalysis.Razor.Lexing.CSharp;
using Jerrycurl.CodeAnalysis.Razor.Parsing;
using Jerrycurl.CodeAnalysis.Razor.Parsing.Directives;
using Jerrycurl.CodeAnalysis.Razor.Parsing.Fragments;

namespace Jerrycurl.CodeAnalysis.Razor.Generation
{
    public class RazorGenerator
    {
        public GeneratorOptions Options { get; }

        public RazorGenerator(GeneratorOptions options)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public ProjectionResult Generate(RazorPageData pageData)
        {
            ISource template = new StringSource(this.Options.TemplateCode);

            Projector projector = new Projector(template);

            foreach (RazorFragment fragment in this.MergeContent(pageData.Content))
            {
                switch (fragment)
                {
                    case SqlFragment sql:
                        projector.Open("execute")
                            .WriteLine($"WriteLiteral({CSharp.Literal(sql.Text)});");
                        break;
                    case CodeFragment code when code.CodeType == CSharpType.Statement:
                        projector.Open("execute").WritePragma(code);
                        break;
                    case CodeFragment code when code.CodeType == CSharpType.Expression:
                        projector.Open("execute")
                            .Write("Write(")
                            .WritePragma(code)
                            .WriteLine(");");
                        break;
                }
            }

            if (pageData.SourceChecksum != null)
            {
                projector.Open("pragmachecksum")
                    .Write($"#pragma checksum \"{pageData.SourceName}\" \"{{ff1816ec-aa5e-4d10-87f7-6f4963833460}}\" \"{pageData.SourceChecksum}\"");
            }

            if (pageData.Model?.Text != null)
                projector.Open("model").WritePragma(pageData.Model);
            else
                projector.Open("model").Write("dynamic");

            if (pageData.Result?.Text != null)
                projector.Open("result").WritePragma(pageData.Result);
            else
                projector.Open("result").Write("object");

            if (pageData.Class?.Text != null)
                projector.Open("class").WritePragma(pageData.Class);
            else if (this.Options.Class?.Text != null)
                projector.Open("class").Write(this.Options.Class.Text);
            else
                projector.Open("class").Write("MyRazorPage");

            if (pageData.Template?.Text != null)
            {
                projector.Open("template")
                    .Write("[global::Jerrycurl.Mvc.Metadata.Annotations.Template(")
                    .WritePragma(pageData.Template)
                    .Write(")]");
            }

            foreach (InjectDirective inject in pageData.Injections ?? new InjectDirective[0])
            {
                if (string.IsNullOrWhiteSpace(inject.Type.Text) || string.IsNullOrWhiteSpace(inject.Variable?.Text))
                    projector.WriteWarning(inject.Type, "Injection ignored. Please specify both type and variable name.");
                else
                {
                    projector.Open("injectiondefs")
                        .WriteLine("[global::Jerrycurl.Mvc.Metadata.Annotations.Inject]")
                        .Write("public ")
                        .WritePragma(inject.Type)
                        .WritePragma(inject.Variable)
                        .WriteLine(" { get; set; }");
                }
            }

            foreach (InjectDirective project in pageData.Projections ?? new InjectDirective[0])
            {
                projector.Open("injectiondefs");

                if (string.IsNullOrWhiteSpace(project.Type?.Text) || string.IsNullOrWhiteSpace(project.Variable?.Text))
                    projector.WriteWarning(project.Type, "Projection ignored. Please specify both type and variable name.");
                else
                {
                    projector.WriteLine("[global::Jerrycurl.Mvc.Metadata.Annotations.Inject]")
                        .Write("public global::Jerrycurl.Mvc.Projections.IProjection<")
                        .WritePragma(project.Type)
                        .Write("> ")
                        .WritePragma(project.Variable)
                        .WriteLine(" { get; set; }");
                }
            }

            foreach (RazorFragment import in this.Options.Imports ?? new RazorFragment[0])
            {
                projector.Open("globalimports")
                    .Write("using ")
                    .WritePragma(import, terminate: true)
                    .WriteLine();
            }

            foreach (RazorFragment import in pageData.Imports ?? new RazorFragment[0])
            {
                projector.Open("localimports")
                    .Write("using ")
                    .WritePragma(import, terminate: true)
                    .WriteLine();
            }

            if (!string.IsNullOrWhiteSpace(pageData.Namespace?.Text))
            {
                projector.Open("beginnamespace")
                    .Write($"namespace ")
                    .WritePragma(pageData.Namespace)
                    .WriteLine(" {");

                projector.Open("endnamespace")
                    .Write("}");
            }
            else if (!string.IsNullOrWhiteSpace(this.Options.Namespace?.Text))
            {
                projector.Open("beginnamespace")
                    .Write($"namespace ")
                    .Write(this.Options.Namespace.Text)
                    .WriteLine(" {");

                projector.Open("endnamespace")
                    .Write("}");
            }

            return projector.Generate();
        }

        private IEnumerable<RazorFragment> MergeContent(IEnumerable<RazorFragment> fragments)
        {
            List<SqlFragment> sqlBatch = new List<SqlFragment>();

            fragments = fragments.SkipWhile(f => f is SqlFragment sql && string.IsNullOrWhiteSpace(sql.Text));

            if (fragments.FirstOrDefault() is SqlFragment sql2)
            {
                SqlFragment trimSql = new SqlFragment()
                {
                    SourceName = sql2.SourceName,
                    Span = sql2.Span,
                    Text = sql2.Text.TrimStart()
                };

                sqlBatch.Add(trimSql);

                fragments = fragments.Skip(1);
            }

            foreach (RazorFragment fragment in fragments)
            {
                if (fragment is SqlFragment sql3)
                {
                    sqlBatch.Add(sql3);

                    continue;
                }
                else if (sqlBatch.Any())
                {
                    yield return this.MergeSql(sqlBatch);

                    sqlBatch.Clear();
                }

                yield return fragment;
            }

            if (sqlBatch.Any())
                yield return this.MergeSql(sqlBatch);
        }

        private SqlFragment MergeSql(IEnumerable<SqlFragment> sqlBatch)
        {
            return new SqlFragment()
            {
                Text = string.Join("", sqlBatch.Select(f => f.Text)),
            };
        }
    }
}

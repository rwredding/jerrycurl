namespace Jerrycurl.Mvc
{
    public interface IDialect
    {
        string Identifier(string id);
        string Parameter(string parameterName);
        string Variable(string variableName);
        string Literal(object value);
        string String(string value);

        string Qualifier { get; }

        DialectSupport Support { get; }
    }
}

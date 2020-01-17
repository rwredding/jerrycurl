namespace Jerrycurl.Tools.Scaffolding.Model
{
    public class TypeMapping
    {
        public string DbName { get; }
        public string ClrName { get; }
        public bool IsValueType { get; }

        public TypeMapping(string dbName, string clrName, bool isValueType)
        {
            this.DbName = dbName;
            this.ClrName = clrName;
            this.IsValueType = isValueType;
        }
    }
}

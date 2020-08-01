using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.Extensions
{
    internal static class ReflectionExtensions
    {
        public static bool IsNotNullableValueType(this Type type) => (type.IsValueType && Nullable.GetUnderlyingType(type) == null);
        public static CustomAttributeData GetNamedAttributeData(this IEnumerable<CustomAttributeData> source, string attributeName)
            => source.FirstOrDefault(x => x.AttributeType.FullName == attributeName);

        public static bool IsNullableType(PropertyInfo property)
        {
            if (property.PropertyType.IsValueType)
                throw new ArgumentException("Property must be a reference type", nameof(property));

            CustomAttributeData nullData = property.CustomAttributes.GetNamedAttributeData("System.Runtime.CompilerServices.NullableAttribute");

            if (nullData != null && nullData.ConstructorArguments.Count == 1)
            {
                CustomAttributeTypedArgument attributeArgument = nullData.ConstructorArguments[0];

                if (attributeArgument.ArgumentType == typeof(byte[]))
                {
                    ReadOnlyCollection<CustomAttributeTypedArgument> args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value;

                    if (args.Count > 0 && args[0].ArgumentType == typeof(byte))
                        return (byte)args[0].Value == 2;
                }
                else if (attributeArgument.ArgumentType == typeof(byte))
                    return (byte)attributeArgument.Value == 2;
            }

            CustomAttributeData contextData = property.DeclaringType.CustomAttributes.GetNamedAttributeData("System.Runtime.CompilerServices.NullableContextAttribute");

            if (contextData != null && contextData.ConstructorArguments.Count == 1 && contextData.ConstructorArguments[0].ArgumentType == typeof(byte))
                return (byte)contextData.ConstructorArguments[0].Value == 2;

            return false;
        }
    }
}

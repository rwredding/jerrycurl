using System;
using System.Linq;
using System.Reflection;

namespace Jerrycurl.Reflection
{
    internal static class TypeExtensions
    {
        public static bool IsOpenGeneric(this Type type, Type openType, out Type argument)
        {
            bool result = type.IsOpenGeneric(openType, out Type[] arguments);

            argument = arguments[0];

            return result;
        }

        public static bool IsOpenGeneric(this Type type, Type openType, out Type[] arguments)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            arguments = new Type[openType.GetGenericArguments().Length];

            if (type.IsGenericType && type.GetGenericTypeDefinition() == openType)
            {
                arguments = type.GetGenericArguments();

                return true;
            }

            return false;
        }

        public static bool HasParameters(this MethodInfo methodInfo, params Type[] parameterTypes)
        {
            return methodInfo.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(parameterTypes);
        }

        public static bool HasSignature(this MethodInfo methodInfo, Type returnType, params Type[] parameterTypes)
        {
            return ((returnType ?? typeof(void)) == methodInfo.ReturnType && methodInfo.HasParameters(parameterTypes));
        }

        public static string GetSanitizedFullName(this Type type)
        {
            if (type.Namespace == null)
                return type.GetSanitizedName();

            return type.Namespace + "." + type.GetSanitizedName();
        }

        public static string GetSanitizedName(this Type type)
        {
            Type[] generics = type.GetGenericArguments();

            string typeName = type.Name;

            int pingdex;
            if ((pingdex = typeName.LastIndexOf('`')) > -1)
                typeName = typeName.Remove(pingdex);

            if (generics.Length > 0)
                return $"{typeName}<{string.Join(", ", generics.Select(GetSanitizedName))}>";

            return typeName;
        }
    }
}

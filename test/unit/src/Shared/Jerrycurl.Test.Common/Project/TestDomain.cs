using System;
using System.Linq;
using System.Reflection;
using Jerrycurl.Mvc;

namespace Jerrycurl.Test.Project
{
    public class TestDomain : IDomain
    {
        public void Configure(DomainOptions options)
        {
            Type conventionType = Assembly.GetEntryAssembly().GetExportedTypes().FirstOrDefault(this.IsValidDatabaseConvention);

            if (conventionType == null)
                throw new InvalidOperationException("DatabaseConvention implementation not found.");

            DatabaseConvention database = (DatabaseConvention)Activator.CreateInstance(conventionType);

            database.Configure(options);
        }

        private bool IsValidDatabaseConvention(Type type)
        {
            if (!typeof(DatabaseConvention).IsAssignableFrom(type))
                return false;
            else if (type.IsAbstract)
                return false;
            else if (type.GetConstructor(Type.EmptyTypes) == null)
                return false;

            return true;
        }
    }
}

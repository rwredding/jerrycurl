[![NuGet](https://img.shields.io/nuget/v/Jerrycurl)](https://nuget.org/packages/Jerrycurl)
[![Build status](https://ci.appveyor.com/api/projects/status/onendmfb6ywd33je/branch/master?svg=true)](https://ci.appveyor.com/project/rwredding/jerrycurl/branch/master)
[![Test status](https://img.shields.io/appveyor/tests/rwredding/jerrycurl/master)](https://ci.appveyor.com/project/rwredding/jerrycurl/branch/master/tests)
[![Gitter chat](https://badges.gitter.im/gitterHQ/gitter.png)](https://gitter.im/jerrycurl-mvc/community)
# Jerrycurl - Razor-powered ORM for .NET
**Jerrycurl** is a free and lightweight **ORM for .NET** that focuses on elegant and type-safe **SQL** written with **Razor syntax**.
```sql
-- Queries/Customers/GetStats.cssql
@result CustomerStatsView
@model CustomerFilter
@project Order o

SELECT
    @R.Star(),
    @R.Star(m => m.Address),
    (
        SELECT  COUNT(*)
        FROM    @o.Tbl()
        WHERE   @o.Col(m => m.CustomerId) = @R.Col(m => m.Id)
    )   AS @R.Prop(m => m.NumberOfOrders)
FROM
    @R.Tbl()
LEFT JOIN
    @R.Tbl(m => m.Address) ON @R.Col(m => m.Address.Id) = @R.Col(m => m.AddressId)
WHERE
    @R.Col(m => m.CreatedDate) >= @M.Par(m => m.CreatedAfter)
ORDER BY
    @R.Col(m => m.CreatedDate) DESC
```

It allows you to build robust data access layers around ASP.NET-like **MVC** and **CQS** conventions that organizes your code into models, queries, commands and **accessors**.

```csharp
// Accessors/CustomersAccessor.cs
public class CustomersAccessor : Accessor
{
    public IList<CustomerStatsView> GetStats(DateTime createdAfter)
    {
        var filter = new CustomerFilter
        {
            CreatedAfter = createdAfter,
        };
        
        return this.Query<CustomerStatsView>(model: filter);
    }
}
```

## Features
* [Official support](https://nuget.org/packages/?q=Jerrycurl.Vendors) for SQL Server, PostgreSQL, MySQL, Oracle and SQLite
* [CLI tool](https://nuget.org/packages/dotnet-jerry) to easily generate classes from your database schema
* Extensive collection of type-safe Razor extensions for all boilerplate SQL
* Single **queries** that map complete object graphs of any [cardinality](https://en.wikipedia.org/wiki/Cardinality_(data_modeling))
* Batchable **commands** through simple `@foreach` expressions
* [High performance](https://github.com/rhodosaur/RawDataAccessBencher/blob/master/Results/20191115_jerrycurl.txt) and support for all operations synchronously or asynchronously
* Organized, ASP.NET-like project conventions with [MVC](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93controller)
* Native [command-query separation](https://en.wikipedia.org/wiki/Command%E2%80%93query_separation) suitable for [ACID](https://en.wikipedia.org/wiki/ACID) or [BASE](https://en.wikipedia.org/wiki/Eventual_consistency)/[CQRS](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs) scenarios
* JSON support through `Newtonsoft.Json` or `System.Text.Json`
* Integration with existing Entity Framework Core models
* Modern language features with .NET Standard 2.1 and C# 8
* Free and [available via NuGet](https://www.nuget.org/packages?q=Jerrycurl)

To learn more about Jerrycurl and how to get started, read [our official docs](https://jerrycurl.net/documentation) or check our [samples repo](https://github.com/rwredding/jerrycurl-samples).

## Building from source
Jerrycurl can be built on [any OS supported by .NET Core](https://docs.microsoft.com/en-us/dotnet/core/install/dependencies) and included in this repository is a [script](build.ps1) that performs all build-related tasks.

### Prerequisites
* .NET Core SDK 3.0
* .NET Core Runtime 2.1+ / 3.0 (to run tests)
* PowerShell 5.0+ (PowerShell Core on Linux/macOS) 
* Visual Studio 2019 (16.3+) (optional)
* Docker (optional - for live database testing)

### Clone, Build and Test
Clone the repository and run our build script from PowerShell.
```powershell
PS> git clone https://github.com/rwredding/jerrycurl
PS> cd jerrycurl
PS> .\build.ps1 [-NoTest] [-NoPack]
```

This runs the `Restore`, `Clean`, `Build`, `[Test]` and `[Pack]` targets on `jerrycurl.sln` and places any packaged `.nupkg` in the `/artifacts/packages` folder. Each target can also be run manually in Visual Studio if preferred.

By default, the `Test` target skips any test that requires live running database server. To help you to include these, you can use our [`docker compose` script](test/tools/boot-dbs.ps1) to boot up instances of our supported databases.

```powershell
PS> .\test\tools\boot-dbs.ps1 up sqlserver,mysql,postgres,oracle
```

Please allow ~60 seconds for the databases to be ready after which you can re-run `build.ps1`, which will then automatically target the included databases instances. When done, you can tear everything down again.

```powershell
PS> .\test\tools\boot-dbs.ps1 down sqlserver,mysql,postgres,oracle
```

> If you already have an empty database running that can be used for testing, you can manually specify its connection string in the environment variable `JERRY_SQLSERVER_CONN`, `JERRY_MYSQL_CONN`, `JERRY_POSTGRES_CONN` or `JERRY_ORACLE_CONN`.

> Oracle Database requires that you are logged into Docker and have accepted their [terms of service](https://hub.docker.com/_/oracle-database-enterprise-edition).



## License
Copyright © 2019 AC Dancode

This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along with this program. If not, see http://www.gnu.org/licenses/.

## Legal
By submitting a Pull Request, you disavow any rights or claims to any changes
submitted to the Jerrycurl project and assign the copyright of
those changes to AC Dancode.

If you cannot or do not want to reassign those rights (your employment
contract for your employer may not allow this), you should not submit a PR.
Open an issue and someone else can do the work.

This is a legal way of saying "If you submit a PR to us, that code becomes ours".
99.9% of the time that's what you intend anyways; we hope it doesn't scare you
away from contributing.

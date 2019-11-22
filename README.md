[![Build status](https://ci.appveyor.com/api/projects/status/onendmfb6ywd33je?svg=true)](https://ci.appveyor.com/project/rwredding/jerrycurl)
[![License LGPLv3](https://img.shields.io/badge/license-LGPLv3-green.svg)](http://www.gnu.org/licenses/lgpl-3.0.html)
[![Gitter chat](https://badges.gitter.im/gitterHQ/gitter.png)](https://gitter.im/jerrycurl-mvc/community)
# Jerrycurl - Razor-powered ORM for SQL lovers
**Jerrycurl** is an MVC-based **ORM** that focuses on elegant and type-safe **SQL** written with **Razor syntax**.
```sql
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

## Features
* Official support for SQL Server, PostgreSQL, MySQL, Oracle and SQLite
* CLI tool to easily generate classes from your database schema
* Extensive collection of Razor extensions for all boilerplate SQL
* Multiset queries with `one-to-one`, `one-to-many`, `many-to-one`, `many-to-many` and `self-join` support
* Batchable commands through simple `@foreach` expressions
* Reusable subqueries and subcommands with *partials*
* [High performance](https://github.com/rhodosaur/RawDataAccessBencher/blob/master/Results/20191115_jerrycurl.txt) for all operations - in both sync and async
* Organized, ASP.NET-like project conventions with [MVC](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93controller)
* Native [command-query separation](https://en.wikipedia.org/wiki/Command%E2%80%93query_separation) suitable for [ACID](https://en.wikipedia.org/wiki/ACID) or [BASE](https://en.wikipedia.org/wiki/Eventual_consistency) operations
* JSON support with `Newtonsoft.Json` or `System.Text.Json`
* Integration with existing Entity Framework Core projects
* Modern language features with .NET Standard 2.1 and C# 8
* ...and more

To learn more about Jerrycurl and how to get started, read [our official docs](https://jerrycurl.net/documentation) or check our [samples repo](https://github.com/rwredding/jerrycurl-samples).

## Building from source
Jerrycurl can be built on any OS supported by .NET Core and included in this repository is a script that performs all build related tasks.

### Prerequisites
* .NET Core SDK 3.0
* .NET Core Runtime 2.1+ / 3.0 (to run tests)
* PowerShell 5.0+ (PowerShell Core on Linux/macOS) 
* Visual Studio 2019 (16.3+) (optional)
* Docker (optional - for vendor testing)

### Clone and Build
Clone the repository and run our build script from PowerShell.
```powershell
PS> git clone https://github.com/rwredding/jerrycurl
PS> cd jerrycurl
PS> .\build.ps1 [-NoTest] [-NoPack]
```
This runs the `Restore`, `Clean`, `Build`, `[Test]` and `[Pack]` targets on `jerrycurl.sln`. Each target can also be run manually in Visual Studio if preferred.

> Packaged `.nupkgs` are placed in `/artifacts/packages`.

### Test
The script above *cannot include all tests* out of the box, as those testing specific databases often require a live running server. To help you include these in your run, you can use our [`docker compose` script](test/tools/boot-dbs.ps1) with PowerShell to boot up instances of the required databases.

```powershell
PS> .\test\tools\boot-dbs.ps1 up sqlserver,mysql,postgres,oracle
```
Please allow ~30 seconds for the databases to be ready after which you can re-run your tests.

> Oracle Database requires that you are logged into Docker and have accepted their [terms of service](https://hub.docker.com/_/oracle-database-enterprise-edition).

> If you already have an empty database running that can be used for testing, you can manually specify its connection string in the environment variable `JERRY_SQLSERVER_CONN`, `JERRY_MYSQL_CONN`, `JERRY_POSTGRES_CONN` or `JERRY_ORACLE_CONN`.

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

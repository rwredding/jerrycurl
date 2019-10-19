[![Build status](https://ci.appveyor.com/api/projects/status/onendmfb6ywd33je?svg=true)](https://ci.appveyor.com/project/rwredding/jerrycurl)
[![License LGPLv3](https://img.shields.io/badge/license-LGPLv3-green.svg)](http://www.gnu.org/licenses/lgpl-3.0.html)
# Jerrycurl - Razor SQL for .NET
**Jerrycurl** is a lightweight and highly customizable **object-relational mapper** with emphasis on writing elegant and performant **SQL** with **Razor** and **C#**.

```sql
@result MyShop.Data.Views.Orders.OrderUnsentInvoiceView
@model MyShop.Data.Models.CustomerParams

@project Order o
@project Customer c

SELECT
    @o.Col(m => m.InvoiceNumber)        AS @R.Prop(m => m.InvoiceNumber),
    @o.Col(m => m.Completed)            AS @R.Prop(m => m.InvoiceDate),
    @c.Col(m => m.Email)                AS @R.Prop(m => m.CustomerEmail),
    @c.Col(m => m.Address)              AS @R.Prop(m => m.CustomerAddress)
FROM
    @o.Tbl()
INNER JOIN
    @c.Tbl() ON @c.Col(m => m.Id) = @o.Col(m => m.CustomerId)
WHERE
    @o.Col(m => m.CustomerId) = @M.Par(m => m.CustomerId)
    AND
    @o.Col(m => m.Sent) IS NULL
    AND
    @o.Col(m => m.Completed) IS NOT NULL
ORDER BY
    @o.Col(m => m.Completed) ASC
```

## Features
* Beautiful and injection-safe Razor syntax with SQL and strongly typed C#
* Blazingly fast queries with `one-to-one`, `one-to-none`, `one-to-many`, `many-to-many` and `self-join` support
* Simple batch generation with Razor `@foreach`
* Reusable subqueries and subcommands with Razor *partials* and *templates*
* Easily maintainable, ASP.NET-like projects with [MVC](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93controller)
* High decoupling with [command-query separation](https://en.wikipedia.org/wiki/Command%E2%80%93query_separation)
* JSON support with `Newtonsoft.Json` or `System.Text.Json`
* Integration with Entity Framework Core models
* Modern language features with .NET Standard 2.0 and 2.1
* ...and more

To learn more about Jerrycurl and how to get started, read [our official docs](https://jerrycurl.net/docs) or check our [sample repo](https://github.com/rwredding/jerrycurl-sample).

## Building from source
Jerrycurl can be built on any OS supported by .NET Core and included in this repository is a build script that performs all build related tasks.

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
The script above runs most tests, but will **skip those** that require a live running database server. To enable these you can run our [`docker compose` script](test/tools/boot-dbs.ps1) with PowerShell to boot up instances of our supported databases.

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

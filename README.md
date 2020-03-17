[![NuGet](https://img.shields.io/nuget/v/Jerrycurl)](https://nuget.org/packages/Jerrycurl)
[![Build status](https://ci.appveyor.com/api/projects/status/onendmfb6ywd33je/branch/master?svg=true)](https://ci.appveyor.com/project/rwredding/jerrycurl/branch/master)
[![Test status](https://img.shields.io/appveyor/tests/rwredding/jerrycurl/master)](https://ci.appveyor.com/project/rwredding/jerrycurl/branch/master/tests)
[![Gitter chat](https://badges.gitter.im/gitterHQ/gitter.png)](https://gitter.im/jerrycurl-mvc/community)
# Jerrycurl - MVC and Razor-powered ORM for .NET

**Jerrycurl** is an object-relational framework that allows developers to build **data access layers** in a way similar to how web applications are built with **ASP.NET MVC**.

### Installation
Jerrycurl can be installed into any [SDK-style](https://docs.microsoft.com/en-us/nuget/resources/check-project-format) C# project from NuGet. The main package contains support for compiling `.cssql` files into your project and executing them via the built-in MVC engine. Additionally you can install support for [one of our supported databases](https://nuget.org/packages?q=Jerrycurl.Vendors) from NuGet as well.

```shell
> dotnet add package Jerrycurl
> dotnet add package Jerrycurl.Vendors.SqlServer
```

#### Tooling
If you want to generate a ready-to-go object model from your database, install our [CLI](https://www.nuget.org/packages/dotnet-jerry/) from NuGet.
```shell
> dotnet tool install --global dotnet-jerry
```
This enables the `jerry` executable anywhere on your machine. In our case we'll use the `scaffold` command to generate a `.cs` file with classes matching a local database about movies.
```shell
> jerry scaffold -v sqlserver -c "SERVER=.;DATABASE=moviedb;TRUSTED_CONNECTION=true" -ns "MovieDb.Database"
Connecting to database 'moviedb'...
Generating...
Generated 7 tables and 21 columns in Database.cs.
```
To learn more about our CLI, type in `jerry help`.

### MVC design
After installing the packages above you can start adding the different components to your project. This should feel familiar to anyone acquainted with ASP.NET MVC, and for the most part Jerrycurl aligns itself with this framework, only with a slightly different terminology. 

So where your ASP.NET application contains models, controllers and Razor HTML-based views, Jerrycurl separates your project into models, accessors and procedures written with Razor SQL syntax.

#### Model layer
The model layer is a collection of POCO-like classes that represent tables and customized datasets for your operations. Each model can be mapped at any depth with any type of data relationship: one-to-one, one-to-many, many-to-one and self-joins.
```csharp
// Database.cs
[Table("dbo", "Movie")]
class Movie
{
    [Id, Key("PK_Movie")]
    public int Id { get; set; }
    public string Title { get; set; }
    public int Year { get; set; }
}
```
```csharp
// Views/Movies/MovieTaglineView.cs
class MovieTaglineView : Movie
{
    public string Tagline { get; set; }
}
```
```csharp
// Views/Movies/MovieRolesView.cs
class MovieRolesView : Movie
{
    public IList<MovieRole> Roles { get; set; }
}
```

#### Procedure (view) layer
Procedures are written in `.cssql` files and separated into **commands** that write data (`INSERT`, `UPDATE`, `DELETE`) and **queries** that read data (`SELECT`). Both are written with a combination of **SQL and Razor code** generating SQL payloads directly from your object model.
```
-- Queries/Movies/GetMovies.cssql
@result MovieTaglineView
@model MovieFilter
@project MovieDetails d

SELECT     @R.Star(),
           @d.Col(m => m.Tagline) AS @R.Prop(m => m.Tagline)
FROM       @R.Tbl()
LEFT JOIN  @d.Tbl() ON @d.Col(m => m.MovieId) = @R.Col(m => m.Id)
WHERE      @R.Col(m => m.Year) >= @M.Par(m => m.SinceYear)
```
```
-- Commands/Movies/AddMovies.cssql
@model Movie

@foreach (var v in this.M.Vals())
{
    INSERT INTO @v.TblName() ( @v.In().ColNames() )
    OUTPUT      @v.Out().Cols("INSERTED").As().Props()
    VALUES                   ( @v.In().Pars() )
}
```

#### Accessor (controller) layer
Accessors provide the bridge from your code to the consumer by exposing a collection of methods that executes Razor commands and queries and maps their resulting data sets to matching objects.
```csharp
// Accessors/MoviesAccessor.cs
public class MoviesAccessor : Accessor
{
    public IList<MovieTaglineView> GetMovies(int sinceYear) // -> Queries/Movies/GetMovies.cssql
        => this.Query<MovieTaglineView>(model: new MovieFilter { SinceYear = sinceYear });
    
    public void AddMovies(IList<Movie> newMovies) // -> Commands/Movies/AddMovies.cssql
        => this.Execute(model: newMovies);
}
```

#### Domain (application) layer
Domains provide a central place for fetching configuration for any (or a subset of) your database operations.
```csharp
// MovieDomain.cs
class MovieDomain : IDomain
{
    public void Configure(DomainOptions options)
    {
        options.UseSqlServer("SERVER=.;DATABASE=moviedb;TRUSTED_CONNECTION=true");
        options.UseJson();
    }
}
```

## Features
* [Official support](https://nuget.org/packages/?q=Jerrycurl.Vendors) for SQL Server, PostgreSQL, MySQL, Oracle and SQLite
* [CLI tool](https://nuget.org/packages/dotnet-jerry) to easily generate classes from your database schema
* Extensive collection of typesafe Razor extensions for all boilerplate SQL
* Single **queries** that map complete object graphs of any [cardinality](https://en.wikipedia.org/wiki/Cardinality_(data_modeling))
* Batchable **commands** through simple `@foreach` expressions
* Easy integration with any dependency injection container
* [High performance](https://github.com/rhodosaur/RawDataAccessBencher/blob/master/Results/20191115_jerrycurl.txt) and support for all operations synchronously or asynchronously
* Organized, ASP.NET-like project conventions with [MVC](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93controller)
* Native [command-query separation](https://en.wikipedia.org/wiki/Command%E2%80%93query_separation) suitable for [ACID](https://en.wikipedia.org/wiki/ACID) or [BASE](https://en.wikipedia.org/wiki/Eventual_consistency)/[CQRS](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs) scenarios
* JSON support through `Newtonsoft.Json` or `System.Text.Json`
* Integration with existing Entity Framework Core models
* Modern language features with .NET Standard 2.1 and C# 8
* Free and [available via NuGet](https://www.nuget.org/packages?q=Jerrycurl)

To learn more about Jerrycurl and how to get started, visit [our official site](https://jerrycurl.net) or check our [samples repo](https://github.com/rwredding/jerrycurl-samples).

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

By default, the `Test` target skips any unit test that requires live running database server. To help you to include these, you can run our [`docker compose` script](test/tools/boot-dbs.ps1) to boot up instances of our supported databases.

```powershell
PS> .\test\tools\boot-dbs.ps1 up sqlserver,mysql,postgres,oracle
```

Please allow ~60 seconds for the databases to be ready after which you can re-run `build.ps1`; it will then automatically target the included databases instances. When done, you can tear everything down again.

```powershell
PS> .\test\tools\boot-dbs.ps1 down sqlserver,mysql,postgres,oracle
```

> If you already have an empty database running that can be used for testing, you can manually specify its connection string in the environment variable `JERRY_SQLSERVER_CONN`, `JERRY_MYSQL_CONN`, `JERRY_POSTGRES_CONN` or `JERRY_ORACLE_CONN`.

> Pulling the Oracle Database image requires that you are logged into Docker and have accepted their [terms of service](https://hub.docker.com/_/oracle-database-enterprise-edition).

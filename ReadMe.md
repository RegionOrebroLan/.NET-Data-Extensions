# .NET-Data-Extensions

Data additions and extensions for .NET.

[![NuGet](https://img.shields.io/nuget/v/RegionOrebroLan.Data.svg?label=NuGet)](https://www.nuget.org/packages/RegionOrebroLan.Data)

## Integration-tests

Database-names and database-filenames can not be longer than 128 characters. You can read more here: [CREATE DATABASE/Arguments](https://docs.microsoft.com/en-us/sql/t-sql/statements/create-database-transact-sql?view=sql-server-2017#arguments).

Therefore the resources, used by the integration-tests, are copied to the temporary-folder (Path.GetTempPath()) to minimize the risk for to long paths. Avoiding a dependency to where on the filesystem this repository was cloned.
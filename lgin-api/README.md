# aspnet-core-3-registration-login-api

ASP.NET Core 3.1 - Simple API for User Management, Authentication and Registration

For documentation and instructions check out https://jasonwatmore.com/post/2019/10/14/aspnet-core-3-simple-api-for-authentication-registration-and-user-management

dotnet ef migrations add InitialCreate --context SqliteDataContext --output-dir Migrations/SqliteMigrations

ASPNETCORE_ENVIRONMENT=Production dotnet ef migrations add InitialCreate --context DataContext --output-dir Migrations/SqlServerMigrations
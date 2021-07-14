using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Threading;

namespace Discount.API.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host, int retry = 0)
        {
            using (var scope = host.Services.CreateScope())
            {
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();  

                try
                {
                    logger.LogInformation("Migrating postgresql started");

                    using var conn = new NpgsqlConnection(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
                    conn.Open();

                    using var command = new NpgsqlCommand
                    {
                        Connection = conn
                    };

                    command.CommandText = "DROP TABLE IF EXISTS Coupon";
                    command.ExecuteNonQuery();

                    command.CommandText = @"CREATE TABLE Coupon(Id SERIAL PRIMARY KEY, 
                                                                ProductName VARCHAR(100) NOT NULL,
                                                                Description TEXT,
                                                                Amount INT)";
                    command.ExecuteNonQuery();

                    command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('IPhone X', 'IPhone Discount', 150);";
                    command.ExecuteNonQuery();

                    command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('Samsung 10', 'Samsung Discount', 100);";
                    command.ExecuteNonQuery();

                    logger.LogInformation("Migrating postgresql ended");
                }
                catch (NpgsqlException ex)
                {
                    logger.LogError("Migrating postgresql failed.", ex);

                    Thread.Sleep(5000);

                    if (retry > 0)
                    {
                        retry--;
                        MigrateDatabase<TContext>(host, retry);
                    }
                    
                }
                catch(Exception ex)
                {
                    logger.LogError("Migrating database failed.", ex);

                    throw;
                }
            }
            
            return host;
        }
    }
}

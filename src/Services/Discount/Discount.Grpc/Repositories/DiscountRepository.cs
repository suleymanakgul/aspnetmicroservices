using Dapper;
using Discount.Grpc.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace Discount.Grpc.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly IConfiguration _configuration;

        public DiscountRepository(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<bool> AddDiscount(Coupon coupon)
        {
            await using var conn = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            int affectedRows = await conn.ExecuteAsync("INSERT INTO Coupon(ProductName, Description, Amount) VALUES (@ProductName, @Description, @Amount)",
                new { ProductName = coupon.ProductName, Description = coupon.Description, Amount = coupon.Amount });

            return affectedRows > 0;
        }

        public async Task<bool> DeleteDiscount(string productName)
        {
            await using var conn = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            int affectedRows = await conn.ExecuteAsync(@"DELETE FROM Coupon WHERE ProductName = @ProductName",
                                                                new { ProductName = productName });

            return affectedRows > 0;
        }

        public async Task<Coupon> GetDiscount(string productName)
        {
            await using var conn = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var coupon = await conn.QueryFirstOrDefaultAsync<Coupon>("SELECT * FROM Coupon WHERE ProductName = @ProductName", new { ProductName = productName });

            if (coupon == null)
            {
                return new Coupon();
            }

            return coupon;
        }

        public async Task<bool> UpdateDiscount(Coupon coupon)
        {
            await using var conn = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            int affectedRows = await conn.ExecuteAsync(@"UPDATE Coupon SET ProductName = @ProductName, Description = @Description, Amount = @Amount
                                                                WHERE Id = @Id", 
                                                        new { ProductName = coupon.ProductName, Description = coupon.Description, Amount = coupon.Amount, Id = coupon.Id});

            return affectedRows > 0;
        }
    }
}

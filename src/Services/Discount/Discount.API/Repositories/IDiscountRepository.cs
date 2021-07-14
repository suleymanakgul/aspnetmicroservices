using Discount.API.Entities;
using System.Threading.Tasks;

namespace Discount.API.Repositories
{
    public interface IDiscountRepository
    {
        Task<Coupon> GetDiscount(string productName);

        Task<bool> DeleteDiscount(string productName);

        Task<bool> AddDiscount(Coupon coupon);

        Task<bool> UpdateDiscount(Coupon coupon);
    }
}

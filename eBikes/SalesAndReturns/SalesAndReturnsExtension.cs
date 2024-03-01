#nullable disable
using SalesAndReturns.BLL;
using SalesAndReturns.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SalesAndReturns
{
    public static class GroceryPickingExtension
    {
        public static void SalesAndReturnsBackendDependencies(this IServiceCollection services,
            Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<SalesAndReturnsContext>(options);

            //add services using .AddTransient<t>(...)
            services.AddTransient<SalesAndReturnsServices>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<SalesAndReturnsContext>();
                return new SalesAndReturnsServices(context);
            }
            );
        }
    }
}

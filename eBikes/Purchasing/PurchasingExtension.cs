using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Purchasing.BLL;
using Purchasing.DAL;

namespace Purchasing
{
    public static class PurchasingExtension
    {
        public static void PurchasingBackendDependencies(this IServiceCollection services,
    Action<DbContextOptionsBuilder> options)
        {
            //  register the DBContext class in Chinook2018 with the service collection
            services.AddDbContext<PurchasingContext>(options);

            //  add any services that you create in the class library
            //  using .AddTransient<t>(...)
            services.AddTransient<PurchasingServices>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<PurchasingContext>();
                return new PurchasingServices(context);
            }
            );
        }
    }
}

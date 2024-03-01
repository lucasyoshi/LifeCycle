using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Receiving.BLL;
using Receiving.DAL;

namespace Receiving
{
    public static class ReceivingExtension
    {
        public static void ReceivingBackendDependencies(this IServiceCollection services,
    Action<DbContextOptionsBuilder> options)
        {
            //  register the DBContext class in Chinook2018 with the service collection
            services.AddDbContext<ReceivingContext>(options);

            //  add any services that you create in the class library
            //  using .AddTransient<t>(...)
            services.AddTransient<ReceivingServices>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<ReceivingContext>();
                return new ReceivingServices(context);
            }
            );
        }
    }
}

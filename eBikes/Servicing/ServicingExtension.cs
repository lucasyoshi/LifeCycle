using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Servicing.BLL;
using Servicing.DAL;

namespace Servicing
{
    public static class ServicingExtension
    {
        public static void ServicingBackendDependencies (this IServiceCollection services, Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<ServicingContext>(options);

            services.AddTransient<ServicingServices>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<ServicingContext>();
                return new ServicingServices(context);
            }
            );
        }

    }
}
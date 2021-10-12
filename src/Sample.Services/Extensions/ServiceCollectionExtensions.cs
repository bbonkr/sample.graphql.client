using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGraphqlService(this IServiceCollection services)
        {
            services.AddOptions<GraphqlOptions>().Configure<IConfiguration>((options, configuration) =>
            {
                configuration.GetSection(GraphqlOptions.Name).Bind(options);
            });

            services.AddScoped<GraphqlService>();

            return services;
        }
    }
}

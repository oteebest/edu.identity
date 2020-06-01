using Identity.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Util
{
    public static class UtilService
    {
        public static void RegisterUtilCoreServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.Configure<IdentityConfig>(configuration.GetSection("IdentityConfig"));

        }
    }
}

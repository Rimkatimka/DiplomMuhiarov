// EnergyMeteringSystem.Services/DynamicForms/Extensions/ServiceCollectionExtensions.cs
using EnergyMeteringSystem.Services.DynamicForms.Builders;
using EnergyMeteringSystem.Services.DynamicForms.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyMeteringSystem.Services.DynamicForms.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDynamicForms(this IServiceCollection services)
        {
            services.AddSingleton<IMetadataService, MetadataService>();
            services.AddScoped<IDynamicRepository, DynamicRepository>();
            services.AddScoped<IFormBuilder, DynamicFormBuilder>();
        }
    }
}
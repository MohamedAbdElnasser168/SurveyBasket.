namespace SurveyBasket.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            
            services.AddServicesToContainer();
            services.AddMappsterConf();
            services.AddControllers();
            services.AddOpenApi();
            services.AddFluentValidation();


            return services;
        }

        public static IServiceCollection AddServicesToContainer(this IServiceCollection services)
        {
            // Add services to the container.
            services.AddScoped<IPollService, PollService>();

            return services;
        }

        public static IServiceCollection AddMappsterConf(this IServiceCollection services)
        {
            var mappingConfig = TypeAdapterConfig.GlobalSettings;
            mappingConfig.Scan(Assembly.GetExecutingAssembly());
            services.AddSingleton<IMapper>(new Mapper(mappingConfig));            

            return services;
        }

        public static IServiceCollection AddFluentValidation(this IServiceCollection services)
        {
            services
              .AddFluentValidationAutoValidation()
              .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}

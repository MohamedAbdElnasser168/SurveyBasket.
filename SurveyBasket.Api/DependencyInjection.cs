namespace SurveyBasket.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            // Register API related services here in the future


            // Add services to the container.

            services.AddScoped<IPollService, PollService>();

            // Add Mapster configurations
            // ????? ????????? ???? ???? ?????? ?????

            var mappingConfig = TypeAdapterConfig.GlobalSettings;
            mappingConfig.Scan(Assembly.GetExecutingAssembly());

            services.AddSingleton<IMapper>(new Mapper(mappingConfig));
            ////////
            ///
            // Add FluentValidation validators
            //services.AddScoped<IValidator<CreatePollRequest>, CreatePollRequestValidator>();
            services
              .AddFluentValidationAutoValidation()
              .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());


            services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            services.AddOpenApi();


            return services;
        }

        

    }
}

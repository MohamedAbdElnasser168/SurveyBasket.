
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.IdentityModel.Tokens;
using SurveyBasket.Api.Settings;
using System.Text;

namespace SurveyBasket.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddControllers();


            // Cache configuration

            services.AddHybridCache();






            // Read allowed origins from configuration
            // var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();

            // CORS configuration

            services.AddCors(options =>
                options.AddDefaultPolicy(builder =>
                    builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                )
                );
             




            #region CommentAboutCors
            //services.AddCors(options =>
            ////{
            ////    options.AddPolicy("AllowAll", builder =>
            ////        builder
            ////        .AllowAnyOrigin()
            ////        .AllowAnyMethod()       
            ////        .AllowAnyHeader()
            ////    );
            //    //options.AddPolicy("CustomPolicy", builder =>
            //    //    builder
            //    //    .WithOrigins()
            //    //    .AllowAnyMethod()
            //    //    .AllowAnyHeader()
            //    //);

            //});

            #endregion


            var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException(" Connection String 'DefaultConnection' Not Found");


            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));


            services.AddAuthConfig(configuration);
            services.AddServicesToContainer();
            services.AddMappsterConf();
            services.AddOpenApi();
            services.AddFluentValidation();


            // read mail settings from configuration and register it in the container
            // for options pattern
            services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));


            return services;
        }

        public static IServiceCollection AddServicesToContainer(this IServiceCollection services)
        {
            // Add services to the container.
            services.AddScoped<IPollService, PollService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IQuestionService,QuestionService>();
            services.AddScoped<IVoteService, VoteService>();
            services.AddScoped<IResultService, ResultService>();
            services.AddScoped<IEmailSender, EmailService>();

            // Add global exception handler
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            services.AddHttpContextAccessor();


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

        public static IServiceCollection AddAuthConfig(this IServiceCollection services,IConfiguration configuration)
        {

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            services.AddSingleton<IJwtProvider, JwtProvider>();


            // for options pattern
            //services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
            // for options pattern with validation
            services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            var JwtSettings = configuration.GetSection(JwtOptions.SectionName)
                                        .Get<JwtOptions>();


            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(
                options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings?.Key!)),
                        ValidIssuer =JwtSettings?.Issuer,
                        ValidAudience = JwtSettings?.Audience

                    };
                });



            services.Configure<IdentityOptions>(options =>
            {
                // Default Password settings.
                options.Password.RequiredLength = 6;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
            });



            //var test = new
            //{
            //    IssuerSigningKey = configuration["Jwt:Key"]!,
            //    ValidIssuer = configuration["Jwt:Issuer"],
            //    ValidAudience = configuration["Jwt:Audience"]
            //};

            return services;
        }

    }
}

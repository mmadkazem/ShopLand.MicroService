using Catalog.Api.Common.Infrastructure.Consumers;

namespace Catalog.Api.Common.Extensions;


public static class IServiceCollectionExtension
{
    public static void AddServiceCollection(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(typeof(CatalogValidationBehavior<,>).Assembly);
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CatalogValidationBehavior<,>));
        });
        builder.Services.BrokerConfigure(builder.Configuration);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddCarter();
        builder.Services.AddExceptionHandlerMiddlewareServices();
        builder.Services.AddProjectServices(builder.Configuration);
        builder.Services.AddSwaggerConfig();
    }

    private static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(option =>
        {
            option.UseNpgsql(configuration.GetConnectionString("CatalogConStr"));
        });
        services.AddScoped<ICatalogItemRepository, CatalogItemRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    private static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Liaro", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection BrokerConfigure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(configure =>
        {
            var brokerConfig = BrokerOptionFactory.Create();
            configure.AddConsumer<MediaUploadedEventConsumer>();
            configure.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(brokerConfig.Host);
                cfg.ConfigureEndpoints(ctx);
            });
        });
        return services;
    }

    private static IServiceCollection AddExceptionHandlerMiddlewareServices(this IServiceCollection services)
    {
        services.AddScoped<CatalogValidationsExceptionMiddleware>();
        return services;
    }

    public static WebApplication UseExceptionHandler(this WebApplication app)
    {
        app.UseMiddleware<CatalogValidationsExceptionMiddleware>();
        return app;
    }
}
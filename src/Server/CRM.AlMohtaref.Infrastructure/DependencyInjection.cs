using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Infrastructure.Data;
using CRM.AlMohtaref.Infrastructure.ExternalServices;
using CRM.AlMohtaref.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.AlMohtaref.Infrastructure.ExternalServices;

namespace CRM.AlMohtaref.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddHttpContextAccessor();

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
            );

            options.LogTo(Console.WriteLine, LogLevel.Information);
        });

        AddRepositories(services);
        AddServices(services);

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILookupRepository, LookupRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IQuestionCampaignRepository, QuestionCampaignRepository>();
        services.AddScoped<IQuestionConfigurationRepository, QuestionConfigurationRepository>();
        services.AddScoped<IOptionRepository, OptionRepository>();
        services.AddScoped<IOptionSetRepository, OptionSetRepository>();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IOptionSetHashService, OptionSetHashService>();

        return services;
    }
}
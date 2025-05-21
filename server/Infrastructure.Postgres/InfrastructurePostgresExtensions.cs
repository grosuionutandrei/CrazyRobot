using Application;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Infrastructure.Postgres.Repositories;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Postgres;

public static class InfrastructurePostgresExtensions
{
    public static IServiceCollection AddDataSourceAndRepositories(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((service, options) =>
        {
            var provider = services.BuildServiceProvider();
            options.UseNpgsql(
                provider.GetRequiredService<IOptionsMonitor<AppOptions>>().CurrentValue.DbConnectionString);
            options.EnableSensitiveDataLogging();
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IEventRepository,   EventRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();      
        services.AddScoped<IAdminSurveyRepository, AdminSurveyRepository>();
        services.AddScoped<IUserSurveyRepository, UserSurveyRepository>();
        services.AddScoped<Seeder>();

        return services;
    }
}
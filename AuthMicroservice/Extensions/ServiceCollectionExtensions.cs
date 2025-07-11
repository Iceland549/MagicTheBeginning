﻿using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Application.UseCases;
using AuthMicroservice.Infrastructure.Config;
using AuthMicroservice.Infrastructure.Email;
using AuthMicroservice.Infrastructure.Persistence;
using AuthMicroservice.Infrastructure.Persistence.Repositories;
using AuthMicroservice.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using AuthMicroservice.Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;


namespace AuthMicroservice.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers authentication and token management services.
        /// </summary>
        public static IServiceCollection AddAuthMicroserviceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurations
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));

            // Repositories
            services.AddScoped<IUserRepository, EfUserRepository>();
            services.AddScoped<IRoleRepository, EfRoleRepository>();
            services.AddScoped<IEmailTokenRepository, EfEmailTokenRepository>();
            services.AddScoped<IRefreshTokenRepository, EfRefreshTokenRepository>();

            // Security services
            services.AddScoped<IJwtService, JwtTokenGenerator>();  // Pure JWT gen/validation
            services.AddScoped<IAuthService, AuthService>();       // Auth flow orchestration
            services.AddScoped<IEmailService, SmtpEmailService>();

            // UseCases
            services.AddScoped<RegisterUserUseCase>();
            services.AddScoped<LoginUseCase>();
            services.AddScoped<LogoutUseCase>();
            services.AddScoped<RefreshTokenUseCase>();
            services.AddScoped<SeedAdminUseCase>();
            services.AddScoped<GetProfileUseCase>();
            services.AddScoped<GenerateEmailConfirmationUseCase>();
            services.AddScoped<ConfirmEmailUseCase>();
            services.AddScoped<GenerateResetPasswordUseCase>();
            services.AddScoped<ResetPasswordUseCase>();

            // AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfile));

            return services;
        }
    

        /// <summary>
        /// Adds database context for Entity Framework Core.
        /// </summary>
        public static IServiceCollection AddAuthDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            return services;
        }
    }
}
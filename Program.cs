using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Repositories;
using Event_and_parking_reservation_system.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Event_and_parking_reservation_system.Middleware;
using System.Text;
using Event_and_parking_reservation_system.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Event_and_parking_reservation_system
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description =
                            "Enter the JWT access token."
                    }
                );

                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
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
                    }
                );
            });

            string connectionString =
                builder.Configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection was not found.");

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

            builder.Services.AddScoped<ICustomerService, CustomerService>();

            builder.Services.AddScoped<
                IPasswordHasher<Customer>,
                PasswordHasher<Customer>
            >();

            builder.Services.AddScoped<
                IAuthService, AuthService>();

            builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService
>();
            builder.Services.AddScoped<
                IEmailService, DevelopmentEmailService>();

            builder.Services.AddScoped<
            IEmailVerificationService,
            EmailVerificationService
             >();
            builder.Services.AddScoped<
            IPasswordResetService,
            PasswordResetService
            >();

            builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName)
);

            JwtSettings jwtSettings =
                builder.Configuration
                    .GetSection(JwtSettings.SectionName)
                    .Get<JwtSettings>()
                ?? throw new InvalidOperationException(
                    "JWT settings are not configured."
                );

            if (string.IsNullOrWhiteSpace(jwtSettings.Key))
            {
                throw new InvalidOperationException(
                    "JWT secret key is not configured."
                );
            }

            builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings.Key
                        )
                    ),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };
    });

            builder.Services.AddAuthorization();

            builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(
        EmailSettings.SectionName
    )
);

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseMiddleware<ActiveCustomerMiddleware>();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
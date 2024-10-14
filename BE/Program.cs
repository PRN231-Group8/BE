using System.Text;
using System.Text.Json;
using ExploreNow.Validations.Location;
using ExploreNow.Validations.Photo;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PRN231.ExploreNow.BusinessObject.Contracts.Repositories;
using PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Utilities;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories;
using PRN231.ExploreNow.Repositories.Repositories.Interface;
using PRN231.ExploreNow.Repositories.UnitOfWorks;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;
using PRN231.ExploreNow.Services.Services;
using StackExchange.Redis;
using PRN231.ExploreNow.Validations;
using PRN231.ExploreNow.Validations.Interface;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

var builder = WebApplication.CreateBuilder(args);

#region Configure Key Vault and Secrets
// Configure key vault in Azure Portal
var keyVaultUrl = builder.Configuration["KeyVault:KeyVaultUrl"];

// Use DefaultAzureCredential
var credential = new DefaultAzureCredential();

// Create SecretClient with DefaultAzureCredential
var client = new SecretClient(new Uri(keyVaultUrl), credential);

// Fetch secrets from Key Vault
var secrets = new Dictionary<string, string>
	{
		{"SMTP:Secret", client.GetSecret("SMTPSecret").Value.Value},
		{"JWT:Secret", client.GetSecret("SecretJWT").Value.Value},
		{"SMTP:Username", client.GetSecret("SMTPUsername").Value.Value},
		{"SMTP:Password", client.GetSecret("SMTPPassword").Value.Value},
		{"GoogleAuthSettings:Google:ClientId", client.GetSecret("ClientId").Value.Value},
		{"GoogleAuthSettings:Google:ClientSecret", client.GetSecret("ClientSecret").Value.Value},
	};

// Update configuration with secrets from Key Vault
foreach (var secret in secrets)
{
	builder.Configuration[secret.Key] = secret.Value;
}

// Add Azure Key Vault as a configuration provider
builder.Configuration.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());
#endregion

#region Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("local");

// Add DbContext and MySQL configuration
if (builder.Environment.IsDevelopment())
{
	// Use MySQL in development
	builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
}
else
{
	// Use PostgreSQL in production
	builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseNpgsql(connectionString));
}
#endregion

#region Configure Identity Options
// Add Identity and configure Identity options
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders();

builder.Services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = builder.Configuration["Redis"];
});

builder.Services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(builder.Configuration["Redis"]));

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
	options.Password.RequiredLength = 3;
	options.Password.RequireDigit = false;
	options.Password.RequireLowercase = false;
	options.Password.RequireUppercase = false;
	options.Password.RequireNonAlphanumeric = false;
	options.SignIn.RequireConfirmedEmail = false;
	options.User.RequireUniqueEmail = true;
});
#endregion

#region Configure Scoped
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IValidator<LocationsRequest>, LocationRequestValidator>();
builder.Services.AddScoped<IValidator<PhotoRequest>, PhotoRequestValidator>();
builder.Services.AddScoped<EmailVerify>();
builder.Services.AddScoped<TokenGenerator>();
builder.Services.AddScoped<ITokenValidator, TokenValidator>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailVerify, EmailVerify>();
#endregion

#region Configure Health Checks For Azure Server
builder.Services.AddHealthChecks()
	.AddCheck("self", () => HealthCheckResult.Healthy());
#endregion

#region JwtBear and Authentication, Swagger API

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add Authentication and JwtBearer
var jwtSettings = builder.Configuration.GetSection("JWT");

builder.Services
	.AddAuthentication(options =>
	{
		// options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(options =>
	{
		options.SaveToken = true;
		options.RequireHttpsMetadata = false;
		options.TokenValidationParameters = new TokenValidationParameters()
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = jwtSettings["ValidIssuer"],
			ValidAudience = jwtSettings["ValidAudience"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"])),
		};
	})
	.AddGoogle(googleOptions =>
	{
		//IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("GoogleAuthSettings:Google");
		//googleOptions.ClientId = googleAuthNSection["ClientId"];
		//googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];
		googleOptions.ClientId = builder.Configuration["GoogleAuthSettings:Google:ClientId"];
		googleOptions.ClientSecret = builder.Configuration["GoogleAuthSettings:Google:ClientSecret"];
	});

builder.Services.Configure<JwtBearerOptions>(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtSettings["ValidIssuer"],
		ValidAudience = jwtSettings["ValidAudience"],
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"])),
	};
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Name = "Authorization",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Description = "Please enter your token with this format: ''Bearer YOUR_TOKEN''",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
		BearerFormat = "JWT",
		Scheme = "bearer"
	});
	options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Name = "Bearer",
				In = ParameterLocation.Header,
				Reference = new OpenApiReference
				{
					Id = "Bearer",
					Type = ReferenceType.SecurityScheme
				}
			},
			new List<string>()
		}
	});
});
#endregion

//builder.Services.AddCors(options =>
//{
//	options.AddDefaultPolicy(
//		builder =>
//		{
//			builder.WithOrigins("https://localhost:4200")
//				   .AllowAnyHeader()
//				   .AllowAnyMethod();
//		});
//});

#region Config Cors
builder.Services.AddCors(p =>
	p.AddPolicy(
		"corspolicy",
		build =>
		{
			build
				.WithOrigins("http://localhost:4200")
				.AllowAnyMethod()
				.AllowAnyHeader();
		}
	)
);
#endregion

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
//{
//	app.UseSwagger();
//	app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "PRN231.ExploreNow.API V1"); });
//}

app.UseRouting();

app.UseCors("corspolicy");

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
	endpoints.MapControllers();
});

// Cấu hình Swagger cho các môi trường
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
	app.UseSwagger();
	app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "PRN231.ExploreNow.API V1"); });
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
	ResponseWriter = async (context, report) =>
	{
		context.Response.ContentType = "application/json";
		var result = JsonSerializer.Serialize(
			new
			{
				status = report.Status.ToString(),
				checks = report.Entries.Select(e => new { key = e.Key, value = e.Value.Status.ToString() })
			});
		await context.Response.WriteAsync(result);
	}
});

//app.MapControllers();

app.Run();
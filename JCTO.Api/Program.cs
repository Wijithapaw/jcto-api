using JCTO.Api.Middlewares;
using JCTO.Data;
using JCTO.Domain;
using JCTO.Domain.ConfigSettings;
using JCTO.Domain.Dtos;
using JCTO.Domain.Services;
using JCTO.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddDbContext<JctoDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJctoDbContext>(s => s.GetRequiredService<JctoDbContext>());

builder.Services.AddControllers(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.Filters.Add(new AuthorizeFilter(policy));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "JCTO API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

//Aws cognito Identity configuration
var cognitoSettings = builder.Configuration.GetSection("AwsCognitoSettings").Get<AwsCognitoSettings>();

var validIssuer = $"https://cognito-idp.{cognitoSettings.Region}.amazonaws.com/{cognitoSettings.UserPoolId}";
var validAudience = cognitoSettings.ClientId;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
   .AddJwtBearer(options =>
   {
       options.MetadataAddress = $"{validIssuer}/.well-known/openid-configuration";
       options.TokenValidationParameters = new TokenValidationParameters
       {
           ValidIssuer = validIssuer,
           ValidateIssuerSigningKey = true,
           ValidateIssuer = true,
           ValidateLifetime = true,
           ValidAudience = validAudience,
           ValidateAudience = true,
           RoleClaimType = "cognito:groups",
           AudienceValidator = (audiences, securityToken, validationParameters) =>
           {
               //This is necessary because Cognito tokens doesn't have "aud" claim. Instead the audience is set in "client_id"
               var castedToken = securityToken as JwtSecurityToken;
               var clientId = castedToken?.Payload["client_id"]?.ToString();
               return validAudience.Equals(clientId);
           }
       };
   });

//Cors configure
var AllowedOrigins = "_allowedOrigins";
var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowedOrigins,
         policy =>
         {
             policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod();
         });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<JctoDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Map("/healthz", () => "ok");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseErrorHandling();

app.UseCors(AllowedOrigins);

app.UseAuthentication();

app.UseAuthorization();

app.UseUserContext();

app.MapControllers();

app.Run();

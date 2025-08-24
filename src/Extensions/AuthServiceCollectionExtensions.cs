using Gay.Silverbranch.Utilities.Security.Constants;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Gay.Silverbranch.Utilities.Security.Extensions;

#pragma warning disable CS1591

public static class AuthServiceCollectionExtensions
{

    // public static IServiceCollection AddSecurity(this IServiceCollection services, ConfigurationManager config)
    // {
    //     var securityKey = config.GetConnectionString("Jwt:Key");
    //     var securityAPIKey = config.GetConnectionString("APIKey");
    //     
    //     var securityIssuer = config["Resume:Jwt:Issuer"];
    //     var securityAudiance = config["Resume:Jwt:Audience"];
    //     var metaAddress = config["Resume:Jwt:MetadataAddress"];
    //
    //     services.AddScoped<APIKeyAuthFilter>();
    //
    //     services.AddAuthentication(x =>
    //         {
    //             x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //             x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //             x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    //         })
    //         .AddJwtBearer(x =>
    //         {
    //             x.RequireHttpsMetadata = false;
    //             x.Audience = securityAudiance;
    //             x.MetadataAddress = metaAddress!;
    //             x.TokenValidationParameters = new TokenValidationParameters
    //             {
    //                 // IssuerSigningKey = new SymmetricSecurityKey(
    //                 //     Encoding.UTF8.GetBytes(securityKey!)),
    //                 ValidateIssuerSigningKey = true,
    //                 ValidateLifetime = true,
    //                 ValidateIssuer = true,
    //                 ValidateAudience = true,
    //                 ValidIssuer = securityIssuer,
    //                 ValidAudience = securityAudiance,
    //             };
    //         });
    //
    //     services.AddAuthorization(x =>
    //     {
    //         // x.AddPolicy(AuthConstants.AdminUserPolicyName, 
    //         //     p => p.RequireClaim(AuthConstants.AdminUserClaimName, "true"));
    //
    //         x.AddPolicy(AuthConstants.AdminUserPolicyName,
    //             p => p.AddRequirements(new AdminAuthRequirement(securityAPIKey)));
    //
    //         x.AddPolicy(AuthConstants.TrustedMemberPolicyName,
    //         p => p.RequireAssertion(c =>
    //                 c.User.HasClaim(m => m is { Type: AuthConstants.AdminUserClaimName, Value: "true" }) ||
    //                 c.User.HasClaim(m => m is { Type: AuthConstants.TrustedMemberClaimName, Value: "true" })));
    //     });
    //     
    //     return services;
    // }
    
    public static IServiceCollection AddKeycloakAuthApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddKeycloakWebApi(configuration,
                opt =>
                {   
                    opt.RequireHttpsMetadata = false;
                    //opt.Audience = configuration["Keycloak:Audience"];
                });
        
        services.AddAuthorization()
            .AddKeycloakAuthorization(opt =>
            {
                opt.EnableRolesMapping = RolesClaimTransformationSource.ResourceAccess;
                opt.RolesResource = configuration["Keycloak:resource"];
            })
            .AddAuthorizationBuilder()
            .AddPolicy(
                AuthConstants.AdminUserPolicyName,
                policy => policy.RequireResourceRoles(AuthConstants.AdminUserClaimName))
            .AddPolicy(
                AuthConstants.TrustedMemberPolicyName,
                policy => policy.RequireResourceRoles(AuthConstants.TrustedMemberClaimName))
            .AddPolicy(
                AuthConstants.MemberPolicyName,
                policy => policy.RequireResourceRoles(AuthConstants.MemberClaimName));
        
        return services;
    }
    
    
    
    private static string _keycloakResource = "";

    public static IServiceCollection AddKeycloakAuthApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var keycloakConfig = configuration.GetSection("Keycloak");
        _keycloakResource = keycloakConfig["resource"];

        services.AddAuthentication(PopulateConfigureOptions())
            //.AddKeycloakWebApp(builder.Configuration);
            .AddKeycloakWebApp(keycloakConfig,
                //configureCookieAuthenticationOptions: opt => { },
                configureOpenIdConnectOptions: opt =>
                {
                    opt.ResponseType = OpenIdConnectResponseType.Code;
                    opt.RequireHttpsMetadata = false;
                    opt.SaveTokens = true;
                });

        services.AddAuthorization()
            .AddKeycloakAuthorization(PopulateGenerateConfigureKeycloakAuthorizationOptions)
            .AddAuthorizationBuilder()
            .AddPolicy(
                AuthConstants.AdminUserPolicyName,
                policy => policy.RequireResourceRoles(AuthConstants.AdminUserClaimName))
            .AddPolicy(
                AuthConstants.TrustedMemberPolicyName,
                policy => policy.RequireResourceRoles(AuthConstants.TrustedMemberClaimName))
            .AddPolicy(
                AuthConstants.MemberPolicyName,
                policy => policy.RequireResourceRoles(AuthConstants.MemberClaimName));
        
        return services;
    }

    private static void PopulateGenerateConfigureKeycloakAuthorizationOptions(KeycloakAuthorizationOptions opt)
    {
        opt.EnableRolesMapping = RolesClaimTransformationSource.ResourceAccess;
        opt.RolesResource = _keycloakResource;
    }

    private static Action<AuthenticationOptions> PopulateConfigureOptions()
    {
        return options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = OpenIdConnectDefaults.AuthenticationScheme;
        };
    }
}

#pragma warning restore CS1591s
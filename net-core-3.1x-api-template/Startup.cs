using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InnovaFunding.AuctionService.AuthorizationPolicies;
using InnovaFunding.AuctionService.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

namespace net_core_3._1x_api_template
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        { 
           
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftWebApi(options =>
                    {
                        Configuration.Bind("AzureAdB2C", options);

                        options.TokenValidationParameters.NameClaimType = "name";
                    },
                    options =>
                    {
                        Configuration.Bind("AzureAdB2C", options);
                    });
            
            
            services.AddControllers();
            
            services.AddAuthorization(options =>
            {
                // Create policy to check for the scope 'read'
                options.AddPolicy("ReadScope",
                    policy => policy.Requirements.Add(new ScopesRequirement("read")));
            });
            
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(typeof(ApiExceptionFilter));
            });
            
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api Name", Version = "v1" });
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri
                            (
                                new StringBuilder().Append(Configuration.GetValue<string>("AzureAdB2C:Instance"))
                                .Append("/")
                                .Append(Configuration.GetValue<string>("AzureAdB2C:Domain"))
                                .Append("/oauth2/v2.0/authorize?p=")
                                .Append(Configuration.GetValue<string>("AzureAdB2C:SignUpSignInPolicyId")).ToString()
                            ),
                            
                            Scopes = new Dictionary<string, string>
                            {
                                { "https://<tenant>.onmicrosoft.com/<apiname>/<scopename>", "Access read operations" },
                            }
                        }
                    }
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                        },
                        new[] { "read"}
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api Name");
                
                c.OAuthClientId("<SwaggerclientId>");//swagger client id
                c.OAuthAppName("swagger - ui");
                c.OAuthScopeSeparator(" ");
                c.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { { "https://<tenant>.onmicrosoft.com/api/<scopename>", "<scopename>" }}); //scopes as url
                c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
            });
            
            app.UseCors(
                options => options.WithOrigins("*").AllowAnyMethod()
            );
        }
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.Util;
using TestIdPCore.Models;
using TestIdPCore.Services;
using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Logging;
using Microsoft.Extensions.Logging;

namespace TestIdPCore
{
    public class Startup
    {
        public static IWebHostEnvironment AppEnvironment { get; private set; }
        private readonly ILogger<Startup> _logger;

        public Startup(IWebHostEnvironment env, IConfiguration configuration, ILogger<Startup> logger)
        {
            AppEnvironment = env;

            Configuration = configuration;
            _logger = logger;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            _logger.LogInformation(">>>>>> ConfigureServices0");

            IdentityModelEventSource.ShowPII = true;
            _logger.LogInformation(">>>>>> ConfigureServices1");

            services.BindConfig<Settings>(Configuration, "Settings");
            _logger.LogInformation(">>>>>> ConfigureServices2");

            services.BindConfig<Saml2Configuration>(Configuration, "Saml2", (serviceProvider, saml2Configuration) =>
            {
                _logger.LogInformation(">>>>>> ConfigureServices3");

                // saml2Configuration.SignAuthnRequest = true;
                saml2Configuration.SigningCertificate = CertificateUtil.Load(AppEnvironment.MapToPhysicalFilePath(Configuration["Saml2:SigningCertificateFile"]), Configuration["Saml2:SigningCertificatePassword"], X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
                _logger.LogInformation(">>>>>> ConfigureServices4");

                if (!saml2Configuration.SigningCertificate.IsValidLocalTime())
                {
                    _logger.LogInformation(">>>>>> ConfigureServices5");
                    throw new Exception("The IdP signing certificates has expired.");
                }
                _logger.LogInformation(">>>>>> ConfigureServices6");

                saml2Configuration.AllowedAudienceUris.Add(saml2Configuration.Issuer);
                _logger.LogInformation(">>>>>> ConfigureServices7");

                return saml2Configuration;
            });

            services.AddSaml2();
            _logger.LogInformation(">>>>>> ConfigureServices8");

            services.AddHttpClient();
            _logger.LogInformation(">>>>>> ConfigureServices9");

            services.AddScoped<IUserService, UserService>();
            _logger.LogInformation(">>>>>> ConfigureServices10");

            services.AddControllersWithViews();
            _logger.LogInformation(">>>>>> ConfigureServices11");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSaml2();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

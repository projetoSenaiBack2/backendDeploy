using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Patrimonio.Contexts;
using Patrimonio.Interfaces;
using Patrimonio.Repositories;
using System;
using System.IO;
using System.Reflection;


namespace Patrimonio
{
    public class Startup
    {   

        public Startup(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            

            services
              .AddControllers()
              .AddNewtonsoftJson(options => {
                  options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                  options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
              });

            services.AddCors(options =>
            {
                options.AddPolicy("CorPolicy",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });



            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Patrimonio.webAPI", Version = "v1" });
            });

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "JwtBearer";
                    options.DefaultChallengeScheme = "JwtBearer";
                })

                .AddJwtBearer("JwtBearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("patrimonio-chave-autenticacao")),
                        ClockSkew = TimeSpan.FromMinutes(30),
                        ValidIssuer = "patrimonio.webAPI",
                        ValidAudience = "patrimonio.webAPI"
                    };
                });


            services.AddDbContext<PatrimonioContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("Default"))
            );


            services.AddTransient<DbContext, PatrimonioContext>();
            services.AddTransient<IUsuarioRepository, UsuarioRepository>();
            services.AddTransient<IEquipamentoRepository, EquipamentoRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Patrimonio.webAPI");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseCors("CorPolicy");

            app.UseStaticFiles(); // For the wwwroot folder

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles/Images")),
                RequestPath = "/img"
            });


            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

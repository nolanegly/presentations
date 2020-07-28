using AutoMapper;
using FluentValidation.AspNetCore;
using HtmlTags;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VolunteerConvergence.Infrastructure;
using VolunteerConvergence.Infrastructure.MediatrBehaviors;
using VolunteerConvergence.Infrastructure.Tags;
using VolunteerConvergence.Infrastructure.Tags.SelectBuilders;
using VolunteerConvergence.Models;

namespace VolunteerConvergence
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<VolConContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddMediatR(typeof(Startup));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

            services.AddAutoMapper(typeof(IDomainEntity));

            services.AddRazorPages(rpo =>
                {
                    rpo.RootDirectory = "/Features";
                    rpo.Conventions.ConfigureFilter(new ValidatorPageFilter());
                    rpo.Conventions.ConfigureFilter(new ExceptionPageFilter());
                })
                .AddFluentValidation(cfg =>
                {
                    cfg.RegisterValidatorsFromAssemblyContaining<Startup>();
                    cfg.RunDefaultMvcValidationAfterFluentValidationExecutes = true; // true by default
                });

            services.AddHtmlTags(new TagConventions());

            services.AddMvc(opt => 
                opt.ModelBinderProviders.Insert(0, new EntityModelBinderProvider()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}

using Application.Activities;
using Application.Core;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Interfaces;
using Infrastructure.Security;
using Infrastructure.Photos;

namespace API.Extensions;
public static class ApplicationsServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config){
        services.AddDbContext<DataContext>(opt => {
            //opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
            //opt.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            opt.UseSqlServer(config.GetConnectionString("DefaultConnection"));
        });

        services.AddCors(opt => {
            opt.AddPolicy("CorsPolicy", policy => {
                policy.AllowAnyMethod().AllowAnyHeader().AllowCredentials().WithOrigins("http://localhost:3000");
            });
        });

        //services.AddMediatR(typeof(List.Handler).Assembly);
        //services.AddAutoMapper(typeof(MappingProfiles).Assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(List.Handler).Assembly);
        });

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MappingProfiles>();
        });

        services.AddScoped<IUserAccessor, UserAccessor>();
        services.AddScoped<IPhotoAccessor, PhotoAccessor>();

        services.Configure<CloudinarySettings>(config.GetSection("Cloudinary"));
        services.AddSignalR();

        return services;
    }
}
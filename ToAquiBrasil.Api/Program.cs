using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ToAquiBrasil.Api.Configuration.Models;
using ToAquiBrasil.Api.Extensions;
using ToAquiBrasil.Api.Mappers;
using ToAquiBrasil.Core.Mappers;
using ToAquiBrasil.Core.Queries;
using ToAquiBrasil.Core.Queries.Abstractions;
using ToAquiBrasil.Core.Services;
using ToAquiBrasil.Core.Services.Abstractions;
using ToAquiBrasil.Data;

namespace ToAquiBrasil.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        if (builder.Environment.IsDevelopment())
        {
            //builder.Configuration.AddUserSecrets<Program>();
        }

        // Add services to the container.

        builder.Services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
        
        // Add response caching
        builder.Services.AddResponseCaching();
        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        var settings = builder.Configuration.Get<ServiceConfig>();
        builder.Services.AddSingleton(settings!);
        builder.Services.AddSingleton<IIpLocationServiceConfig>(settings.BigDataCloudApi);
        
        // Register mapper services
        builder.Services.AddSingleton<ListingMapper>();
        builder.Services.AddSingleton<ListingEntityMapper>();
        builder.Services.AddSingleton<CitiesMapper>();
        builder.Services.AddSingleton<IpLocationMapper>();
        builder.Services.AddSingleton<CityMapper>();
        builder.Services.AddSingleton<LocationMapper>();
        
        builder.Services.AddHttpClient<ISearchCityService, SearchCityService>(cfg =>
        {
            cfg.BaseAddress = new Uri(settings.CountriesNowApi.BaseUrl);
        });
        
        builder.Services.AddHttpClient<IIpLocationService, IpLocationService>(cfg =>
        {
            cfg.BaseAddress = new Uri(settings.BigDataCloudApi.BaseUrl);
        });

        // Add Nominatim geocoding service
        builder.Services.AddHttpClient<IGeocodingService, NominatimGeocodingService>();

        builder.Services.AddCors(cfg => cfg.AddDefaultPolicy(cfg => cfg.AllowAnyOrigin()));
        var connectionString = builder.Configuration.GetConnectionString("DBConnectionString");

        builder.Services.AddHealthChecks().AddSqlServer(connectionString!);
        builder.Services.AddDbContext<ToAquiBrasilDbContext>(sqlOptions =>
            {
                sqlOptions.UseSqlServer(connectionString!,
                    opts =>
                    {
                        opts.EnableRetryOnFailure(5);
                        opts.UseNetTopologySuite();
                    });
                sqlOptions.EnableDetailedErrors(builder.Environment.IsDevelopment());
                sqlOptions.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
            }
        );
        builder.Services.AddTransient<IListingQueries, ListingQueries>();
        builder.Services.AddTransient<ILayoutQueries, LayoutQueries>();
        builder.Services.AddTransient<IRadiusConverterService, RadiusConverterService>();
        builder.Services.AddTransient<IPointFabric, PointFabric>();
        builder.Services.AddTransient<IOpeningHoursService, OpeningHoursService>();

        // Add OpenStreetMap service
        builder.Services.AddHttpClient<IGeoReverseService, OpenStreetMapService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Add global exception handler
        app.UseGlobalExceptionHandler();

        app.UseHttpsRedirection();

        // Add response caching middleware
        app.UseResponseCaching();

        app.UseAuthorization();

        app.UseCors();

        app.MapControllers();

        app.UseHealthChecks("/health");

        app.Run();
    }
}
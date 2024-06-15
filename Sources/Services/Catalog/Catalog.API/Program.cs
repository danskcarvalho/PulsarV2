using Pulsar.Services.Shared.API.Utils;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddCors(options =>
{
    // this defines a CORS policy called "default"
    options.AddPolicy("CorsDefaultPolicy", policy =>
    {
        HashSet<string> origins = new HashSet<string>();
        origins.UnionWith(builder.Configuration.GetSection("AllowedCorsOrigins").GetChildren().Select(c => c.Value).Where(c => c != null).ToList()!);
        foreach (var child in builder.Configuration.GetSection("IdentityServer:Clients").GetChildren())
        {
            origins.UnionWith(child.GetSection("AllowedCorsOrigins").GetChildren().Select(c => c.Value).Where(c => c != null).ToList()!);
        }

        policy.WithOrigins(origins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers().AddJsonOptions(j =>
{
    j.JsonSerializerOptions.Converters.Add(new AssertDateTimeConverter());
    j.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

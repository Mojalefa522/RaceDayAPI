using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using RaceDayAPI.Data;
using RaceDayAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<PasswordService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDayAPI",
        Version = "v1",
        Description = "API for managing events, categories, enrolments, and results."
    });

    c.AddSecurityDefinition("CookieAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = ".AspNetCore.Session",
        Description = "Paste the .AspNetCore.Session cookie value here."
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("CookieAuth", document)] = []
    });
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
    db.Database.Migrate();

    if (!db.Roles.Any())
    {
        db.Roles.AddRange(
            new RaceDayAPI.Models.Role { RoleName = "Organiser", Description = "Can create and manage events" },
            new RaceDayAPI.Models.Role { RoleName = "Participant", Description = "Can enrol in events and view results" }
        );
        db.SaveChanges();
    }
}

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using System.Linq;

namespace RaceDayAPI.Tests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
                    db.Database.EnsureCreated();

                    if (!db.Roles.Any())
                    {
                        db.Roles.Add(new Role
                        {
                            RoleName = "Organiser",
                            Description = "Creates and manages events"
                        });
                        db.Roles.Add(new Role
                        {
                            RoleName = "Participant",
                            Description = "Enrols in events and views results"
                        });
                        db.SaveChanges();
                    }
                }
            });
        }
    }
}

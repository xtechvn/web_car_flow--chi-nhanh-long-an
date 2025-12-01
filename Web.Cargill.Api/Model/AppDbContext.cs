using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Web.Cargill.Api.Model
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<VehicleAudio> VehicleAudio { get; set; }
        public DbSet<VehicleInspection> VehicleInspection { get; set; }

    }
}

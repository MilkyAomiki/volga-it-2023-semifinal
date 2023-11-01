using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Simbir.GO.BLL.Models;

namespace Simbir.GO.BLL;

public class SimbirGoDbContext : IdentityDbContext
{
    public SimbirGoDbContext(DbContextOptions<SimbirGoDbContext> options) : base(options)
    {
    }

    public DbSet<Transport> Transports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}


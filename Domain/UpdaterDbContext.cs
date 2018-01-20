
using Microsoft.EntityFrameworkCore;

namespace Updater.Domain
{
    public class UpdaterDbContext : DbContext
    {
        public UpdaterDbContext(DbContextOptions<UpdaterDbContext> options) : base(options)
        {
        }

        public DbSet<ImageEvent> EventHistory { get; set; }
    }
}
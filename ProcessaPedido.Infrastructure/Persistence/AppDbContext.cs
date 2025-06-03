using Microsoft.EntityFrameworkCore;
using ProcessaPedido.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ProcessaPedido.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<Entrega> Entregas { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entrega>().OwnsOne(e => e.Destinatario);
            modelBuilder.Entity<Entrega>().HasMany(e => e.Itens).WithOne().OnDelete(DeleteBehavior.Cascade);
        }
    }
}

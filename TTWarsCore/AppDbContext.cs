﻿using Microsoft.EntityFrameworkCore;
using TTWarsCore.Models;

namespace TTWarsCore
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Accounts");

                entity.HasKey(e => e.Id)
                    .HasName("PK_ACCOUNTS");

                entity.HasIndex(e => new { e.Username, e.Server })
                    .IsUnique()
                    .HasDatabaseName("INDEX_ACCOUNTS");
            });

            modelBuilder.Entity<Access>(entity =>
            {
                entity.ToTable("Access");

                entity.HasKey(e => e.Id)
                    .HasName("PK_ACCESSES");
            });
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Access> Accesses { get; set; }
    }
}
﻿using Microsoft.EntityFrameworkCore;
using System.IO;
using TbsCore.Helpers;
using TbsCore.Models.Database;

namespace TbsCore.Database
{
    public class TbsContext : DbContext
    {
        public TbsContext()
        {
            if (!Directory.Exists(IoHelperCore.TbsPath)) Directory.CreateDirectory(IoHelperCore.TbsPath);
            Database.EnsureCreated();
        }

        public TbsContext(DbContextOptions<TbsContext> options) : base(options)
        {
        }

        public DbSet<DbAccount> DbAccount { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={@IoHelperCore.SqlitePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbAccount>().ToTable("DbAccount");
        }
    }
}
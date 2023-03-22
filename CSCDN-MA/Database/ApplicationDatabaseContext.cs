using CSCDNMA.Model;
using CzomPack.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CSCDNMA.Database;

public class ApplicationDatabaseContext : DbContext
{
    public ApplicationDatabaseContext([NotNull] DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //if (_connectionString is not null) optionsBuilder.UseSqlServer(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //try
        //{
        //    if (File.Exists(Globals.ProductsFile))
        //    {
        //        //var prods = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Globals.ProductsFile)).Select(itm => new Product { Id = Guid.Parse(itm.Key), Name = itm.Value }).ToList();
        //        var prods = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Globals.ProductsFile)).Select(itm => new Product { Id = itm.Key, Name = itm.Value }).ToList();
        //        Products.AddRange(prods);
        //        this.SaveChanges();
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Logger.Error<ApplicationDatabaseContext>($"{ex}");
        //}
        //modelBuilder.Entity<Product>().Property(e => e.Id).HasConversion(to => to.ToString("N"), from => Guid.ParseExact(from, "N"));
        //modelBuilder.Entity<AssetConfigItem>().Property(e => e.ProductId).HasConversion(to => to.ToString("N"), from => Guid.ParseExact(from, "N"));
    }

    public DbSet<Product> Products { get; set; }
    
	public DbSet<AssetConfigItem> AccessConfig { get; set; }
}

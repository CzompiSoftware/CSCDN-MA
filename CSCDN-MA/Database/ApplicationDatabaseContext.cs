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
    public string ConnectionString { get; } = null;

    public ApplicationDatabaseContext([NotNull] DbContextOptions options, string connectionString) : base(options)
    {
        ConnectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (ConnectionString is not null) optionsBuilder.UseSqlServer(ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		try
		{
			if (File.Exists(Globals.ProductsFile))
			{
				var prods = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Globals.ProductsFile)).Select(itm => new Product { Id = Guid.Parse(itm.Key), Name = itm.Value }).ToList();
				Products.AddRange(prods);
				this.SaveChanges();
			}
		}
		catch (Exception ex)
		{
			Logger.Error($"{ex}");
		}
		modelBuilder.Entity<Product>().Property(e => e.Id).HasConversion(to => to.ToString(), from => Guid.Parse(from));
        modelBuilder.Entity<AssetConfigItem>().Property(e => e.ProductId).HasConversion(to => to.ToString(), from => Guid.Parse(from));
    }

    public DbSet<Product> Products { get; set; }
    
	public DbSet<AssetConfigItem> AssetConfig { get; set; }
}

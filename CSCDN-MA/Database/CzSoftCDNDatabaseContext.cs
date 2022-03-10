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

public class CzSoftCDNDatabaseContext : DbContext
{
    public string ConnectionString { get; } = null;

    public CzSoftCDNDatabaseContext(string connectionString) : base()
    {
        ConnectionString = connectionString;
    }
    public CzSoftCDNDatabaseContext([NotNull] DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (ConnectionString is not null) optionsBuilder.UseSqlServer(ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<AccessConfigItem> AccessConfig { get; set; }

}

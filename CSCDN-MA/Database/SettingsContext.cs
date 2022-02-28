using CSCDNMA.Controllers;
using CSCDNMA.Model;
using Microsoft.EntityFrameworkCore;
using System;

namespace CSCDNMA.Database;

public class SettingsContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<WhitelistItem> Whitelist { get; set; }


    public SettingsContext()
    {
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={Globals.DataSourceLocation}");
}

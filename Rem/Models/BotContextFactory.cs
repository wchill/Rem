using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Rem.Models
{
    public class BotContextFactory : IDesignTimeDbContextFactory<BotContext>
    {
        public BotContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BotContext>();
            var dbConnectionStr = Environment.GetEnvironmentVariable("REM_DB_CONNECTION_STR");
            if (dbConnectionStr == null)
            {
                throw new ArgumentException("Need to set REM_DB_CONNECTION_STR");
            }
            optionsBuilder.UseSqlite(dbConnectionStr);

            return new BotContext(optionsBuilder.Options);
        }
    }
}
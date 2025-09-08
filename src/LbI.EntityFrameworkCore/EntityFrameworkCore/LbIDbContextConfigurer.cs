using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace LbI.EntityFrameworkCore;

public static class LbIDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<LbIDbContext> builder, string connectionString)
    {
        builder.UseSqlServer(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<LbIDbContext> builder, DbConnection connection)
    {
        builder.UseSqlServer(connection);
    }
}

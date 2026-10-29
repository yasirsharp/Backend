using DataAccess.Concrete.EntityFramework;
using Microsoft.EntityFrameworkCore.Design;

namespace DataAccess.MySql
{
    /// <summary>
    /// MySQL migration'ları için "dotnet ef" fabrikası. Gerçek bir sunucuya bağlanmadan migration ve SQL betiği
    /// üretebilmek için sürüm sabit verilir (MySQL 8.0; üretilen şema MariaDB 10.5+ ile de uyumludur).
    /// </summary>
    public class MySqlDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DuzceUniversiteContext>
    {
        public DuzceUniversiteContext CreateDbContext(string[] args)
        {
            DuzceUniversiteContext.Provider = DatabaseProvider.MySql;
            DuzceUniversiteContext.MySqlServerVersion = "8.0.36-mysql";
            DuzceUniversiteContext.ConnectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Server=localhost;Port=3306;Database=sinav_takvimi;User=root;Password=;";
            return new DuzceUniversiteContext();
        }
    }
}

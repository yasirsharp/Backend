using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataAccess.Concrete.EntityFramework
{
    /// <summary>
    /// "dotnet ef" komutları (migration oluşturma, SQL betiği üretme) için kullanılır.
    /// Bağlantı dizesi ConnectionStrings__DefaultConnection ortam değişkeninden alınır; betik üretmek için
    /// gerçek bir veritabanına bağlanmak gerekmez.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DuzceUniversiteContext>
    {
        public DuzceUniversiteContext CreateDbContext(string[] args)
        {
            DuzceUniversiteContext.ConnectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Server=localhost;Database=DuzceUniversitesiSinavTakvimi;Trusted_Connection=True;TrustServerCertificate=True";
            return new DuzceUniversiteContext();
        }
    }
}

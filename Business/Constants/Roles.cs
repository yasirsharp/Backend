namespace Business.Constants
{
    /// <summary>
    /// OperationClaims tablosundaki rol adları ve [Authorize(Roles = ...)] için hazır rol grupları.
    /// Bir endpoint'in hangi rollere açık olduğunu değiştirmek için sadece buradaki grupları düzenlemek yeterlidir.
    /// </summary>
    public static class Roles
    {
        public const string SuperAdmin = "super.admin";
        public const string Admin = "Admin";
        public const string BolumGorevlisi = "bolum.gorevlisi";
        public const string GorevliPersonel = "gorevli.personel";
        public const string AkademikPersonel = "akademik.personel";
        public const string Ogrenci = "ogrenci";

        /// <summary>Kullanıcı, yetki, bölüm ve personel yönetimi.</summary>
        public const string Yonetici = SuperAdmin + "," + Admin;

        /// <summary>Ders, derslik, DBAP ve sınav planlaması yapabilen roller.</summary>
        public const string Planlayici = Yonetici + "," + BolumGorevlisi;

        /// <summary>Öğrenci olmayan tüm personel rolleri.</summary>
        public const string Personel = Planlayici + "," + GorevliPersonel + "," + AkademikPersonel;
    }
}

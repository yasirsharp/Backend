namespace Business.Constants
{
    /// <summary>
    /// Duyuru hedef kitleleri. Kullanıcının kitleleri tüm rollerinden hesaplanır
    /// (ör. bölüm görevlisi hem "personel" hem kendi bölümünün duyurularını görür).
    /// </summary>
    public static class AnnouncementAudiences
    {
        public const string All = "all";
        public const string Ogrenci = "ogrenci";
        public const string Personel = "personel";
        public const string Yonetici = "yonetici";

        public static readonly string[] Values = { All, Ogrenci, Personel, Yonetici };
    }

    public static class AnnouncementTypes
    {
        public static readonly string[] Values = { "general", "urgent", "maintenance", "event" };
    }
}

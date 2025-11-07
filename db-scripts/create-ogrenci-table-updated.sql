-- =============================================
-- Öğrenci Tablosu Oluşturma Script'i (GÜNCELLENMIŞ)
-- Tarih: 4 Kasım 2025
-- Açıklama: Öğrenci bilgilerini tutmak için ayrı tablo
--           ✅ IEntity Standardı: UpdatedDate ve Status alanları
-- =============================================

USE [DuzceUniversiteSinavTakvimi]
GO

-- =============================================
-- 1. Önce mevcut tabloyu SIL (DROP)
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Ogrenci]') AND type in (N'U'))
BEGIN
    PRINT '⚠️  Ogrenci tablosu zaten mevcut, siliniyor...'
    DROP TABLE [dbo].[Ogrenci]
    PRINT '✅ Ogrenci tablosu silindi.'
END
GO

-- =============================================
-- 2. Yeni Ogrenci tablosunu oluştur
-- =============================================
CREATE TABLE [dbo].[Ogrenci](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [OgrenciNo] [nvarchar](50) NOT NULL,
    [Ad] [nvarchar](100) NOT NULL,
    [Soyad] [nvarchar](100) NOT NULL,
    [BolumId] [int] NOT NULL,
    [Sinif] [int] NULL, -- 1, 2, 3, 4 (Opsiyonel)
    [CreatedDate] [datetime] NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] [datetime] NULL, -- ✅ IEntity standardı (ModifiedDate → UpdatedDate)
    [Status] [bit] NOT NULL DEFAULT 1, -- ✅ IEntity standardı (1: Aktif, 0: Pasif)
    
    CONSTRAINT [PK_Ogrenci] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Ogrenci_User] FOREIGN KEY([UserId]) 
        REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Ogrenci_Bolum] FOREIGN KEY([BolumId]) 
        REFERENCES [dbo].[Bolum]([Id]),
    CONSTRAINT [UQ_Ogrenci_UserId] UNIQUE([UserId]), -- 1-to-1 relationship
    CONSTRAINT [UQ_Ogrenci_OgrenciNo] UNIQUE([OgrenciNo]) -- Her öğrenci no unique
)
PRINT '✅ Ogrenci tablosu başarıyla oluşturuldu (UpdatedDate + Status ile).'
GO

-- =============================================
-- 3. Index'ler ekle (Performance için)
-- =============================================
CREATE NONCLUSTERED INDEX [IX_Ogrenci_UserId] ON [dbo].[Ogrenci]([UserId])
PRINT '✅ Ogrenci.UserId index oluşturuldu.'
GO

CREATE NONCLUSTERED INDEX [IX_Ogrenci_BolumId] ON [dbo].[Ogrenci]([BolumId])
PRINT '✅ Ogrenci.BolumId index oluşturuldu.'
GO

CREATE NONCLUSTERED INDEX [IX_Ogrenci_OgrenciNo] ON [dbo].[Ogrenci]([OgrenciNo])
PRINT '✅ Ogrenci.OgrenciNo index oluşturuldu.'
GO

-- =============================================
-- 4. Doğrulama (Verification)
-- =============================================
PRINT ''
PRINT '🔍 Doğrulama Sorguları:'
PRINT '---------------------------------------------'

-- Tablo var mı?
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Ogrenci]') AND type in (N'U'))
    PRINT '✅ Ogrenci tablosu mevcut.'
ELSE
    PRINT '❌ Ogrenci tablosu bulunamadı!'

-- Kolonlar doğru mu?
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Ogrenci'
ORDER BY ORDINAL_POSITION

PRINT ''
PRINT '✅ Script başarıyla tamamlandı!'
PRINT ''
PRINT '📝 Sonraki Adımlar:'
PRINT '   1. Backend/Entity/Concrete/Ogrenci.cs dosyasını kontrol et'
PRINT '   2. UpdatedDate ve Status alanlarının doğru olduğundan emin ol'
PRINT '   3. Backend projesini build et: dotnet build'
PRINT '   4. Frontend servislerini test et'
GO

-- =============================================
-- 5. Test Data (İsteğe bağlı - yorumdan çıkar)
-- =============================================
/*
-- Test amaçlı örnek öğrenci kaydı
INSERT INTO [dbo].[Ogrenci] (UserId, OgrenciNo, Ad, Soyad, BolumId, Sinif, Status)
VALUES 
    (1, '2021001001', 'Ahmet', 'Yılmaz', 1, 3, 1),
    (2, '2021001002', 'Ayşe', 'Demir', 1, 2, 1),
    (3, '2021002001', 'Mehmet', 'Kaya', 2, 4, 1)

SELECT * FROM [dbo].[Ogrenci]
*/

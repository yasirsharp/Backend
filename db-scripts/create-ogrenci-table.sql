-- =============================================
-- Öğrenci Tablosu Oluşturma Script'i
-- Tarih: 4 Kasım 2025
-- Açıklama: Öğrenci bilgilerini tutmak için ayrı tablo
--           AkademikPersonel pattern'i ile tutarlı
-- =============================================

USE [DuzceUniversiteSinavTakvimi]
GO

-- =============================================
-- 1. Önce Ogrenci tablosunu oluştur
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Ogrenci]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Ogrenci](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [OgrenciNo] [nvarchar](50) NOT NULL,
        [Ad] [nvarchar](100) NOT NULL,
        [Soyad] [nvarchar](100) NOT NULL,
        [BolumId] [int] NOT NULL,
        [Sinif] [int] NULL, -- 1, 2, 3, 4 (Opsiyonel)
        [CreatedDate] [datetime] NOT NULL DEFAULT GETDATE(),
        [ModifiedDate] [datetime] NULL,
        
        CONSTRAINT [PK_Ogrenci] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Ogrenci_User] FOREIGN KEY([UserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Ogrenci_Bolum] FOREIGN KEY([BolumId]) 
            REFERENCES [dbo].[Bolum]([Id]),
        CONSTRAINT [UQ_Ogrenci_UserId] UNIQUE([UserId]), -- 1-to-1 relationship
        CONSTRAINT [UQ_Ogrenci_OgrenciNo] UNIQUE([OgrenciNo]) -- Her öğrenci no unique
    )
    PRINT '✅ Ogrenci tablosu oluşturuldu.'
END
ELSE
BEGIN
    PRINT '⚠️ Ogrenci tablosu zaten mevcut.'
END
GO

-- =============================================
-- 2. Index'ler ekle (Performance için)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ogrenci_UserId' AND object_id = OBJECT_ID('Ogrenci'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Ogrenci_UserId] ON [dbo].[Ogrenci]([UserId])
    PRINT '✅ Ogrenci.UserId index oluşturuldu.'
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ogrenci_BolumId' AND object_id = OBJECT_ID('Ogrenci'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Ogrenci_BolumId] ON [dbo].[Ogrenci]([BolumId])
    PRINT '✅ Ogrenci.BolumId index oluşturuldu.'
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ogrenci_OgrenciNo' AND object_id = OBJECT_ID('Ogrenci'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Ogrenci_OgrenciNo] ON [dbo].[Ogrenci]([OgrenciNo])
    PRINT '✅ Ogrenci.OgrenciNo index oluşturuldu.'
END
GO

-- =============================================
-- 3. Test Data (Opsiyonel - Sadece test için)
-- =============================================
-- İhtiyacın varsa bu kısmı uncomment et ve düzenle

/*
-- Örnek: Mevcut bir User'a Ogrenci rolü ekle
-- Önce User'ın Id'sini bul, sonra Ogrenci kaydı oluştur

DECLARE @UserId INT = 123 -- Değiştir!
DECLARE @BolumId INT = 1  -- Bilgisayar Mühendisliği gibi

IF EXISTS (SELECT 1 FROM Users WHERE Id = @UserId)
BEGIN
    INSERT INTO Ogrenci (UserId, OgrenciNo, Ad, Soyad, BolumId, Sinif)
    SELECT 
        @UserId,
        '2021123456', -- OgrenciNo
        U.Ad,
        U.Soyad,
        @BolumId,
        3 -- 3. sınıf
    FROM Users U
    WHERE U.Id = @UserId
    AND NOT EXISTS (SELECT 1 FROM Ogrenci WHERE UserId = @UserId)
    
    PRINT '✅ Test öğrenci kaydı oluşturuldu.'
END
ELSE
BEGIN
    PRINT '⚠️ Belirtilen UserId bulunamadı.'
END
*/

-- =============================================
-- 4. Verification (Kontrol)
-- =============================================
SELECT 
    'Ogrenci' as TableName,
    COUNT(*) as RecordCount
FROM Ogrenci

PRINT ''
PRINT '========================================='
PRINT '✅ Ogrenci tablosu başarıyla oluşturuldu!'
PRINT '========================================='
PRINT ''
PRINT '📋 Sonraki Adımlar:'
PRINT '1. Backend Entity class (Ogrenci.cs) oluştur'
PRINT '2. Repository pattern (IOgrenciDal, EfOgrenciDal)'
PRINT '3. Service layer (IOgrenciService, OgrenciManager)'
PRINT '4. Controller (OgrenciController)'
PRINT '5. Token generation güncelle (BolumId ekle)'
PRINT ''
GO

/*
 * Script: Bildirim, Duyuru ve Popup Sistemi
 * Açıklama: Bildirim, duyuru ve popup tabloları oluşturur
 * Tarih: 25 Ekim 2025 (Güncellendi)
 * 
 * Yetkilendirme:
 * - Admin: Herkese bildirim/duyuru gönderebilir
 * - Görevli Personel: Sadece kendi bölümüne bildirim gönderebilir
 * 
 * Kullanım:
 * 1. SQL Server Management Studio'da bu dosyayı aç
 * 2. İlk satırda database adını kontrol et
 * 3. F5 ile çalıştır
 */

USE [DuzceUniversiteSinavTakvimi]
GO

PRINT '==================================================================='
PRINT 'Bildirim Sistemi Kurulumu Başlatılıyor...'
PRINT '==================================================================='
PRINT ''

-- ===================================================================
-- 1. Bildirimler (Notifications) Tablosu
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('Notifications') AND type = 'U')
BEGIN
    PRINT '1/3 - Notifications tablosu oluşturuluyor...'
    CREATE TABLE [Notifications] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] INT NOT NULL, -- Hangi kullanıcıya
        [Title] NVARCHAR(200) NOT NULL, -- Bildirim başlığı
        [Message] NVARCHAR(1000) NOT NULL, -- Bildirim içeriği
        [Type] NVARCHAR(50) NOT NULL, -- info, success, warning, error
        [IsRead] BIT NOT NULL DEFAULT 0, -- Okundu mu?
        [ActionUrl] NVARCHAR(500) NULL, -- Tıklanınca gidilecek sayfa (opsiyonel)
        [RelatedEntityType] NVARCHAR(100) NULL, -- SinavDetay, Ders, Bolum, vb.
        [RelatedEntityId] INT NULL, -- İlgili entity'nin ID'si
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [ReadDate] DATETIME NULL, -- Okunma tarihi
        [Status] BIT NOT NULL DEFAULT 1,
        
        CONSTRAINT FK_Notifications_Users FOREIGN KEY ([UserId]) 
            REFERENCES [Users]([Id]) ON DELETE CASCADE
    );
    
    -- Indexler
    CREATE INDEX IX_Notifications_UserId ON [Notifications]([UserId]);
    CREATE INDEX IX_Notifications_IsRead ON [Notifications]([IsRead]);
    CREATE INDEX IX_Notifications_CreatedDate ON [Notifications]([CreatedDate]);
    
    PRINT '✓ Notifications tablosu oluşturuldu'
    PRINT '  - UserId: Bildirimin gönderildiği kullanıcı'
    PRINT '  - Type: info/success/warning/error'
    PRINT '  - IsRead: Okundu/Okunmadı durumu'
    PRINT '  - ActionUrl: Tıklanınca gidilecek link'
END
ELSE
BEGIN
    PRINT '1/3 - Notifications tablosu zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- 2. Duyurular (Announcements) Tablosu
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('Announcements') AND type = 'U')
BEGIN
    PRINT '2/3 - Announcements tablosu oluşturuluyor...'
    CREATE TABLE [Announcements] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Title] NVARCHAR(200) NOT NULL, -- Duyuru başlığı
        [Content] NVARCHAR(MAX) NOT NULL, -- Duyuru içeriği (HTML destekli)
        [Type] NVARCHAR(50) NOT NULL, -- general, urgent, maintenance, event
        [Priority] INT NOT NULL DEFAULT 0, -- 0: Normal, 1: Önemli, 2: Acil
        [TargetAudience] NVARCHAR(100) NOT NULL, -- all, admin, gorevli.personel, personel, ogrenci
        [TargetBolumId] INT NULL, -- 🆕 Belirli bir bölüme özel (NULL = tüm bölümler)
        [PublishDate] DATETIME NOT NULL DEFAULT GETDATE(), -- Yayınlanma tarihi
        [ExpiryDate] DATETIME NULL, -- Bitiş tarihi (opsiyonel)
        [IsActive] BIT NOT NULL DEFAULT 1, -- Aktif/Pasif
        [ShowAsPopup] BIT NOT NULL DEFAULT 0, -- Popup olarak göster
        [CreatedBy] INT NOT NULL, -- Oluşturan kullanıcı
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [UpdatedDate] DATETIME NULL,
        [Status] BIT NOT NULL DEFAULT 1,
        
        CONSTRAINT FK_Announcements_Users FOREIGN KEY ([CreatedBy]) 
            REFERENCES [Users]([Id]),
        CONSTRAINT FK_Announcements_Bolum FOREIGN KEY ([TargetBolumId]) -- 🆕 Bölüm FK
            REFERENCES [Bolum]([Id])
    );
    
    -- Indexler
    CREATE INDEX IX_Announcements_IsActive ON [Announcements]([IsActive]);
    CREATE INDEX IX_Announcements_PublishDate ON [Announcements]([PublishDate]);
    CREATE INDEX IX_Announcements_TargetAudience ON [Announcements]([TargetAudience]);
    CREATE INDEX IX_Announcements_TargetBolumId ON [Announcements]([TargetBolumId]); -- 🆕 Bölüm index
    
    PRINT '✓ Announcements tablosu oluşturuldu'
    PRINT '  - Type: general/urgent/maintenance/event'
    PRINT '  - Priority: 0=Normal, 1=Önemli, 2=Acil'
    PRINT '  - TargetAudience: Hedef kitle (rol bazlı)'
    PRINT '  - TargetBolumId: Belirli bölüme özel (NULL = tüm bölümler)' -- 🆕
    PRINT '  - ShowAsPopup: Popup olarak gösterilsin mi?'
END
ELSE
BEGIN
    PRINT '2/3 - Announcements tablosu zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- 3. Duyuru Okuma Kayıtları (AnnouncementReadStatus)
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('AnnouncementReadStatus') AND type = 'U')
BEGIN
    PRINT '3/3 - AnnouncementReadStatus tablosu oluşturuluyor...'
    CREATE TABLE [AnnouncementReadStatus] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [AnnouncementId] INT NOT NULL,
        [UserId] INT NOT NULL,
        [ReadDate] DATETIME NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT FK_AnnouncementReadStatus_Announcements FOREIGN KEY ([AnnouncementId]) 
            REFERENCES [Announcements]([Id]) ON DELETE CASCADE,
        CONSTRAINT FK_AnnouncementReadStatus_Users FOREIGN KEY ([UserId]) 
            REFERENCES [Users]([Id]) ON DELETE CASCADE,
        CONSTRAINT UQ_AnnouncementReadStatus UNIQUE ([AnnouncementId], [UserId])
    );
    
    -- Indexler
    CREATE INDEX IX_AnnouncementReadStatus_AnnouncementId ON [AnnouncementReadStatus]([AnnouncementId]);
    CREATE INDEX IX_AnnouncementReadStatus_UserId ON [AnnouncementReadStatus]([UserId]);
    
    PRINT '✓ AnnouncementReadStatus tablosu oluşturuldu'
    PRINT '  - Hangi kullanıcı hangi duyuruyu okudu?'
    PRINT '  - UNIQUE constraint ile tekrar kayıt engellendi'
END
ELSE
BEGIN
    PRINT '3/3 - AnnouncementReadStatus tablosu zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- Örnek Veriler (Opsiyonel - Test için)
-- ===================================================================
PRINT 'Örnek veriler ekleniyor...'
PRINT ''

-- Örnek Duyuru
IF NOT EXISTS (SELECT * FROM Announcements WHERE Title = 'Hoş Geldiniz!')
BEGIN
    INSERT INTO Announcements (Title, Content, Type, Priority, TargetAudience, ShowAsPopup, CreatedBy)
    VALUES (
        'Hoş Geldiniz!',
        '<p>Düzce Üniversitesi Sınav Takvimi Sistemine Hoş Geldiniz.</p><p>Bu sistem üzerinden sınav programlarınızı görüntüleyebilir ve yönetebilirsiniz.</p>',
        'general',
        0,
        'all',
        1,
        (SELECT TOP 1 Id FROM Users WHERE Email LIKE '%@duzce.edu.tr%')
    );
    PRINT '✓ Örnek duyuru eklendi'
END
PRINT ''

-- ===================================================================
-- Script Tamamlandı
-- ===================================================================
PRINT '==================================================================='
PRINT 'Bildirim Sistemi Kurulumu Tamamlandı!'
PRINT '==================================================================='
PRINT ''
PRINT 'Oluşturulan Tablolar:'
PRINT '1. Notifications - Kullanıcı bildirimleri (kişisel)'
PRINT '2. Announcements - Duyurular (toplu, rol bazlı)'
PRINT '3. AnnouncementReadStatus - Duyuru okuma takibi'
PRINT ''
PRINT 'Özellikler:'
PRINT '✓ Kullanıcı bazlı bildirimler'
PRINT '✓ Okundu/Okunmadı takibi'
PRINT '✓ Popup duyuru desteği'
PRINT '✓ Rol bazlı hedefleme'
PRINT '✓ Öncelik seviyeleri'
PRINT '✓ Zaman bazlı yayınlama/sonlandırma'
PRINT ''
PRINT 'Sonraki Adımlar:'
PRINT '1. Backend: Entity, Repository, Service, API Controller''ları oluştur'
PRINT '2. Frontend: Bildirim komponenti (header''da bell icon)'
PRINT '3. Frontend: Duyuru listesi sayfası'
PRINT '4. Frontend: Popup modal komponenti'
PRINT ''
PRINT '==================================================================='
GO

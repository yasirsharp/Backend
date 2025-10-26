-- =====================================================================
-- YasirSharp AI - Assistant System
-- Database Migration Script
-- Tarih: 26 Ekim 2025
-- =====================================================================

USE [DuzceUniversiteSinavTakvimi]; -- ⚠️ Database adını kontrol et!
GO

-- =====================================================================
-- 1. AssistantInteractions Tablosu
-- Kullanıcı-bot etkileşim kayıtları
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AssistantInteractions')
BEGIN
    CREATE TABLE AssistantInteractions (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        Question NVARCHAR(1000) NOT NULL,
        Answer NVARCHAR(MAX) NOT NULL,
        PageContext NVARCHAR(100) NOT NULL,
        FeatureUsed NVARCHAR(100) NULL,
        Language NVARCHAR(10) NOT NULL DEFAULT 'tr',
        Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME NULL,
        Status BIT NOT NULL DEFAULT 1,
        
        -- Foreign Key (opsiyonel - Users tablosu varsa)
        CONSTRAINT FK_AssistantInteractions_Users FOREIGN KEY (UserId) 
            REFERENCES Users(Id) ON DELETE CASCADE
    );
    
    -- Index'ler (performance için)
    CREATE NONCLUSTERED INDEX IX_AssistantInteractions_UserId 
        ON AssistantInteractions(UserId);
    
    CREATE NONCLUSTERED INDEX IX_AssistantInteractions_PageContext 
        ON AssistantInteractions(PageContext);
    
    CREATE NONCLUSTERED INDEX IX_AssistantInteractions_Timestamp 
        ON AssistantInteractions(Timestamp DESC);
    
    PRINT '✅ AssistantInteractions tablosu oluşturuldu.';
END
ELSE
BEGIN
    PRINT '⚠️ AssistantInteractions tablosu zaten mevcut.';
END
GO

-- =====================================================================
-- 2. UserAssistantPreferences Tablosu
-- Kullanıcı bot ayarları ve tercihleri
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserAssistantPreferences')
BEGIN
    CREATE TABLE UserAssistantPreferences (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL UNIQUE, -- Her kullanıcı için tek kayıt
        IsEnabled BIT NOT NULL DEFAULT 1, -- Bot aktif/pasif
        HasCompletedOnboarding BIT NOT NULL DEFAULT 0, -- İlk giriş turu tamamlandı mı
        LastInteractionDate DATETIME NULL,
        PreferredLanguage NVARCHAR(10) NOT NULL DEFAULT 'tr',
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME NULL,
        Status BIT NOT NULL DEFAULT 1,
        
        -- Foreign Key (opsiyonel - Users tablosu varsa)
        CONSTRAINT FK_UserAssistantPreferences_Users FOREIGN KEY (UserId) 
            REFERENCES Users(Id) ON DELETE CASCADE
    );
    
    -- Index (UserId unique constraint zaten index oluşturur)
    CREATE NONCLUSTERED INDEX IX_UserAssistantPreferences_UserId 
        ON UserAssistantPreferences(UserId);
    
    PRINT '✅ UserAssistantPreferences tablosu oluşturuldu.';
END
ELSE
BEGIN
    PRINT '⚠️ UserAssistantPreferences tablosu zaten mevcut.';
END
GO

-- =====================================================================
-- 3. Test Data (Opsiyonel - Development için)
-- =====================================================================

-- Test için örnek kullanıcı preference'ı (UserID 1 için)
IF NOT EXISTS (SELECT * FROM UserAssistantPreferences WHERE UserId = 1)
BEGIN
    INSERT INTO UserAssistantPreferences (UserId, IsEnabled, HasCompletedOnboarding, PreferredLanguage)
    VALUES (1, 1, 0, 'tr');
    
    PRINT '✅ Test user preference oluşturuldu (UserId: 1).';
END
GO

-- =====================================================================
-- 4. Verification (Kontrol)
-- =====================================================================

PRINT '';
PRINT '========================================';
PRINT 'Tablo Oluşturma Kontrolü:';
PRINT '========================================';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AssistantInteractions')
    PRINT '✅ AssistantInteractions: MEVCUT';
ELSE
    PRINT '❌ AssistantInteractions: BULUNAMADI';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserAssistantPreferences')
    PRINT '✅ UserAssistantPreferences: MEVCUT';
ELSE
    PRINT '❌ UserAssistantPreferences: BULUNAMADI';

PRINT '========================================';
GO

-- =====================================================================
-- 5. Kayıt Sayıları
-- =====================================================================

SELECT 
    'AssistantInteractions' AS TableName, 
    COUNT(*) AS RecordCount 
FROM AssistantInteractions
UNION ALL
SELECT 
    'UserAssistantPreferences' AS TableName, 
    COUNT(*) AS RecordCount 
FROM UserAssistantPreferences;
GO

PRINT '';
PRINT '✅ YasirSharp AI - Database migration tamamlandı!';
PRINT '🚀 Şimdi Backend API layer''ı oluşturabilirsin.';
GO

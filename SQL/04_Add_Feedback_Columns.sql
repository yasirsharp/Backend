-- YasirSharp AI - Feedback Columns Migration
-- Tarih: 2025-10-26
-- Amaç: AssistantInteraction tablosuna feedback alanları ekleme

USE [DuzceUniversiteSinavTakvimi]
GO

-- 1. IsHelpful kolonu ekle (thumbs up/down)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AssistantInteractions]') AND name = 'IsHelpful')
BEGIN
    ALTER TABLE [dbo].[AssistantInteractions]
    ADD IsHelpful BIT NULL;
    PRINT 'IsHelpful kolonu eklendi.';
END
ELSE
BEGIN
    PRINT 'IsHelpful kolonu zaten mevcut.';
END
GO

-- 2. ErrorReport kolonu ekle (hata bildirimi metni)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AssistantInteractions]') AND name = 'ErrorReport')
BEGIN
    ALTER TABLE [dbo].[AssistantInteractions]
    ADD ErrorReport NVARCHAR(MAX) NULL;
    PRINT 'ErrorReport kolonu eklendi.';
END
ELSE
BEGIN
    PRINT 'ErrorReport kolonu zaten mevcut.';
END
GO

-- 3. FeedbackTimestamp kolonu ekle (feedback zamanı)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AssistantInteractions]') AND name = 'FeedbackTimestamp')
BEGIN
    ALTER TABLE [dbo].[AssistantInteractions]
    ADD FeedbackTimestamp DATETIME2 NULL;
    PRINT 'FeedbackTimestamp kolonu eklendi.';
END
ELSE
BEGIN
    PRINT 'FeedbackTimestamp kolonu zaten mevcut.';
END
GO

-- 4. İndeks oluştur (Performans için)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AssistantInteractions]') AND name = 'IX_AssistantInteractions_Feedback')
BEGIN
    CREATE NONCLUSTERED INDEX IX_AssistantInteractions_Feedback
    ON [dbo].[AssistantInteractions] (IsHelpful, FeedbackTimestamp)
    WHERE IsHelpful IS NOT NULL;
    PRINT 'IX_AssistantInteractions_Feedback indeksi oluşturuldu.';
END
ELSE
BEGIN
    PRINT 'IX_AssistantInteractions_Feedback indeksi zaten mevcut.';
END
GO

-- 5. Hata bildirimleri için indeks
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AssistantInteractions]') AND name = 'IX_AssistantInteractions_ErrorReport')
BEGIN
    CREATE NONCLUSTERED INDEX IX_AssistantInteractions_ErrorReport
    ON [dbo].[AssistantInteractions] (FeedbackTimestamp)
    WHERE ErrorReport IS NOT NULL AND ErrorReport != '';
    PRINT 'IX_AssistantInteractions_ErrorReport indeksi oluşturuldu.';
END
ELSE
BEGIN
    PRINT 'IX_AssistantInteractions_ErrorReport indeksi zaten mevcut.';
END
GO

PRINT '✅ Migration tamamlandı!';
GO

-- Test sorguları (opsiyonel)
-- SELECT TOP 10 * FROM AssistantInteractions WHERE IsHelpful IS NOT NULL ORDER BY FeedbackTimestamp DESC;
-- SELECT TOP 10 * FROM AssistantInteractions WHERE ErrorReport IS NOT NULL ORDER BY FeedbackTimestamp DESC;

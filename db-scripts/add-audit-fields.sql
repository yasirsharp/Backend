-- =====================================================
-- Script: Audit Alanları Ekleme (CreatedDate, UpdatedDate)
-- Açıklama: Tüm tablolara denetim alanları eklenir
-- Tarih: 2025-10-20
-- =====================================================

USE [DuzceUniversiteSinavTakvimiDB]; -- Database adınızı buraya yazın
GO

PRINT 'Audit alanları ekleniyor...';
GO

-- =====================================================
-- 1. Users Tablosu
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE [dbo].[Users] ADD CreatedDate DATETIME NOT NULL DEFAULT GETDATE();
    PRINT '✓ Users.CreatedDate eklendi';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[Users] ADD UpdatedDate DATETIME NULL;
    PRINT '✓ Users.UpdatedDate eklendi';
END
GO

-- =====================================================
-- 2. Bolum Tablosu
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bolum]') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE [dbo].[Bolum] ADD CreatedDate DATETIME NOT NULL DEFAULT GETDATE();
    PRINT '✓ Bolum.CreatedDate eklendi';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bolum]') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[Bolum] ADD UpdatedDate DATETIME NULL;
    PRINT '✓ Bolum.UpdatedDate eklendi';
END
GO

-- =====================================================
-- 3. Ders Tablosu
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Ders]') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE [dbo].[Ders] ADD CreatedDate DATETIME NOT NULL DEFAULT GETDATE();
    PRINT '✓ Ders.CreatedDate eklendi';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Ders]') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[Ders] ADD UpdatedDate DATETIME NULL;
    PRINT '✓ Ders.UpdatedDate eklendi';
END
GO

-- =====================================================
-- 4. Derslik Tablosu
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Derslik]') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE [dbo].[Derslik] ADD CreatedDate DATETIME NOT NULL DEFAULT GETDATE();
    PRINT '✓ Derslik.CreatedDate eklendi';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Derslik]') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[Derslik] ADD UpdatedDate DATETIME NULL;
    PRINT '✓ Derslik.UpdatedDate eklendi';
END
GO

-- =====================================================
-- 5. AkademikPersonel Tablosu
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AkademikPersonel]') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE [dbo].[AkademikPersonel] ADD CreatedDate DATETIME NOT NULL DEFAULT GETDATE();
    PRINT '✓ AkademikPersonel.CreatedDate eklendi';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AkademikPersonel]') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[AkademikPersonel] ADD UpdatedDate DATETIME NULL;
    PRINT '✓ AkademikPersonel.UpdatedDate eklendi';
END
GO

-- =====================================================
-- 6. SinavDetay Tablosu
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SinavDetay]') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE [dbo].[SinavDetay] ADD CreatedDate DATETIME NOT NULL DEFAULT GETDATE();
    PRINT '✓ SinavDetay.CreatedDate eklendi';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SinavDetay]') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[SinavDetay] ADD UpdatedDate DATETIME NULL;
    PRINT '✓ SinavDetay.UpdatedDate eklendi';
END
GO

-- =====================================================
-- 7. DersBolumAkademikPersonel Tablosu
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DersBolumAkademikPersonel]') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE [dbo].[DersBolumAkademikPersonel] ADD CreatedDate DATETIME NOT NULL DEFAULT GETDATE();
    PRINT '✓ DersBolumAkademikPersonel.CreatedDate eklendi';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DersBolumAkademikPersonel]') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[DersBolumAkademikPersonel] ADD UpdatedDate DATETIME NULL;
    PRINT '✓ DersBolumAkademikPersonel.UpdatedDate eklendi';
END
GO

-- =====================================================
-- 8. BolumAkademikPersoneller Tablosu
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BolumAkademikPersoneller]') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE [dbo].[BolumAkademikPersoneller] ADD CreatedDate DATETIME NOT NULL DEFAULT GETDATE();
    PRINT '✓ BolumAkademikPersoneller.CreatedDate eklendi';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BolumAkademikPersoneller]') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[BolumAkademikPersoneller] ADD UpdatedDate DATETIME NULL;
    PRINT '✓ BolumAkademikPersoneller.UpdatedDate eklendi';
END
GO

-- =====================================================
-- 9. DersBolum Tablosu
-- =====================================================
-- NOT: DersBolum tablosunda zaten CreatedDate var, sadece UpdatedDate ekliyoruz
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DersBolum]') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE [dbo].[DersBolum] ADD CreatedDate DATETIME NOT NULL DEFAULT GETDATE();
    PRINT '✓ DersBolum.CreatedDate eklendi';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DersBolum]') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[DersBolum] ADD UpdatedDate DATETIME NULL;
    PRINT '✓ DersBolum.UpdatedDate eklendi';
END
GO

-- =====================================================
-- 10. DerslikBolum Tablosu
-- =====================================================
-- NOT: DerslikBolum tablosunda zaten CreatedDate var, sadece UpdatedDate ekliyoruz
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DerslikBolum]') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE [dbo].[DerslikBolum] ADD CreatedDate DATETIME NOT NULL DEFAULT GETDATE();
    PRINT '✓ DerslikBolum.CreatedDate eklendi';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DerslikBolum]') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[DerslikBolum] ADD UpdatedDate DATETIME NULL;
    PRINT '✓ DerslikBolum.UpdatedDate eklendi';
END
GO

-- =====================================================
-- 11. SinavDerslik Tablosu
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SinavDerslik]') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE [dbo].[SinavDerslik] ADD CreatedDate DATETIME NOT NULL DEFAULT GETDATE();
    PRINT '✓ SinavDerslik.CreatedDate eklendi';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SinavDerslik]') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[SinavDerslik] ADD UpdatedDate DATETIME NULL;
    PRINT '✓ SinavDerslik.UpdatedDate eklendi';
END
GO

-- =====================================================
-- 12. OperationClaims Tablosu
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[OperationClaims]') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE [dbo].[OperationClaims] ADD CreatedDate DATETIME NOT NULL DEFAULT GETDATE();
    PRINT '✓ OperationClaims.CreatedDate eklendi';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[OperationClaims]') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[OperationClaims] ADD UpdatedDate DATETIME NULL;
    PRINT '✓ OperationClaims.UpdatedDate eklendi';
END
GO

-- =====================================================
-- 13. UserOperationClaims Tablosu
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserOperationClaims]') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE [dbo].[UserOperationClaims] ADD CreatedDate DATETIME NOT NULL DEFAULT GETDATE();
    PRINT '✓ UserOperationClaims.CreatedDate eklendi';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserOperationClaims]') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[UserOperationClaims] ADD UpdatedDate DATETIME NULL;
    PRINT '✓ UserOperationClaims.UpdatedDate eklendi';
END
GO

-- =====================================================
-- TAMAMLANDI
-- =====================================================
PRINT '';
PRINT '✅ Audit alanları başarıyla eklendi!';
PRINT '';
PRINT 'ÖZET:';
PRINT '- Tüm tablolara CreatedDate (DATETIME NOT NULL) eklendi';
PRINT '- Tüm tablolara UpdatedDate (DATETIME NULL) eklendi';
PRINT '- Mevcut veriler için CreatedDate = GETDATE() olarak ayarlandı';
PRINT '- UpdatedDate güncelleme yapıldığında EF tarafından otomatik doldurulacak';
GO

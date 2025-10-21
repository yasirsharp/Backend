/*
 * Script: Status Alanı Ekleme
 * Açıklama: Tüm entity'lere Status (Aktif/Pasif) alanı ekler
 * Tarih: 21 Ekim 2025
 * 
 * Kullanım:
 * 1. SQL Server Management Studio'da bu dosyayı aç
 * 2. İlk satırda database adını kontrol et
 * 3. F5 ile çalıştır
 * 
 * NOT: Bu script idempotent'tir (birden fazla çalıştırılabilir)
 */

USE [DuzceUniversiteSinavTakvimi]
GO

PRINT '==================================================================='
PRINT 'Status Alanı Ekleme Script Başlatılıyor...'
PRINT '==================================================================='
PRINT ''

-- ===================================================================
-- 1. Users Tablosu
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Status')
BEGIN
    PRINT '1/13 - Users tablosuna Status alanı ekleniyor...'
    ALTER TABLE [Users]
    ADD [Status] BIT NOT NULL DEFAULT 1; -- Default: Aktif
    PRINT '✓ Users.Status eklendi (Default: Aktif)'
END
ELSE
BEGIN
    PRINT '1/13 - Users.Status zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- 2. Bolumler Tablosu
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bolumler') AND name = 'Status')
BEGIN
    PRINT '2/13 - Bolumler tablosuna Status alanı ekleniyor...'
    ALTER TABLE [Bolumler]
    ADD [Status] BIT NOT NULL DEFAULT 1;
    PRINT '✓ Bolumler.Status eklendi (Default: Aktif)'
END
ELSE
BEGIN
    PRINT '2/13 - Bolumler.Status zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- 3. Dersler Tablosu
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Dersler') AND name = 'Status')
BEGIN
    PRINT '3/13 - Dersler tablosuna Status alanı ekleniyor...'
    ALTER TABLE [Dersler]
    ADD [Status] BIT NOT NULL DEFAULT 1;
    PRINT '✓ Dersler.Status eklendi (Default: Aktif)'
END
ELSE
BEGIN
    PRINT '3/13 - Dersler.Status zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- 4. Derslikler Tablosu
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Derslikler') AND name = 'Status')
BEGIN
    PRINT '4/13 - Derslikler tablosuna Status alanı ekleniyor...'
    ALTER TABLE [Derslikler]
    ADD [Status] BIT NOT NULL DEFAULT 1;
    PRINT '✓ Derslikler.Status eklendi (Default: Aktif)'
END
ELSE
BEGIN
    PRINT '4/13 - Derslikler.Status zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- 5. AkademikPersoneller Tablosu
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AkademikPersoneller') AND name = 'Status')
BEGIN
    PRINT '5/13 - AkademikPersoneller tablosuna Status alanı ekleniyor...'
    ALTER TABLE [AkademikPersoneller]
    ADD [Status] BIT NOT NULL DEFAULT 1;
    PRINT '✓ AkademikPersoneller.Status eklendi (Default: Aktif)'
END
ELSE
BEGIN
    PRINT '5/13 - AkademikPersoneller.Status zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- 6. DersBolumAkademikPersoneller Tablosu
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DersBolumAkademikPersoneller') AND name = 'Status')
BEGIN
    PRINT '6/13 - DersBolumAkademikPersoneller tablosuna Status alanı ekleniyor...'
    ALTER TABLE [DersBolumAkademikPersoneller]
    ADD [Status] BIT NOT NULL DEFAULT 1;
    PRINT '✓ DersBolumAkademikPersoneller.Status eklendi (Default: Aktif)'
END
ELSE
BEGIN
    PRINT '6/13 - DersBolumAkademikPersoneller.Status zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- 7. SinavDetaylari Tablosu
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SinavDetaylari') AND name = 'Status')
BEGIN
    PRINT '7/13 - SinavDetaylari tablosuna Status alanı ekleniyor...'
    ALTER TABLE [SinavDetaylari]
    ADD [Status] BIT NOT NULL DEFAULT 1;
    PRINT '✓ SinavDetaylari.Status eklendi (Default: Aktif)'
END
ELSE
BEGIN
    PRINT '7/13 - SinavDetaylari.Status zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- 8. BolumAkademikPersoneller Tablosu
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BolumAkademikPersoneller') AND name = 'Status')
BEGIN
    PRINT '8/13 - BolumAkademikPersoneller tablosuna Status alanı ekleniyor...'
    ALTER TABLE [BolumAkademikPersoneller]
    ADD [Status] BIT NOT NULL DEFAULT 1;
    PRINT '✓ BolumAkademikPersoneller.Status eklendi (Default: Aktif)'
END
ELSE
BEGIN
    PRINT '8/13 - BolumAkademikPersoneller.Status zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- 9. DersBolumler Tablosu
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DersBolumler') AND name = 'Status')
BEGIN
    PRINT '9/13 - DersBolumler tablosuna Status alanı ekleniyor...'
    ALTER TABLE [DersBolumler]
    ADD [Status] BIT NOT NULL DEFAULT 1;
    PRINT '✓ DersBolumler.Status eklendi (Default: Aktif)'
END
ELSE
BEGIN
    PRINT '9/13 - DersBolumler.Status zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- 10. DerslikBolumler Tablosu
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DerslikBolumler') AND name = 'Status')
BEGIN
    PRINT '10/13 - DerslikBolumler tablosuna Status alanı ekleniyor...'
    ALTER TABLE [DerslikBolumler]
    ADD [Status] BIT NOT NULL DEFAULT 1;
    PRINT '✓ DerslikBolumler.Status eklendi (Default: Aktif)'
END
ELSE
BEGIN
    PRINT '10/13 - DerslikBolumler.Status zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- 11. SinavDerslikler Tablosu
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SinavDerslikler') AND name = 'Status')
BEGIN
    PRINT '11/13 - SinavDerslikler tablosuna Status alanı ekleniyor...'
    ALTER TABLE [SinavDerslikler]
    ADD [Status] BIT NOT NULL DEFAULT 1;
    PRINT '✓ SinavDerslikler.Status eklendi (Default: Aktif)'
END
ELSE
BEGIN
    PRINT '11/13 - SinavDerslikler.Status zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- 12. OperationClaims Tablosu
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('OperationClaims') AND name = 'Status')
BEGIN
    PRINT '12/13 - OperationClaims tablosuna Status alanı ekleniyor...'
    ALTER TABLE [OperationClaims]
    ADD [Status] BIT NOT NULL DEFAULT 1;
    PRINT '✓ OperationClaims.Status eklendi (Default: Aktif)'
END
ELSE
BEGIN
    PRINT '12/13 - OperationClaims.Status zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- 13. UserOperationClaims Tablosu
-- ===================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserOperationClaims') AND name = 'Status')
BEGIN
    PRINT '13/13 - UserOperationClaims tablosuna Status alanı ekleniyor...'
    ALTER TABLE [UserOperationClaims]
    ADD [Status] BIT NOT NULL DEFAULT 1;
    PRINT '✓ UserOperationClaims.Status eklendi (Default: Aktif)'
END
ELSE
BEGIN
    PRINT '13/13 - UserOperationClaims.Status zaten mevcut (Atlanıyor)'
END
PRINT ''

-- ===================================================================
-- Script Tamamlandı
-- ===================================================================
PRINT '==================================================================='
PRINT 'Status Alanı Ekleme Script Tamamlandı!'
PRINT '==================================================================='
PRINT ''
PRINT 'Özet:'
PRINT '- Tüm entity tablolarına Status (BIT) alanı eklendi'
PRINT '- Default değer: 1 (Aktif)'
PRINT '- 0: Pasif (Geçici olarak devre dışı)'
PRINT '- 1: Aktif (Kullanımda)'
PRINT ''
PRINT 'Sonraki Adımlar:'
PRINT '1. Backend: Entity class''larına Status property''si ekle'
PRINT '2. Backend: Repository ve Service katmanlarını güncelle'
PRINT '3. Frontend: CRUD formlarına Status toggle ekle'
PRINT '4. Frontend: Listelerde Status badge göster'
PRINT ''
PRINT '==================================================================='
GO

-- =====================================================
-- Migration: AkademikPersonelMusaitlik tablosuna IsMusait alanı ekle
-- Tarih: 2025
-- Açıklama: String-based müsait/meşgul kontrolünden boolean alana geçiş
-- DBeaver uyumlu (GO komutu yok)
-- =====================================================

-- 1. IsMusait kolonunu ekle (varsayılan: false = meşgul)
-- Önce kolonun var olup olmadığını kontrol et
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('AkademikPersonelMusaitlik') 
    AND name = 'IsMusait'
)
ALTER TABLE AkademikPersonelMusaitlik 
ADD IsMusait BIT NOT NULL DEFAULT 0;

-- 2. Mevcut kayıtları Neden alanına göre güncelle
-- 'Müsait' veya 'musait' içeren kayıtları IsMusait = 1 yap
UPDATE AkademikPersonelMusaitlik
SET IsMusait = 1
WHERE LOWER(Neden) LIKE '%müsait%' 
   OR LOWER(Neden) LIKE '%musait%';

-- 3. Sonuçları kontrol et
SELECT 
    COUNT(*) AS ToplamKayit,
    SUM(CASE WHEN IsMusait = 1 THEN 1 ELSE 0 END) AS MusaitKayit,
    SUM(CASE WHEN IsMusait = 0 THEN 1 ELSE 0 END) AS MesgulKayit
FROM AkademikPersonelMusaitlik;

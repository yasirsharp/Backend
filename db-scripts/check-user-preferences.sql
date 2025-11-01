-- UserAssistantPreferences kontrol ve test scripti
-- =====================================================================

-- 1. Tablo var mı kontrol et
SELECT 
    TABLE_NAME, 
    TABLE_TYPE 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'UserAssistantPreferences';

-- 2. Tüm kayıtları listele
SELECT 
    Id,
    UserId,
    IsEnabled,
    HasCompletedOnboarding,
    PreferredLanguage,
    LastInteractionDate,
    CreatedDate,
    UpdatedDate,
    Status
FROM UserAssistantPreferences;

-- 3. User ID = 1 için kayıt var mı kontrol et
SELECT 
    *
FROM UserAssistantPreferences
WHERE UserId = 1;

-- 4. Eğer User ID = 1 için kayıt yoksa, ekle
IF NOT EXISTS (SELECT 1 FROM UserAssistantPreferences WHERE UserId = 1)
BEGIN
    INSERT INTO UserAssistantPreferences (UserId, IsEnabled, HasCompletedOnboarding, PreferredLanguage, Status)
    VALUES (1, 1, 0, 'tr', 1);
    
    PRINT '✅ UserId=1 için default preference kaydı oluşturuldu.';
END
ELSE
BEGIN
    PRINT '⚠️ UserId=1 için preference kaydı zaten var.';
END

-- 5. Tekrar kontrol et
SELECT 
    Id,
    UserId,
    IsEnabled,
    HasCompletedOnboarding,
    PreferredLanguage,
    LastInteractionDate,
    CreatedDate,
    Status
FROM UserAssistantPreferences
WHERE UserId = 1;

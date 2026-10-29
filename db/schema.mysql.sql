CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `AkademikPersonel` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Ad` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Unvan` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `UserId` int NOT NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_AkademikPersonel` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `AnnouncementReadStatus` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AnnouncementId` int NOT NULL,
        `UserId` int NOT NULL,
        `ReadDate` datetime(6) NOT NULL,
        CONSTRAINT `PK_AnnouncementReadStatus` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `AssistantInteractions` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `UserId` int NOT NULL,
        `Question` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Answer` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `PageContext` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `FeatureUsed` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `Language` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Timestamp` datetime(6) NOT NULL,
        `IsHelpful` tinyint(1) NULL,
        `ErrorReport` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `FeedbackTimestamp` datetime(6) NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_AssistantInteractions` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `Bolum` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Ad` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Bolum` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `BolumAkademikPersoneller` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `BolumId` int NOT NULL,
        `AkademikPersonelId` int NOT NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_BolumAkademikPersoneller` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `Ders` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Ad` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Kod` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Ders` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `DersBolum` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `DersId` int NOT NULL,
        `BolumId` int NOT NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_DersBolum` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `DersBolumAkademikPersonel` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `DersId` int NOT NULL,
        `BolumId` int NOT NULL,
        `AkademikPersonelId` int NOT NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_DersBolumAkademikPersonel` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `Derslik` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Ad` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Kapasite` int NOT NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Derslik` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `DerslikBolum` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `DerslikId` int NOT NULL,
        `BolumId` int NOT NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_DerslikBolum` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `Notifications` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `UserId` int NOT NULL,
        `Title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Message` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `IsRead` tinyint(1) NOT NULL,
        `ActionUrl` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `RelatedEntityType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `RelatedEntityId` int NULL,
        `ReadDate` datetime(6) NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Notifications` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `Ogrenci` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `UserId` int NOT NULL,
        `OgrenciNo` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Ad` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Soyad` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `BolumId` int NOT NULL,
        `Sinif` int NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Ogrenci` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `OperationClaims` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_OperationClaims` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `SinavDerslik` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `DerslikId` int NOT NULL,
        `SinavDetayId` int NOT NULL,
        `GozetmenId` int NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_SinavDerslik` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `SinavDetay` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `DersBolumAkademikPersonelId` int NOT NULL,
        `SinavTarihi` datetime(6) NOT NULL,
        `SinavBaslangicSaati` time(6) NOT NULL,
        `SinavBitisSaati` time(6) NOT NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_SinavDetay` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `UserAssistantPreferences` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `UserId` int NOT NULL,
        `IsEnabled` tinyint(1) NOT NULL,
        `HasCompletedOnboarding` tinyint(1) NOT NULL,
        `LastInteractionDate` datetime(6) NULL,
        `PreferredLanguage` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_UserAssistantPreferences` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `UserOperationClaims` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `UserId` int NOT NULL,
        `OperationClaimId` int NOT NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_UserOperationClaims` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `Users` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `FirstName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `LastName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Email` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `UserName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `PasswordHash` varbinary(128) NOT NULL,
        `PasswordSalt` varbinary(128) NOT NULL,
        `Status` tinyint(1) NOT NULL,
        `MustChangePassword` tinyint(1) NOT NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `AkademikPersonelMusaitlik` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AkademikPersonelId` int NOT NULL,
        `BaslangicTarihi` datetime(6) NOT NULL,
        `BitisTarihi` datetime(6) NULL,
        `BaslangicSaati` time(6) NULL,
        `BitisSaati` time(6) NULL,
        `TekrarTipi` int NOT NULL,
        `TekrarGunu` int NULL,
        `IsMusait` tinyint(1) NOT NULL,
        `Neden` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `Aciklama` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_AkademikPersonelMusaitlik` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AkademikPersonelMusaitlik_AkademikPersonel_AkademikPersonelId` FOREIGN KEY (`AkademikPersonelId`) REFERENCES `AkademikPersonel` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE TABLE `Announcements` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Content` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Type` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Priority` int NOT NULL,
        `TargetAudience` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `TargetBolumId` int NULL,
        `PublishDate` datetime(6) NOT NULL,
        `ExpiryDate` datetime(6) NULL,
        `IsActive` tinyint(1) NOT NULL,
        `ShowAsPopup` tinyint(1) NOT NULL,
        `CreatedBy` int NOT NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `Status` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Announcements` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Announcements_Bolum_TargetBolumId` FOREIGN KEY (`TargetBolumId`) REFERENCES `Bolum` (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    INSERT INTO `OperationClaims` (`Id`, `CreatedDate`, `Name`, `Status`, `UpdatedDate`)
    VALUES (1, TIMESTAMP '2026-01-01 00:00:00', 'super.admin', TRUE, NULL),
    (2, TIMESTAMP '2026-01-01 00:00:00', 'akademik.personel', TRUE, NULL),
    (3, TIMESTAMP '2026-01-01 00:00:00', 'ogrenci', TRUE, NULL),
    (4, TIMESTAMP '2026-01-01 00:00:00', 'bolum.gorevlisi', TRUE, NULL),
    (5, TIMESTAMP '2026-01-01 00:00:00', 'gorevli.personel', TRUE, NULL),
    (6, TIMESTAMP '2026-01-01 00:00:00', 'Admin', TRUE, NULL);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_AkademikPersonel_UserId` ON `AkademikPersonel` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE INDEX `IX_AkademikPersonelMusaitlik_AkademikPersonelId_BaslangicTarihi` ON `AkademikPersonelMusaitlik` (`AkademikPersonelId`, `BaslangicTarihi`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_AnnouncementReadStatus_AnnouncementId_UserId` ON `AnnouncementReadStatus` (`AnnouncementId`, `UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE INDEX `IX_Announcements_TargetBolumId` ON `Announcements` (`TargetBolumId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Bolum_Ad` ON `Bolum` (`Ad`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_BolumAkademikPersoneller_BolumId_AkademikPersonelId` ON `BolumAkademikPersoneller` (`BolumId`, `AkademikPersonelId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Ders_Kod` ON `Ders` (`Kod`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_DersBolum_DersId_BolumId` ON `DersBolum` (`DersId`, `BolumId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE INDEX `IX_DersBolumAkademikPersonel_AkademikPersonelId` ON `DersBolumAkademikPersonel` (`AkademikPersonelId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_DersBolumAkademikPersonel_DersId_BolumId_AkademikPersonelId` ON `DersBolumAkademikPersonel` (`DersId`, `BolumId`, `AkademikPersonelId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Derslik_Ad` ON `Derslik` (`Ad`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_DerslikBolum_DerslikId_BolumId` ON `DerslikBolum` (`DerslikId`, `BolumId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE INDEX `IX_Notifications_UserId_IsRead` ON `Notifications` (`UserId`, `IsRead`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE INDEX `IX_Ogrenci_BolumId` ON `Ogrenci` (`BolumId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Ogrenci_OgrenciNo` ON `Ogrenci` (`OgrenciNo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Ogrenci_UserId` ON `Ogrenci` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_OperationClaims_Name` ON `OperationClaims` (`Name`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE INDEX `IX_SinavDerslik_DerslikId` ON `SinavDerslik` (`DerslikId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE INDEX `IX_SinavDerslik_GozetmenId` ON `SinavDerslik` (`GozetmenId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE INDEX `IX_SinavDerslik_SinavDetayId` ON `SinavDerslik` (`SinavDetayId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE INDEX `IX_SinavDetay_DersBolumAkademikPersonelId` ON `SinavDetay` (`DersBolumAkademikPersonelId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE INDEX `IX_SinavDetay_SinavTarihi` ON `SinavDetay` (`SinavTarihi`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_UserOperationClaims_UserId_OperationClaimId` ON `UserOperationClaims` (`UserId`, `OperationClaimId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Users_Email` ON `Users` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Users_UserName` ON `Users` (`UserName`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029121526_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261029121526_InitialCreate', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;


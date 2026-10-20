IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [AkademikPersonel] (
        [Id] int NOT NULL IDENTITY,
        [Ad] nvarchar(200) NOT NULL,
        [Unvan] nvarchar(100) NOT NULL,
        [UserId] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_AkademikPersonel] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [AnnouncementReadStatus] (
        [Id] int NOT NULL IDENTITY,
        [AnnouncementId] int NOT NULL,
        [UserId] int NOT NULL,
        [ReadDate] datetime2 NOT NULL,
        CONSTRAINT [PK_AnnouncementReadStatus] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [AssistantInteractions] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Question] nvarchar(max) NOT NULL,
        [Answer] nvarchar(max) NOT NULL,
        [PageContext] nvarchar(max) NOT NULL,
        [FeatureUsed] nvarchar(max) NULL,
        [Language] nvarchar(max) NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [IsHelpful] bit NULL,
        [ErrorReport] nvarchar(max) NULL,
        [FeedbackTimestamp] datetime2 NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_AssistantInteractions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [Bolum] (
        [Id] int NOT NULL IDENTITY,
        [Ad] nvarchar(200) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_Bolum] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [BolumAkademikPersoneller] (
        [Id] int NOT NULL IDENTITY,
        [BolumId] int NOT NULL,
        [AkademikPersonelId] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_BolumAkademikPersoneller] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [Ders] (
        [Id] int NOT NULL IDENTITY,
        [Ad] nvarchar(200) NOT NULL,
        [Kod] nvarchar(50) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_Ders] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [DersBolum] (
        [Id] int NOT NULL IDENTITY,
        [DersId] int NOT NULL,
        [BolumId] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_DersBolum] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [DersBolumAkademikPersonel] (
        [Id] int NOT NULL IDENTITY,
        [DersId] int NOT NULL,
        [BolumId] int NOT NULL,
        [AkademikPersonelId] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_DersBolumAkademikPersonel] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [Derslik] (
        [Id] int NOT NULL IDENTITY,
        [Ad] nvarchar(100) NOT NULL,
        [Kapasite] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_Derslik] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [DerslikBolum] (
        [Id] int NOT NULL IDENTITY,
        [DerslikId] int NOT NULL,
        [BolumId] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_DerslikBolum] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(2000) NOT NULL,
        [Type] nvarchar(20) NOT NULL,
        [IsRead] bit NOT NULL,
        [ActionUrl] nvarchar(500) NULL,
        [RelatedEntityType] nvarchar(50) NULL,
        [RelatedEntityId] int NULL,
        [ReadDate] datetime2 NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [Ogrenci] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [OgrenciNo] nvarchar(20) NOT NULL,
        [Ad] nvarchar(100) NOT NULL,
        [Soyad] nvarchar(100) NOT NULL,
        [BolumId] int NOT NULL,
        [Sinif] int NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_Ogrenci] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [OperationClaims] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_OperationClaims] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [SinavDerslik] (
        [Id] int NOT NULL IDENTITY,
        [DerslikId] int NOT NULL,
        [SinavDetayId] int NOT NULL,
        [GozetmenId] int NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_SinavDerslik] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [SinavDetay] (
        [Id] int NOT NULL IDENTITY,
        [DersBolumAkademikPersonelId] int NOT NULL,
        [SinavTarihi] datetime2 NOT NULL,
        [SinavBaslangicSaati] time NOT NULL,
        [SinavBitisSaati] time NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_SinavDetay] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [UserAssistantPreferences] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [IsEnabled] bit NOT NULL,
        [HasCompletedOnboarding] bit NOT NULL,
        [LastInteractionDate] datetime2 NULL,
        [PreferredLanguage] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_UserAssistantPreferences] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [UserOperationClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [OperationClaimId] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_UserOperationClaims] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [UserName] nvarchar(100) NOT NULL,
        [PasswordHash] varbinary(128) NOT NULL,
        [PasswordSalt] varbinary(128) NOT NULL,
        [Status] bit NOT NULL,
        [MustChangePassword] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [AkademikPersonelMusaitlik] (
        [Id] int NOT NULL IDENTITY,
        [AkademikPersonelId] int NOT NULL,
        [BaslangicTarihi] datetime2 NOT NULL,
        [BitisTarihi] datetime2 NULL,
        [BaslangicSaati] time NULL,
        [BitisSaati] time NULL,
        [TekrarTipi] int NOT NULL,
        [TekrarGunu] int NULL,
        [IsMusait] bit NOT NULL,
        [Neden] nvarchar(100) NULL,
        [Aciklama] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_AkademikPersonelMusaitlik] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AkademikPersonelMusaitlik_AkademikPersonel_AkademikPersonelId] FOREIGN KEY ([AkademikPersonelId]) REFERENCES [AkademikPersonel] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE TABLE [Announcements] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [Type] nvarchar(30) NOT NULL,
        [Priority] int NOT NULL,
        [TargetAudience] nvarchar(50) NOT NULL,
        [TargetBolumId] int NULL,
        [PublishDate] datetime2 NOT NULL,
        [ExpiryDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [ShowAsPopup] bit NOT NULL,
        [CreatedBy] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [Status] bit NOT NULL,
        CONSTRAINT [PK_Announcements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Announcements_Bolum_TargetBolumId] FOREIGN KEY ([TargetBolumId]) REFERENCES [Bolum] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedDate', N'Name', N'Status', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[OperationClaims]'))
        SET IDENTITY_INSERT [OperationClaims] ON;
    EXEC(N'INSERT INTO [OperationClaims] ([Id], [CreatedDate], [Name], [Status], [UpdatedDate])
    VALUES (1, ''2026-01-01T00:00:00.0000000'', N''super.admin'', CAST(1 AS bit), NULL),
    (2, ''2026-01-01T00:00:00.0000000'', N''akademik.personel'', CAST(1 AS bit), NULL),
    (3, ''2026-01-01T00:00:00.0000000'', N''ogrenci'', CAST(1 AS bit), NULL),
    (4, ''2026-01-01T00:00:00.0000000'', N''bolum.gorevlisi'', CAST(1 AS bit), NULL),
    (5, ''2026-01-01T00:00:00.0000000'', N''gorevli.personel'', CAST(1 AS bit), NULL),
    (6, ''2026-01-01T00:00:00.0000000'', N''Admin'', CAST(1 AS bit), NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedDate', N'Name', N'Status', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[OperationClaims]'))
        SET IDENTITY_INSERT [OperationClaims] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AkademikPersonel_UserId] ON [AkademikPersonel] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AkademikPersonelMusaitlik_AkademikPersonelId_BaslangicTarihi] ON [AkademikPersonelMusaitlik] ([AkademikPersonelId], [BaslangicTarihi]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AnnouncementReadStatus_AnnouncementId_UserId] ON [AnnouncementReadStatus] ([AnnouncementId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Announcements_TargetBolumId] ON [Announcements] ([TargetBolumId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Bolum_Ad] ON [Bolum] ([Ad]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BolumAkademikPersoneller_BolumId_AkademikPersonelId] ON [BolumAkademikPersoneller] ([BolumId], [AkademikPersonelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Ders_Kod] ON [Ders] ([Kod]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DersBolum_DersId_BolumId] ON [DersBolum] ([DersId], [BolumId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DersBolumAkademikPersonel_AkademikPersonelId] ON [DersBolumAkademikPersonel] ([AkademikPersonelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DersBolumAkademikPersonel_DersId_BolumId_AkademikPersonelId] ON [DersBolumAkademikPersonel] ([DersId], [BolumId], [AkademikPersonelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Derslik_Ad] ON [Derslik] ([Ad]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DerslikBolum_DerslikId_BolumId] ON [DerslikBolum] ([DerslikId], [BolumId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId_IsRead] ON [Notifications] ([UserId], [IsRead]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Ogrenci_BolumId] ON [Ogrenci] ([BolumId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Ogrenci_OgrenciNo] ON [Ogrenci] ([OgrenciNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Ogrenci_UserId] ON [Ogrenci] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OperationClaims_Name] ON [OperationClaims] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SinavDerslik_DerslikId] ON [SinavDerslik] ([DerslikId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SinavDerslik_GozetmenId] ON [SinavDerslik] ([GozetmenId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SinavDerslik_SinavDetayId] ON [SinavDerslik] ([SinavDetayId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SinavDetay_DersBolumAkademikPersonelId] ON [SinavDetay] ([DersBolumAkademikPersonelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SinavDetay_SinavTarihi] ON [SinavDetay] ([SinavTarihi]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserOperationClaims_UserId_OperationClaimId] ON [UserOperationClaims] ([UserId], [OperationClaimId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_UserName] ON [Users] ([UserName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020155606_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261020155606_InitialCreate', N'9.0.0');
END;

COMMIT;
GO


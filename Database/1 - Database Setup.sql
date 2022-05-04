CREATE DATABASE [Illustre];
GO

USE [Illustre];
GO

-- Roles:
-- SuperAdmin = 0
-- Editor = 1
-- User = 2

CREATE TABLE [Accounts] (
    [Id]              INT             IDENTITY(1,1),
    [Email]           VARCHAR(320)    NOT NULL,
    [Salt]            CHAR(32)        NOT NULL,
    [PasswordHash]    CHAR(64)        NOT NULL,
    [Username]        VARCHAR(50)     NOT NULL,
    [Role]            INT             NOT NULL,
    [SessionGuid]     CHAR(32)        NULL,
    [LastLogin]       DATETIME        NULL,
    [IsActive]        BIT             NOT NULL          CONSTRAINT    [DF_Account_IsActive]    DEFAULT    1,

    CONSTRAINT    [PK_Account]                PRIMARY KEY    ([Id]),
	CONSTRAINT    [CHK_Account_Role]          CHECK          ([Role]            IN    (0,    1,    2)),
    CONSTRAINT    [UQ_Account_Email]          UNIQUE         ([Email]),
    CONSTRAINT    [UQ_Account_SessionGuid]    UNIQUE         ([SessionGuid])
);
GO

CREATE TABLE [Tags] (
    [Id]          INT             IDENTITY(1,1),
    [Title]       VARCHAR(900)    NOT NULL,
    [IsActive]    BIT             NOT NULL          CONSTRAINT    [DF_Tag_IsActive]    DEFAULT    1,

    CONSTRAINT    [PK_Tag]          PRIMARY KEY    ([Id]),
	CONSTRAINT    [UQ_Tag_Title]    UNIQUE         ([Title])
);
GO

CREATE TABLE [Images] (
    [Id]          INT             IDENTITY(1,1),
    [Title]       VARCHAR(900)    NOT NULL,
    [IsActive]    BIT             NOT NULL          CONSTRAINT    [DF_Image_IsActive]    DEFAULT    1,

    CONSTRAINT    [PK_Image]    PRIMARY KEY    ([Id])
);
GO

CREATE TABLE [ImageProperties] (
    [Id]          INT    IDENTITY(1,1),
    [ImageId]     INT    NOT NULL,
    [TagId]       INT    NOT NULL,
    [IsActive]    BIT    NOT NULL          CONSTRAINT    [DF_ImageProperty_IsActive]    DEFAULT    1,

    CONSTRAINT    [PK_ImageProperty]            PRIMARY KEY    ([Id]),
    CONSTRAINT    [FK_ImageProperty_ImageId]    FOREIGN KEY    ([ImageId])    REFERENCES    [Images]    ([Id]),
    CONSTRAINT    [FK_ImageProperty_TagId]      FOREIGN KEY    ([TagId])      REFERENCES    [Tags]      ([Id]),
	CONSTRAINT    [UQ_ImageProperty_Ids]        UNIQUE         ([ImageId],    [TagId])
);
GO

CREATE TABLE [Reactions] (
    [Id]           INT    IDENTITY(1,1),
    [AccountId]    INT    NOT NULL,
    [ImageId]      INT    NOT NULL,
    [IsLiked]      BIT    NOT NULL,
    [IsActive]     BIT    NOT NULL          CONSTRAINT    [DF_Reaction_IsActive]    DEFAULT    1,

    CONSTRAINT    [PK_Reaction]              PRIMARY KEY    ([Id]),
    CONSTRAINT    [FK_Reaction_AccountId]    FOREIGN KEY    ([AccountId])    REFERENCES    [Accounts]    ([Id]),
    CONSTRAINT    [FK_Reaction_ImageId]      FOREIGN KEY    ([ImageId])      REFERENCES    [Images]      ([Id]),
	CONSTRAINT    [UQ_Reaction_Ids]          UNIQUE         ([AccountId],    [ImageId])
);
GO

-- SuperAdmin password: =8K4Kbzt44,Wvv
INSERT INTO [Accounts] (
    [Email],
    [Salt],
    [PasswordHash],
    [Username],
    [Role]
)
VALUES (
    'superadmin@example.com',
    '476F95CB4B4A27F4F08853C327EFD808',
    'B22C85A69EE37B7C5702753E0BD0E5A5AAADF9B21B4DD40C6B3097962A188D8A',
    'SuperAdmin',
    0
);
GO

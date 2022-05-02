CREATE DATABASE [Illustre];
GO

USE [Illustre];
GO

CREATE TABLE [Accounts] (
    [Id]              INT             IDENTITY(1,1),
    [Email]           VARCHAR(320)    NOT NULL,
    [Salt]            BINARY(16)      NOT NULL,
    [PasswordHash]    BINARY(32)      NOT NULL,
    [Username]        VARCHAR(50)     NOT NULL,
    [Role]            TINYINT         NOT NULL,
    [SessionGuid]     BINARY(16)      NULL,
    [LastLogin]       DATETIME        NULL,
    [IsActive]        BIT             NOT NULL          CONSTRAINT    [DF_Account_IsActive]    DEFAULT    1,

    CONSTRAINT    [PK_Account]                PRIMARY KEY    ([Id]),
    CONSTRAINT    [UQ_Account_Email]          UNIQUE         ([Email]),
    CONSTRAINT    [UQ_Account_Username]       UNIQUE         ([Username]),
    CONSTRAINT    [UQ_Account_SessionGuid]    UNIQUE         ([SessionGuid])
);
GO

CREATE TABLE [Tags] (
    [Id]          INT             IDENTITY(1,1),
    [Title]       VARCHAR(MAX)    NOT NULL,
    [IsActive]    BIT             NOT NULL          CONSTRAINT    [DF_Tag_IsActive]    DEFAULT    1,

    CONSTRAINT    [PK_Tag]    PRIMARY KEY    ([Id])
);
GO

CREATE TABLE [Images] (
    [Id]          INT             IDENTITY(1,1),
    [Title]       VARCHAR(MAX)    NOT NULL,
    [IsActive]    BIT             NOT NULL          CONSTRAINT    [DF_Image_IsActive]    DEFAULT    1,

    CONSTRAINT    [PK_Image]    PRIMARY KEY    ([Id])
);
GO

CREATE TABLE [ImageProperties] (
    [Id]          INT    IDENTITY(1,1),
    [ImageId]     INT    NOT NULL,
    [TagId]       INT    NOT NULL,
    [IsActive]    BIT    NOT NULL          CONSTRAINT    [DF_ImageProperty_IsActive]    DEFAULT    1,

    CONSTRAINT    [PK_ImageProperty]              PRIMARY KEY    ([Id]),
    CONSTRAINT    [FK_ImageProperties_ImageId]    FOREIGN KEY    ([ImageId])    REFERENCES    [Images]([Id]),
    CONSTRAINT    [FK_ImageProperties_TagId]      FOREIGN KEY    ([TagId])      REFERENCES    [Tags]([Id])
);
GO

CREATE TABLE [Reactions] (
    [Id]           INT    IDENTITY(1,1),
    [AccountId]    INT    NOT NULL,
    [ImageId]      INT    NOT NULL,
    [IsLiked]      BIT    NOT NULL,
    [IsActive]     BIT    NOT NULL          CONSTRAINT    [DF_Reaction_IsActive]    DEFAULT    1,

    CONSTRAINT    [PK_Reaction]              PRIMARY KEY    ([Id]),
    CONSTRAINT    [FK_Reaction_AccountId]    FOREIGN KEY    ([AccountId])    REFERENCES    [Accounts]([Id]),
    CONSTRAINT    [FK_Reaction_ImageId]      FOREIGN KEY    ([ImageId])      REFERENCES    [Images]([Id])
);
GO

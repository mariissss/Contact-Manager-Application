IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ContactManagerDB')
BEGIN
    CREATE DATABASE ContactManagerDB;
END
GO

USE ContactManagerDB;
GO

IF OBJECT_ID(N'[dbo].[Contacts]', N'U') IS NOT NULL 
    DROP TABLE [dbo].[Contacts];
GO
IF OBJECT_ID(N'[dbo].[CsvFiles]', N'U') IS NOT NULL 
    DROP TABLE [dbo].[CsvFiles];
GO

CREATE TABLE [dbo].[CsvFiles] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [FileName] NVARCHAR(255) NOT NULL,
    [UploadDate] DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE [dbo].[Contacts] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [CsvFileId] INT NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [DateOfBirth] DATE NOT NULL,
    [Married] BIT NOT NULL,
    [Phone] NVARCHAR(50) NOT NULL,
    [Salary] DECIMAL(18, 2) NOT NULL,
    
    CONSTRAINT [FK_Contacts_CsvFiles] FOREIGN KEY ([CsvFileId]) 
        REFERENCES [dbo].[CsvFiles]([Id]) ON DELETE CASCADE
);
GO


-- старий скрипт створення БД (працює до 5 коміту включно), там ще немає таблиці для збереження файлів

--IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ContactManagerDB')
--BEGIN
--    CREATE DATABASE ContactManagerDB;
--END
--GO

--USE ContactManagerDB;
--GO

--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Contacts]') AND type in (N'U'))
--BEGIN
--    CREATE TABLE [dbo].[Contacts] (
--        [Id] INT IDENTITY(1,1) PRIMARY KEY,
--        [Name] NVARCHAR(100) NOT NULL,
--        [DateOfBirth] DATE NOT NULL,
--        [Married] BIT NOT NULL,
--        [Phone] NVARCHAR(50) NOT NULL,
--        [Salary] DECIMAL(18, 2) NOT NULL
--    );
--END
--GO

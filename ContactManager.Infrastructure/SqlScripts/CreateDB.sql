IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ContactManagerDB')
BEGIN
    CREATE DATABASE ContactManagerDB;
END
GO

USE ContactManagerDB;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Contacts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Contacts] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [DateOfBirth] DATE NOT NULL,
        [Married] BIT NOT NULL,
        [Phone] NVARCHAR(50) NOT NULL,
        [Salary] DECIMAL(18, 2) NOT NULL
    );
END
GO
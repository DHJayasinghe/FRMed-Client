USE [master]
GO
CREATE DATABASE [FacialRecMgmtDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'FacialRecMgmtDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA\FacialRecMgmtDB.mdf' , SIZE = 29696KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'FacialRecMgmtDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA\FacialRecMgmtDB_log.ldf' , SIZE = 43264KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [FacialRecMgmtDB] SET COMPATIBILITY_LEVEL = 120
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [FacialRecMgmtDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [FacialRecMgmtDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [FacialRecMgmtDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [FacialRecMgmtDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET  DISABLE_BROKER 
GO
ALTER DATABASE [FacialRecMgmtDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [FacialRecMgmtDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET RECOVERY FULL 
GO
ALTER DATABASE [FacialRecMgmtDB] SET  MULTI_USER 
GO
ALTER DATABASE [FacialRecMgmtDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [FacialRecMgmtDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [FacialRecMgmtDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [FacialRecMgmtDB] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [FacialRecMgmtDB] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'FacialRecMgmtDB', N'ON'
GO
USE [FacialRecMgmtDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdmissionsCorePopulated](
	[PatientID] [uniqueidentifier] NOT NULL,
	[AdmissionID] [int] NOT NULL,
	[AdmissionStartDate] [datetime] NOT NULL,
	[AdmissionEndDate] [datetime] NOT NULL,
 CONSTRAINT [PK_AdmissionsCorePopulated] PRIMARY KEY CLUSTERED 
(
	[PatientID] ASC,
	[AdmissionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AdmissionsDiagnosesCorePopulate](
	[PatientID] [uniqueidentifier] NOT NULL,
	[AdmissionID] [int] NOT NULL,
	[PrimaryDiagnosisCode] [varchar](50) NOT NULL,
	[PrimaryDiagnosisDescription] [varchar](250) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LabsCorePopulated](
	[PatientID] [uniqueidentifier] NOT NULL,
	[AdmissionID] [int] NOT NULL,
	[LabName] [varchar](250) NOT NULL,
	[LabValue] [float] NULL,
	[LabUnits] [varchar](50) NOT NULL,
	[LabDateTime] [datetime] NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PatientCorePopulated](
	[PatientID] [uniqueidentifier] NOT NULL,
	[PatientGender] [varchar](50) NOT NULL,
	[PatientDateOfBirth] [datetime] NOT NULL,
	[PatientRace] [varchar](50) NOT NULL,
	[PatientMaritalStatus] [varchar](50) NOT NULL,
	[PatientLanguage] [varchar](100) NOT NULL,
	[PatientPopulationPercentageBelowPoverty] [float] NULL,
 CONSTRAINT [PK_PatientCorePopulated] PRIMARY KEY CLUSTERED 
(
	[PatientID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PatientProfile](
	[PatientID] [uniqueidentifier] NOT NULL,
	[FirstName] [varchar](200) NOT NULL,
	[LastName] [varchar](200) NOT NULL,
	[ProfilePicture] [varchar](200) NOT NULL,
	[PreCalFaceEncoding] [varchar](max) NULL,
	[CreatedDate] [datetime] NOT NULL CONSTRAINT [DF_PatientProfile_CreatedDate]  DEFAULT (getdate()),
	[ModifiedDate] [datetime] NULL,
 CONSTRAINT [PK_PatientProfile] PRIMARY KEY CLUSTERED 
(
	[PatientID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[AdmissionsCorePopulated]  WITH CHECK ADD  CONSTRAINT [FK_AdmissionsCorePopulated_PatientCorePopulated] FOREIGN KEY([PatientID])
REFERENCES [dbo].[PatientCorePopulated] ([PatientID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AdmissionsCorePopulated] CHECK CONSTRAINT [FK_AdmissionsCorePopulated_PatientCorePopulated]
GO
ALTER TABLE [dbo].[AdmissionsDiagnosesCorePopulate]  WITH CHECK ADD  CONSTRAINT [FK_AdmissionsDiagnosesCorePopulate_AdmissionsCorePopulated] FOREIGN KEY([PatientID], [AdmissionID])
REFERENCES [dbo].[AdmissionsCorePopulated] ([PatientID], [AdmissionID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AdmissionsDiagnosesCorePopulate] CHECK CONSTRAINT [FK_AdmissionsDiagnosesCorePopulate_AdmissionsCorePopulated]
GO
ALTER TABLE [dbo].[LabsCorePopulated]  WITH CHECK ADD  CONSTRAINT [FK_LabsCorePopulated_AdmissionsCorePopulated] FOREIGN KEY([PatientID], [AdmissionID])
REFERENCES [dbo].[AdmissionsCorePopulated] ([PatientID], [AdmissionID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LabsCorePopulated] CHECK CONSTRAINT [FK_LabsCorePopulated_AdmissionsCorePopulated]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Pre-calculated face encoding of Patient Profile Picture' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PatientProfile', @level2type=N'COLUMN',@level2name=N'PreCalFaceEncoding'
GO
USE [master]
GO
ALTER DATABASE [FacialRecMgmtDB] SET  READ_WRITE 
GO

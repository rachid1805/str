-- =============================================
-- **IMPORTANT**
-- Une base de donnée nommée MedWatch doit être créée
-- manuellement avant l'exécution de ce script.
--
-- Script généré avec SQL Server 2012.
-- =============================================

USE [MedWatch]
GO
/****** Object:  Table [dbo].[HospitalEvent]    Script Date: 28/02/2016 6:18:57 PM ******/
IF OBJECT_ID('[dbo].[HospitalEvent]', 'U') IS NOT NULL 
  DROP TABLE [dbo].[HospitalEvent]; 
GO
/****** Object:  Table [dbo].[Hospital]    Script Date: 28/02/2016 6:18:57 PM ******/
IF OBJECT_ID('[dbo].[Hospital]', 'U') IS NOT NULL 
  DROP TABLE [dbo].[Hospital];
GO
/****** Object:  Table [dbo].[Disease]    Script Date: 28/02/2016 6:18:57 PM ******/
IF OBJECT_ID('[dbo].[Disease]', 'U') IS NOT NULL 
  DROP TABLE [dbo].[Disease]
GO
/****** Object:  StoredProcedure [dbo].[HospitalEventSelectCommand]    Script Date: 28/02/2016 6:18:57 PM ******/
IF OBJECT_ID('[dbo].[HospitalEventSelectCommand]', 'P') IS NOT NULL 
  DROP PROCEDURE [dbo].[HospitalEventSelectCommand]
GO
/****** Object:  StoredProcedure [dbo].[HospitalEventInsertCommand]    Script Date: 28/02/2016 6:18:57 PM ******/
IF OBJECT_ID('[dbo].[HospitalEventInsertCommand]', 'P') IS NOT NULL 
  DROP PROCEDURE [dbo].[HospitalEventInsertCommand]
GO
/****** Object:  StoredProcedure [dbo].[HospitalEventInsertCommand]    Script Date: 28/02/2016 6:18:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jean-Sébastien Maheu
-- Create date: 2016-02-27
-- Description:	Insert an event into the database.
-- =============================================
CREATE PROCEDURE [dbo].[HospitalEventInsertCommand] 
	@HospitalId int,
	@PatientId int,
	@EventType smallint, 
	@EventTimestamp datetime2(7),
	@DiseaseType smallint,
	@DoctorId int
AS
BEGIN
	SET NOCOUNT ON;

    INSERT INTO [HospitalEvent] (HospitalId, PatientId, EventType, EventTimestamp, DiseaseType, DoctorId) 
	VALUES (@HospitalId, @PatientId, @EventType, @EventTimestamp, @DiseaseType, @DoctorId)
END

GO
/****** Object:  StoredProcedure [dbo].[HospitalEventSelectCommand]    Script Date: 28/02/2016 6:18:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jean-Sébastien Maheu
-- Create date: 2016-02-27
-- Description:	For a specific hospital, select all the events that are after the provided event id.
-- =============================================
CREATE PROCEDURE [dbo].[HospitalEventSelectCommand]
	@HospitalId int,
	@AfterEventId bigint,
	@MaxEventCount int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP (@MaxEventCount) Id, PatientId, EventType, EventTimestamp, DiseaseType, DoctorId
	FROM [dbo].[HospitalEvent]
	WHERE HospitalId = @HospitalId AND Id > @AfterEventId
END

GO
/****** Object:  Table [dbo].[Hospital]    Script Date: 28/02/2016 6:18:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Hospital](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[AssignedDoctors] [int] NOT NULL,
 CONSTRAINT [PK_Hospital] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Disease]    Script Date: 28/02/2016 6:18:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Disease](
	[Id] [smallint] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Priority] [smallint] NOT NULL,
	[RequiredTime] [int] NOT NULL,
	[TimeUnit] [smallint] NOT NULL,
 CONSTRAINT [PK_Disease] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[HospitalEvent]    Script Date: 28/02/2016 6:18:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HospitalEvent](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[HospitalId] [int] NOT NULL,
	[PatientId] [int] NOT NULL,
	[EventType] [smallint] NOT NULL,
	[EventTimestamp] [datetime2](7) NOT NULL,
	[DiseaseType] [smallint] NULL,
	[DoctorId] [int] NULL,
 CONSTRAINT [PK_Event] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
INSERT [dbo].[Hospital] ([Id], [Name], [AssignedDoctors]) VALUES (1, N'Sainte-Justine', 10)
INSERT [dbo].[Hospital] ([Id], [Name], [AssignedDoctors]) VALUES (2, N'Maisonneuve-Rosemont', 15)
INSERT [dbo].[Hospital] ([Id], [Name], [AssignedDoctors]) VALUES (3, N'Saint-Luc', 8)
INSERT [dbo].[Hospital] ([Id], [Name], [AssignedDoctors]) VALUES (4, N'Notre-Dame', 12)
INSERT [dbo].[Hospital] ([Id], [Name], [AssignedDoctors]) VALUES (5, N'Fleury', 5)
INSERT [dbo].[Hospital] ([Id], [Name], [AssignedDoctors]) VALUES (6, N'Hotel-Dieu', 14)
INSERT [dbo].[Hospital] ([Id], [Name], [AssignedDoctors]) VALUES (7, N'Sacre-Coeur', 13)
INSERT [dbo].[Hospital] ([Id], [Name], [AssignedDoctors]) VALUES (8, N'Royal-Victoria', 7)

INSERT [dbo].[Disease] ([Id], [Name], [Priority], [RequiredTime], [TimeUnit]) VALUES (0, N'Cardiac', 0, 30, 2)
INSERT [dbo].[Disease] ([Id], [Name], [Priority], [RequiredTime], [TimeUnit]) VALUES (1, N'Respiratory-Failure', 0, 25, 2)
INSERT [dbo].[Disease] ([Id], [Name], [Priority], [RequiredTime], [TimeUnit]) VALUES (2, N'Pneumonia', 1, 20, 2)
INSERT [dbo].[Disease] ([Id], [Name], [Priority], [RequiredTime], [TimeUnit]) VALUES (3, N'Bronchitis', 1, 20, 2)
INSERT [dbo].[Disease] ([Id], [Name], [Priority], [RequiredTime], [TimeUnit]) VALUES (4, N'Fracture', 2, 15, 2)
INSERT [dbo].[Disease] ([Id], [Name], [Priority], [RequiredTime], [TimeUnit]) VALUES (5, N'Gastro', 2, 10, 2)
INSERT [dbo].[Disease] ([Id], [Name], [Priority], [RequiredTime], [TimeUnit]) VALUES (6, N'Influenza', 3, 10, 2)
INSERT [dbo].[Disease] ([Id], [Name], [Priority], [RequiredTime], [TimeUnit]) VALUES (7, N'Cold', 4, 5, 2)

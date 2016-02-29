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
DROP TABLE [dbo].[HospitalEvent]
GO
/****** Object:  Table [dbo].[Hospital]    Script Date: 28/02/2016 6:18:57 PM ******/
DROP TABLE [dbo].[Hospital]
GO
/****** Object:  StoredProcedure [dbo].[HospitalEventSelectCommand]    Script Date: 28/02/2016 6:18:57 PM ******/
DROP PROCEDURE [dbo].[HospitalEventSelectCommand]
GO
/****** Object:  StoredProcedure [dbo].[HospitalEventInsertCommand]    Script Date: 28/02/2016 6:18:57 PM ******/
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
	@AfterEventId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Id, PatientId, EventType, EventTimestamp, DiseaseType, DoctorId
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
 CONSTRAINT [PK_Hospital] PRIMARY KEY CLUSTERED 
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
	[Id] [int] IDENTITY(1,1) NOT NULL,
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
INSERT [dbo].[Hospital] ([Id], [Name]) VALUES (1, N'Sainte-Justine')
INSERT [dbo].[Hospital] ([Id], [Name]) VALUES (2, N'Maisonneuve-Rosemont')
INSERT [dbo].[Hospital] ([Id], [Name]) VALUES (3, N'Saint-Luc')
INSERT [dbo].[Hospital] ([Id], [Name]) VALUES (4, N'Notre-Dame')
INSERT [dbo].[Hospital] ([Id], [Name]) VALUES (5, N'Fleury')
INSERT [dbo].[Hospital] ([Id], [Name]) VALUES (6, N'Hotel-Dieu')
INSERT [dbo].[Hospital] ([Id], [Name]) VALUES (7, N'Sacre-Coeur')
INSERT [dbo].[Hospital] ([Id], [Name]) VALUES (8, N'Royal-Victoria')

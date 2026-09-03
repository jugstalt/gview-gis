CREATE DATABASE [#FDB#]
GO
CREATE TABLE [dbo].[FDB_Datasets](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) COLLATE Latin1_General_CI_AS NULL,
	[SpatialReferenceID] [int] NULL,
	[ImageDataset] [bit] NULL,
	[ImageSpace] [nvarchar](255) COLLATE Latin1_General_CI_AS NULL,
 CONSTRAINT [PK_FDB_Datasets] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[FDB_DatasetGeometryType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DatasetID] [int] NULL,
	[GeometryType] [int] NULL,
	[SIMinX] [float] NULL,
	[SIMinY] [float] NULL,
	[SIMaxX] [float] NULL,
	[SIMaxY] [float] NULL,
	[SIRATIO] [float] NULL,
	[MaxPerNode] [int] NULL,
	[MaxLevels] [int] NULL,
 CONSTRAINT [PK_FDB_GeometryType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[FDB_FeatureClasses](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) COLLATE Latin1_General_CI_AS NULL,
	[Aliasname] [nvarchar](255) COLLATE Latin1_General_CI_AS NULL,
	[DatasetID] [int] NULL,
	[GeometryType] [int] NULL,
	[ShapeField] [nvarchar](255) COLLATE Latin1_General_CI_AS NULL,
	[HasZ] [bit] NOT NULL,
	[HasM] [bit] NOT NULL,
	[MinX] [float] NULL,
	[MinY] [float] NULL,
	[MaxX] [float] NULL,
	[MaxY] [float] NULL,
	[FVersion] [bigint] NULL,
	[SI] [nvarchar](50) COLLATE Latin1_General_CI_AS NULL,
	[SIMinX] [float] NULL,
	[SIMinY] [float] NULL,
	[SIMaxX] [float] NULL,
	[SIMaxY] [float] NULL,
	[SIRATIO] [float] NULL,
	[MaxPerNode] [int] NULL,
	[MaxLevels] [int] NULL,
	[SIVersion] [bigint] NULL,
 CONSTRAINT [PK_FDB_FeatureClasses] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[FDB_FeatureClassFields](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FClassID] [int] NULL,
	[FieldName] [ntext] COLLATE Latin1_General_CI_AS NULL,
	[Aliasname] [ntext] COLLATE Latin1_General_CI_AS NULL,
	[FieldType] [int] NULL,
	[DefaultValue] [nvarchar](255) COLLATE Latin1_General_CI_AS NULL,
	[IsRequired] [bit] NOT NULL,
	[IsEditable] [bit] NOT NULL,
	[AutoFieldGUID] [nvarchar](40) COLLATE Latin1_General_CI_AS NULL,
 CONSTRAINT [PK_FDB_FeatureClassFields] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE TABLE [dbo].[FDB_ReleaseInfo](
	[Major] [int] NULL,
	[Minor] [int] NULL,
	[Bugfix] [int] NULL
) ON [PRIMARY]
GO
INSERT INTO [dbo].[FDB_ReleaseInfo] (Major,Minor,Bugfix) VALUES (1,2,0)
GO
CREATE TABLE [dbo].[FDB_SpatialReference](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) COLLATE Latin1_General_CI_AS NULL,
	[Description] [nvarchar](255) COLLATE Latin1_General_CI_AS NULL,
	[Params] [ntext] COLLATE Latin1_General_CI_AS NULL,
	[DatumName] [nvarchar](255) COLLATE Latin1_General_CI_AS NULL,
	[DatumParam] [nvarchar](255) COLLATE Latin1_General_CI_AS NULL,
 CONSTRAINT [PK_FDB_SpatialReference] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE TABLE [dbo].[FDB_NetworkClasses](
	[NetworkId] [int] NULL,
	[FCID] [int] NULL,
	[Properties] [image] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE TABLE [dbo].[FDB_Networks](
	[ID] [int] NULL,
	[Properties] [image] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO



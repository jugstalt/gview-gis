CREATE TABLE [FDB_Datasets](
	[ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[Name] [nvarchar](255) NULL,
	[SpatialReferenceID] [int] NULL,
	[ImageDataset] [bit] NULL,
	[ImageSpace] [nvarchar](255) NULL
)
GO
CREATE TABLE [FDB_DatasetGeometryType](
	[ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[DatasetID] [int] NULL,
	[GeometryType] [int] NULL,
	[SIMinX] [float] NULL,
	[SIMinY] [float] NULL,
	[SIMaxX] [float] NULL,
	[SIMaxY] [float] NULL,
	[SIRATIO] [float] NULL,
	[MaxPerNode] [int] NULL,
	[MaxLevels] [int] NULL
)
GO
CREATE TABLE [FDB_FeatureClasses](
	[ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[Name] [nvarchar](255) NULL,
	[Aliasname] [nvarchar](255) NULL,
	[DatasetID] [int] NULL,
	[GeometryType] [int] NULL,
	[ShapeField] [nvarchar](255) NULL,
	[HasZ] [bit] NOT NULL,
	[HasM] [bit] NOT NULL,
	[MinX] [float] NULL,
	[MinY] [float] NULL,
	[MaxX] [float] NULL,
	[MaxY] [float] NULL,
	[FVersion] [bigint] NULL,
	[SI] [nvarchar](50) NULL,
	[SIMinX] [float] NULL,
	[SIMinY] [float] NULL,
	[SIMaxX] [float] NULL,
	[SIMaxY] [float] NULL,
	[SIRATIO] [float] NULL,
	[MaxPerNode] [int] NULL,
	[MaxLevels] [int] NULL,
	[SIVersion] [bigint] NULL)
GO
CREATE TABLE [FDB_FeatureClassFields](
	[ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[FClassID] [int] NULL,
	[FieldName] [ntext] NULL,
	[Aliasname] [ntext] NULL,
	[FieldType] [int] NULL,
	[DefaultValue] [nvarchar](255) NULL,
	[IsRequired] [bit] NOT NULL,
	[IsEditable] [bit] NOT NULL,
	[AutoFieldGUID] [nvarchar](40) NULL)
GO
CREATE TABLE [FDB_ReleaseInfo](
	[Major] [int] NULL,
	[Minor] [int] NULL,
	[Bugfix] [int] NULL)
GO
INSERT INTO [FDB_ReleaseInfo] (Major,Minor,Bugfix) VALUES (1,2,0)
GO
CREATE TABLE [FDB_SpatialReference](
	[ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[Name] [nvarchar](255)  NULL,
	[Description] [nvarchar](255) NULL,
	[Params] [ntext] NULL,
	[DatumName] [nvarchar](255) NULL,
	[DatumParam] [nvarchar](255) NULL)
GO
CREATE TABLE [FDB_NetworkClasses](
	[NetworkId] [int] NULL,
	[FCID] [int] NULL,
	[Properties] [image] NULL
)
GO
CREATE TABLE [FDB_Networks](
	[ID] [int] NULL,
	[Properties] [image] NULL)
GO



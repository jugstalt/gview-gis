create database #fdb#
	with encoding = 'utf8'
       tablespace = pg_default;

create table "FDB_Datasets"(
	"ID" serial primary key,
	"Name" varchar(255) null,
	"SpatialReferenceID" int null,
	"ImageDataset" boolean null,
	"ImageSpace" varchar(255) null
)
without oids;
 
create table "FDB_DatasetGeometryType"(
	"ID" serial primary key,
	"DatasetID" int null,
	"GeometryType" int null,
	"SIMinX" float8 null,
	"SIMinY" float8 null,
	"SIMaxX" float8 null,
	"SIMaxY" float8 null,
	"SIRATIO" float8 null,
	"MaxPerNode" int null,
	"MaxLevels" int null
)
without oids;
 
create table "FDB_FeatureClasses"(
	"ID" serial primary key,
	"Name" varchar(255) null,
	"Aliasname" varchar(255) null,
	"DatasetID" int null,
	"GeometryType" int null,
	"ShapeField" varchar(255) null,
	"HasZ" boolean  not null,
	"HasM" boolean not null,
	"MinX" float8 null,
	"MinY" float8 null,
	"MaxX" float8 null,
	"MaxY" float8 null,
	"FVersion" bigint null,
	"SI" varchar(50) null,
	"SIMinX" float8 null,
	"SIMinY" float8 null,
	"SIMaxX" float8 null,
	"SIMaxY" float8 null,
	"SIRATIO" float8 null,
	"MaxPerNode" int null,
	"MaxLevels" int null,
	"SIVersion" bigint null
)
without oids;
 
create table "FDB_FeatureClassFields"(
	"ID" serial primary key,
	"FClassID" int null,
	"FieldName" varchar(255) null,
	"Aliasname" varchar(255) null,
	"FieldType" int null,
	"DefaultValue" varchar(255) null,
	"IsRequired" boolean not null,
	"IsEditable" boolean not null,
	"AutoFieldGUID" varchar(40) null
)
without oids;
 
create table "FDB_ReleaseInfo"(
	"Major" int null,
	"Minor" int null,
	"Bugfix" int null
)
without oids;
 
insert into "FDB_ReleaseInfo" ("Major","Minor","Bugfix") values (1,2,0);
 
create table "FDB_SpatialReference"(
	"ID" serial primary key,
	"Name" varchar(255) null,
	"Description" varchar(255) null,
	"Params" varchar(255) null,
	"DatumName" varchar(255) null,
	"DatumParam" varchar(255) null
)
without oids;
 
create table "FDB_NetworkClasses"(
	"NetworkId" int null,
	"FCID" int null,
	"Properties" bytea null
)
without oids;
 
create table "FDB_Networks"(
	"ID" int null,
	"Properties" bytea null
)
without oids;



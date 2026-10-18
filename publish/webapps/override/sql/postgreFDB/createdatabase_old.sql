create database #fdb#
	with encoding = 'utf8'
       tablespace = pg_default;

create table fdb_datasets(
	"id" serial primary key,
	"name" varchar(255) null,
	"spatialreferenceid" int null,
	"imagedataset" boolean null,
	"imagespace" varchar(255) null
)
without oids;
 
create table fdb_datasetgeometrytype(
	"id" serial primary key,
	"datasetid" int null,
	"geometrytype" int null,
	"siminx" float8 null,
	"siminy" float8 null,
	"simaxx" float8 null,
	"simaxy" float8 null,
	"siratio" float8 null,
	"maxpernode" int null,
	"maxlevels" int null
)
without oids;
 
create table fdb_featureclasses(
	"id" serial primary key,
	"name" varchar(255) null,
	"aliasname" varchar(255) null,
	"datasetid" int null,
	"geometrytype" int null,
	"shapefield" varchar(255) null,
	"hasz" boolean  not null,
	"hasm" boolean not null,
	"minx" float8 null,
	"miny" float8 null,
	"maxx" float8 null,
	"maxy" float8 null,
	"fversion" bigint null,
	"si" varchar(50) null,
	"siminx" float8 null,
	"siminy" float8 null,
	"simaxx" float8 null,
	"simaxy" float8 null,
	"siratio" float8 null,
	"maxpernode" int null,
	"maxlevels" int null,
	"siversion" bigint null
)
without oids;
 
create table fdb_featureclassfields(
	"id" serial primary key,
	"fclassid" int null,
	"fieldname" varchar(255) null,
	"aliasname" varchar(255) null,
	"fieldtype" int null,
	"defaultvalue" varchar(255) null,
	"isrequired" boolean not null,
	"iseditable" boolean not null,
	"autofieldguid" varchar(40) null
)
without oids;
 
create table fdb_releaseinfo(
	"major" int null,
	"minor" int null,
	"bugfix" int null
)
without oids;
 
insert into fdb_releaseinfo (major,minor,bugfix) values (1,2,0);
 
create table fdb_spatialreference(
	"id" serial primary key,
	"name" varchar(255) null,
	"description" varchar(255) null,
	"params" varchar(255) null,
	"datumname" varchar(255) null,
	"datumparam" varchar(255) null
)
without oids;
 
create table fdb_networkclasses(
	"networkid" int null,
	"fcid" int null,
	"properties" bytea null
)
without oids;
 
create table fdb_networks(
	"id" int null,
	"properties" bytea null
)
without oids;



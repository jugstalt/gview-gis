echo off

cd ..\blazor\gView.Web

dotnet publish gView.Web.csproj /p:PublishProfile=win64
if errorlevel 1 goto error
dotnet build gView.Web.csproj /p:DeployOnBuild=true /p:PublishProfile=win64
if errorlevel 1 goto error

dotnet publish gView.Web.csproj /p:PublishProfile=linux64
if errorlevel 1 goto error
dotnet build gView.Web.csproj /p:DeployOnBuild=true /p:PublishProfile=linux64
if errorlevel 1 goto error

dotnet publish gView.Web.csproj /p:PublishProfile=docker-linux64
if errorlevel 1 goto error
dotnet build gView.Web.csproj /p:DeployOnBuild=true /p:PublishProfile=docker-linux64
if errorlevel 1 goto error

echo ==================
echo Publish Successful
echo ==================

goto end

:error
echo *****************
echo An error occurred
echo *****************

pause

:end
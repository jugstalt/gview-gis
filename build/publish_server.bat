echo off

cd ..\gView.Server

dotnet publish gView.Server.csproj /p:PublishProfile=win64
if errorlevel 1 goto error
dotnet build gView.Server.csproj /p:DeployOnBuild=true /p:PublishProfile=win64
if errorlevel 1 goto error

dotnet publish gView.Server.csproj /p:PublishProfile=linux64
if errorlevel 1 goto error
dotnet build gView.Server.csproj /p:DeployOnBuild=true /p:PublishProfile=linux64
if errorlevel 1 goto error

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
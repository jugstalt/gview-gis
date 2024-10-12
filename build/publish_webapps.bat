echo off

echo =====================
echo Publish gView.WebApps
echo =====================

cd .\..\src\blazor\gView.WebApps

echo Windows
::dotnet publish -c Release -p:PublishProfile=win64
::if errorlevel 1 goto error

dotnet build -c Release -p:DeployOnBuild=true -p:PublishProfile=win64
if errorlevel 1 goto error

echo Linux
::dotnet publish -c Release -p:PublishProfile=linux64
::if errorlevel 1 goto error

dotnet build -c Release -p:DeployOnBuild=true -p:PublishProfile=linux64
if errorlevel 1 goto error

echo Docker
::dotnet publish -c Release -p:PublishProfile=docker-linux64
::if errorlevel 1 goto error

dotnet build -c Release -p:DeployOnBuild=true -p:PublishProfile=docker-linux64
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

cd .\..\..\..\build
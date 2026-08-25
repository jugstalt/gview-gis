echo off

echo ====================
echo Publish gView.Server
echo ====================

cd .\..\src\gView.Server

echo Windows
dotnet build -c Release -p:DeployOnBuild=true -p:PublishProfile=win64-portable -p:SkiaVariant=Skia3
if errorlevel 1 goto error

echo Linux
dotnet build -c Release -p:DeployOnBuild=true -p:PublishProfile=linux64-portable -p:SkiaVariant=Skia3
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

cd .\..\..\build
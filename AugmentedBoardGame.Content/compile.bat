@echo off
cd %~dp0
if exist compiled goto cleanup
goto compile
:cleanup
rem echo Removing existing compiled assets...
rem del /S /Q compiled
:compile
echo Compiling assets...
cd assets
..\..\Protobuild.exe --execute ProtogameAssetTool ^
  -o ..\compiled ^
  -p Windows
cd ..
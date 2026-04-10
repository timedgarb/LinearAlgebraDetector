@echo off
echo ========================================
echo  Linear Algebra AI Detector - BRAg inc.
echo  Publishing single .exe file...
echo ========================================
echo.

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish

echo.
echo Publish complete! Check the 'publish' folder.
echo Executable: publish\LinearAlgebraDetector.exe
pause
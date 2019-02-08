@echo off

dotnet clean -v m

dotnet restore -v m

dotnet build -v m

for /f "tokens=*" %%i IN ('dir bin\Debug\*.nupkg /s /b') DO (
dotnet nuget push "%%i" -k d3cb76e3-890c-4874-a882-267e518aee3d -s https://www.myget.org/F/ollon-buildtools/
)
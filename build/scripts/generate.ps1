


#Import-Module 'C:\Program Files\dotnet\sdk\NuGetFallbackFolder\microsoft.entityframeworkcore.tools\2.2.0\tools\EntityFrameworkCore.psm1' -Force -Verbose

$DirectoryName = [System.IO.DirectoryInfo]::new($PSScriptRoot).Name


$ErrorActionPreference = [System.Management.Automation.ActionPreference]::Continue

$DbContext = "DatasetApiDbContext"

$BasePath = "$PSScriptRoot\..\..\bin\obj\$DirectoryName\Debug"

$GeneratedDir = "$PSScriptRoot\Generated\"
$OutputGeneratedDir = "$PSScriptRoot\Generated\Models"

$OutputDir = "$PSScriptRoot\Entities"
$ObjDir = "$PSScriptRoot\obj"
$Provider = "Microsoft.EntityFrameworkCore.SqlServer"

$DataSource = $env:ComputerName
$InitialCatalog = "Dataset.Api"

$Connection = "Data Source=$DataSource;Initial Catalog=$InitialCatalog;Integrated Security=True;Pooling=False;MultipleActiveResultSets=True;Connect Timeout=30"

$DbContextArguments = "ef dbcontext scaffold `"$Connection`" `"$Provider`" -o `"$OutputDir`" -c `"$DbContext`" --force --msbuildprojectextensionspath `"$BasePath`""

$MigrationsArguments = "ef migrations add InitialMigration"


if (Test-Path $OutputGeneratedDir)
{
    Remove-Item -Path $OutputGeneratedDir -Recurse -Force -Verbose
    #New-Item -Path $OutputDir -Verbose
}

if (Test-Path $OutputDir)
{
    Remove-Item -Path $OutputDir -Filter '*.cs' -Recurse -Force -Verbose
}
else
{
    New-Item -ItemType Directory -Path $OutputDir -Force -Verbose
}

#Scaffold-DbContext -Connection $Connection -Provider $Provider -OutputDir $OutputDir -Context $DbContext -Force -Verbose

Start-Process dotnet.exe -ArgumentList $DbContextArguments -NoNewWindow -Verbose -Wait

if (Test-Path $ObjDir)
{
    Remove-Item -Path $ObjDir -Recurse -Force -Verbose
}

if (Test-Path $OutputDir)
{
    Move-Item -Path $OutputDir -Destination $GeneratedDir -Force -Verbose
    
}
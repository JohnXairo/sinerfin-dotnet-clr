# build.ps1 — Compila sinerfin-dotnet-clr sin Visual Studio
# Compatible con PowerShell 5.1 (Windows Server 2022)
param([string]$Configuration = "Release")
$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# Buscar nuget.exe en PATH (sin ?. que no existe en PS 5.1)
$nugetCmd = Get-Command nuget.exe -ErrorAction SilentlyContinue
if ($nugetCmd) {
    $nuget = $nugetCmd.Source
} else {
    Write-Host "Descargando nuget.exe..." -ForegroundColor Yellow
    Invoke-WebRequest "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile "$root\nuget.exe"
    $nuget = "$root\nuget.exe"
}

Write-Host "[1/3] Restaurando paquetes..." -ForegroundColor Cyan
& $nuget restore "$root\sinerfin-dotnet-clr.csproj" -PackagesDirectory "$root\packages"
if ($LASTEXITCODE -ne 0) { throw "nuget restore fallo" }

$msbuildPaths = @(
    "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
)
$msbuild = $msbuildPaths | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $msbuild) { throw "MSBuild no encontrado. Instala Build Tools for Visual Studio 2022." }
Write-Host "MSBuild: $msbuild" -ForegroundColor DarkGray

Write-Host "[2/3] Compilando ($Configuration)..." -ForegroundColor Cyan
& $msbuild "$root\sinerfin-dotnet-clr.csproj" /p:Configuration=$Configuration /p:Platform=AnyCPU /verbosity:minimal
if ($LASTEXITCODE -ne 0) { throw "MSBuild fallo" }

$exe = "$root\bin\$Configuration\sinerfin-dotnet-clr.exe"
if (Test-Path $exe) {
    Write-Host "`n[3/3] Build exitoso: $exe" -ForegroundColor Green
    Write-Host "`nInstalar como servicio Windows (como Administrador):"
    Write-Host "  netsh http add urlacl url=http://+:8080/ user=`"NT AUTHORITY\SYSTEM`""
    Write-Host "  cd bin\$Configuration"
    Write-Host "  .\sinerfin-dotnet-clr.exe install"
    Write-Host "  .\sinerfin-dotnet-clr.exe start"
} else { throw "Ejecutable no generado: $exe" }

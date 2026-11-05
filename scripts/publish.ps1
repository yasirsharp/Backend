<#
.SYNOPSIS
  Frontend'i derler, API'nin wwwroot klasörüne kopyalar ve IIS'e kopyalanacak yayın paketini üretir.

.EXAMPLE
  .\scripts\publish.ps1 -FrontendPath ..\sinav-takvimi-frontend
  .\scripts\publish.ps1 -FrontendPath C:\kaynak\sinav-takvimi-frontend -Output C:\yayin\sinav-takvimi
  .\scripts\publish.ps1 -FrontendPath ..\sinav-takvimi-frontend -Runtime win-x64 -WebConfig deploy\web.plesk.config

.NOTES
  Gerekenler: .NET 8 SDK, Node.js 20+ (npm). Çıktı klasörünün içeriği IIS sitesinin fiziksel yoluna kopyalanır.
  Kurulum adımları için docs\KURULUM.md dosyasına bakın.
#>
param(
    [Parameter(Mandatory = $true)][string]$FrontendPath,
    [string]$Output = (Join-Path $PSScriptRoot "..\publish"),
    [switch]$SkipFrontend,
    # Ör. win-x64: sadece o platformun çalışma zamanı dosyaları pakete girer (paket küçülür)
    [string]$Runtime = "",
    # Pakete kopyalanacak web.config şablonu (Plesk + MySQL için deploy\web.plesk.config)
    [string]$WebConfig = "deploy\web.config"
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$apiProject = Join-Path $root "API\API.csproj"
$wwwroot = Join-Path $root "API\wwwroot"

if (-not $SkipFrontend) {
    $FrontendPath = Resolve-Path $FrontendPath
    Write-Host "==> Frontend derleniyor: $FrontendPath" -ForegroundColor Cyan
    Push-Location $FrontendPath
    try {
        # API ile aynı adresten sunulduğu için API adresi boş bırakılır (istekler /api'ye gider)
        $env:VITE_API_BASE_URL = ""
        npm ci
        if ($LASTEXITCODE -ne 0) { throw "npm ci başarısız" }
        npm run build
        if ($LASTEXITCODE -ne 0) { throw "npm run build başarısız" }
    }
    finally { Pop-Location }

    Write-Host "==> dist -> API\wwwroot" -ForegroundColor Cyan
    if (Test-Path $wwwroot) { Remove-Item $wwwroot -Recurse -Force }
    Copy-Item (Join-Path $FrontendPath "dist") $wwwroot -Recurse
}

Write-Host "==> dotnet publish -> $Output" -ForegroundColor Cyan
if (Test-Path $Output) { Remove-Item $Output -Recurse -Force }
dotnet publish $apiProject -c Release -o $Output
if ($LASTEXITCODE -ne 0) { throw "dotnet publish başarısız" }
if ($Runtime) {
    # Taşınabilir paketten hedef platform dışındaki yerel kütüphaneleri çıkar (ör. win-x64 → sadece win, win-x64 kalır).
    # RID'li publish, Core'daki eski Microsoft.AspNetCore.Mvc.Core 2.x paket zinciri yüzünden NU1605 veriyor.
    $family = $Runtime.Split('-')[0]
    Get-ChildItem (Join-Path $Output "runtimes") -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -ne $Runtime -and $_.Name -ne $family } |
        Remove-Item -Recurse -Force
}

Copy-Item (Join-Path $root $WebConfig) (Join-Path $Output "web.config") -Force
New-Item -ItemType Directory -Force -Path (Join-Path $Output "logs") | Out-Null
Copy-Item (Join-Path $root "db\schema.sql") (Join-Path $Output "schema.sql") -Force
Copy-Item (Join-Path $root "db\schema.mysql.sql") (Join-Path $Output "schema.mysql.sql") -Force

Write-Host ""
Write-Host "Hazır: $Output" -ForegroundColor Green
Write-Host "Sonraki adımlar: docs\KURULUM.md (IIS sitesi, ortam değişkenleri, logs klasörüne yazma izni)."

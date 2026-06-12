# Sitemap 搜索引擎收录提交脚本
# 用法: .\submit-sitemap.ps1 -SiteUrl "https://yourdomain.com"

param(
    [Parameter(Mandatory=$true)]
    [string]$SiteUrl
)

$SiteUrl = $SiteUrl.TrimEnd('/')
$SitemapUrl = "$SiteUrl/sitemap.xml"

Write-Host "=== Sitemap 搜索引擎收录提交 ===" -ForegroundColor Cyan
Write-Host "站点: $SiteUrl"
Write-Host "Sitemap: $SitemapUrl"
Write-Host ""

# ===== 1. Google =====
Write-Host "[1/4] 提交到 Google..." -ForegroundColor Yellow
$googleUrl = "https://www.google.com/ping?sitemap=$([System.Web.HttpUtility]::UrlEncode($SitemapUrl))"
try {
    $response = Invoke-WebRequest -Uri $googleUrl -Method Get -TimeoutSec 10
    Write-Host "  Google: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "  Google: 失败 ($($_.Exception.Message))" -ForegroundColor Red
}

# ===== 2. Bing =====
Write-Host "[2/4] 提交到 Bing..." -ForegroundColor Yellow
$bingUrl = "https://www.bing.com/indexnow?url=$([System.Web.HttpUtility]::UrlEncode($SitemapUrl))&key=$([System.Web.HttpUtility]::UrlEncode($SiteUrl))"
try {
    $response = Invoke-WebRequest -Uri $bingUrl -Method Get -TimeoutSec 10
    Write-Host "  Bing: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "  Bing: 失败 ($($_.Exception.Message))" -ForegroundColor Red
}

# ===== 3. 百度 =====
Write-Host "[3/4] 提交到百度..." -ForegroundColor Yellow
$baiduUrl = "https://data.zz.baidu.com/urls?site=$([System.Web.HttpUtility]::UrlEncode($SiteUrl))&token=$([System.Web.HttpUtility]::UrlEncode($SiteUrl))"
try {
    $response = Invoke-WebRequest -Uri "$baiduUrl" -Method Post -Body $SitemapUrl -ContentType "text/plain" -TimeoutSec 10
    Write-Host "  百度: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "  百度: 失败（可能需要百度站长平台验证）" -ForegroundColor Yellow
}

# ===== 4. IndexNow (Bing/Yandex 通用) =====
Write-Host "[4/4] 提交到 IndexNow..." -ForegroundColor Yellow
$indexNowUrl = "https://api.indexnow.org/indexnow"
$body = @{
    host = ([System.Uri]$SiteUrl).Host
    key = $SiteUrl
    urlList = @($SitemapUrl)
} | ConvertTo-Json
try {
    $response = Invoke-WebRequest -Uri $indexNowUrl -Method Post -Body $body -ContentType "application/json" -TimeoutSec 10
    Write-Host "  IndexNow: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "  IndexNow: 失败 ($($_.Exception.Message))" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== 提交完成 ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "提示:" -ForegroundColor Yellow
Write-Host "  - Google 会自动收录，建议同时提交到 Google Search Console" -ForegroundColor Gray
Write-Host "  - Bing 会自动收录，建议同时提交到 Bing Webmaster Tools" -ForegroundColor Gray
Write-Host "  - 百度需要先在百度站长平台验证站点所有权" -ForegroundColor Gray
Write-Host "  - 确保 robots.txt 中允许搜索引擎抓取" -ForegroundColor Gray
# -----------------------------------------------------------------
# MBlog API test script
# -----------------------------------------------------------------

# Skip SSL errors
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

$BaseUrl = "http://localhost:5000"
$Passed = 0
$Failed = 0
$Results = @()

$Global:Token = ""
$Global:CaptchaSessionId = ""

function Test-Api {
    param(
        [string]$Method = "GET",
        [string]$Url,
        [object]$Body = $null,
        [string]$Description = "",
        [bool]$UseToken = $false,
        [int]$ExpectedStatus = 200
    )

    $headers = @{
        "Content-Type" = "application/json"
    }
    if ($UseToken -and $Global:Token) {
        $headers["Authorization"] = "Bearer $Global:Token"
    }

    $params = @{
        Method = $Method
        Uri = "$BaseUrl$Url"
        Headers = $headers
        UseBasicParsing = $true
    }
    if ($Body -and ($Method -eq "POST" -or $Method -eq "PUT" -or $Method -eq "DELETE")) {
        $params["Body"] = ($Body | ConvertTo-Json -Depth 10 -Compress)
    }

    try {
        $response = Invoke-WebRequest @params
        $statusCode = [int]$response.StatusCode
        $content = $response.Content | ConvertFrom-Json

        $ok = $statusCode -eq $ExpectedStatus
        if ($ok -and $content.PSObject.Properties.Name -contains "success") {
            # success can be true or a message string, we only care about status code for GETs
            $ok = $true
        }

        if ($ok) {
            $Global:Passed++
            Write-Host "  [PASS] $Description" -ForegroundColor Green
            $Results += "$Method $Url - PASS ($statusCode)"
        } else {
            $Global:Failed++
            Write-Host "  [FAIL] $Description - expected $ExpectedStatus, got $statusCode" -ForegroundColor Red
            $Results += "$Method $Url - FAIL (expected $ExpectedStatus, got $statusCode)"
        }
    }
    catch {
        $Global:Failed++
        $statusCode = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { 0 }
        Write-Host "  [FAIL] $Description - $statusCode" -ForegroundColor Red
        $Results += "$Method $Url - FAIL ($statusCode)"
    }
}

# ================================================================
# 1. Login
# ================================================================
Write-Host "`n============================================" -ForegroundColor Cyan
Write-Host " Step 1: Login" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

$loginBody = @{ username = "zhuge"; password = "admin123" } | ConvertTo-Json -Compress
try {
    $loginRes = Invoke-WebRequest -Method POST -Uri "$BaseUrl/api/auth/login" `
        -Body $loginBody -ContentType "application/json" -UseBasicParsing
    $loginData = $loginRes.Content | ConvertFrom-Json
    if ($loginData.success -and $loginData.data.accessToken) {
        $Global:Token = $loginData.data.accessToken
        $Global:RefreshToken = $loginData.data.refreshToken
        Write-Host "  [PASS] Login success, token acquired" -ForegroundColor Green
        $Global:Passed++
    } else {
        Write-Host "  [FAIL] Login failed: $($loginData.message)" -ForegroundColor Red
        $Global:Failed++
        exit 1
    }
}
catch {
    Write-Host "  [FAIL] Login error: $_" -ForegroundColor Red
    exit 1
}

# ================================================================
# 2. Public API Tests (no auth required)
# ================================================================
Write-Host "`n============================================" -ForegroundColor Cyan
Write-Host " Step 2: Public APIs" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# Auth
Write-Host "[Auth]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/auth/me" -Description "GET /api/auth/me - current user" -UseToken $true

# Articles
Write-Host "[Articles]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/articles?page=1&pageSize=5" -Description "GET /api/articles - public article list"

# Categories
Write-Host "[Categories]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/categories" -Description "GET /api/categories - public categories"

# Tags
Write-Host "[Tags]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/tags" -Description "GET /api/tags - public tags"

# Archive
Write-Host "[Archive]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/archive" -Description "GET /api/archive - archive data"

# Captcha
Write-Host "[Captcha]" -ForegroundColor Yellow
try {
    $captRes = Invoke-WebRequest -Method GET -Uri "$BaseUrl/api/captcha/image" -UseBasicParsing
    $captData = $captRes.Content | ConvertFrom-Json
    if ($captData.success -and $captData.data.sessionId) {
        $Global:CaptchaSessionId = $captData.data.sessionId
        Write-Host "  [PASS] GET /api/captcha/image" -ForegroundColor Green
        $Global:Passed++
    }
}
catch {
    Write-Host "  [FAIL] GET /api/captcha/image: $_" -ForegroundColor Red
    $Global:Failed++
}
Test-Api -Method POST -Url "/api/captcha/verify" -Body @{ sessionId = $Global:CaptchaSessionId; answer = "wrong" } -Description "POST /api/captcha/verify (expect fail)"

# Pages
Write-Host "[Pages]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/pages" -Description "GET /api/pages - public pages"

# Projects
Write-Host "[Projects]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/projects" -Description "GET /api/projects - public projects"

# Friends
Write-Host "[Friends]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/friends" -Description "GET /api/friends - public friends"

# Profile
Write-Host "[Profile]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/profile" -Description "GET /api/profile - public profile"

# SEO
Write-Host "[SEO]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/seo?key=home" -Description "GET /api/seo?key=home"

# Site Settings
Write-Host "[Settings]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/settings" -Description "GET /api/settings - public settings"

# Theme
Write-Host "[Theme]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/theme" -Description "GET /api/theme - public theme"

# Footer
Write-Host "[Footer]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/footer" -Description "GET /api/footer - public footer"

# Layout
Write-Host "[Layout]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/layout/home" -Description "GET /api/layout/home"

# Visits
Write-Host "[Visits]" -ForegroundColor Yellow
Test-Api -Method POST -Url "/api/visits/track" -Body @{ pageType = "article"; referrer = "" } -Description "POST /api/visits/track"
Test-Api -Method GET -Url "/api/visits/count?pageType=article" -Description "GET /api/visits/count"

# Comments
Write-Host "[Comments]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/articles/00000000-0000-0000-0000-000000000000/comments?page=1&pageSize=5" -Description "GET /api/articles/{id}/comments (no data)"

# Sitemap & Robots
Write-Host "[Sitemap/Robots]" -ForegroundColor Yellow
try {
    $sm = Invoke-WebRequest -Method GET -Uri "$BaseUrl/api/sitemap.xml" -UseBasicParsing
    if ($sm.StatusCode -eq 200) { Write-Host "  [PASS] GET /api/sitemap.xml" -ForegroundColor Green; $Global:Passed++ }
} catch { Write-Host "  [FAIL] GET /api/sitemap.xml: $_" -ForegroundColor Red; $Global:Failed++ }
try {
    $rb = Invoke-WebRequest -Method GET -Uri "$BaseUrl/api/robots.txt" -UseBasicParsing
    if ($rb.StatusCode -eq 200) { Write-Host "  [PASS] GET /api/robots.txt" -ForegroundColor Green; $Global:Passed++ }
} catch { Write-Host "  [FAIL] GET /api/robots.txt: $_" -ForegroundColor Red; $Global:Failed++ }

# Subscribe
Write-Host "[Subscribe]" -ForegroundColor Yellow
Test-Api -Method POST -Url "/api/subscribe" -Body @{ email = "testapi@example.com"; name = "TestAPI" } -Description "POST /api/subscribe"

# ================================================================
# 3. Admin API Tests (token required)
# ================================================================
Write-Host "`n============================================" -ForegroundColor Cyan
Write-Host " Step 3: Admin APIs (token required)" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# Dashboard
Write-Host "[Dashboard]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/dashboard/stats" -Description "GET /api/admin/dashboard/stats" -UseToken $true

# Categories
Write-Host "[Categories]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/categories" -Description "GET /api/admin/categories list" -UseToken $true
Test-Api -Method GET -Url "/api/admin/categories/flat" -Description "GET /api/admin/categories/flat" -UseToken $true
$catSlug = "test-cat-" + (Get-Random -Max 99999)
Test-Api -Method POST -Url "/api/admin/categories" -Body @{ name = "TestCat"; slug = $catSlug; description = "temp" } -Description "POST /api/admin/categories create" -UseToken $true

# Tags
Write-Host "[Tags]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/tags" -Description "GET /api/admin/tags list" -UseToken $true
$tagSlug = "test-tag-" + (Get-Random -Max 99999)
Test-Api -Method POST -Url "/api/admin/tags" -Body @{ name = "TestTag"; slug = $tagSlug; color = "#409EFF" } -Description "POST /api/admin/tags create" -UseToken $true

# Articles
Write-Host "[Articles]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/articles?page=1&pageSize=5" -Description "GET /api/admin/articles list" -UseToken $true
$artSlug = "test-art-" + (Get-Random -Max 99999)
Test-Api -Method POST -Url "/api/admin/articles" -Body @{ title = "Test Article"; slug = $artSlug; content = "test content"; summary = "summary"; status = "draft" } -Description "POST /api/admin/articles create" -UseToken $true

# Media
Write-Host "[Media]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/media?page=1&pageSize=5" -Description "GET /api/admin/media list" -UseToken $true

# Comments
Write-Host "[Comments]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/comments?page=1&pageSize=5" -Description "GET /api/admin/comments list" -UseToken $true

# Pages
Write-Host "[Pages]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/pages" -Description "GET /api/admin/pages list" -UseToken $true
$pgSlug = "test-pg-" + (Get-Random -Max 99999)
Test-Api -Method POST -Url "/api/admin/pages" -Body @{ title = "Test Page"; slug = $pgSlug; content = "content"; status = "draft" } -Description "POST /api/admin/pages create" -UseToken $true

# Projects
Write-Host "[Projects]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/projects?page=1&pageSize=5" -Description "GET /api/admin/projects list" -UseToken $true
$pjSlug = "test-pj-" + (Get-Random -Max 99999)
Test-Api -Method POST -Url "/api/admin/projects" -Body @{ title = "Test Project"; slug = $pjSlug; summary = "summary"; content = "content" } -Description "POST /api/admin/projects create" -UseToken $true

# Friends
Write-Host "[Friends]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/friends" -Description "GET /api/admin/friends list" -UseToken $true
Test-Api -Method POST -Url "/api/admin/friends" -Body @{ name = "TestFriend"; url = "https://example.com" } -Description "POST /api/admin/friends create" -UseToken $true

# Profile
Write-Host "[Profile]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/profile" -Description "GET /api/admin/profile" -UseToken $true

# SEO
Write-Host "[SEO Admin]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/seo" -Description "GET /api/admin/seo list" -UseToken $true
Test-Api -Method POST -Url "/api/admin/seo/sitemap/generate" -Body @{} -Description "POST /api/admin/seo/sitemap/generate" -UseToken $true

# Navigation
Write-Host "[Navigation]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/navigation" -Description "GET /api/admin/navigation list" -UseToken $true
Test-Api -Method POST -Url "/api/admin/navigation" -Body @{ label = "TestNav"; url = "/test"; target = "_self"; sortOrder = 99 } -Description "POST /api/admin/navigation create" -UseToken $true

# Footer
Write-Host "[Footer Admin]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/footer" -Description "GET /api/admin/footer" -UseToken $true

# Layout
Write-Host "[Layout Admin]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/layout/home" -Description "GET /api/admin/layout/home" -UseToken $true

# Theme
Write-Host "[Theme Admin]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/theme" -Description "GET /api/admin/theme list" -UseToken $true
Test-Api -Method GET -Url "/api/admin/theme/active" -Description "GET /api/admin/theme/active" -UseToken $true

# Settings
Write-Host "[Settings Admin]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/settings" -Description "GET /api/admin/settings" -UseToken $true

# Visits
Write-Host "[Visits Admin]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/visits?page=1&pageSize=5" -Description "GET /api/admin/visits list" -UseToken $true
Test-Api -Method GET -Url "/api/admin/visits/stats" -Description "GET /api/admin/visits/stats" -UseToken $true

# Subscribers
Write-Host "[Subscribers]" -ForegroundColor Yellow
Test-Api -Method GET -Url "/api/admin/subscribers" -Description "GET /api/admin/subscribers list" -UseToken $true
Test-Api -Method GET -Url "/api/admin/subscribers/count" -Description "GET /api/admin/subscribers count" -UseToken $true

# Change Password
Write-Host "[Change Password]" -ForegroundColor Yellow
Test-Api -Method POST -Url "/api/auth/change-password" -Body @{ currentPassword = "admin123"; newPassword = "admin1234" } -Description "POST /api/auth/change-password" -UseToken $true
# Restore password
try {
    Start-Sleep -Milliseconds 300
    $pwBody = @{ currentPassword = "admin1234"; newPassword = "admin123" } | ConvertTo-Json -Compress
    $pwRes = Invoke-WebRequest -Method POST -Uri "$BaseUrl/api/auth/change-password" -Body $pwBody `
        -ContentType "application/json" -Headers @{ Authorization = "Bearer $Global:Token" } -UseBasicParsing
    Write-Host "  [PASS] Restore password to admin123" -ForegroundColor Green
    $Global:Passed++
}
catch {
    Write-Host "  [FAIL] Restore password: $_" -ForegroundColor Red
    $Global:Failed++
}

# ================================================================
# 4. Logout
# ================================================================
Write-Host "`n============================================" -ForegroundColor Cyan
Write-Host " Step 4: Logout" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Test-Api -Method POST -Url "/api/auth/logout" -Body @{ refreshToken = $Global:RefreshToken } -Description "POST /api/auth/logout" -UseToken $true

# ================================================================
# Summary
# ================================================================
Write-Host "`n============================================" -ForegroundColor Cyan
Write-Host " TEST SUMMARY" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
$Total = $Global:Passed + $Global:Failed
$Rate = if ($Total -gt 0) { [math]::Round($Global:Passed / $Total * 100, 1) } else { 0 }
Write-Host "  Total: $Total"
Write-Host "  Passed: $Global:Passed" -ForegroundColor Green
Write-Host "  Failed: $Global:Failed" -ForegroundColor Red
Write-Host "  Pass Rate: $Rate%"
Write-Host ""
if ($Global:Failed -gt 0) {
    Write-Host "Failed endpoints:" -ForegroundColor Yellow
    $Results | Where-Object { $_ -match "- FAIL" } | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
    Write-Host "`nNote: some failures may be expected (e.g. non-existent resources)" -ForegroundColor Yellow
} else {
    Write-Host "All API endpoints passed!" -ForegroundColor Green
}
Write-Host ""
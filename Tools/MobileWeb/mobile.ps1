[CmdletBinding()]
param(
    [ValidateSet('Build', 'Analyze', 'Serve', 'All')]
    [string]$Mode = 'All',
    [int]$Port = 8080,
    [switch]$NoBrowser
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$projectVersionFile = Join-Path $repoRoot 'ProjectSettings\ProjectVersion.txt'
$buildDir = Join-Path $repoRoot 'Builds\WebGL-Mobile'
$reportDir = Join-Path $repoRoot 'Builds\Reports'
$logDir = Join-Path $repoRoot 'Builds\Logs'
$logFile = Join-Path $logDir 'mobile-webgl-build.log'

function Write-Step([string]$Text) {
    Write-Host "`n==> $Text" -ForegroundColor Cyan
}

function Require-Node {
    $node = Get-Command node -ErrorAction SilentlyContinue
    if (-not $node) {
        throw 'Node.js is required for mobile validation, build analysis and local serving and was not found in PATH.'
    }
    return $node.Source
}

function Invoke-SourceValidation {
    $node = Require-Node
    New-Item -ItemType Directory -Force -Path $reportDir | Out-Null

    Write-Step 'Validating mobile source contract'
    & $node (Join-Path $PSScriptRoot 'preflight.mjs') --root $repoRoot
    if ($LASTEXITCODE -ne 0) {
        throw "Mobile source preflight failed with exit code $LASTEXITCODE."
    }

    Write-Step 'Auditing SampleScene mobile complexity'
    & $node (Join-Path $PSScriptRoot 'audit-scene.mjs') --root $repoRoot --report-dir $reportDir
    if ($LASTEXITCODE -ne 0) {
        throw "Mobile scene audit failed with exit code $LASTEXITCODE."
    }
}

function Get-UnityVersion {
    if (-not (Test-Path $projectVersionFile)) {
        throw "Missing $projectVersionFile"
    }

    $line = Get-Content $projectVersionFile | Where-Object { $_ -match '^m_EditorVersion:\s*(.+)$' } | Select-Object -First 1
    if (-not $line) {
        throw 'Could not read m_EditorVersion from ProjectVersion.txt.'
    }

    return ($line -replace '^m_EditorVersion:\s*', '').Trim()
}

function Find-UnityEditor([string]$Version) {
    $candidates = New-Object System.Collections.Generic.List[string]

    if ($env:UNITY_PATH) {
        $candidates.Add($env:UNITY_PATH)
    }

    if ($env:UNITY_EDITOR_PATH) {
        $candidates.Add($env:UNITY_EDITOR_PATH)
    }

    if ($env:ProgramFiles) {
        $candidates.Add((Join-Path $env:ProgramFiles "Unity\Hub\Editor\$Version\Editor\Unity.exe"))
    }

    if (${env:ProgramFiles(x86)}) {
        $candidates.Add((Join-Path ${env:ProgramFiles(x86)} "Unity\Hub\Editor\$Version\Editor\Unity.exe"))
    }

    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path $candidate)) {
            return (Resolve-Path $candidate).Path
        }
    }

    $hubRoot = if ($env:ProgramFiles) { Join-Path $env:ProgramFiles 'Unity\Hub\Editor' } else { $null }
    if ($hubRoot -and (Test-Path $hubRoot)) {
        $fallback = Get-ChildItem $hubRoot -Directory -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -like '6000.4*' } |
            Sort-Object Name -Descending |
            ForEach-Object { Join-Path $_.FullName 'Editor\Unity.exe' } |
            Where-Object { Test-Path $_ } |
            Select-Object -First 1

        if ($fallback) {
            Write-Warning "Exact Unity $Version was not found. Falling back to $fallback"
            return $fallback
        }
    }

    throw @"
Unity $Version was not found.
Install that editor version in Unity Hub, or set UNITY_PATH to Unity.exe.
Expected default path:
  C:\Program Files\Unity\Hub\Editor\$Version\Editor\Unity.exe
"@
}

function Invoke-MobileBuild {
    $version = Get-UnityVersion
    $unity = Find-UnityEditor $version

    New-Item -ItemType Directory -Force -Path $logDir | Out-Null
    Write-Step "Building Mobile WebGL with Unity $version"
    Write-Host "Unity: $unity"
    Write-Host "Log:   $logFile"

    $arguments = @(
        '-batchmode',
        '-nographics',
        '-quit',
        '-projectPath', $repoRoot,
        '-executeMethod', 'Karlolegend.Gradomraz.Editor.GradomrazBuild.BuildMobileWebGL',
        '-logFile', $logFile
    )

    & $unity @arguments
    $exitCode = $LASTEXITCODE

    if ($exitCode -ne 0) {
        Write-Host "`nLast Unity log lines:" -ForegroundColor Yellow
        if (Test-Path $logFile) {
            Get-Content $logFile -Tail 160
        }
        throw "Unity Mobile WebGL build failed with exit code $exitCode."
    }

    if (-not (Test-Path (Join-Path $buildDir 'index.html'))) {
        throw "Unity exited successfully but $buildDir\index.html does not exist."
    }

    Write-Host 'Unity produced the mobile WebGL output.' -ForegroundColor Green
}

function Invoke-SmokeTest {
    $node = Require-Node
    Write-Step 'Smoke-testing generated Mobile WebGL package'
    & $node (Join-Path $PSScriptRoot 'smoke-build.mjs') --root $buildDir
    if ($LASTEXITCODE -ne 0) {
        throw "Generated WebGL smoke test failed with exit code $LASTEXITCODE."
    }
}

function Invoke-Analysis {
    $node = Require-Node
    Write-Step 'Analyzing mobile WebGL payload'
    & $node (Join-Path $PSScriptRoot 'analyze-build.mjs') --root $buildDir --report-dir $reportDir
    if ($LASTEXITCODE -ne 0) {
        throw "Build analyzer failed with exit code $LASTEXITCODE."
    }
}

function Invoke-VerifiedBuild {
    Invoke-SourceValidation
    Invoke-MobileBuild
    Invoke-SmokeTest
    Invoke-Analysis
    Write-Host "`nMobile build passed source validation, Unity build, package smoke test and payload analysis." -ForegroundColor Green
}

function Invoke-Server {
    if (-not (Test-Path (Join-Path $buildDir 'index.html'))) {
        throw "No mobile build found at $buildDir. Run Build first."
    }

    $node = Require-Node
    Write-Step "Serving mobile build on port $Port"

    if (-not $NoBrowser) {
        Start-Process "http://localhost:$Port/"
    }

    & $node (Join-Path $PSScriptRoot 'serve.mjs') --root $buildDir --port $Port --host 0.0.0.0
    if ($LASTEXITCODE -ne 0) {
        throw "Local server exited with code $LASTEXITCODE."
    }
}

Push-Location $repoRoot
try {
    switch ($Mode) {
        'Build'   { Invoke-VerifiedBuild }
        'Analyze' { Invoke-Analysis }
        'Serve'   { Invoke-Server }
        'All' {
            Invoke-VerifiedBuild
            Invoke-Server
        }
    }
}
finally {
    Pop-Location
}

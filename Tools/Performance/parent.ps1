[CmdletBinding()]
param(
    [ValidateSet('Web', 'Windows', 'Both', 'Serve')]
    [string]$Mode = 'Both',
    [int]$Port = 8080,
    [switch]$NoBrowser
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$projectVersionFile = Join-Path $repoRoot 'ProjectSettings\ProjectVersion.txt'
$webDir = Join-Path $repoRoot 'Builds\WebGL'
$windowsExe = Join-Path $repoRoot 'Builds\GRADOMRAZ-Windows\GRADOMRAZ.exe'
$reportDir = Join-Path $repoRoot 'Builds\Reports'
$logDir = Join-Path $repoRoot 'Builds\Logs'
$webLog = Join-Path $logDir 'parent-webgl-build.log'
$windowsLog = Join-Path $logDir 'parent-windows-build.log'

function Write-Step([string]$Text) {
    Write-Host "`n==> $Text" -ForegroundColor Cyan
}

function Require-Node {
    $node = Get-Command node -ErrorAction SilentlyContinue
    if (-not $node) { throw 'Node.js was not found in PATH.' }
    return $node.Source
}

function Get-UnityVersion {
    $line = Get-Content $projectVersionFile | Where-Object { $_ -match '^m_EditorVersion:\s*(.+)$' } | Select-Object -First 1
    if (-not $line) { throw 'Could not read Unity version from ProjectVersion.txt.' }
    return ($line -replace '^m_EditorVersion:\s*', '').Trim()
}

function Find-UnityEditor([string]$Version) {
    $candidates = @()
    if ($env:UNITY_PATH) { $candidates += $env:UNITY_PATH }
    if ($env:UNITY_EDITOR_PATH) { $candidates += $env:UNITY_EDITOR_PATH }
    if ($env:ProgramFiles) { $candidates += (Join-Path $env:ProgramFiles "Unity\Hub\Editor\$Version\Editor\Unity.exe") }
    if (${env:ProgramFiles(x86)}) { $candidates += (Join-Path ${env:ProgramFiles(x86)} "Unity\Hub\Editor\$Version\Editor\Unity.exe") }

    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path $candidate)) { return (Resolve-Path $candidate).Path }
    }

    throw "Unity $Version was not found. Install it in Unity Hub or set UNITY_PATH to Unity.exe."
}

function Invoke-SourceValidation {
    $node = Require-Node
    New-Item -ItemType Directory -Force -Path $reportDir | Out-Null

    Write-Step 'Validating OKICE performance source contract'
    & $node (Join-Path $repoRoot 'Tools\MobileWeb\preflight.mjs') --root $repoRoot
    if ($LASTEXITCODE -ne 0) { throw "Source preflight failed with exit code $LASTEXITCODE." }

    Write-Step 'Auditing SampleScene complexity'
    & $node (Join-Path $repoRoot 'Tools\MobileWeb\audit-scene.mjs') --root $repoRoot --report-dir $reportDir
    if ($LASTEXITCODE -ne 0) { throw "Scene audit failed with exit code $LASTEXITCODE." }
}

function Invoke-UnityBuild([string]$Method, [string]$LogFile, [string]$ExpectedOutput, [string]$Label) {
    $version = Get-UnityVersion
    $unity = Find-UnityEditor $version
    New-Item -ItemType Directory -Force -Path $logDir | Out-Null

    Write-Step "$Label with Unity $version"
    Write-Host "Unity: $unity"
    Write-Host "Log:   $LogFile"

    & $unity '-batchmode' '-nographics' '-quit' '-projectPath' $repoRoot '-executeMethod' $Method '-logFile' $LogFile
    $exitCode = $LASTEXITCODE

    if ($exitCode -ne 0) {
        if (Test-Path $LogFile) {
            Write-Host "`nLast Unity log lines:" -ForegroundColor Yellow
            Get-Content $LogFile -Tail 180
        }
        throw "$Label failed with exit code $exitCode."
    }

    if (-not (Test-Path $ExpectedOutput)) {
        throw "$Label exited without an error but expected output was not created: $ExpectedOutput"
    }

    Write-Host "$Label produced successfully." -ForegroundColor Green
}

function Invoke-WebBuild {
    Invoke-UnityBuild 'Karlolegend.Gradomraz.Editor.GradomrazBuild.BuildWebGL' $webLog (Join-Path $webDir 'index.html') 'Parent-compatible WebGL build'

    $node = Require-Node
    Write-Step 'Smoke-testing parent-compatible WebGL'
    & $node (Join-Path $repoRoot 'Tools\MobileWeb\smoke-build.mjs') --root $webDir
    if ($LASTEXITCODE -ne 0) { throw "WebGL smoke test failed with exit code $LASTEXITCODE." }

    Write-Step 'Analyzing parent-compatible WebGL payload'
    & $node (Join-Path $repoRoot 'Tools\MobileWeb\analyze-build.mjs') --root $webDir --report-dir $reportDir
    if ($LASTEXITCODE -ne 0) { throw "WebGL payload analysis failed with exit code $LASTEXITCODE." }
}

function Invoke-WindowsBuild {
    Invoke-UnityBuild 'Karlolegend.Gradomraz.Editor.GradomrazBuild.BuildWindows64' $windowsLog $windowsExe 'Parent-compatible Windows build'
}

function Invoke-Server {
    if (-not (Test-Path (Join-Path $webDir 'index.html'))) {
        throw 'No parent-compatible WebGL build exists. Run Web or Both first.'
    }

    $node = Require-Node
    Write-Step "Serving parent-compatible WebGL on port $Port"
    if (-not $NoBrowser) { Start-Process "http://localhost:$Port/" }
    & $node (Join-Path $repoRoot 'Tools\MobileWeb\serve.mjs') --root $webDir --port $Port --host 0.0.0.0
    if ($LASTEXITCODE -ne 0) { throw "Local server exited with code $LASTEXITCODE." }
}

Push-Location $repoRoot
try {
    switch ($Mode) {
        'Web' {
            Invoke-SourceValidation
            Invoke-WebBuild
        }
        'Windows' {
            Invoke-SourceValidation
            Invoke-WindowsBuild
        }
        'Both' {
            Invoke-SourceValidation
            Invoke-WebBuild
            Invoke-WindowsBuild
            Write-Host "`nParent baseline outputs are ready:" -ForegroundColor Green
            Write-Host "  Web:     $webDir"
            Write-Host "  Windows: $windowsExe"
        }
        'Serve' { Invoke-Server }
    }
}
finally {
    Pop-Location
}

param(
    [Parameter(Mandatory = $true)] [string]$UnityEditorPath,
    [string]$ReportDirectory,
    [string]$ValidationProjectPath,
    [string]$TestFilter
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$validationRoot = if ($ValidationProjectPath) { [System.IO.Path]::GetFullPath($ValidationProjectPath) }
    else { Join-Path ([System.IO.Path]::GetTempPath()) ('ARPG-ScriptsTests-' + [guid]::NewGuid().ToString('N')) }
$markerPath = Join-Path $validationRoot 'scripts-validation-project.txt'
if (Test-Path -LiteralPath $validationRoot)
{
    if (!(Test-Path -LiteralPath $markerPath) -or (Get-Content -LiteralPath $markerPath -Raw) -cne $projectRoot)
    { throw 'ValidationProjectPath must be a validation project previously created by this runner.' }
    foreach ($relativePath in @('Assets\Scripts', 'Assets\ScriptableObject', 'Assets\Plugins\UniTask'))
    {
        $targetPath = [System.IO.Path]::GetFullPath((Join-Path $validationRoot $relativePath))
        if (!$targetPath.StartsWith($validationRoot.TrimEnd('\') + '\', [System.StringComparison]::OrdinalIgnoreCase))
        { throw "Validation copy is outside the validation project: $targetPath" }
        if (Test-Path -LiteralPath $targetPath) { Remove-Item -LiteralPath $targetPath -Recurse -Force }
    }
}
$assetsRoot = Join-Path $validationRoot 'Assets'
$editorRoot = Join-Path $assetsRoot 'Editor'
$pluginRoot = Join-Path $assetsRoot 'Plugins'
$settingsRoot = Join-Path $validationRoot 'ProjectSettings'
$packageRoot = Join-Path $validationRoot 'Packages'
$probeRoot = Join-Path $assetsRoot 'Validation'
New-Item -ItemType Directory -Path $editorRoot, $pluginRoot, $settingsRoot, $packageRoot, $probeRoot -Force | Out-Null
[System.IO.File]::WriteAllText($markerPath, $projectRoot)
Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets\Scripts') -Destination $assetsRoot -Recurse -Force
Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets\ScriptableObject') -Destination $assetsRoot -Recurse -Force
Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets\Plugins\UniTask\Runtime') -Destination (Join-Path $pluginRoot 'UniTask') -Recurse -Force
Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets\Plugins\Sirenix\Assemblies\Sirenix.OdinInspector.Attributes.dll') -Destination $pluginRoot
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'ScriptCleanupRegressionTests.cs') -Destination $editorRoot
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'PoolTimerProbe.cs') -Destination $probeRoot
# Trigger tags belong to the isolated fixture; do not edit the original project settings.
$tagSettingsPath = Join-Path $settingsRoot 'TagManager.asset'
$tagSettings = Get-Content -LiteralPath (Join-Path $projectRoot 'ProjectSettings\TagManager.asset') -Raw -Encoding UTF8
if ($tagSettings.Contains('tags: []')) { $tagSettings = $tagSettings.Replace('tags: []', ('tags:' + [Environment]::NewLine + '  - Water' + [Environment]::NewLine + '  - Zipline')) }
[System.IO.File]::WriteAllText($tagSettingsPath, $tagSettings, [System.Text.UTF8Encoding]::new($false))
Copy-Item -LiteralPath (Join-Path $projectRoot 'ProjectSettings\ProjectVersion.txt') -Destination $settingsRoot
Copy-Item -LiteralPath (Join-Path $projectRoot 'ProjectSettings\ProjectSettings.asset') -Destination $settingsRoot
if (Test-Path -LiteralPath (Join-Path $projectRoot 'UserSettings\Search.index'))
{
    $searchSettingsRoot = Join-Path $validationRoot 'UserSettings'
    New-Item -ItemType Directory -Path $searchSettingsRoot -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $projectRoot 'UserSettings\Search.index') -Destination $searchSettingsRoot
}
$manifest = Get-Content -LiteralPath (Join-Path $projectRoot 'Packages\manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$dependencies = [ordered]@{}
foreach ($dependency in $manifest.dependencies.PSObject.Properties)
{
    if ($dependency.Name.StartsWith('com.unity.modules.')) { $dependencies[$dependency.Name] = $dependency.Value }
}
foreach ($packageName in @('com.unity.inputsystem', 'com.unity.ugui'))
{
    $cachedPackage = Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Library\PackageCache') -Directory |
        Where-Object { $_.Name.StartsWith($packageName + '@') } | Select-Object -First 1
    $dependencies[$packageName] = if ($cachedPackage) { 'file:' + $cachedPackage.FullName.Replace('\', '/') } else { $manifest.dependencies.$packageName }
}
[System.IO.File]::WriteAllText((Join-Path $packageRoot 'manifest.json'), (@{ dependencies = $dependencies } | ConvertTo-Json -Depth 5), [System.Text.UTF8Encoding]::new($false))
$logPath = Join-Path $validationRoot 'editor.log'
if (Test-Path -LiteralPath (Join-Path $validationRoot 'results.json')) { Remove-Item -LiteralPath (Join-Path $validationRoot 'results.json') }
Write-Output "Validation project: $validationRoot"
$arguments = @('-batchmode', '-nographics', '-projectPath', ('"' + $validationRoot + '"'),
    '-executeMethod', 'ScriptCleanupRegressionTests.Run', '-logFile', ('"' + $logPath + '"'))
if ($TestFilter) { $arguments += @('-arpgTestFilter', ('"' + $TestFilter + '"')) }
$process = Start-Process -FilePath $UnityEditorPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
$timer = [System.Diagnostics.Stopwatch]::StartNew()
while (!$process.WaitForExit(1000))
{
    if ($timer.Elapsed.TotalSeconds -gt 300) { $process.Kill(); throw "Scripts tests timed out. See $logPath" }
}
$reportPath = Join-Path $validationRoot 'results.json'
if ($ReportDirectory)
{
    New-Item -ItemType Directory -Path $ReportDirectory -Force | Out-Null
    Copy-Item -LiteralPath $logPath -Destination (Join-Path $ReportDirectory 'unity-editor.log')
    if (Test-Path -LiteralPath $reportPath) { Copy-Item -LiteralPath $reportPath -Destination (Join-Path $ReportDirectory 'results.json') }
}
if (!(Test-Path -LiteralPath $reportPath))
{
    Get-Content -LiteralPath $logPath -Tail 70 -Encoding UTF8
    throw "Unity did not produce results. See $logPath"
}
Get-Content -LiteralPath $reportPath -Encoding UTF8
if ($process.ExitCode -ne 0) { throw "Scripts regression tests failed. See $logPath" }

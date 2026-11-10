param(
    [Parameter(Mandatory = $true)] [string]$UnityEditorPath
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$validationRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('ARPG-MeleeTests-' + [guid]::NewGuid().ToString('N'))
$assetsRoot = Join-Path $validationRoot 'Assets'
$editorRoot = Join-Path $assetsRoot 'Editor'
$pluginRoot = Join-Path $assetsRoot 'Plugins'
$settingsRoot = Join-Path $validationRoot 'ProjectSettings'
$packageRoot = Join-Path $validationRoot 'Packages'
New-Item -ItemType Directory -Path $editorRoot, $pluginRoot, $settingsRoot, $packageRoot -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets\Scripts') -Destination $assetsRoot -Recurse
Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets\Plugins\UniTask\Runtime') -Destination (Join-Path $pluginRoot 'UniTask') -Recurse
Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets\Plugins\Sirenix\Assemblies\Sirenix.OdinInspector.Attributes.dll') -Destination $pluginRoot
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'MeleeStateMachineRegressionTests.cs') -Destination $editorRoot
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'MeleeInputRegressionTests.cs') -Destination $editorRoot
Copy-Item -LiteralPath (Join-Path $projectRoot 'ProjectSettings\ProjectVersion.txt') -Destination $settingsRoot
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
Write-Output "Validation project: $validationRoot"
$arguments = @('-batchmode', '-nographics', '-projectPath', ('"' + $validationRoot + '"'),
    '-executeMethod', 'MeleeStateMachineRegressionTests.Run', '-logFile', ('"' + $logPath + '"'))
$process = Start-Process -FilePath $UnityEditorPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
$timer = [System.Diagnostics.Stopwatch]::StartNew()
while (!$process.WaitForExit(1000))
{
    if ($timer.Elapsed.TotalSeconds -gt 300) { $process.Kill(); throw "Melee tests timed out. See $logPath" }
}
$reportPath = Join-Path $validationRoot 'results.json'
if (!(Test-Path -LiteralPath $reportPath))
{
    Get-Content -LiteralPath $logPath -Tail 70 -Encoding UTF8
    throw "Unity did not produce results. See $logPath"
}
Get-Content -LiteralPath $reportPath -Encoding UTF8
if ($process.ExitCode -ne 0) { throw "Melee regression tests failed. See $logPath" }

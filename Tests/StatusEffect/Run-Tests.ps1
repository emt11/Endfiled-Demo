param(
    [Parameter(Mandatory = $true)]
    [string]$UnityEditorPath
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$validationRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('ARPG-StatusEffectTests-' + [guid]::NewGuid().ToString('N'))
$sourceRoot = Join-Path $validationRoot 'Assets\Runtime'
$editorRoot = Join-Path $validationRoot 'Assets\Editor'
$pluginRoot = Join-Path $validationRoot 'Assets\Plugins'
$settingsRoot = Join-Path $validationRoot 'ProjectSettings'
$packageRoot = Join-Path $validationRoot 'Packages'
New-Item -ItemType Directory -Path $sourceRoot, $editorRoot, $pluginRoot, $settingsRoot, $packageRoot | Out-Null

Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets\Scripts\Runtime\ValueAttribute') -Destination $sourceRoot -Recurse
Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets\Scripts\Runtime\StatusEffect') -Destination $sourceRoot -Recurse
Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets\Plugins\UniTask\Runtime') -Destination (Join-Path $pluginRoot 'UniTask') -Recurse
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'StatusEffectRegressionTests.cs') -Destination $editorRoot
Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets\Plugins\Sirenix\Assemblies\Sirenix.OdinInspector.Attributes.dll') -Destination $pluginRoot
Copy-Item -LiteralPath (Join-Path $projectRoot 'ProjectSettings\ProjectVersion.txt') -Destination $settingsRoot
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
$manifestText = @{ dependencies = $dependencies } | ConvertTo-Json -Depth 5
[System.IO.File]::WriteAllText((Join-Path $packageRoot 'manifest.json'), $manifestText, [System.Text.UTF8Encoding]::new($false))

$logPath = Join-Path $validationRoot 'editor.log'
Write-Output "Validation project: $validationRoot"
$arguments = @('-batchmode', '-nographics', '-projectPath', ('"' + $validationRoot + '"'),
    '-executeMethod', 'StatusEffectRegressionTests.Run', '-logFile', ('"' + $logPath + '"'))
$process = Start-Process -FilePath $UnityEditorPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
if (!$process.WaitForExit(180000))
{
    $process.Kill()
    throw "StatusEffect tests timed out. See $logPath"
}
$reportPath = Join-Path $validationRoot 'results.json'
if (Test-Path -LiteralPath $reportPath)
{
    Get-Content -LiteralPath $reportPath -Encoding UTF8
}
else
{
    Get-Content -LiteralPath $logPath -Tail 80 -Encoding UTF8
    throw "Unity did not produce results. See $logPath"
}
if ($process.ExitCode -ne 0) { throw "StatusEffect regression tests failed. See $logPath" }

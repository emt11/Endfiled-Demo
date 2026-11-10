param(
    [Parameter(Mandatory = $true)]
    [string]$UnityEditorPath
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$validationRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('ARPG-ValueAttributeTests-' + [guid]::NewGuid().ToString('N'))
$sourceRoot = Join-Path $validationRoot 'Assets\ValueAttribute'
$editorRoot = Join-Path $validationRoot 'Assets\Editor'
$pluginRoot = Join-Path $validationRoot 'Assets\Plugins'
$settingsRoot = Join-Path $validationRoot 'ProjectSettings'
$packageRoot = Join-Path $validationRoot 'Packages'
New-Item -ItemType Directory -Path $sourceRoot, $editorRoot, $pluginRoot, $settingsRoot, $packageRoot | Out-Null

# This suite exercises the standalone attribute model. The MonoBehaviour adapter
# also depends on StatusEffect and is compiled with it by the full Scripts suite.
Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Assets\Scripts\Runtime\ValueAttribute') -Filter '*.cs' |
    Where-Object { $_.Name -ne 'CharacterAttributeComponent.cs' } |
    Copy-Item -Destination $sourceRoot
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'ValueAttributeRegressionTests.cs') -Destination $editorRoot
Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets\Plugins\Sirenix\Assemblies\Sirenix.OdinInspector.Attributes.dll') -Destination $pluginRoot
Copy-Item -LiteralPath (Join-Path $projectRoot 'ProjectSettings\ProjectVersion.txt') -Destination $settingsRoot
if (Test-Path -LiteralPath (Join-Path $projectRoot 'UserSettings\Search.index'))
{
    $searchSettingsRoot = Join-Path $validationRoot 'UserSettings'
    New-Item -ItemType Directory -Path $searchSettingsRoot -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $projectRoot 'UserSettings\Search.index') -Destination $searchSettingsRoot
}
[System.IO.File]::WriteAllText((Join-Path $packageRoot 'manifest.json'), '{"dependencies":{}}', [System.Text.UTF8Encoding]::new($false))

$logPath = Join-Path $validationRoot 'editor.log'
Write-Output "Validation project: $validationRoot"
$arguments = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $validationRoot + '"'),
    '-executeMethod', 'ValueAttributeRegressionTests.Run', '-logFile', ('"' + $logPath + '"'))
$process = Start-Process -FilePath $UnityEditorPath -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
$reportPath = Join-Path $validationRoot 'results.json'
if (Test-Path -LiteralPath $reportPath)
{
    Get-Content -LiteralPath $reportPath -Encoding UTF8
}
else
{
    Get-Content -LiteralPath $logPath -Tail 100 -Encoding UTF8
    throw "Unity did not produce results. See $logPath"
}
if ($process.ExitCode -ne 0)
{
    throw "ValueAttribute regression tests failed. See $logPath"
}

param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$runner = Join-Path $root "packages\NUnit.ConsoleRunner.3.8.0\tools\nunit3-console.exe"
$testDll = Join-Path $root "UnitTests\bin\$Configuration\UnitTests.dll"

if (-not (Test-Path $runner)) {
    throw "NUnit console runner not found at: $runner"
}

if (-not (Test-Path $testDll)) {
    throw "Test assembly not found at: $testDll. Build the UnitTests project first."
}

& $runner $testDll

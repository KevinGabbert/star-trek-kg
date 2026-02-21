param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$testProj = Join-Path $root "UnitTests\UnitTests.csproj"

if (-not (Test-Path $testProj)) {
    throw "Test project not found at: $testProj"
}

& dotnet test $testProj -c $Configuration

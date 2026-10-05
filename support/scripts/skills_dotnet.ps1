<# 
.SYNOPSIS
    Instala manualmente los .NET Skills de Microsoft en el proyecto actual.

.DESCRIPTION
    Este script clona el repositorio oficial https://github.com/dotnet/skills
    en una carpeta temporal y copia los skills recomendados a:

        .github\skills

    Debe ejecutarse desde la raíz de tu repositorio/proyecto.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\support\scripts\skills_dotnet.ps1

#>

param(
    [switch]$CleanExisting
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "Installing Microsoft .NET Skills..." -ForegroundColor Cyan
Write-Host ""

# 1. Validate Git
if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Host "Git is not installed or is not available in PATH." -ForegroundColor Red
    Write-Host "Install Git first: https://git-scm.com/downloads"
    exit 1
}

# 2. Detect project root
$ProjectRoot = (Get-Location).Path
$SkillsDestinationRoot = Join-Path $ProjectRoot ".github\skills"

Write-Host "Project root: $ProjectRoot" -ForegroundColor Gray
Write-Host "Skills destination: $SkillsDestinationRoot" -ForegroundColor Gray
Write-Host ""

# 3. Create .github\skills folder
New-Item -ItemType Directory -Force $SkillsDestinationRoot | Out-Null

# 4. Clone Microsoft dotnet/skills repo into temp folder
$SkillsRepo = Join-Path $env:TEMP "dotnet-skills"

if (Test-Path $SkillsRepo) {
    Write-Host "Removing previous temporary clone..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $SkillsRepo -ErrorAction SilentlyContinue
}

Write-Host "Cloning official Microsoft .NET skills repository..." -ForegroundColor Cyan
git clone --depth 1 https://github.com/dotnet/skills $SkillsRepo

if (-not (Test-Path $SkillsRepo)) {
    Write-Host "Failed to clone dotnet/skills repository." -ForegroundColor Red
    exit 1
}

# 5. Optional clean
if ($CleanExisting) {
    Write-Host ""
    Write-Host "Cleaning existing .github\skills content..." -ForegroundColor Yellow
    Get-ChildItem $SkillsDestinationRoot -Force | Remove-Item -Recurse -Force
}

# 6. Skills to copy
$Skills = @(
    @{
        Name = "dotnet-webapi"
        Source = "plugins\dotnet-aspnetcore\skills\dotnet-webapi"
    },
    @{
        Name = "create-blazor-project"
        Source = "plugins\dotnet-blazor\skills\create-blazor-project"
    },
    @{
        Name = "author-component"
        Source = "plugins\dotnet-blazor\skills\author-component"
    },
    @{
        Name = "fetch-and-send-data"
        Source = "plugins\dotnet-blazor\skills\fetch-and-send-data"
    },
    @{
        Name = "optimizing-ef-core-queries"
        Source = "plugins\dotnet-data\skills\optimizing-ef-core-queries"
    },
    @{
        Name = "run-tests"
        Source = "plugins\dotnet-test\skills\run-tests"
    },
    @{
        Name = "dotnet-test-frameworks"
        Source = "plugins\dotnet-test\skills\dotnet-test-frameworks"
    }
)

Write-Host ""
Write-Host "Copying selected .NET skills..." -ForegroundColor Cyan

$Copied = 0
$Skipped = 0

foreach ($Skill in $Skills) {
    $SourcePath = Join-Path $SkillsRepo $Skill.Source
    $DestinationPath = Join-Path $SkillsDestinationRoot $Skill.Name

    if (-not (Test-Path $SourcePath)) {
        Write-Host "Skipped: $($Skill.Name) - source folder not found: $SourcePath" -ForegroundColor Yellow
        $Skipped++
        continue
    }

    $SkillFile = Join-Path $SourcePath "SKILL.md"

    if (-not (Test-Path $SkillFile)) {
        Write-Host "Skipped: $($Skill.Name) - SKILL.md not found." -ForegroundColor Yellow
        $Skipped++
        continue
    }

    if (Test-Path $DestinationPath) {
        Remove-Item -Recurse -Force $DestinationPath
    }

    Copy-Item -Recurse $SourcePath $DestinationPath -Force

    Write-Host "Copied: $($Skill.Name)" -ForegroundColor Green
    $Copied++
}

# 7. Summary
Write-Host ""
Write-Host "Installation completed." -ForegroundColor Cyan
Write-Host "Copied skills: $Copied" -ForegroundColor Green
Write-Host "Skipped skills: $Skipped" -ForegroundColor Yellow

Write-Host ""
Write-Host "Installed skills location:" -ForegroundColor Cyan
Write-Host $SkillsDestinationRoot

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Commit the .github\skills folder to your repository."
Write-Host "2. Restart VS Code or run: Developer: Reload Window."
Write-Host "3. In Copilot Chat Agent mode, ask Copilot to use the relevant .NET skill."

Write-Host ""
Write-Host "Example prompts:" -ForegroundColor Cyan
Write-Host 'Use the dotnet-webapi skill to create a Minimal API endpoint.'
Write-Host 'Use the author-component skill to create a Blazor component.'
Write-Host 'Use the run-tests skill to execute and fix the test suite.'

Write-Host ""

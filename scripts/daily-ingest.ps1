<#
.SYNOPSIS
    Triggers the daily price download.

.DESCRIPTION
    The Windows counterpart to daily-ingest.sh. The application has no internal scheduler by
    design, so this script is what Task Scheduler runs after the close.

    Register it (weekdays at 21:30 local):

        $action  = New-ScheduledTaskAction -Execute 'pwsh.exe' `
                     -Argument '-NoProfile -File C:\finance-analysis\scripts\daily-ingest.ps1'
        $trigger = New-ScheduledTaskTrigger -Weekly `
                     -DaysOfWeek Monday,Tuesday,Wednesday,Thursday,Friday -At 21:30
        Register-ScheduledTask -TaskName 'FinanceAnalysis-DailyIngest' `
                     -Action $action -Trigger $trigger -Description 'Daily OHLCV ingest'

.PARAMETER TradeDate
    Optional yyyy-MM-dd date to ingest. Omit to let the API resolve the latest trading day.

.PARAMETER EnvFile
    Path to a .env file to read configuration from. Defaults to the repository root's .env.
#>
[CmdletBinding()]
param(
    [string] $TradeDate,
    [string] $EnvFile = (Join-Path $PSScriptRoot '..' '.env')
)

$ErrorActionPreference = 'Stop'

# Environment wins over the file, so a scheduled task can override without editing anything.
if (Test-Path -LiteralPath $EnvFile) {
    foreach ($line in Get-Content -LiteralPath $EnvFile) {
        if ($line -match '^\s*([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(.*)$') {
            $name = $Matches[1]
            if (-not [Environment]::GetEnvironmentVariable($name)) {
                Set-Item -Path "env:$name" -Value $Matches[2].Trim('"')
            }
        }
    }
}

$apiKey = $env:INTERNAL_API_KEY
if ([string]::IsNullOrWhiteSpace($apiKey)) {
    Write-Error "INTERNAL_API_KEY is not set (looked in the environment and $EnvFile)."
    exit 78
}

$port = if ($env:API_LOOPBACK_PORT) { $env:API_LOOPBACK_PORT } else { '5080' }
$base = if ($env:API_BASE_URL) { $env:API_BASE_URL } else { "http://127.0.0.1:$port" }

$url = "$base/api/internal/ingestion/daily-prices"
if ($TradeDate) {
    $url += "?tradeDate=$TradeDate"
}

Write-Host "[$(Get-Date -Format o)] POST $url"

try {
    $response = Invoke-WebRequest -Uri $url -Method Post `
        -Headers @{ 'X-Internal-Api-Key' = $apiKey } `
        -TimeoutSec 30 `
        -SkipHttpErrorCheck

    Write-Host $response.Content

    # 202 queued, 200 already ingested and skipped. Both mean the day is handled.
    if ($response.StatusCode -in 200, 202) {
        Write-Host "[$(Get-Date -Format o)] Ingestion accepted (HTTP $($response.StatusCode))."
        exit 0
    }

    Write-Error "Ingestion request failed with HTTP $($response.StatusCode)."
    exit 1
}
catch {
    Write-Error "Ingestion request failed: $_"
    exit 1
}

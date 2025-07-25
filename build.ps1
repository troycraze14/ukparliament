function ci-up {
    Write-Host "🚀 Starting CI stack with TeamCity server, agent and local registry..."
    docker compose -f compose.ci.yml up --build -d

    $url = "http://localhost:8111"
    Write-Host "⏳ Waiting for TeamCity server to respond at $url..."

    while ($true) {
        try {
            $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 5
            if ($response.StatusCode -eq 200) {
                Write-Host "✅ TeamCity server is ready!"
                break
            }
        } catch {
            # Not ready yet
        }
        Start-Sleep -Seconds 2
    }
}

# You can call it directly, or dot-source this script to get the function in your shell
if ($MyInvocation.InvocationName -eq "ci-up") {
    ci-up
} else {
    Write-Host "Function 'ci-up' is defined. You can call it to start the CI stack."
}
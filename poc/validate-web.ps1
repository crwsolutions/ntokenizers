# Validation for the PoC web app: Kestrel with sync I/O disabled.
# Usage: .\validate-web.ps1 [-BaseUrl http://localhost:5080]

param(
    [string]$BaseUrl = "http://localhost:5080"
)

$ErrorActionPreference = "Stop"
$script:failures = 0

function Assert($condition, $id, $name, $detail) {
    if ($condition) {
        Write-Host "PASS $id $name" -ForegroundColor Green
    } else {
        Write-Host "FAIL $id $name" -ForegroundColor Red
        if (-not [string]::IsNullOrEmpty($detail)) {
            Write-Host "       detail: $detail"
        }
        $script:failures++
    }
}

# W0: the server's in-memory architecture self-check.
try {
    $selfCheck = Invoke-RestMethod -Uri "$BaseUrl/self-check"
    Assert $selfCheck.allPassed "W0" "server self-check (architecture, in-memory)" `
        ((($selfCheck.checks | Where-Object { -not $_.pass }) | ForEach-Object { "$($_.id) $($_.detail)" }) -join "; ")
} catch {
    Assert $false "W0" "server self-check reachable" $_.Exception.Message
}

# W1-W4: the streaming request, with first-token timing.
$sw = [System.Diagnostics.Stopwatch]::StartNew()
$statusCode = 0
$firstByteMs = -1
$firstTokenMs = -1
$totalMs = 0
$body = ""
$firstTokenMarker = "<h1>PoC sample</h1>"

try {
    $http = [System.Net.Http.HttpClient]::new()
    $request = [System.Net.Http.HttpRequestMessage]::new("GET", "$BaseUrl/stream-html?delay=80")
    $response = $http.SendAsync($request, [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead).GetAwaiter().GetResult()
    $statusCode = [int]$response.StatusCode

    $stream = $response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
    $encoding = [System.Text.Encoding]::UTF8
    $buffer = New-Object byte[] 4096
    $sb = New-Object System.Text.StringBuilder

    $read = $stream.ReadAsync($buffer, 0, $buffer.Length).GetAwaiter().GetResult()
    $firstByteMs = $sw.ElapsedMilliseconds

    while ($read -gt 0) {
        [void]$sb.Append($encoding.GetString($buffer, 0, $read))
        if ($firstTokenMs -lt 0 -and $sb.ToString().Contains($firstTokenMarker)) {
            $firstTokenMs = $sw.ElapsedMilliseconds
        }
        $read = $stream.ReadAsync($buffer, 0, $buffer.Length).GetAwaiter().GetResult()
    }

    $totalMs = $sw.ElapsedMilliseconds
    $body = $sb.ToString()
} catch {
    $body = $_.Exception.ToString()
}

Assert ($statusCode -eq 200) "W1" "Kestrel served the request with AllowSynchronousIO=false" "status=$statusCode ${body.Substring(0, [Math]::Min(200, $body.Length))}"
Assert ($firstTokenMs -ge 0 -and $firstTokenMs -lt ($totalMs / 2)) "W2" "progressive delivery (first token well before the end)" "firstByte=${firstByteMs}ms firstToken=${firstTokenMs}ms total=${totalMs}ms"

$markers = @(
    "<h1>PoC sample</h1>",
    "<p>This is a plain paragraph.</p>",
    "data-lang=`"csharp`"",
    "tok-keyword`">var<",
    "tok-stringvalue",
    "tok-comment`">// a comment<",
    "<blockquote>",
    "data-lang=`"js`"",
    "tok-keyword`">let<",
    "<p>Last paragraph.</p>"
)
foreach ($marker in $markers) {
    Assert ($body.Contains($marker)) "W3" "content: $marker" "marker missing"
}

Assert ($body.TrimEnd().EndsWith("</html>")) "W4" "complete document (ends with </html>)" "response truncated"

Write-Host ""
if ($script:failures -eq 0) {
    Write-Host "ALL WEB CHECKS PASSED" -ForegroundColor Green
} else {
    Write-Host "$script:failures WEB CHECK(S) FAILED" -ForegroundColor Red
}
exit ([int][bool]$script:failures)

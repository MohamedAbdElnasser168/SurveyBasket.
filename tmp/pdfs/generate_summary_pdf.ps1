$ErrorActionPreference = "Stop"

$repoRoot = "C:\Users\abdel\source\repos\SurveyBasket"
$outputDir = Join-Path $repoRoot "output\pdf"
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

$outputPath = Join-Path $outputDir "SurveyBasket-app-summary.pdf"

function Escape-PdfText {
    param([string]$Text)
    return $Text.Replace("\", "\\").Replace("(", "\(").Replace(")", "\)")
}

$lines = @(
    @{ Y = 808; Size = 20; Text = "SurveyBasket App Summary"; Font = "F2" }
    @{ Y = 786; Size = 9; Text = "Evidence source: SurveyBasket.Api repo contents"; Font = "F1" }

    @{ Y = 758; Size = 12; Text = "What It Is"; Font = "F2" }
    @{ Y = 742; Size = 9; Text = "SurveyBasket is an ASP.NET Core Web API for creating polls, managing questions, collecting votes, and returning results data."; Font = "F1" }
    @{ Y = 730; Size = 9; Text = "The repo shows JWT-based authentication, SQL Server persistence via EF Core, and OpenAPI/Swagger support in development."; Font = "F1" }

    @{ Y = 706; Size = 12; Text = "Who It's For"; Font = "F2" }
    @{ Y = 690; Size = 9; Text = "Primary persona: Not found in repo."; Font = "F1" }
    @{ Y = 678; Size = 9; Text = "Based on controllers and services, the API appears intended for an authenticated client app used by poll managers and poll participants."; Font = "F1" }

    @{ Y = 654; Size = 12; Text = "What It Does"; Font = "F2" }
    @{ Y = 638; Size = 9; Text = "- Auth endpoints issue JWTs, refresh tokens, and support refresh-token revocation."; Font = "F1" }
    @{ Y = 626; Size = 9; Text = "- Poll endpoints create, read, update, delete, list current polls, and toggle publish status."; Font = "F1" }
    @{ Y = 614; Size = 9; Text = "- Question endpoints manage poll-scoped questions and toggle question active status."; Font = "F1" }
    @{ Y = 602; Size = 9; Text = "- Voting endpoints return available questions for a user and accept submitted answers."; Font = "F1" }
    @{ Y = 590; Size = 9; Text = "- Results endpoints return raw vote data, votes per day, and votes per question."; Font = "F1" }
    @{ Y = 578; Size = 9; Text = "- FluentValidation validators and a global exception handler shape request validation and errors."; Font = "F1" }
    @{ Y = 566; Size = 9; Text = "- Development mode exposes OpenAPI plus Swagger UI for exploring the API."; Font = "F1" }

    @{ Y = 542; Size = 12; Text = "How It Works"; Font = "F2" }
    @{ Y = 526; Size = 9; Text = "- Host: Program.cs builds the ASP.NET Core app, enables CORS, controllers, HTTPS, authz, and dev OpenAPI/Swagger."; Font = "F1" }
    @{ Y = 514; Size = 9; Text = "- API layer: Controllers include Auth, Polls, Questions, Votes, and Results."; Font = "F1" }
    @{ Y = 502; Size = 9; Text = "- Business layer: Scoped services such as AuthService, PollService, QuestionService, VoteService, and ResultService handle use cases."; Font = "F1" }
    @{ Y = 490; Size = 9; Text = "- Data layer: ApplicationDbContext uses EF Core with SQL Server and IdentityDbContext<ApplicationUser>."; Font = "F1" }
    @{ Y = 478; Size = 9; Text = "- Security flow: AuthService uses UserManager<ApplicationUser> and JwtProvider to validate users, mint JWTs, and store refresh tokens."; Font = "F1" }
    @{ Y = 466; Size = 9; Text = "- Mapping/validation: Mapster maps entities/contracts; FluentValidation auto-validates request models."; Font = "F1" }
    @{ Y = 454; Size = 9; Text = "- Data flow: HTTP request -> controller -> service -> EF Core/Identity -> SQL Server -> mapped response or ProblemDetails error."; Font = "F1" }

    @{ Y = 430; Size = 12; Text = "How To Run"; Font = "F2" }
    @{ Y = 414; Size = 9; Text = "1. Use a .NET SDK that supports target framework net10.0. Exact SDK version: Not found in repo."; Font = "F1" }
    @{ Y = 402; Size = 9; Text = "2. Ensure SQL Server is available and update DefaultConnection in SurveyBasket.Api/appsettings.json if needed."; Font = "F1" }
    @{ Y = 390; Size = 9; Text = "3. Set Jwt:Key before startup; the checked-in appsettings.json leaves it empty."; Font = "F1" }
    @{ Y = 378; Size = 9; Text = "4. Apply the EF Core migrations to create/update the database. Exact command: Not found in repo."; Font = "F1" }
    @{ Y = 366; Size = 9; Text = "5. Run: dotnet run --project SurveyBasket.Api"; Font = "F1" }
    @{ Y = 354; Size = 9; Text = "6. In development, open https://localhost:7099/swagger/index.html or use http://localhost:5033."; Font = "F1" }

    @{ Y = 326; Size = 8; Text = "Key evidence: Program.cs, DependencyInjection.cs, appsettings.json, launchSettings.json, controllers, services, ApplicationDbContext, and EF migrations."; Font = "F1" }
    @{ Y = 314; Size = 8; Text = "Rendering note: generated in this environment without reportlab/poppler, so image-based visual verification was not available."; Font = "F1" }
)

$contentBuilder = New-Object System.Text.StringBuilder
foreach ($line in $lines) {
    $escaped = Escape-PdfText $line.Text
    [void]$contentBuilder.AppendLine("BT /$($line.Font) $($line.Size) Tf 54 $($line.Y) Td ($escaped) Tj ET")
}

$content = $contentBuilder.ToString()
$contentBytes = [System.Text.Encoding]::ASCII.GetBytes($content)

$objects = @(
    "<< /Type /Catalog /Pages 2 0 R >>",
    "<< /Type /Pages /Count 1 /Kids [3 0 R] >>",
    "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 842] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>",
    "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
    "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>"
)

$streamObject = "<< /Length $($contentBytes.Length) >>`nstream`n$content`nendstream"

$pdfBuilder = New-Object System.Text.StringBuilder
[void]$pdfBuilder.Append("%PDF-1.4`n")

$offsets = New-Object System.Collections.Generic.List[int]
$offsets.Add(0) | Out-Null

for ($i = 0; $i -lt $objects.Count; $i++) {
    $offsets.Add($pdfBuilder.Length) | Out-Null
    [void]$pdfBuilder.Append("$($i + 1) 0 obj`n")
    [void]$pdfBuilder.Append($objects[$i])
    [void]$pdfBuilder.Append("`nendobj`n")
}

$offsets.Add($pdfBuilder.Length) | Out-Null
[void]$pdfBuilder.Append("6 0 obj`n")
[void]$pdfBuilder.Append($streamObject)
[void]$pdfBuilder.Append("`nendobj`n")

$xrefOffset = $pdfBuilder.Length
[void]$pdfBuilder.Append("xref`n0 7`n")
[void]$pdfBuilder.Append("0000000000 65535 f `n")
for ($i = 1; $i -le 6; $i++) {
    [void]$pdfBuilder.AppendFormat("{0:0000000000} 00000 n `n", $offsets[$i])
}
[void]$pdfBuilder.Append("trailer`n<< /Size 7 /Root 1 0 R >>`nstartxref`n")
[void]$pdfBuilder.Append($xrefOffset)
[void]$pdfBuilder.Append("`n%%EOF")

[System.IO.File]::WriteAllBytes($outputPath, [System.Text.Encoding]::ASCII.GetBytes($pdfBuilder.ToString()))
Write-Output $outputPath

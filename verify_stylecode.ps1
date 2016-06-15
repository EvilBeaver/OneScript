Param(
  [int] $level = 1,
  [string] $output,
  [string] $projects = "ScriptEngine;oscript;ScriptEngine.HostedScript;TestApp"
)

$InspectCodeToolPath = "JetBrains.ReSharper.CommandLineTools\inspectcode.exe"
$categories = @("CodeStyleIssues", "ConstraintViolation")
$levels = @{
    "SUGGESTION" = 1;
    "WARNING" = 2;
    "ERROR" = 3;
}
$logFile = "codestyle.log"

& "$($InspectCodeToolPath)" --toolset=12.0 --output=$logFile --project="$($projects)" src\1Script.sln  | Out-Null

Write-Host "Analysis Completed" -Foreground Green

# Parse report
[xml] $reportXml = Get-Content $logFile
$report = $reportXml.Report

$codeStyleIssueTypes = @{}
foreach ($issueType in $report.IssueTypes.IssueType) {
    if (-not ($categories -contains $issueType.CategoryId)) {
        continue
    }
       
    $codeStyleIssueTypes.Add($issueType.Id, $issueType)
}

foreach ($project in $report.Issues.Project) {
    foreach ($issue in $project.Issue) {
        if (-not $codeStyleIssueTypes.ContainsKey($issue.TypeId)) {
            continue
        }
        
        $type = $codeStyleIssueTypes[$issue.TypeId]  
        if ($levels[$type.Severity] -lt $level) {
            continue
        }
              
        $message = "[$($type.Severity)] $($issue.File):$($issue.Line) `"$($issue.Message)`""
        
        $color = "White"
        switch ($levels[$type.Severity]) {
	        1 { $color = "Green" } 
            2 { $color = "Yellow" }
            3 { $color = "Red" }
        }   
             
        if (-not [string]::IsNullOrEmpty($output)) {
            $message | Out-File $output -Append
        }   
        else {          
            Write-Host $message -Foreground $color
        }
    }
}
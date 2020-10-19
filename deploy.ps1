Param (
    [Parameter(Mandatory=$true)][string]$lambda
)
$root = $pwd
switch ($lambda) {
    "Mandabot" { set-location "$root/src/Mandabot" }
    "MandabotGetNotes" { set-location "$root/src/Mandabot.GetNotes" }
}
dotnet lambda deploy-function $lambda
if (-Not $?) {
    write-output "The deploy failed"
}
set-location $root
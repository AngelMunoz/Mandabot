$root = $pwd
set-location "$root/src/Mandabot"
dotnet lambda deploy-function $lambda
if (-Not $?) {
    write-output "The deploy failed..."
}
set-location $root
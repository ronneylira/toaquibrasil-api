Write-Host -ForegroundColor Green "Removing last migration..."

$csproj = (Resolve-Path "$PSScriptRoot/../ToAquiBrasil.Api/")
$csproj = "${csproj}ToAquiBrasil.Api.csproj"

echo "> dotnet ef migrations remove --startup-project ""${csproj}"""
pushd $PSScriptRoot
dotnet ef migrations remove --startup-project "$csproj"
popd


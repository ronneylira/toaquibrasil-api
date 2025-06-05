[CmdletBinding()]
Param(
    [Alias("Env")]
    [Parameter(Position=0,Mandatory=$true)]
    [string]$Environment
)


Write-Host -ForegroundColor Green "Update Database..."

$csproj = (Resolve-Path "$PSScriptRoot/../ToAquiBrasil.Api/")
$csproj = "${csproj}ToAquiBrasil.Api.csproj"
$temp = $env:ASPNETCORE_ENVIRONMENT

echo "> dotnet ef database update --startup-project ""${csproj}"""
pushd $PSScriptRoot
$env:ASPNETCORE_ENVIRONMENT = "$Environment"
dotnet ef database update --startup-project "$csproj"
$env:ASPNETCORE_ENVIRONMENT = $temp
popd

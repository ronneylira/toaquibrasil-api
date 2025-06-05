[CmdletBinding()]
Param(
    [Parameter(Position=0,Mandatory=$true)]
    [string]$LastMigration
)

$csproj = (Resolve-Path "$PSScriptRoot/../ToAquiBrasil.Api/")
$csproj = "${csproj}ToAquiBrasil.Api.csproj"

Write-Host -ForegroundColor Green "Revert to migration $LastMigration ..."
echo "> dotnet ef database update ""$LastMigration"" --startup-project ""${csproj}"""
pushd $PSScriptRoot
dotnet ef database update "$LastMigration" --startup-project "$csproj"
popd

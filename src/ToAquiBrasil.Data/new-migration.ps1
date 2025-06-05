[CmdletBinding()]
Param(
    [Alias("Name")]
    [Parameter(Position=0,Mandatory=$true)]
    [string]$MigrationName
)

$csproj = (Resolve-Path "$PSScriptRoot/../ToAquiBrasil.Api/")
$csproj = "${csproj}ToAquiBrasil.Api.csproj"

Write-Host -ForegroundColor Green "Adding migration $MigrationName ..."
echo "> dotnet ef migrations add ""$MigrationName"" --startup-project ""${csproj}"""
pushd $PSScriptRoot
dotnet ef migrations add "$MigrationName" --startup-project "$csproj"
popd

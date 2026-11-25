[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot)
)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($RepositoryRoot)
$authority = Join-Path $root 'ASEISA.API/BD'
$secondary = Join-Path $root 'Seguridad/Seguridad.API/BD'
[xml]$main = Get-Content -LiteralPath (Join-Path $authority 'BD.sqlproj') -Raw
[xml]$reference = Get-Content -LiteralPath (Join-Path $secondary 'BD.sqlproj') -Raw
$ns = [Xml.XmlNamespaceManager]::new($reference.NameTable)
$ns.AddNamespace('m', 'http://schemas.microsoft.com/developer/msbuild/2003')
function Assert-Contract([bool]$Condition, [string]$Message) {
    if (!$Condition) { throw $Message }
}
Assert-Contract ($reference.Project.DefaultTargets -eq 'Build') 'El destino predeterminado debe quedar bloqueado.'
Assert-Contract (!$reference.SelectNodes('//m:Import|//m:UsingTask|//m:ProjectReference|//m:Build|//m:Sdk', $ns).Count -and !$reference.Project.HasAttribute('Sdk')) 'El secundario no puede importar compiladores ni compilar objetos.'
Assert-Contract ($reference.SelectSingleNode('//m:DeployToDatabase', $ns).InnerText -eq 'False') 'El despliegue secundario debe estar deshabilitado.'
$reject = $reference.SelectSingleNode('//m:Target[@Name="_RejectSqlArtifactGeneration"]', $ns)
Assert-Contract ($null -ne $reject) 'Falta el bloqueo de artefactos.'
$errorNode = $reject.SelectSingleNode('m:Error', $ns)
Assert-Contract ($null -ne $errorNode -and !$errorNode.HasAttribute('Condition') -and $errorNode.Code -eq 'ASEISA_SQL_REFERENCE_ONLY' -and !$reject.HasAttribute('Condition')) 'El bloqueo debe fallar siempre, sin condicion configurable.'
foreach ($target in @('Build','Rebuild','Publish','Deploy','SqlBuild','SqlPublish','SqlDeploy','GenerateDeployScript','GeneratePublishScript','Package')) {
    $node = $reference.SelectSingleNode('//m:Target[@Name="' + $target + '"]', $ns)
    Assert-Contract ($null -ne $node -and $node.DependsOnTargets -eq '_RejectSqlArtifactGeneration' -and !$node.HasAttribute('Condition') -and !$node.ChildNodes.Count) ("Destino secundario sin proteccion: " + $target)
}
$buildIncludes = @($main.Project.ItemGroup.Build | Where-Object { $_.Include } | ForEach-Object { $_.Include.Replace('\','/') })
Assert-Contract (@($buildIncludes | Sort-Object -Unique).Count -eq $buildIncludes.Count) 'Hay objetos SQL duplicados en el proyecto autoritativo.'
$links = @($reference.SelectNodes('//m:None', $ns))
Assert-Contract ($links.Count -eq 24) 'Deben mantenerse las 24 referencias SQL compartidas.'
$seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($link in $links) {
    $relative = $link.Link.Replace('\','/')
    Assert-Contract ($relative.StartsWith('dbo/') -and $relative.EndsWith('.sql') -and !$relative.Contains('..')) 'Ruta logica SQL invalida.'
    Assert-Contract ($seen.Add($relative)) ("Referencia duplicada: " + $relative)
    Assert-Contract ($link.Include.Replace('\','/') -eq '../../../ASEISA.API/BD/' + $relative) ("Referencia fuera de la fuente autoritativa: " + $relative)
    $resolved = [IO.Path]::GetFullPath((Join-Path $secondary $link.Include))
    $expected = [IO.Path]::GetFullPath((Join-Path $authority $relative))
    Assert-Contract ($resolved -eq $expected -and (Test-Path -LiteralPath $resolved -PathType Leaf)) ("Referencia rota: " + $relative)
    $mainIncludes = @($main.Project.ItemGroup.Build | Where-Object { $_ -and $_.Include } | ForEach-Object { $_.Include.Replace('\','/') })
    Assert-Contract ($relative -in $mainIncludes) ("Objeto ausente del proyecto autoritativo: " + $relative)
}
$duplicates = @(Get-ChildItem -LiteralPath $secondary -Recurse -File -Filter '*.sql' | Where-Object {
    $_.FullName -notmatch '[\\/](bin|obj)[\\/]'
})
Assert-Contract (!$duplicates.Count) 'Hay copias SQL fisicas en el proyecto secundario; utilizar enlaces.'
$packages = @(Get-ChildItem -LiteralPath $secondary -Recurse -File -Filter '*.dacpac')
Assert-Contract (!$packages.Count) 'Hay un DACPAC secundario obsoleto: aislarlo antes de continuar.'
foreach ($item in @($main.Project.ItemGroup.Build) + @($main.Project.ItemGroup.PostDeploy) + @($main.Project.ItemGroup.PreDeploy)) {
    if ($null -eq $item -or !$item.Include) { continue }
    Assert-Contract (Test-Path -LiteralPath (Join-Path $authority $item.Include) -PathType Leaf) ("Objeto autoritativo ausente: " + $item.Include)
}
$listed = @(@($main.Project.ItemGroup.Build) + @($main.Project.ItemGroup.PostDeploy) + @($main.Project.ItemGroup.PreDeploy) | ForEach-Object {
    if ($_.Include) { [IO.Path]::GetFullPath((Join-Path $authority $_.Include)) }
})
foreach ($file in Get-ChildItem -LiteralPath $authority -Recurse -File -Filter '*.sql' | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }) {
    Assert-Contract ($file.FullName -in $listed) ("SQL autoritativo fuera del proyecto: " + $file.Name)
}
$solution = Get-Content -LiteralPath (Join-Path $root 'Seguridad/Seguridad.API/Seguridad.sln') -Raw
Assert-Contract ($solution -match 'BD\\BD.sqlproj') 'El proyecto secundario debe conservarse en la solucion.'
Assert-Contract ($solution -notmatch '(?im)^\s*\{73E61DD7-788A-4AEB-9BA5-44525D2E996B\}[^\r\n]*\.(Build|Deploy)\.0\s*=') 'El proyecto secundario no debe compilarse ni desplegarse desde la solucion.'
Write-Output 'OK: 24 enlaces, fuente SQL unica, destinos bloqueados y sin copias ni DACPAC secundarios.'

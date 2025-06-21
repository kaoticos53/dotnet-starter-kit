<#
.SYNOPSIS
    Elimina las carpetas bin y obj de un proyecto .NET
.DESCRIPTION
    Este script busca recursivamente las carpetas 'bin' y 'obj' en el directorio actual
    y sus subdirectorios, y las elimina para liberar espacio y limpiar el proyecto.
    Es útil después de cambios importantes en la estructura del proyecto o para solucionar
    problemas de compilación.
.NOTES
    Versión: 1.0
    Fecha: $(Get-Date -Format "yyyy-MM-dd")
    Autor: Asistente de Desarrollo
#>

# Configuración
$ErrorActionPreference = "Stop"
$rootPath = $PSScriptRoot
$foldersToDelete = @("bin", "obj")
$deletedFolders = @()
$totalFreed = 0

Write-Host "=== Limpieza de carpetas de compilación ===" -ForegroundColor Cyan
Write-Host "Directorio raíz: $rootPath"
Write-Host "Buscando carpetas: $($foldersToDelete -join ', ')"

# Función para formatear el tamaño en bytes a un formato legible
function Format-FileSize {
    param ([long]$size)
    
    $suffix = "B", "KB", "MB", "GB", "TB"
    $index = 0
    
    while ($size -gt 1024 -and $index -lt $suffix.Length - 1) {
        $size = $size / 1024
        $index++
    }
    
    return "{0:N2} {1}" -f $size, $suffix[$index]
}

try {
    # Buscar y eliminar carpetas
    foreach ($folder in $foldersToDelete) {
        $paths = Get-ChildItem -Path $rootPath -Directory -Recurse -Force -Filter $folder -ErrorAction SilentlyContinue | 
                 Where-Object { $_.FullName -notmatch '\\.(git|vs|github|vscode)\\' } |
                 Select-Object -ExpandProperty FullName
        
        foreach ($path in $paths) {
            try {
                # Calcular tamaño antes de eliminar
                $folderSize = (Get-ChildItem -Path $path -Recurse -File -Force | 
                             Measure-Object -Property Length -Sum).Sum
                
                # Eliminar la carpeta
                Remove-Item -Path $path -Recurse -Force -ErrorAction Stop
                
                $deletedFolders += $path
                $totalFreed += $folderSize
                
                Write-Host "Eliminada: $path" -ForegroundColor Green
                if ($folderSize -gt 0) {
                    Write-Host "  Tamaño liberado: $(Format-FileSize $folderSize)" -ForegroundColor DarkGray
                }
            }
            catch {
                Write-Host "No se pudo eliminar $path : $_" -ForegroundColor Red
            }
        }
    }
    
    # Mostrar resumen
    Write-Host "\n=== Resumen ===" -ForegroundColor Cyan
    Write-Host "Carpetas eliminadas: $($deletedFolders.Count)"
    Write-Host "Espacio total liberado: $(Format-FileSize $totalFreed)"
    
    if ($deletedFolders.Count -gt 0) {
        Write-Host "\nLista de carpetas eliminadas:" -ForegroundColor Yellow
        $deletedFolders | ForEach-Object { Write-Host "- $_" }
    } else {
        Write-Host "No se encontraron carpetas para eliminar." -ForegroundColor Yellow
    }
    
    Write-Host "\nLimpieza completada exitosamente." -ForegroundColor Green
}
catch {
    Write-Host "\nOcurrió un error durante la limpieza: $_" -ForegroundColor Red
    exit 1
}

exit 0

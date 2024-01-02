function Set-Project-Properties($targetVersion) {
    $copyright = $(get-copyright)

    write-host "$product $targetVersion"
    write-host $copyright

    set-regenerated-file "$pwd/Directory.Build.props" @"
<Project>
    <PropertyGroup>
        <Product>$product</Product>
        <Version>$targetVersion</Version>
        <Copyright>$copyright</Copyright>
        <LangVersion>latest</LangVersion>
    </PropertyGroup>
</Project>
"@
}

function Get-Copyright {
    $date = Get-Date
    $year = $date.Year
    $copyrightSpan = if ($year -eq $yearInitiated) { $year } else { "$yearInitiated-$year" }
    return "© $copyrightSpan $owner"
}

function Publish-Project {
    $project = Split-Path $pwd -Leaf
    Write-Host "Publishing $project"
    dotnet publish --configuration $configuration --self-contained -r "win-x64" --output $stage/$project /nologo
    exec { Compress-Archive $stage/$project/* -DestinationPath $publish/"$project.zip"}
}

function Set-Regenerated-File($path, $newContent) {
    if (-not (test-path $path -PathType Leaf)) {
        $oldContent = $null
    } else {
        $oldContent = [IO.File]::ReadAllText($path)
    }

    if ($newContent -ne $oldContent) {
        write-host "Generating $path"
        [System.IO.File]::WriteAllText($path, $newContent, [System.Text.Encoding]::UTF8)
    }
}

function Remove-Directory-Silently($path) {
    if (test-path $path) {
        write-host "Deleting $path"
        Remove-Item $path -recurse -force -ErrorAction SilentlyContinue | out-null
    }
    New-Item $path -ItemType "directory" | out-null
}
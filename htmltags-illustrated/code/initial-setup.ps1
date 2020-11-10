# Setup the environment to use psake, if not already present.
# This is one time setup that only needs to be run once.

# Set PSGallery as trusted so we can install packages from there
Write-Host 'Trusting PS Gallery'
Set-PSRepository -Name "PSGallery" -InstallationPolicy Trusted

# Install psake
Write-Host 'Installing psake'
Install-Module -Name psake -Scope CurrentUser -Force

# Restore any local tools for this project that are specified in the tools manifest (.config\dotnet-tools.json)
Write-Host 'Restoring local tools'
dotnet tool restore
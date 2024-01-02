# Initial Setup
The build script uses psake, a PowerShell tool for defining and executing build tasks.

As a one time setup step, run `initial-setup.ps1` to
 * install psake
 * install local tools defined in the tools manifest (.config\dotnet-tools.json)

 # Compiling
 From a PowerShell prompt, navigate to the repository's root directory (the one containing psakefile.ps1) and execute `Invoke-Psake`.
 
 The default task will compile the project.

 You can look at the other defined tasks in `psakefile.ps1` and execute them like this: `Invoke-Psake Foo` (where Foo is a task, like Compile).

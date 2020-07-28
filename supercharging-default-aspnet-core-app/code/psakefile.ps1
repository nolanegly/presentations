include "./psake-build-helpers.ps1"

properties {
    # code
    $configuration = 'Release'
    $version = '1.0.999'
    $owner = 'Nolan Egly'
    $product = 'Volunteer Convergence'
    $yearInitiated = '2020'
    # build directories
    $projectRootDirectory = "$(resolve-path .)"
    $publish = "$projectRootDirectory/publish"
    $stage = "$projectRootDirectory/stage"
    # database
    $dbServer = "(LocalDb)\mssqllocaldb"
    $dbName = "VolunteerConvergence"
}
 
task default -depends Info, Compile, Migrate-Db, Test -description "Compile, run any new migrations, and run automated tests"
task CI -depends Clean, Info, Compile, Migrate-Db, Publish -description "Continuous Integration process"
task Rebuild -depends Clean, Info, Compile -description "Rebuild the code"

task Info -description "Display runtime information" {
    exec { dotnet --info }
}

task ? -alias help -description "Display help content and possible targets" {
    # This is a psake internal method
	WriteDocumentation
}

task Compile -description "Compile the solution" {
    exec { set-project-properties $version } -workingDirectory src
    exec { dotnet build --configuration $configuration /nologo } -workingDirectory src
}

task Test -description "Run automated tests" {
    get-childitem . src/*Tests -directory | foreach-object {
		Write-Output 'Running tests in ' $_.fullname
        exec { dotnet fixie --configuration $configuration --no-build } -workingDirectory $_.fullname
    }
}

task Publish -description "Publish the primary projects for distribution" {
    remove-directory-silently $publish
    remove-directory-silently $stage
    exec { publish-project } -workingDirectory 'src/VolunteerConvergence'
}
  
task Clean -description "Clean out all the binary folders" {
    exec { dotnet clean --configuration $configuration /nologo } -workingDirectory src
    remove-directory-silently $publish
    remove-directory-silently $stage
    remove-directory-silently $testResults
}

task Migrate-Db -description "Migrates the database" {
    exec { & dotnet rh /d=$dbName /f=db /s=$dbServer /silent /env=LOCAL }

    $dbTestName = $dbName + "Test"
    exec { & dotnet rh /d=$dbTestName /f=db /s=$dbServer /silent /drop }
    # AUTOMATEDTESTS isn't really used, but using this "environment" prevents LOCAL scripts from running.
    # (rh uses local as a default environment if it isn't explicitly specified)
    exec { & dotnet rh /d=$dbTestName /f=db /s=$dbServer /silent /simple /env=AUTOMATEDTESTS }
}

task Recreate-Db -description "Drop and completely recreate the database" {
	exec { & dotnet rh /d=$dbName /f=db /s=$dbServer /silent /drop }

    Invoke-Task Migrate-Db
}


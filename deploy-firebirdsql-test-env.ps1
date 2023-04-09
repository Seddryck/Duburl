Param(
	[switch] $force=$false
	, $package= "Firebird-4.0.2.2816-0-x64"
	, $config= "Release"
)
if ($force) {
	Write-Warning "Forcing QA testing for FirebirdSQL"
}
$binPath = "./DubUrl.QA/bin/$config/net6.0/"
$rootUrl = "https://github.com/FirebirdSQL/firebird/releases/download/"
If (-not($env:PATH -like "7-zip")) {
	$env:PATH += ";C:\Program Files\7-Zip"
}

$firebirdPath = "C:/Program Files/Firebird/"
If (-not (Test-Path -Path $firebirdPath)) {
	$firebirdPath = $firebirdPath -replace "C:", "E:"
	If (-not (Test-Path -Path $firebirdPath)) {
		$firebirdPath = $firebirdPath -replace "E:", "C:"
	}
}
$firebirdPath = "C:/Program Files/Firebird/Firebird_$($package.Split(".")[0].Split("-")[1])_$($package.Split(".")[1])"
Write-Host "Using '$firebirdPath' as FirebirdSQL installation folder"

$filesChanged = & git diff --name-only HEAD HEAD~1
if ($force -or ($filesChanged -like "*firebird*")) {
	Write-Host "Deploying FirebirdSQL testing environment"

	if (-not (Test-Path -Path $firebirdPath\firebird.exe)) {
		$firebirdVersion = "v$($package.Split(".")[0].Split("-")[1]).$($package.Split(".")[1]).$($package.Split(".")[2])"
		Write-Host "`tDownloading FirebirdSQL $firebirdVersion ..."
		Invoke-WebRequest "$rootUrl/$firebirdVersion/$package.exe" -OutFile "$env:temp\firebird-install.exe"
		Write-Host "`tInstalling FirebirdSQL ..."
		& "$env:temp\firebird-install.exe" "/VERYSILENT /NORESTART /NOICONS /LOG=`"$env:temp\firebird-log.txt`".Split(" ")
		if ((Test-Path -Path $firebirdPath\firebird.exe)) {
			Write-Host "`tFirebirdSQL installed in $firebirdPath"
		} else {
			Write-Host "`tCannot find the location of the installation of FirebirdSQL"
			Get-Content($env:temp\firebird-log.txt) | Write-Host 
		}
	} else {
		Write-Host "`tFirebirdSQL already installed: skipping installation."
	}



	# Starting service
	$firebirdServiceName = "FirebirdServerDefaultInstance"
    $service = Get-Service -Name $firebirdServiceName
     
    if ($service.Status -eq "Running") {
        Write-Host "`tService '$($service.DisplayName)' already started."
    } else {
        Write-Host "`tStarting service '$($service.DisplayName)' ..."
        Start-Service -Name $firebirdServiceName
        Write-Host "`tService started."
    }

	# Deploying database based on script
	$databasePath = "$binPath\Customer.fdb"
	If (Test-Path -Path $databasePath) {
		Remove-Item -Path $databasePath
	}

	Write-host "`tCreating database at $databasePath"
	If (-not($env:PATH -like $firebirdPath)) {
		$env:PATH += ";$firebirdPath"
	}
	Get-Content ".\DubUrl.QA\FirebirdSQL\deploy-firebird-database.sql" | & isql.exe -u SYSADMIN -p masterkey -i ".\Duburl.QA\FirebirdSQL\deploy-firebird-database.sql" -b -e -q

	# Installing ODBC driver
	Write-host "`tDeploying FirebirdSQL ODBC drivers"
	$odbcDriverInstalled = $false;
	$drivers = Get-OdbcDriver -Name "*firebird*" -Platform "64-bit"
	If ($drivers.Length -eq 0) {
		Write-Host "`t`tDownloading FirebirdSQL ODBC driver ..."
		Invoke-WebRequest "https://downloads.sourceforge.net/project/firebird/firebird-ODBC-driver/2.0.5-Release/Firebird_ODBC_2.0.5.156_x64.exe?ts=gAAAAABkLAh47QYDiggM29OgN08H8hWFCY2_ph5GKLpc0ho-5aHKxXoczAWxsSMuc8MIKS55x8LtD3fAFyAX3Da2O0PDyo-4oA%3D%3D&amp;use_mirror=deac-fra&amp;r=https%3A%2F%2Ffirebirdsql.org%2F" `
		    -OutFile "$env:temp\firebird_odbc.exe"
		
		try { Write-Host "`t`tInstalling FirebirdSQL ODBC driver ..."
		    & "$env:temp\firebird_odbc.exe" "/VERYSILENT /NORESTART /NOICONS".Split(" ") | Out-Host
			$odbcDriverInstalled = $true;
	    } catch {
			Write-Host "An exception was caught: $($_.Exception.Message)"
		}

		if ($odbcDriverInstalled = $true -eq $true) {
			Write-Host "`t`tChecking installation ..."
			Get-OdbcDriver -Name "*firebird*"
			Write-Host "`tDeployment of FirebirdSQL ODBC driver finalized."	
		}
	} else {
		$odbcDriverInstalled = $true;
		Write-Host "`t`tDrivers already installed:"
		Get-OdbcDriver -Name "*firebird*" -Platform "64-bit"
		Write-Host "`tSkipping installation of new drivers"
	}

	# Running QA tests
	Write-Host "Running QA tests related to FirebirdSQL"
	& dotnet build DubUrl.QA -c Release --nologo

	# To avoid to run the two test-suites in parallel
	& dotnet test DubUrl.QA --filter "(TestCategory=FirebirdSQL""&""TestCategory=AdoProvider)" -c Release --test-adapter-path:. --logger:Appveyor --no-build --nologo
	If ($odbcDriverInstalled -eq $true) {
		& dotnet test DubUrl.QA --filter "(TestCategory=FirebirdSQL""&""TestCategory=ODBC)" -c Release --test-adapter-path:. --logger:Appveyor --no-build --nologo
	}

	# Stoping service
    $service = Get-Service -Name $firebirdServiceName
    if ($service.Status -eq "Stopped") {
        Write-Host "Service '$($service.DisplayName)' already stopped."
    } else {
        Write-Host "Stopping service '$($service.DisplayName)' ..."
        Stop-Service -Name $firebirdServiceName
        Write-Host "Service stopped."
    }
} else {
	Write-Host "Skipping the deployment and run of QA testing for DuckDB"
}
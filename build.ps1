./.env.ps1

dotnet build Build

Get-Item -Path bin | Remove-Item -Force -Confirm:$false -Recurse
New-Item -Path bin -ItemType:Directory
Copy-Item Build/bin/Debug/net6.0/* bin/ -Recurse

dotnet bin/Build.dll $args

./.env.ps1

dotnet build build

Get-Item -Path bin | Remove-Item -Force -Confirm:$false -Recurse
New-Item -Path bin -ItemType:Directory
Copy-Item build/bin/Debug/net5.0/* bin/ -Recurse

dotnet bin/build.dll

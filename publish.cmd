dotnet clean
rd /s /q bin 
rd /s /q obj
dotnet nuget locals all --clear
dotnet restore
mkdir bin\Release\publish
dotnet publish -c Release --sc -o ./bin/Release/publish

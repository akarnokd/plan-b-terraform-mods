@echo off
dotnet new bepinex5plugin -n %1 -T net462 -U 2021.3.14
dotnet restore %1

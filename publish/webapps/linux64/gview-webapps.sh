#!/bin/bash

cd ./webapps

dotnet gView.WebApps.dll --urls http://localhost:45623 &

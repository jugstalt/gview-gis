#!/bin/bash

cd ./server

dotnet gView.Server.dll --urls http://localhost:45622 &

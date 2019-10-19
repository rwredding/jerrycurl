#!/bin/bash

if which pwsh &> /dev/null; then
    pwsh ./build.ps1
else    
    echo "Please install PowerShell Core."
fi
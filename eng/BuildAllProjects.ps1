#!/usr/bin/env pwsh
# SPDX-FileCopyrightText: 2022 smdn <smdn@smdn.jp>
# SPDX-License-Identifier: MIT

$RepositoryRootDirectory = [System.IO.Path]::GetFullPath(
  [System.IO.Path]::Join($PSScriptRoot, "../")
)

# download Smdn.MSBuild.ProjectAssets.* first
dotnet restore ${RepositoryRootDirectory}eng\InstallProjectAssets.proj

# create a solution for the build target projects
Set-Location $RepositoryRootDirectory

dotnet new sln

# add build target projects to the solution
dotnet sln add ${RepositoryRootDirectory}src/Smdn.*/*.csproj

# restore dependencies
dotnet restore

# then build all projects
dotnet build --no-restore

@echo off

:: Have to register the shared repo as 'shared-lib' to current git repo first.
:: > git remote add shared-lib git@github.com:jihworks/UnityInfrastructure.git

:: set PREFIX={UnityProject}/Assets/Plugins/UnityInfrastructure
:: {UnityProject} is current root folder name of the Unity project.

set PREFIX=UnityEraOfNitrate/Assets/Plugins/UnityInfrastructure
set REMOTE=shared-lib
set BRANCH=main

if "%1" == "add" goto :ADD
if "%1" == "pull" goto :PULL
if "%1" == "push" goto :PUSH
goto :USAGE

:ADD
echo [INFO] Adding...
git subtree add --prefix=%PREFIX% %REMOTE% %BRANCH% --squash
goto :END

:PULL
echo [INFO] Pulling...
git subtree pull --prefix=%PREFIX% %REMOTE% %BRANCH% --squash
goto :END

:PUSH
echo [INFO] Pushing...
git subtree push --prefix=%PREFIX% %REMOTE% %BRANCH%
goto :END

:USAGE
echo [USAGE] sync-shared.bat [add / pull / push]
goto :END

:END
pause

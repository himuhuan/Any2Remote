@echo off
rem ################### This file is a part of Any2Remote Project ###################
rem Script For AdminClient pre-build
rem #################################################################################

rem AdminClient relies on the Server and AdminRunner, which are launched by AdminClient as external programs.
rem After ensuring that both the Server and AdminRunner are successfully published, 
rem copy the published application to the Assets folder before building AdminClient.

cd  %~dp0

xcopy /d ".\Any2Remote.Windows.Server\bin\Release\net6.0-windows10.0.18362.0\publish" ".\Any2Remote.Windows.AdminClient\Assets\Any2RemoteServer" /E /Y /i
xcopy /d ".\Any2Remote.Windows.AdminRunner\bin\Release\net6.0-windows10.0.18362.0\publish" ".\Any2Remote.Windows.AdminClient\Assets\AdminRunner" /E /Y /i
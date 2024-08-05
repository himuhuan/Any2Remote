!define APP_NAME "Any2Remote"
!define APP_VERSION "1.2.0"

Name "${APP_NAME} ${APP_VERSION}"
OutFile "Any2Remote.Setup.exe"
InstallDir "$PROGRAMFILES64\HimuQAQ\Any2Remote"
ICON ".\Any2Remote.Windows.AdminClient\Assets\WindowIcon.ico"

Function .onInit
  MessageBox MB_ICONEXCLAMATION|MB_OK "请关闭所有的反病毒软件以确保安装顺利进行。当你关闭反病毒软件的实时扫描功能后，点击 确定 以进行安装。" IDOK +2
  Abort ;
FunctionEnd

Section ""
  SetOutPath "$INSTDIR\net6desktopruntime_x64"
  File ".\Any2Remote.Windows.Setup\Release\net6desktopruntime_x64\windowsdesktop-runtime-6.0.32-win-x64.exe"
  SetOutPath "$INSTDIR"
  File ".\Any2Remote.Windows.Setup\Release\setup.exe"
  File ".\Any2Remote.Windows.Setup\Release\Any2Remote.Windows.Setup.msi"
  ExecWait '"$INSTDIR\setup.exe"'
  Delete "$INSTDIR\net6desktopruntime_x64\windowsdesktop-runtime-6.0.32-win-x64.exe"
  Delete "$INSTDIR\Any2Remote.Windows.Setup.msi"
  Delete "$INSTDIR\setup.exe"
  Exec "$INSTDIR\..\Any2Remote Manager\Any2Remote.Windows.AdminClient.exe"
SectionEnd

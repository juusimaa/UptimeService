;--------------------------------
; Definitions
!define NAME "Uptime Service"
!define REG_PATH "UptimeService"

; The name of the installer
Name ${NAME}

; The file to write
OutFile "UptimeServiceInstaller.exe"

; The default installation directory
InstallDir "$DOCUMENTS\${NAME}"

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\${REG_PATH}" "Install_Dir"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

;--------------------------------

; Pages

Page components
Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

;--------------------------------

; The stuff to install
Section "UptimeService (required)" 

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
  File "..\UptimeService\bin\Release\Common.dll"
  File "..\UptimeService\bin\Release\UptimeService.exe"
  File "..\UptimeClient\bin\Release\UptimeClient.exe"
  
    ; Write the installation path into the registry
  WriteRegStr HKLM "SOFTWARE\${REG_PATH}" "Install_Dir" "$INSTDIR"
  
    ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "DisplayName" "${REG_PATH}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
  
SectionEnd

; Optional section (can be disabled by the user)
Section "Start Menu Shortcuts"

  CreateDirectory "$SMPROGRAMS\${NAME}"
  CreateShortCut "$SMPROGRAMS\${NAME}\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  CreateShortCut "$SMPROGRAMS\${NAME}\${NAME}.lnk" "$INSTDIR\UptimeClient.exe" "" "$INSTDIR\UptimeClient.exe" 0
  
SectionEnd

;--------------------------------

; Uninstaller

Section "Uninstall"
  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}"
  DeleteRegKey HKLM "SOFTWARE\${REG_PATH}"

  ; Remove files and uninstaller
  Delete $INSTDIR\Common.dll
  Delete $INSTDIR\UptimeService.exe
  Delete $INSTDIR\UptimeClient.exe
  Delete $INSTDIR\uninstall.exe

  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\${NAME}\*.*"

  ; Remove directories used
  RMDir "$DOCUMENTS\${NAME}"
  RMDir "$INSTDIR"

SectionEnd
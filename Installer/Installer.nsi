;--------------------------------
;Includes
!include 'LogicLib.nsh'

; Definitions
!define NAME "Uptime Service"
!define REG_PATH "UptimeService"
!define SERVICE_NAME "UptimeService"

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
; Macros
!macro StopService
	; Stop a service and waits for file release. Be sure to pass the service name, not the display name.
	SimpleSC::StopService "${SERVICE_NAME}" 1 30
	Pop $0 ; returns an errorcode (<>0) otherwise success (0)
	${If} $0 == 0
		DetailPrint "Service stopped successfully."
	${Else}
		DetailPrint "Service stop failed."
	${EndIf}
!macroend

!macro StartService
	; Start a service. Be sure to pass the service name, not the display name.
	SimpleSC::StartService "${SERVICE_NAME}" "" 30
	Pop $0 ; returns an errorcode (<>0) otherwise success (0)
	${If} $0 == 0
		DetailPrint "Service started successfully."
	${Else}
		DetailPrint "Service start failed."
	${EndIf}
!macroend

!macro InstallService
	; Install a service - ServiceType own process - StartType automatic - NoDependencies - Logon as System Account
	SimpleSC::InstallService "${SERVICE_NAME}" "Uptime service" "16" "2" "$INSTDIR\UptimeService.exe" "" "" ""
	Pop $0 ; returns an errorcode (<>0) otherwise success (0)  
	${If} $0 == 0
		DetailPrint "Service installed successfully."
	${Else}
		DetailPrint "Service install failed."
		SetErrors
	${EndIf}
!macroend

!macro RemoveService
	; Remove a service
	SimpleSC::RemoveService "${SERVICE_NAME}" 	
	Pop $0 ; returns an errorcode (<>0) otherwise success (0)
	${If} $0 == 0
		DetailPrint "Service removed successfully."
	${Else}
		DetailPrint "Service remove failed."
		SetErrors
	${EndIf}
!macroend

;--------------------------------

; The stuff to install
Section "UptimeService (required)" 
	
	!insertmacro StopService
	!insertmacro RemoveService
	
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

	!insertmacro InstallService
	!insertmacro StartService
	
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
  
  	!insertmacro StopService
	!insertmacro RemoveService
	
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
; Codename ZeroKey

#define MyAppName "ZeroKey"
#define MyAppVersion "2.0.0.205"
#define MyAppPublisher "Wolkenhof"
#define MyAppURL "https://wolkenhof.com/"
#define MyAppExeName "ZeroKey.UI.View.exe"
#define AppGuid "{A33A7C95-A915-4A87-8E33-0A429819B22E}"
#define AppPath = "{app}\App"
#define ServiceCredentialPath = "{app}\Service\Credential"
#define ServiceManagementPath = "{app}\Service\Management"
#define ServiceAppPath = "{app}\Service\Service"
#define ServiceAppPluginsPath = "{app}\Service\Service\Plugins"
#define ServiceAppMedatixxPath = "{app}\Service\Service\medatixx"
#define RegistryKey = "{{8EB4E5F7-9DFB-4674-897C-2A584934CDBE}"
#define ProviderNameKey = "NFCCredentialProvider"
#define VCmsg "Installing Microsoft Visual C++ Redistributable ..."
#define NETmsg "Installing Microsoft .NET 6.0 ..."

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64

AppId={{#AppGuid}
AppName= {#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

DefaultDirName={pf}\{#MyAppPublisher}\{#MyAppName}
DisableProgramGroupPage=yes

OutputDir=..\Result
;OutputBaseFilename={#MyAppName}_{#GitCommitHash}_{#MyAppVersion}
OutputBaseFilename={#MyAppName}_INTERNAL_T0_{#MyAppVersion}
CloseApplications=force

Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; exe file
Source: "..\..\bin\Release\UI\ZeroKey.UI.View.exe"; DestDir: {#AppPath}; Flags: ignoreversion; \
    BeforeInstall: TaskKill('ZeroKey.UI.View.exe')
; Application files
Source: "..\..\bin\Release\UI\ZeroKey.UI.View.exe.config"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\NLog.config"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\Autofac.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\Autofac.Extras.CommonServiceLocator.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\GalaSoft.MvvmLight.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\GalaSoft.MvvmLight.Extras.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\GalaSoft.MvvmLight.Platform.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\Meziantou.Framework.Win32.CredentialManager.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\Microsoft.Practices.ServiceLocation.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\Newtonsoft.Json.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\ZeroKey.UI.ViewModel.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\ZeroKeyServiceCommon.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\NLog.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\System.Diagnostics.EventLog.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\System.Security.Principal.Windows.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\System.ServiceProcess.ServiceController.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\System.Windows.Interactivity.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\Icon.ico"; DestDir: {#AppPath}; Flags: ignoreversion

; Service files
Source: "..\..\bin\Release\Service\ZeroKeyServiceHost.exe.config"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\Newtonsoft.Json.dll"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\ZeroKeyServiceCommon.dll"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\ZeroKeyServiceCore.dll"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\ZeroKeyServiceHost.exe"; DestDir: {#ServiceAppPath}; Flags: ignoreversion

; Service plugin files
Source: "..\..\bin\Release\Service\Plugins\ZeroKey.Plugin.Lock.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\Plugins\ZeroKey.Plugin.Unlock.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\Plugins\Newtonsoft.Json.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion

; Plugin (medatixx)
Source: "..\..\bin\Release\Service\medatixx\CommandLine.dll"; DestDir: {#ServiceAppMedatixxPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\medatixx\Microsoft.Toolkit.Uwp.Notifications.dll"; DestDir: {#ServiceAppMedatixxPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\medatixx\Microsoft.Windows.SDK.NET.dll"; DestDir: {#ServiceAppMedatixxPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\medatixx\Newtonsoft.Json.dll"; DestDir: {#ServiceAppMedatixxPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\medatixx\ZeroKey.Plugin.medatixx.deps.json"; DestDir: {#ServiceAppMedatixxPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\medatixx\ZeroKey.Plugin.medatixx.dll"; DestDir: {#ServiceAppMedatixxPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\medatixx\ZeroKey.Plugin.medatixx.exe"; DestDir: {#ServiceAppMedatixxPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\medatixx\ZeroKey.Plugin.medatixx.pdb"; DestDir: {#ServiceAppMedatixxPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\medatixx\ZeroKey.Plugin.medatixx.runtimeconfig.json"; DestDir: {#ServiceAppMedatixxPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\medatixx\WinRT.Runtime.dll"; DestDir: {#ServiceAppMedatixxPath}; Flags: ignoreversion

; Management
;Source: "..\..\bin\Release\Management\CredentialRegistration.exe.config"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
;Source: "..\..\bin\Release\Management\Newtonsoft.Json.dll"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
;Source: "..\..\bin\Release\Management\ZeroKeyServiceCommon.dll"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
;Source: "..\..\bin\Release\Management\CredentialRegistration.exe"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
;Source: "..\..\bin\Release\Management\WinAPIWrapper.dll"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion

; Credential registration
;Source: "..\..\bin\Release\Credential\CredUILauncher.exe"; DestDir: {#ServiceCredentialPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Credential\NFCCredentialProvider.dll"; DestDir: {#ServiceCredentialPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Credential\tileimage.bmp"; DestDir: {#ServiceCredentialPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Credential\NFCCredentialProvider.dll"; DestDir: {sys};

; Visual C++ 2015
;Source: "vc_redist.x64.exe"; DestDir: {tmp}; Flags: deleteafterinstall

; .NET 6.0
;Source: "windowsdesktop-runtime-6.0.5-win-x64.exe"; DestDir: {tmp}; Flags: deleteafterinstall

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{commonprograms}\Wolkenhof\{#MyAppName}"; Filename: "{app}\App\{#MyAppExeName}"

[Code] 
//#include "dotnet.pas"
#include "checkinstalled.pas"
//#include "vc.pas"

function InitializeSetup(): Boolean;
begin
    //if not NET6NeedsInstall() then
    //begin
    //    result := false;    
    //end;

    //if not CheckNetFramework() then
    //begin
    //    result := false;
    //end else
    begin
        result := CheckInstalledVersion();
    end;
end; 

procedure TaskKill(FileName: String);
var
  ResultCode: Integer;
begin
    Exec('taskkill.exe', '/f /im ' + '"' + FileName + '"', '', SW_HIDE,
     ewWaitUntilTerminated, ResultCode);
end;

[Run]
;Filename: "{tmp}\vc_redist.x64.exe"; StatusMsg: "{#VCmsg}"; Check: IsWin64 and VCRedistNeedsInstall
;Filename: "{tmp}\windowsdesktop-runtime-6.0.5-win-x64.exe"; StatusMsg: "{#NETmsg}"; Check: IsWin64
Filename: "{#AppPath}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
Filename: "{#ServiceAppPath}\ZeroKeyServiceHost.exe"; Flags: runascurrentuser; Parameters: "--install"

[UninstallRun]
Filename: "{#ServiceAppPath}\ZeroKeyServiceHost.exe"; Parameters: "--uninstall"
Filename: "{sys}\NFCCredentialProvider.dll"

[Registry]
; [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{8EB4E5F7-9DFB-4674-897C-2A584934CDBE}]
; @="NFCCredentialProvider"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{#RegistryKey}"; ValueType: string; ValueName: ""; ValueData: "{#ProviderNameKey}"; Flags: uninsdeletekey

; [HKEY_CLASSES_ROOT\CLSID\{8EB4E5F7-9DFB-4674-897C-2A584934CDBE}]
; @="NFCCredentialProvider"
Root: HKCR; Subkey: "CLSID\{#RegistryKey}"; ValueType: string; ValueName: ""; ValueData: "{#ProviderNameKey}"; Flags: uninsdeletekey

; [HKEY_CLASSES_ROOT\CLSID\{8EB4E5F7-9DFB-4674-897C-2A584934CDBE}\InprocServer32]
; @="NFCCredentialProvider.dll"
; "ThreadingModel"="Apartment"
Root: HKCR; Subkey: "CLSID\{#RegistryKey}\InprocServer32"; ValueType: string; ValueName: ""; ValueData: "{#ProviderNameKey}.dll"
Root: HKCR; Subkey: "CLSID\{#RegistryKey}\InprocServer32"; ValueType: string; ValueName: "ThreadingModel"; ValueData: "Apartment"
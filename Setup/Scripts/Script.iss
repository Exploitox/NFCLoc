; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "NFCLoc"
#define GitCommitHash "c4aa8c2"
#define MyAppVersion "2.0.0.57"
#define MyAppPublisher "Wolkenhof"
#define MyAppURL "http://wolkenhof.com/"
#define MyAppExeName "NFCLoc.UI.View.exe"
#define AppGuid "{F7D4EF32-2D80-441A-A499-3E6000BFCEBA}"
#define AppPath = "{app}\App"
#define ServiceCredentialPath = "{app}\Service\Credential"
#define ServiceManagementPath = "{app}\Service\Management"
#define ServiceAppPath = "{app}\Service\Service"
#define ServiceAppPluginsPath = "{app}\Service\Service\Plugins"
#define medatixxPluginPath = "{app}\Service\medatixx"
#define medatixxCredManager = "{app}\Management\medatixx"
#define RegistryKey = "{{8EB4E5F7-9DFB-4674-897C-2A584934CDBE}"
#define ProviderNameKey = "NFCLocCredentialProvider"
#define VCmsg "Installing Microsoft Visual C++ Redistributable...."

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64

AppId={{#AppGuid}
AppName= {#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

DefaultDirName={pf}\{#MyAppPublisher}\{#MyAppName}
DisableProgramGroupPage=yes

OutputDir=..\Result
OutputBaseFilename={#MyAppName}_{#GitCommitHash}_{#MyAppVersion}

Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; exe file
Source: "..\..\bin\Release\UI\NFCLoc.UI.View.exe"; DestDir: {#AppPath}; Flags: ignoreversion

; Application files
Source: "..\..\bin\Release\UI\NFCLoc.UI.View.exe.config"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\NLog.config"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\Autofac.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\Autofac.Extras.CommonServiceLocator.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\GalaSoft.MvvmLight.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\GalaSoft.MvvmLight.Extras.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\GalaSoft.MvvmLight.Platform.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\Microsoft.Practices.ServiceLocation.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\Newtonsoft.Json.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\NFCLoc.UI.ViewModel.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\NFCLocServiceCommon.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\NLog.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\System.Windows.Interactivity.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\UI\Icon.ico"; DestDir: {#AppPath}; Flags: ignoreversion

; Service files
Source: "..\..\bin\Release\Service\NFCLocServiceHost.exe.config"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\Newtonsoft.Json.dll"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\NFCLocServiceCommon.dll"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\NFCLocServiceCore.dll"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\NFCLocServiceHost.exe"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
;Source: "..\..\bin\Release\Service\WinAPIWrapper.dll"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\Plugins\NFCLoc.Plugin.Lock.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\Plugins\NFCLoc.Plugin.Unlock.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion
;Source: "..\..\bin\Release\Service\Plugins\NFCLocServiceCommon.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion
;Source: "..\..\bin\Release\Service\Plugins\NFCLocServiceCore.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\Plugins\Newtonsoft.Json.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion

Source: "..\..\bin\Release\Management\CredentialRegistration.exe.config"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Management\Newtonsoft.Json.dll"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Management\NFCLocServiceCommon.dll"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Management\CredentialRegistration.exe"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
;Source: "..\..\bin\Release\Management\WinAPIWrapper.dll"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion

Source: "..\..\bin\Release\Credential\CredUILauncher.exe"; DestDir: {#ServiceCredentialPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Credential\NFCLocCredentialProvider.dll"; DestDir: {#ServiceCredentialPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Credential\tileimage.bmp"; DestDir: {#ServiceCredentialPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Credential\NFCLocCredentialProvider.dll"; DestDir: {sys};

; Plugin (medatixx)
Source: "..\..\NFCLoc.Plugin.medatixx\bin\Release\net6.0-windows10.0.19041.0\CommandLine.dll"; DestDir: {#medatixxPluginPath}; Flags: ignoreversion
Source: "..\..\NFCLoc.Plugin.medatixx\bin\Release\net6.0-windows10.0.19041.0\Microsoft.Toolkit.Uwp.Notifications.dll"; DestDir: {#medatixxPluginPath}; Flags: ignoreversion
Source: "..\..\NFCLoc.Plugin.medatixx\bin\Release\net6.0-windows10.0.19041.0\Microsoft.Windows.SDK.NET.dll"; DestDir: {#medatixxPluginPath}; Flags: ignoreversion
Source: "..\..\NFCLoc.Plugin.medatixx\bin\Release\net6.0-windows10.0.19041.0\Newtonsoft.Json.dll"; DestDir: {#medatixxPluginPath}; Flags: ignoreversion
Source: "..\..\NFCLoc.Plugin.medatixx\bin\Release\net6.0-windows10.0.19041.0\NFCLoc.Plugin.medatixx.deps.json"; DestDir: {#medatixxPluginPath}; Flags: ignoreversion
Source: "..\..\NFCLoc.Plugin.medatixx\bin\Release\net6.0-windows10.0.19041.0\NFCLoc.Plugin.medatixx.dll"; DestDir: {#medatixxPluginPath}; Flags: ignoreversion
Source: "..\..\NFCLoc.Plugin.medatixx\bin\Release\net6.0-windows10.0.19041.0\NFCLoc.Plugin.medatixx.exe"; DestDir: {#medatixxPluginPath}; Flags: ignoreversion
Source: "..\..\NFCLoc.Plugin.medatixx\bin\Release\net6.0-windows10.0.19041.0\NFCLoc.Plugin.medatixx.pdb"; DestDir: {#medatixxPluginPath}; Flags: ignoreversion
Source: "..\..\NFCLoc.Plugin.medatixx\bin\Release\net6.0-windows10.0.19041.0\NFCLoc.Plugin.medatixx.runtimeconfig.json"; DestDir: {#medatixxPluginPath}; Flags: ignoreversion
Source: "..\..\NFCLoc.Plugin.medatixx\bin\Release\net6.0-windows10.0.19041.0\WinRT.Runtime.dll"; DestDir: {#medatixxPluginPath}; Flags: ignoreversion

; medatixx Credential Manager
Source: "..\..\Management\NFCLoc.CredManager.medatixx\bin\Release\net6.0-windows\publish\NFCLoc.CredManager.medatixx.exe"; DestDir: {#medatixxCredManager}; Flags: ignoreversion
Source: "..\..\Management\NFCLoc.CredManager.medatixx\bin\Release\net6.0-windows\publish\D3DCompiler_47_cor3.dll"; DestDir: {#medatixxCredManager}; Flags: ignoreversion
Source: "..\..\Management\NFCLoc.CredManager.medatixx\bin\Release\net6.0-windows\publish\PenImc_cor3.dll"; DestDir: {#medatixxCredManager}; Flags: ignoreversion
Source: "..\..\Management\NFCLoc.CredManager.medatixx\bin\Release\net6.0-windows\publish\PresentationNative_cor3.dll"; DestDir: {#medatixxCredManager}; Flags: ignoreversion
Source: "..\..\Management\NFCLoc.CredManager.medatixx\bin\Release\net6.0-windows\publish\vcruntime140_cor3.dll"; DestDir: {#medatixxCredManager}; Flags: ignoreversion
Source: "..\..\Management\NFCLoc.CredManager.medatixx\bin\Release\net6.0-windows\publish\wpfgfx_cor3.dll"; DestDir: {#medatixxCredManager}; Flags: ignoreversion
Source: "..\..\Management\NFCLoc.CredManager.medatixx\bin\Release\net6.0-windows\publish\Newtonsoft.Json.xml"; DestDir: {#medatixxCredManager}; Flags: ignoreversion

; Visual C++ 2015
Source: "vc_redist.x64.exe"; DestDir: {tmp}; Flags: deleteafterinstall

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\App\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\App\{#MyAppExeName}"; Tasks: desktopicon

[Code]
#include "dotnet.pas"
#include "checkinstalled.pas"
#include "vc.pas"

function InitializeSetup(): Boolean;
begin
    if not CheckNetFramework() then
    begin
        result := false;
    end else
    begin
        result := CheckInstalledVersion();
    end;
end; 

[Run]
Filename: "{tmp}\vc_redist.x64.exe"; StatusMsg: "{#VCmsg}"; Check: IsWin64 and VCRedistNeedsInstall
Filename: "{#AppPath}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
Filename: "{#ServiceAppPath}\NFCLocServiceHost.exe"; Flags: runascurrentuser; Parameters: "--install"

[UninstallRun]
Filename: "{#ServiceAppPath}\NFCLocServiceHost.exe"; Parameters: "--uninstall"
Filename: "{sys}\NFCLocCredentialProvider.dll"

[Registry]
; [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{8EB4E5F7-9DFB-4674-897C-2A584934CDBE}]
; @="NFCLocCredentialProvider"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{#RegistryKey}"; ValueType: string; ValueName: ""; ValueData: "{#ProviderNameKey}"; Flags: uninsdeletekey

; [HKEY_CLASSES_ROOT\CLSID\{8EB4E5F7-9DFB-4674-897C-2A584934CDBE}]
; @="NFCLocCredentialProvider"
Root: HKCR; Subkey: "CLSID\{#RegistryKey}"; ValueType: string; ValueName: ""; ValueData: "{#ProviderNameKey}"; Flags: uninsdeletekey

; [HKEY_CLASSES_ROOT\CLSID\{8EB4E5F7-9DFB-4674-897C-2A584934CDBE}\InprocServer32]
; @="NFCLocCredentialProvider.dll"
; "ThreadingModel"="Apartment"
Root: HKCR; Subkey: "CLSID\{#RegistryKey}\InprocServer32"; ValueType: string; ValueName: ""; ValueData: "{#ProviderNameKey}.dll"
Root: HKCR; Subkey: "CLSID\{#RegistryKey}\InprocServer32"; ValueType: string; ValueName: "ThreadingModel"; ValueData: "Apartment"
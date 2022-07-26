#define MyAppName "ZeroKey Server"
#define MyAppVersion "2.0.0.205"
#define MyAppPublisher "Wolkenhof GmbH"
#define MyAppURL "https://github.com/Wolkenhof/ZeroKey"
#define MyAppExeName "ZeroKey.ServerUI.exe"
#define AppGuid "{CD18DAFE-417E-414A-9DBF-90CE29BD1854}"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{CD18DAFE-417E-414A-9DBF-90CE29BD1854}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

DefaultDirName={commonpf}\{#MyAppPublisher}\{#MyAppName}
DisableProgramGroupPage=yes

LicenseFile=license.rtf
ArchitecturesInstallIn64BitMode=x64

OutputDir=..\Result
OutputBaseFilename={#MyAppName}_{#GitCommitHash}_{#MyAppVersion}

CloseApplications=force
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; exe file
Source: "..\..\bin\Release\ServerUI\ZeroKey.ServerUI.exe"; DestDir: {app}; Flags: ignoreversion; \
    BeforeInstall: TaskKill('ZeroKey.ServerUI.exe')
Source: "..\..\bin\Release\ServerUI\ZeroKey.Server.Service.exe"; DestDir: {app}; Flags: ignoreversion; \
    BeforeInstall: TaskKill('ZeroKey.Server.Service.exe')

; Application files
Source: "..\..\bin\Release\ServerUI\Newtonsoft.Json.dll"; DestDir: {app}; Flags: ignoreversion
Source: "..\..\bin\Release\ServerUI\server.pfx"; DestDir: {app}; Flags: ignoreversion
Source: "..\..\bin\Release\ServerUI\System.ServiceProcess.ServiceController.dll"; DestDir: {app}; Flags: ignoreversion
Source: "..\..\bin\Release\ServerUI\Wpf.Ui.dll"; DestDir: {app}; Flags: ignoreversion
Source: "..\..\bin\Release\ServerUI\ZeroKey.Server.Service.exe.config"; DestDir: {app}; Flags: ignoreversion
Source: "..\..\bin\Release\ServerUI\ZeroKey.ServerUI.deps.json"; DestDir: {app}; Flags: ignoreversion
Source: "..\..\bin\Release\ServerUI\ZeroKey.ServerUI.dll"; DestDir: {app}; Flags: ignoreversion
Source: "..\..\bin\Release\ServerUI\ZeroKey.ServerUI.runtimeconfig.json"; DestDir: {app}; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{commonprograms}\Wolkenhof GmbH\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
Filename: "{app}\ZeroKey.Server.Service.exe"; Flags: runascurrentuser; Parameters: "--install"

[UninstallRun]
Filename: "{app}\ZeroKey.Server.Service.exe"; Parameters: "--uninstall"

[Code] 
#include "checkinstalled.pas"

function InitializeSetup(): Boolean;
begin
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

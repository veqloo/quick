; Script generated for Inno Setup 6 / 7
; Veqlo Quick v1.0 Windows 11 Installer

#define MyAppName "Veqlo Quick"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Veqlo"
#define MyAppURL "https://veqlo.com"
#define MyAppExeName "VeqloQuick.exe"

[Setup]
AppId={{65863AF6-B22D-4C52-BB0F-610D11268E04}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={localappdata}\Programs\Veqlo Quick
DisableDirPage=yes
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputBaseFilename=VeqloQuickSetup_Inno
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\package\VeqloQuick.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\package\VeqloQuickShell.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\package\AppxManifest.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\package\Assets\*"; DestDir: "{app}\Assets"; Flags: ignoreversion recursesubdirs createallsubdirs

[Run]
; Register the Sparse Package with Windows 11 File Explorer
Filename: "powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -Command ""Add-AppxPackage -Register '{app}\AppxManifest.xml' -ExternalLocation '{app}'"""; Flags: runhidden waituntilterminated

[UninstallRun]
; Cleanly unregister the package upon removal
Filename: "powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -Command ""$p = Get-AppxPackage -Name 'Veqlo.Quick' -ErrorAction SilentlyContinue; if ($p) {{ Remove-AppxPackage -Package $p.PackageFullName }}"""; Flags: runhidden waituntilterminated

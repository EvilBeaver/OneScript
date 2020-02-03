#include <ISPPBuiltins.iss>
#define AppName "OneScript engine"
#define FSFriendlyName "OneScript"
#define MainExe "TestApp.exe"
#define ArtifactRoot "..\built\tmp"

#define VerMajor
#define VerMinor
#define VerRelease
#define Build

#ifndef Suffix
  #define Suffix "x86"
#endif

#if Suffix == "x64"
  #define Binaries="bin"
#else
  #define Binaries="bin32"
#endif

; duplicates ArtifactsRoot because ISPP can't resolve directives
#expr ParseVersion(ArtifactRoot + "\" + Binaries + "\ScriptEngine.dll",VerMajor,VerMinor,VerRelease,Build)

[Setup]
AppName={#AppName}
AppVersion={#VerMajor}.{#VerMinor}.{#VerRelease}
AppPublisher=1Script Team (Open Source)
DefaultDirName="{pf}\{#FSFriendlyName}"
DefaultGroupName="{#FSFriendlyName}"
OutputBaseFilename="OneScript-{#VerMajor}.{#VerMinor}.{#VerRelease}-{#Suffix}"
DisableProgramGroupPage=yes
UninstallDisplayIcon="{app}\bin\{#MainExe}"
Compression=lzma2
SolidCompression=yes
VersionInfoVersion={#VerMajor}.{#VerMinor}.{#VerRelease}.{#Build}
#if Suffix == "x64"
  ArchitecturesInstallIn64BitMode="x64"
#endif

[InstallDelete]
Type: files; Name: {app}\*.dll
Type: files; Name: {app}\*.exe
Type: files; Name: {app}\*.bat
Type: files; Name: {app}\*.cmd

[Types]
Name: "normal"; Description: "Стандартная установка"
Name: "custom"; Description: "Выборочная установка"; Flags: iscustom

[Components]
Name: "main"; Description: "Основные файлы"; Types: normal custom; Flags: fixed
Name: "isapi"; Description: "Обработчик HTTP-сервисов"; Types: normal custom;
Name: "stdlib"; Description: "Стандартная библиотека скриптов"; Types: normal custom;
Name: "testapp"; Description: "Тестовая консоль (TestApp)";
Name: "docs"; Description: "Документация по свойствам и методам (синтакс-помощник)";

[Files]              
Source: "{#ArtifactRoot}\{#Binaries}\oscript.exe"; DestDir: "{app}\bin"; Components: main
Source: "{#ArtifactRoot}\{#Binaries}\ScriptEngine.HostedScript.dll"; DestDir: "{app}\bin"; Components: main
Source: "{#ArtifactRoot}\{#Binaries}\ScriptEngine.dll"; DestDir: "{app}\bin"; Components: main
Source: "{#ArtifactRoot}\{#Binaries}\OneScript.DebugProtocol.dll"; DestDir: "{app}\bin"; Components: main
Source: "{#ArtifactRoot}\{#Binaries}\OneScript.Language.dll"; DestDir: "{app}\bin"; Components: main
Source: "{#ArtifactRoot}\{#Binaries}\DotNetZip.dll"; DestDir: "{app}\bin"; Components: main
Source: "{#ArtifactRoot}\{#Binaries}\Newtonsoft.Json.dll"; DestDir: "{app}\bin"; Components: main
Source: "{#ArtifactRoot}\{#Binaries}\oscript.cfg"; DestDir: "{app}\bin"; Components: main; Flags: onlyifdoesntexist

Source: "{#ArtifactRoot}\examples\*"; DestDir: "{app}\examples"; Components: main

;isapi
Source: "{#ArtifactRoot}\{#Binaries}\ASPNETHandler.dll"; DestDir: "{app}\bin"; Components: isapi;

; testapp
Source: "{#ArtifactRoot}\{#Binaries}\TestApp.exe"; DestDir: "{app}\bin"; Components: testapp
Source: "{#ArtifactRoot}\{#Binaries}\ICSharpCode.AvalonEdit.dll"; DestDir: "{app}\bin"; Components: testapp

; библиотека
Source: "{#ArtifactRoot}\lib\*"; DestDir: "{app}\lib"; Components: stdlib; Flags: recursesubdirs
Source: "{#ArtifactRoot}\{#Binaries}\*.bat"; DestDir: "{app}\bin"; Components: stdlib

; документация
Source: "{#ArtifactRoot}\doc\*"; DestDir: "{app}\doc"; Components: docs; Flags: recursesubdirs

Source: "dotNetFx40_Full_setup.exe"; DestDir: {tmp}; Flags: deleteafterinstall; Check: not IsRequiredDotNetDetected

[Icons]
Name: "{group}\{#FSFriendlyName}"; Filename: "{app}\bin\{#MainExe}"

[Registry]
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\Session Manager\Environment"; ValueType: expandsz; ValueName: "Path"; ValueData: "{olddata};{app}\bin;"; Check: NeedsAddPath(ExpandConstant('{app}\bin'))

[Run]
Filename: {tmp}\dotNetFx40_Full_setup.exe; Parameters: "/q:a /c:""install /l /q"""; Check: not IsRequiredDotNetDetected; StatusMsg: Microsoft .NET Framework 4.0 is being installed. Please wait..
Filename: "{app}\bin\{#MainExe}"; Description: "Launch application"; Components: testapp; Flags: postinstall nowait skipifsilent unchecked

[Code]

#IFDEF UNICODE
  #DEFINE AW "W"
#ELSE
  #DEFINE AW "A"
#ENDIF
type
  INSTALLSTATE = Longint;
const
  INSTALLSTATE_INVALIDARG = -2;  // An invalid parameter was passed to the function.
  INSTALLSTATE_UNKNOWN = -1;     // The product is neither advertised or installed.
  INSTALLSTATE_ADVERTISED = 1;   // The product is advertised but not installed.
  INSTALLSTATE_ABSENT = 2;       // The product is installed for a different user.
  INSTALLSTATE_DEFAULT = 5;      // The product is installed for the current user.

  // Microsoft Visual C++ 2012 x86 Minimum Runtime - 11.0.61030.0 (Update 4) 
  VC_2012_REDIST_MIN_UPD4_X86 = '{BD95A8CD-1D9F-35AD-981A-3E7925026EBB}';
  VC_2012_REDIST_MIN_UPD4_X64 = '{CF2BEA3C-26EA-32F8-AA9B-331F7E34BA97}';
  // Microsoft Visual C++ 2012 x86 Additional Runtime - 11.0.61030.0 (Update 4) 
  VC_2012_REDIST_ADD_UPD4_X86 = '{B175520C-86A2-35A7-8619-86DC379688B9}';
  VC_2012_REDIST_ADD_UPD4_X64 = '{37B8F9C7-03FB-3253-8781-2517C99D7C00}';

function NeedsAddPath(Param: string): boolean;
var
  OrigPath: string;
begin
  if not RegQueryStringValue(HKEY_LOCAL_MACHINE,'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'Path', OrigPath)
  then begin
    Result := True;
    exit;
  end;
  // look for the path with leading and trailing semicolon
  // Pos() returns 0 if not found
  Result := Pos(';' + UpperCase(Param) + ';', ';' + UpperCase(OrigPath) + ';') = 0;  
  if Result = True then
     Result := Pos(';' + UpperCase(Param) + '\;', ';' + UpperCase(OrigPath) + ';') = 0; 
end;

function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1.4322'     .NET Framework 1.1
//    'v2.0.50727'    .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//    'v4.5'          .NET Framework 4.5
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key: string;
    install, release, serviceCount: cardinal;
    check45, success: boolean;
//var reqNetVer : string;
begin
    // .NET 4.5 installs as update to .NET 4.0 Full
    if version = 'v4.5' then begin
        version := 'v4\Full';
        check45 := true;
    end else
        check45 := false;

    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + version;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0/4.5 uses value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 uses additional value Release
    if check45 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= 378389);
    end;

    result := success and (install = 1) and (serviceCount >= service);
end;

function IsRequiredDotNetDetected(): Boolean;  
begin
    result := IsDotNetDetected('v4\Full', 0);
end;

function InitializeSetup(): Boolean;
begin
    if not IsDotNetDetected('v4\Full', 0) then begin
        MsgBox('{#AppName} requires Microsoft .NET Framework 4.0 Client Profile.'#13#13
          'The installer will attempt to install it', mbInformation, MB_OK);        
    end;
    
    result := true;
end;
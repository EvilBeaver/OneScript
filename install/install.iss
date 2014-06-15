
#include <ISPPBuiltins.iss>
#define AppName "1Script execution engine"
#define FSFriendlyName "1Script execution engine"
#define MainExe "TestApp.exe"

#define VerMajor
#define VerMinor
#define VerRelease
#define Build
#expr ParseVersion("built\ScriptEngine.dll",VerMajor,VerMinor,VerRelease,Build)

[Setup]
AppName={#AppName}
AppVersion={#VerMajor}.{#VerMinor}.{#VerRelease}
DefaultDirName="{pf}\{#FSFriendlyName}"
DefaultGroupName="{#FSFriendlyName}"
DisableProgramGroupPage=yes
UninstallDisplayIcon="{app}\{#MainExe}"
Compression=lzma2
SolidCompression=yes

[Files]
Source: "built\*"; DestDir: "{app}"
Source: "dotNetFx40_Full_setup.exe"; DestDir: {tmp}; Flags: deleteafterinstall; Check: not IsRequiredDotNetDetected

[Icons]
Name: "{group}\{#FSFriendlyName}"; Filename: "{app}\{#MainExe}"

[Run]
Filename: {tmp}\dotNetFx40_Full_setup.exe; Parameters: "/q:a /c:""install /l /q"""; Check: not IsRequiredDotNetDetected; StatusMsg: Microsoft .NET Framework 4.0 is being installed. Please wait..
Filename: "{app}\{#MainExe}"; Description: "Launch application"; Flags: postinstall nowait skipifsilent unchecked

[Code]



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
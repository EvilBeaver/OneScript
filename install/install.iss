#include <ISPPBuiltins.iss>

#define MyAppName      "OneScript"
#define MyAppPublisher "EvilBeaver"
#define MyAppURL       "https://oscript.io"

#define VerMajor
#define VerMinor
#define VerRelease
#define Build

#define ArtifactRoot "..\built\tmp"
#define OvmExeSource "ovm.exe"

#define WizardImageSource "WizardImage.bmp"
#define LicenseSource "..\LICENSE"


#ifndef Suffix
  #define Suffix "x86"
#endif

#if Suffix == "x64"
  #define Binaries="bin"
#else
  #define Binaries="bin32"
#endif

#expr GetVersionComponents(ArtifactRoot + "\" + Binaries + "\ScriptEngine.dll", VerMajor, VerMinor, VerRelease, Build)

[Setup]
AppName                 = {#MyAppName}
AppVersion              = {#VerMajor}.{#VerMinor}.{#VerRelease}
AppPublisher            = {#MyAppPublisher}
AppPublisherURL         = {#MyAppURL}
AppSupportURL           = {#MyAppURL}
AppUpdatesURL           = {#MyAppURL}
DefaultDirName          = {localappdata}\OneScript
LicenseFile             = {#LicenseSource}
DisableProgramGroupPage = yes
OutputBaseFilename      = "OneScript-{#VerMajor}.{#VerMinor}.{#VerRelease}-{#Suffix}"
Compression             = lzma
SolidCompression        = yes
WizardStyle             = modern
DisableWelcomePage      = no
WizardImageFile         = {#WizardImageSource}
PrivilegesRequired      = lowest

#if Suffix == "x64"
  ArchitecturesInstallIn64BitMode="x64"
#endif

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Files]
Source: "{#ArtifactRoot}\*"; DestDir: "{app}\{#VerMajor}.{#VerMinor}.{#VerRelease}"; Flags: recursesubdirs
Source: "{#OvmExeSource}"; DestDir: "{app}"; DestName: "ovm.exe"; Check: InstallOvmSelected


[UninstallDelete]
Type: filesandordirs; Name: "{app}\{#VerMajor}.{#VerMinor}.{#VerRelease}"

[Code]

var
  OvmPage: TWizardPage;
  OvmIntroLabel: TNewStaticText;
  InstallOvmCheckBox: TNewCheckBox;

procedure InitializeWizard;
begin
  OvmPage := CreateCustomPage(wpSelectDir,
    'Менеджер версий OVM',
    'Установка менеджера версий OneScript');

  OvmIntroLabel := TNewStaticText.Create(OvmPage);
  OvmIntroLabel.Parent := OvmPage.Surface;
  OvmIntroLabel.Left := 0;
  OvmIntroLabel.Top := 0;
  OvmIntroLabel.Width := OvmPage.SurfaceWidth;
  OvmIntroLabel.Height := ScaleY(40);
  OvmIntroLabel.AutoSize := False;
  OvmIntroLabel.WordWrap := True;
  OvmIntroLabel.Caption :=
    'OneScript Version Manager (OVM) - утилита, предназначенная для установки, обновления и переключения между различными версиями OneScript';

  InstallOvmCheckBox := TNewCheckBox.Create(OvmPage);
  InstallOvmCheckBox.Parent := OvmPage.Surface;
  InstallOvmCheckBox.Left := 0;
  InstallOvmCheckBox.Top := OvmIntroLabel.Top + OvmIntroLabel.Height + ScaleY(14);
  InstallOvmCheckBox.Width := OvmPage.SurfaceWidth;
  InstallOvmCheckBox.Caption := 'Установить OVM';
  InstallOvmCheckBox.Checked := True;
end;

function InstallOvmSelected(): Boolean;
begin
  Result := InstallOvmCheckBox.Checked;
end;

function RemoveDirectoryLink(const LinkPath: String): Boolean;
var
  ResultCode: Integer;
begin
  if not DirExists(LinkPath) then
  begin
    Result := True;
    Exit;
  end;
  Result := Exec('cmd.exe', '/C rmdir "' + LinkPath + '"',
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

function CreateDirectoryJunction(const LinkPath, TargetPath: String): Boolean;
var
  ResultCode: Integer;
begin
  if not DirExists(TargetPath) then
  begin
    MsgBox('Каталог версии OneScript не найден:' + #13#10 + TargetPath, mbError, MB_OK);
    Result := False;
    Exit;
  end;

  if not RemoveDirectoryLink(LinkPath) then
  begin
    MsgBox('Не удалось удалить существующую ссылку current:' + #13#10 + LinkPath, mbError, MB_OK);
    Result := False;
    Exit;
  end;

  Result := Exec('cmd.exe', '/C mklink /J "' + LinkPath + '" "' + TargetPath + '"',
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);

  if not Result then
    MsgBox(
      'Не удалось создать junction current -> версия.' + #13#10 +
      'Запустите установщик от имени администратора или включите режим разработчика Windows.' + #13#10#13#10 +
      'Link: ' + LinkPath + #13#10 +
      'Target: ' + TargetPath,
      mbError, MB_OK);
end;

function PathContains(const Path, Entry: String): Boolean;
begin
  Result := Pos(';' + Entry + ';', ';' + Path + ';') > 0;
end;

function NeedsAddPath(const Param: String): Boolean;
var
  OrigPath: String;
begin
  if not RegQueryStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', OrigPath) then
  begin
    Result := True;
    Exit;
  end;
  Result := not PathContains(OrigPath, Param);
end;

procedure PrependUserPath(const Entry: String);
var
  OldPath, NewPath: String;
begin
  if not NeedsAddPath(Entry) then
    Exit;
  if RegQueryStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', OldPath) then
  begin
    if OldPath = '' then
      NewPath := Entry
    else
      NewPath := Entry + ';' + OldPath;
    RegWriteExpandStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', NewPath);
  end
  else
    RegWriteExpandStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', Entry);
end;

procedure ActivateOneScriptLikeOvm(const InstallDir: String);
var
  VersionDir, CurrentLink, OscriptBin, OldPath, NewPath: String;
begin
  VersionDir := InstallDir + '\{#VerMajor}.{#VerMinor}.{#VerRelease}';
  CurrentLink := InstallDir + '\current';
  OscriptBin := CurrentLink + '\bin';

  if not CreateDirectoryJunction(CurrentLink, VersionDir) then
    Exit;

  if InstallOvmSelected then
  begin
    RegWriteStringValue(HKEY_CURRENT_USER, 'Environment', 'OVM_INSTALL_PATH', InstallDir);

    RegWriteExpandStringValue(HKEY_CURRENT_USER, 'Environment', 'OVM_OSCRIPTBIN', OscriptBin);

    if RegQueryStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', OldPath) then
    begin
      if not PathContains(OldPath, '%OVM_OSCRIPTBIN%') then
      begin
        if OldPath = '' then
          NewPath := '%OVM_OSCRIPTBIN%'
        else
          NewPath := '%OVM_OSCRIPTBIN%;' + OldPath;
        RegWriteExpandStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', NewPath);
      end;
    end
    else
      RegWriteExpandStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', '%OVM_OSCRIPTBIN%');

    if NeedsAddPath(InstallDir) then
    begin
      if RegQueryStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', OldPath) then
      begin
        if OldPath = '' then
          NewPath := InstallDir
        else
          NewPath := OldPath + ';' + InstallDir;
        RegWriteExpandStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', NewPath);
      end
      else
        RegWriteExpandStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', InstallDir);
    end;
    Exit;
  end;

  PrependUserPath(OscriptBin);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
    ActivateOneScriptLikeOvm(ExpandConstant('{app}'));
end;

function RemoveFromPath(const Path, Entry: String): String;
var
  ResultPath: String;
begin
  ResultPath := Path;
  StringChangeEx(ResultPath, Entry + ';', '', True);
  StringChangeEx(ResultPath, ';' + Entry, '', True);
  if ResultPath = Entry then
    ResultPath := '';
  Result := ResultPath;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  InstallDir, ExistingValue, OldPath, NewPath: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    InstallDir := ExpandConstant('{app}');

    RemoveDirectoryLink(InstallDir + '\current');

    if RegQueryStringValue(HKEY_CURRENT_USER, 'Environment', 'OVM_INSTALL_PATH', ExistingValue) then
      if ExistingValue = InstallDir then
        RegDeleteValue(HKEY_CURRENT_USER, 'Environment', 'OVM_INSTALL_PATH');

    if RegQueryStringValue(HKEY_CURRENT_USER, 'Environment', 'OVM_OSCRIPTBIN', ExistingValue) then
      if ExistingValue = InstallDir + '\current\bin' then
        RegDeleteValue(HKEY_CURRENT_USER, 'Environment', 'OVM_OSCRIPTBIN');

    if RegQueryStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', OldPath) then
    begin
      NewPath := RemoveFromPath(OldPath, '%OVM_OSCRIPTBIN%');
      NewPath := RemoveFromPath(NewPath, InstallDir + '\current\bin');
      NewPath := RemoveFromPath(NewPath, InstallDir);
      if NewPath = '' then
        RegDeleteValue(HKEY_CURRENT_USER, 'Environment', 'Path')
      else
        RegWriteExpandStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', NewPath);
    end;
  end;
end;

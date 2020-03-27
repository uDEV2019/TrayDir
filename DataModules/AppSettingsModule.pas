unit AppSettingsModule;

interface

uses
  System.SysUtils, System.Classes;

type
  TAppSettings = class(TDataModule)
  private
    { Private declarations }
    f:textfile;
  public
    { Public declarations }
    sDirPath: String;
    sConfigPath: String;
    sProgramPath: String;
    bRunShortcutsAsAdmin: boolean;
    bRunFilesAsAdmin: boolean;
    DirList: TStringList;
    procedure Initialize;
    procedure SetDirPath(sPath: String);
    procedure LoadDirList(sPath: String);
    function GetDirList(sPath: String): TStringList;
  end;

var
  AppSettings: TAppSettings;

implementation

{%CLASSGROUP 'Vcl.Controls.TControl'}

{$R *.dfm}

procedure TAppSettings.Initialize;
begin
  sProgramPath := ExtractFilePath(ParamStr(0));
  sConfigPath := sProgramPath+'options.ini';
  DirList:=TStringList.Create;
  bRunShortcutsAsAdmin:=false;
  bRunFilesAsAdmin:=true;
  if FileExists(sConfigPath) then begin
    AssignFile(f,sConfigPath);
    Reset(f);
    ReadLn(f,sDirPath);
    CloseFile(f);
  end else begin
    AssignFile(f,sConfigPath);
    Rewrite(f);
    WriteLn(f,'');
    CloseFile(f);
    sDirPath:='';
  end;
end;

procedure TAppSettings.SetDirPath(sPath: String);
begin
  AssignFile(f,sConfigPath);
  Rewrite(f);
  WriteLn(f,sPath);
  CloseFile(f);
  sDirPath:=sPath;
  LoadDirList(sPath);
end;

procedure TAppSettings.LoadDirList(sPath: String);
begin
  DirList.Clear;
  DirList:=GetDirList(sDirPath);
end;

function TAppSettings.GetDirList(sPath: String): TStringList;
var
  StringList: TStringList;
  SR: TSearchRec;
begin
  StringList := TStringList.Create;
  if (sPath <> '') then begin
    if FindFirst(sPath+'\*', faAnyFile, SR) = 0 then begin
      repeat
        if (SR.Name[1] <> '.') then begin
          StringList.Add(SR.Name);
        end;
      until FindNext(SR) <> 0;
      FindClose(SR);
    end;
  end;
  Result:=StringList;
end;
end.

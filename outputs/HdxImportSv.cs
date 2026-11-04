using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

#if MANUAL_LICENSING && !STORE_MANUAL
#error MANUAL_LICENSING kan bara användas tillsammans med STORE_MANUAL.
#endif

public class Animal {
 public string Cow, Reg, Old, Proposed, Status, Country="752";
 public bool Eligible, InvalidReg;
 public string B="";public bool BKnown;
 public string CurrentTransponder { get { return !Logic.Missing(Old??"") ? Old : !Logic.Missing(B??"") ? B : "-"; } }
}
public static class Logic {
 public static string Hdx(string reg, string country="752") {
  ValidateCountry(country);
  string value=reg.Trim().ToUpperInvariant();
  if(Regex.IsMatch(value,@"^[A-Z]{2}") && !value.StartsWith("SE")) throw new Exception("E001: Utländskt reg-ID stöds inte. Endast SE-ID hanteras.");
  Match m=Regex.Match(value, @"^(?<prefix>SE)[- ]?([0-9]{1,6})[- ]([0-9]{1,4})(?:[- ]([0-9]))?$");
  if(!m.Success) m=Regex.Match(value,@"^(?<prefix>SE)[- ]?([0-9]{6})([0-9]{4})([0-9])?$");
  if(!m.Success) throw new Exception("E002: Ogiltigt reg-ID. Använd SE-plats-individ med valfri kontrollsiffra, till exempel SE-20201-123-5.");
  string digits=m.Groups[1].Value.PadLeft(6,'0')+m.Groups[2].Value.PadLeft(4,'0');
  int sum=5; // Bidraget för SE enligt kontrollsifferformeln.
  for(int i=0;i<digits.Length;i++){int v=(digits[digits.Length-1-i]-'0')*(i%2==0?2:1);sum+=v>9?v-9:v;}
  string check=((10-sum%10)%10).ToString();
  if(m.Groups[3].Success && m.Groups[3].Value!=check) throw new Exception("E003: Fel kontrollsiffra i reg-ID. Förväntad siffra: "+check+".");
  return country+"0"+digits+check;
 }
 public static void ValidateCountry(string country) { if(country!="752") throw new Exception("E007: Endast svensk transponderkod 752 stöds."); }
 public static bool Missing(string s) { return s.Length==0 || Regex.IsMatch(s,@"^\?+$") || Regex.IsMatch(s,@"^0+$") || s=="-"; }
 public static List<Animal> Read(string file, string country="752", bool ignoreExisting=false) {
  ValidateCountry(country);
  var rows=new List<Animal>();int line=0;
  foreach(string raw in File.ReadAllLines(file)) {
   line++; if(string.IsNullOrWhiteSpace(raw)||raw.Trim()=="ZN"||raw.Trim()=="TN") continue;
   string[] c=raw.Split('\t');
   if(c.Length<3 || c.Skip(4).Any(x=>!string.IsNullOrWhiteSpace(x))) throw new Exception("Fel kolumnformat på rad "+line+". Använd svarsfilen från läsbegäran med -X.");
   var a=new Animal { Cow=c[0].Trim(),Reg=c[1].Trim(),Old=c[2].Trim(),Proposed="",Status="",Country=country };
   a.BKnown=c.Length>=4;a.B=a.BKnown?c[3].Trim():"";
   if(a.BKnown&&!Logic.Missing(a.B)&&!Regex.IsMatch(a.B,@"^[0-9]{1,5}$"))throw new Exception("E011: Ogiltigt B-transpondernummer på rad "+line+". Ingen import kan förberedas.");
   if(!Regex.IsMatch(a.Cow,@"^\d{1,6}$") || int.Parse(a.Cow)==0) throw new Exception("Ogiltigt djurnummer på rad "+line);
   a.Cow=int.Parse(a.Cow).ToString("D6");
   if(ignoreExisting && !Missing(a.Old)){a.Status="Befintligt transpondernummer - lämnas orört";rows.Add(a);continue;}
   string calculated="";
   try { calculated=Hdx(a.Reg,country); }
   catch(Exception e) { a.InvalidReg=true;a.Status=Missing(a.Reg)?"E002: Officiellt reg-ID saknas":"Blockerad: "+e.Message; }
   if(!Missing(a.Old) && !Regex.IsMatch(a.Old,@"^752[0-9]{12}$")) {
    if(!a.InvalidReg)a.Status="E006: Befintligt transpondernummer är inte ett svenskt 15-siffrigt 752-nummer. Lämnas orört.";
    a.InvalidReg=true;
   }
   if(!a.InvalidReg) {
    if(!Missing(a.Old)) a.Status="Befintligt nummer - lämnas orört";
    else {a.Proposed=calculated;a.Eligible=true;a.Status="Saknar transpondernummer - klart för återföring";}
   }
   rows.Add(a);
  }
  if(rows.Count==0) throw new Exception("Filen innehåller inga djur.");
  var counts=rows.GroupBy(a=>a.Cow).ToDictionary(g=>g.Key,g=>g.Count());
  var existing=new HashSet<string>(rows.Where(a=>!Missing(a.Old)).Select(a=>a.Old.TrimStart('0')));
  var proposed=rows.Where(a=>a.Eligible).GroupBy(a=>a.Proposed).ToDictionary(g=>g.Key,g=>g.Count());
  foreach(var a in rows) if(a.Eligible) {
   if(counts[a.Cow]>1){a.Eligible=false;a.InvalidReg=true;a.Status="E004: Dubbelt djurnummer";}
   else if(existing.Contains(a.Proposed.TrimStart('0'))||proposed[a.Proposed]>1){a.Eligible=false;a.InvalidReg=true;a.Status="E005: Dubbelt transpondernummer";}
  }
  return rows;
 }
 public static string Header() {
  return "DH00000000000000080009000062400090000808000900003080\r\n"+
   "VH000000"+"DD".PadRight(8)+"HDX import".PadRight(24)+"3.10-SE".PadRight(8)+DateTime.Now.ToString("yyyyMMdd")+"\r\n";
 }
 public static string Request(){return Header()+"RN39000200300001060003003481500030393015000300002050\r\nTN\r\nZN\r\n";}
 public static string Import(IEnumerable<Animal> rows,bool overwriteB=false){
  var b=new StringBuilder(Header());b.Append("DN3900020030000106000303930150\r\n");
  int count=0;foreach(var a in rows.Where(x=>x.Eligible)) {
   BTransponders.RequireKnown(new[]{a});if(BTransponders.HasNumber(a.B)&&!overwriteB)throw new Exception("B-transpondern för djur "+a.Cow+" ska behållas. Djuret får inte ingå i importfilen.");
   if(a.InvalidReg || !Missing(a.Old) || Hdx(a.Reg,a.Country)!=a.Proposed) throw new Exception("Ogiltigt importförslag för djur "+a.Cow+". Ingen importfil skapad.");
   b.Append("VN390002").Append(a.Cow).Append(a.Proposed).Append("\r\n");count++;
  }
  if(count==0) throw new Exception("Det finns inga godkända ändringar att skriva.");
  return b.Append("TN\r\nZN\r\n").ToString();
 }
 public static void Write(string path,string s){File.WriteAllText(path,s,Encoding.ASCII);}
 public static void Test(){
  if(Hdx("SE-020201-1234")!="752002020112349"||Hdx("SE-099999-1234")!="752009999912344")throw new Exception("Kontrollsiffretest misslyckades");
  bool rejected=false;try{Hdx("SE-1234567-4");}catch{rejected=true;}if(!rejected)throw new Exception("Formatkontroll misslyckades");
  if(!Missing("???????????????")||Missing("752002020112349"))throw new Exception("Tomvärdestest misslyckades");
 }
}
public class MainForm:Form {
#if MANUAL_LICENSING
 ManualLicensing licensing=ManualLicensing.Create();Label licenseStatus=new Label{Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,AutoEllipsis=true};
 void UpdateLicenseStatus(){licenseStatus.Text=licensing.Status;}
#endif
 ComboBox bChoice=new ComboBox();
 DataGridView grid=new DataGridView(); Label summary=new Label();TextBox iso=new TextBox();
 const string Country="752";
 List<Animal> rows=new List<Animal>();string source=""; Button save=new Button(),send=new Button();bool busy=false;
 public MainForm(){Branding.Apply(this);
#if STORE_PACKAGE
  Text=ManualEdition.Name+" "+ManualEdition.Version+" - Svenska djur-ID";
#elif STORE_MANUAL
  Text=ManualEdition.Name+" "+ManualEdition.Version+" – testversion";
#else
  Text="HDX import 3.10 - Svenska djur-ID";
#endif
 Width=1180;Height=900;MinimumSize=new Size(980,760);BackColor=Color.FromArgb(242,245,248);StartPosition=FormStartPosition.CenterScreen;Font=new Font("Segoe UI",10);
#if STORE_MANUAL
  string config=ManualEdition.ConfigPath;
#else
  string config=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"HDXimport.ini");
#endif
  Shown+=delegate{try{LogRetention.Clean(ManualLogRoot(),DateTime.UtcNow);}catch(Exception e){summary.Text="Loggrensning: "+e.Message;}};
  var root=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,Padding=new Padding(20)};
  root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));root.RowStyles.Add(new RowStyle(SizeType.Absolute,48));
#if MANUAL_LICENSING
  root.RowCount=5;root.RowStyles.Add(new RowStyle(SizeType.Absolute,52));
#endif
  root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,60));Controls.Add(root);root.Controls.Add(Branding.Header());
  var tabs=new TabControl{Dock=DockStyle.Fill,Padding=new Point(18,8)};
  var manualPage=new TabPage("Manuell körning"){BackColor=Color.FromArgb(242,245,248),Padding=new Padding(12)};
  tabs.TabPages.Add(manualPage);tabs.SelectedIndex=0;
#if !STORE_MANUAL
  var autoPage=new TabPage("Automatik"){BackColor=Color.FromArgb(242,245,248),Padding=new Padding(16)};tabs.TabPages.Add(autoPage);tabs.SelectedIndex=0;
  var autoLayout=new TableLayoutPanel{Dock=DockStyle.Top,ColumnCount=1,RowCount=3,Height=420};autoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,64));autoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,230));autoLayout.RowStyles.Add(new RowStyle(SizeType.Percent,100));autoPage.Controls.Add(autoLayout);
  autoLayout.Controls.Add(new Label{Text="Automatisk import till DelPro\r\nVälj daglig körtid eller starta en enstaka körning i bakgrunden.",Dock=DockStyle.Fill,Padding=new Padding(0,8,0,0)});
  autoLayout.Controls.Add(new SchedulePanel(()=>iso.Text));autoLayout.Controls.Add(new Label{Text="Bakgrundskörningen läser, fyller i godkända saknade transpondernummer och verifierar automatiskt.\r\nBefintliga transpondernummer lämnas orörda. Problem visas i en felruta.\r\n\r\nPå / spara inställningar sparar även ISO-sökvägen ovan. Av stoppar kommande schemakörningar.",Dock=DockStyle.Fill,Padding=new Padding(0,16,0,0),ForeColor=Color.DimGray});
#endif
  var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=5};manualPage.Controls.Add(layout);
  layout.RowStyles.Add(new RowStyle(SizeType.Absolute,38));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,48));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,50));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,58));
  var primary=new FlowLayoutPanel{Dock=DockStyle.Fill};
  primary.Controls.Add(Button("Läs från DelPro",delegate{ReadLive();}));
  send=Button("Skriv till DelPro",delegate{SendLive();});send.Enabled=false;primary.Controls.Add(send);
  bChoice.DropDownStyle=ComboBoxStyle.DropDownList;bChoice.Width=255;bChoice.Margin=new Padding(18,7,3,3);bChoice.Items.AddRange(new object[]{"Behåll B-transpondrar","Skriv över B-transpondrar med ISO"});bChoice.SelectedIndex=0;bChoice.AccessibleName="Hantering av B-transpondrar";primary.Controls.Add(bChoice);bChoice.SelectedIndexChanged+=delegate{if(rows.Count>0)ShowRows();};
  var paths=new FlowLayoutPanel{Dock=DockStyle.Fill};paths.Controls.Add(new Label{Text="ISO-processor:",AutoSize=true,Margin=new Padding(0,7,8,0)});
  iso.Text=@"C:\DelPro\ISO\iso32.exe";if(File.Exists(config))foreach(string line in File.ReadAllLines(config)){if(line.StartsWith("IsoProcessor="))iso.Text=line.Substring(13).Trim();}iso.Width=620;paths.Controls.Add(iso);var browse=Button("Bläddra",delegate{using(var d=new OpenFileDialog{Filter="ISO Processor|iso32.exe|Program|*.exe"})if(d.ShowDialog()==DialogResult.OK){iso.Text=d.FileName;
#if STORE_MANUAL
  ManualEdition.SaveIso(iso.Text);
#endif
  }});paths.Controls.Add(browse);root.Controls.Add(paths);
#if MANUAL_LICENSING
  var licensePanel=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Padding=new Padding(0,3,0,3)};licensePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));licensePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,160));licensePanel.Controls.Add(licenseStatus);licensePanel.Controls.Add(Button("Licens / köp",delegate{using(var window=new ManualLicenseWindow(licensing))window.ShowDialog(this);UpdateLicenseStatus();}));root.Controls.Add(licensePanel);UpdateLicenseStatus();
  Shown+=async delegate{try{await licensing.RefreshForImport(Handle);}catch(Exception e){licensing.StoreDetail=e.Message;}if(!IsDisposed)UpdateLicenseStatus();};
#endif
  root.Controls.Add(tabs);
  layout.Controls.Add(new Label{Text="Sverige: SE-ID och ISO-transponderkod 752",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft});layout.Controls.Add(primary);
  summary.Text="Läs från DelPro för att granska förslag. Befintliga transpondernummer lämnas orörda.";summary.Dock=DockStyle.Fill;layout.Controls.Add(summary);
  grid.BackgroundColor=Color.White;grid.BorderStyle=BorderStyle.None;grid.AlternatingRowsDefaultCellStyle.BackColor=Color.FromArgb(248,250,252);grid.ColumnHeadersDefaultCellStyle.BackColor=Color.FromArgb(226,233,241);grid.EnableHeadersVisualStyles=false;grid.Dock=DockStyle.Fill;grid.ReadOnly=true;grid.AllowUserToAddRows=false;grid.AllowUserToDeleteRows=false;grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;grid.RowHeadersVisible=false;grid.ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.AutoSize;
  foreach(string name in new[]{"Djurnummer","Officiellt reg-ID","Nuvarande transpondernummer","Föreslaget transpondernummer","Status"})grid.Columns.Add(name,name);
  grid.Columns[4].FillWeight=170;layout.Controls.Add(grid);
  var actions=new FlowLayoutPanel{Dock=DockStyle.Fill};
  var footer=new Panel{Dock=DockStyle.Fill,Padding=new Padding(0,8,0,0)};var management=new FlowLayoutPanel{Dock=DockStyle.Left,Width=610};footer.Controls.Add(management);
#if STORE_MANUAL
  management.Controls.Add(Button("Om programmet",delegate{BrandMessage.Show(ManualEdition.About,ManualEdition.Name);}));
#else
  management.Controls.Add(Button("Installation / avinstallation",delegate{Process.Start(new ProcessStartInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"HDXimport-Installation.exe")){UseShellExecute=true,Verb="runas"});}));management.Controls.Add(Button("Sök efter uppdateringar",delegate{OpenUpdater();}));
#endif
  var close=new Button{Text="Stäng",Dock=DockStyle.Right,Width=110};close.Click+=delegate{Close();};footer.Controls.Add(close);root.Controls.Add(footer);FormClosing+=delegate(object sender,FormClosingEventArgs e){if(busy){e.Cancel=true;ShowError("En manuell läsning eller import pågår. Vänta tills den är klar innan du stänger programmet.","Körning pågår");}};
  actions.Controls.Add(Button("Öppna svarsfil",delegate{using(var d=new OpenFileDialog{Filter="Textfiler|*.txt|Alla filer|*.*"})if(d.ShowDialog()==DialogResult.OK)LoadFile(d.FileName);}));
  actions.Controls.Add(Button("Spara läsbegäran",delegate{SaveRequest();}));
  save=Button("Spara importfil",delegate{SaveImport();});save.Enabled=false;actions.Controls.Add(save);layout.Controls.Add(actions);
 }
 #if !STORE_MANUAL
 void OpenUpdater(){
  if(busy)return;
  using(var window=new UpdateWindow())if(window.ShowDialog(this)==DialogResult.OK){
   try{Process.Start(new ProcessStartInfo(window.InstallerPath,"--update-from "+Process.GetCurrentProcess().Id){UseShellExecute=true,Verb="runas"});Close();}
   catch(System.ComponentModel.Win32Exception e){if(e.NativeErrorCode==1223)summary.Text="Uppdateringen avbröts. Programmet och schemat är oförändrade.";else ShowError(e.Message,"Uppdateringen kunde inte starta");}
   catch(Exception e){ShowError(e.Message,"Uppdateringen kunde inte starta");}
  }
 }
 #endif
 Button Button(string text,Action fn){var b=new Button{Text=text,AutoSize=true,Height=35,Padding=new Padding(6)};b.Click+=delegate{if(busy)return;try{fn();}catch(Exception e){BrandMessage.Show(e.Message,"HDX import",MessageBoxButtons.OK,MessageBoxIcon.Error);}};return b;}
 protected virtual void ShowError(string message,string title){if(message.Contains("E009:")){using(var popup=new ErrorWindow(message))popup.ShowDialog(this);}else BrandMessage.Show(message,title);}
 static string ManualLogRoot(){
#if STORE_MANUAL
  return ManualEdition.RunsRoot;
#else
  return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DelProHdx","Runs");
#endif
 }
 protected virtual string Folder(){
#if STORE_MANUAL
 return ManualIsoFiles.CreateRun();
#else
 string p=Path.Combine(ManualLogRoot(),DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));Directory.CreateDirectory(p);return p;
#endif
 }
 void LoadFile(string path){rows=Logic.Read(path,Country,true);source=path;ShowRows();}
 void ShowRows(){grid.Rows.Clear();foreach(var a in rows){int i=grid.Rows.Add(a.Cow,a.Reg,a.CurrentTransponder,a.Proposed,a.Eligible&&BTransponders.HasNumber(a.B)&&bChoice.SelectedIndex==0?"B-transponder behålls – ingen import":a.Status);if(a.InvalidReg){grid.Rows[i].DefaultCellStyle.BackColor=Color.MistyRose;grid.Rows[i].DefaultCellStyle.ForeColor=Color.DarkRed;grid.Rows[i].ErrorText=a.Status;}else if(a.Eligible&&(!BTransponders.HasNumber(a.B)||bChoice.SelectedIndex==1))grid.Rows[i].DefaultCellStyle.BackColor=Color.FromArgb(230,245,230);}
  int n=rows.Count(a=>a.Eligible&&(!BTransponders.HasNumber(a.B)||bChoice.SelectedIndex==1));summary.Text=rows.Count+" djur. "+rows.Count(a=>Logic.Missing(a.Old)&&Logic.Missing(a.B))+" saknar transpondernummer. "+n+" godkända ändringar. "+rows.Count(a=>a.InvalidReg)+" felmarkerade djur (röda rader). Källa: "+source;save.Enabled=send.Enabled=n>0;}
 void SaveRequest(){using(var d=new SaveFileDialog{FileName="request_djur.txt",Filter="Textfil|*.txt"})if(d.ShowDialog()==DialogResult.OK)Logic.Write(d.FileName,Logic.Request());}
#if MANUAL_LICENSING
 async void SaveImport(){if(busy)return;busy=true;try{await licensing.RefreshForImport(Handle);licensing.EnsureExport();SaveImportFile();}catch(Exception e){ShowError(e.Message,"Importfilen kunde inte sparas");}finally{busy=false;UpdateLicenseStatus();}}
#else
 void SaveImport(){SaveImportFile();}
#endif
 void SaveImportFile(){bool overwrite;var eligible=ChooseBImport(rows,out overwrite);if(eligible==null||eligible.Count==0)return;using(var d=new SaveFileDialog{FileName="import_hdx.txt",Filter="Textfil|*.txt"})if(d.ShowDialog()==DialogResult.OK){Logic.Write(d.FileName,Logic.Import(eligible,overwrite));File.Copy(source,d.FileName+".source.txt",true);summary.Text="Importfil sparad. Den har inte skickats till DelPro.";}}

 async System.Threading.Tasks.Task Run(string input,string output,string log,bool columns){
  if(!File.Exists(iso.Text))throw new Exception("Hittar inte iso32.exe. Välj rätt sökväg.");
#if STORE_MANUAL
  ManualIsoFiles.Check(input,output,log);
#endif
  var start=new ProcessStartInfo(iso.Text,"-f \""+input+"\" -o \""+output+"\" -l \""+log+"\""+(columns?" -X":"")){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=Path.GetDirectoryName(iso.Text)};
  using(var p=Process.Start(start)){await System.Threading.Tasks.Task.Run(()=>p.WaitForExit());if(columns){if(p.ExitCode==0){summary.Text="Väntar på att DelPro skriver klart svarsfilen (högst 30 sekunder)...";if(!await ResponseWait.Ready(output))throw new Exception("Väntade i 30 sekunder efter att ISO-processorn avslutades.\r\n"+SharedHelp.MissingResponseHelp(output,log,p.ExitCode));}else if(!File.Exists(output))throw new Exception(SharedHelp.MissingResponseHelp(output,log,p.ExitCode));}if(p.ExitCode!=0)throw new Exception("ISO-processorn avslutades med kod "+p.ExitCode+". Kontrollera loggen: "+log);}
 }
 async void ReadLive(){busy=true;System.Threading.Mutex gate=null;bool locked=false;try{gate=RunGate.Open();locked=RunGate.TryEnter(gate);if(!locked)throw new Exception("En annan körning pågår. Vänta tills den är klar.");string folder=Folder(),req=Path.Combine(folder,"request.txt"),output=Path.Combine(folder,"output.txt"),log=Path.Combine(folder,"log.txt");Logic.Write(req,Logic.Request());summary.Text="Läser från DelPro...";await Run(req,output,log,true);LoadFile(output);}catch(Exception e){ShowError(e.Message,"Läsning misslyckades");}finally{if(locked)gate.ReleaseMutex();if(gate!=null)gate.Dispose();busy=false;}}
 protected virtual bool? ChooseBTransponders(int total,int affected){return bChoice.SelectedIndex==1;}
 List<Animal> ChooseBImport(List<Animal> current,out bool overwrite){overwrite=false;BTransponders.RequireKnown(current);int total=current.Count(x=>BTransponders.HasNumber(x.B));if(total>0){var answer=ChooseBTransponders(total,current.Count(x=>x.Eligible&&BTransponders.HasNumber(x.B)));if(!answer.HasValue)return null;overwrite=answer.Value;}var chosen=BTransponders.Select(current,overwrite);if(chosen.Count==0)summary.Text="B-transpondrarna behålls. Det finns inga andra godkända ändringar att skriva.";return chosen;}
 protected virtual bool ConfirmImport(int count){return BrandMessage.Show("Skriv transpondernummer med landskod "+Country+" för "+count+" djur till DelPro?\nProgrammet läser först om uppgifterna och avbryter om något ändrats.\nEndast djur som omfattas av ditt val skrivs.","Bekräfta återföring till DelPro",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes;}
 async void SendLive(){await SendLiveCore();}
 async System.Threading.Tasks.Task SendLiveCore(){
  {
   var selected=rows.Where(a=>a.Eligible).ToList();
   if(selected.Count==0)return;
   bool overwriteB=false;
   busy=true;bChoice.Enabled=false;string folder="";System.Threading.Mutex gate=null;bool locked=false,importStarted=false;
#if MANUAL_LICENSING
   ImportPermit permit=null;
#endif
   try{
    gate=RunGate.Open();locked=RunGate.TryEnter(gate);if(!locked)throw new Exception("En annan körning pågår. Vänta tills den är klar.");
#if MANUAL_LICENSING
    summary.Text="Kontrollerar licens...";await licensing.RefreshForImport(Handle);permit=licensing.BeginImport();UpdateLicenseStatus();
#endif
    folder=Folder();File.Copy(source,Path.Combine(folder,"preview_source.txt"));
    string req=Path.Combine(folder,"request.txt"),fresh=Path.Combine(folder,"before.txt");Logic.Write(req,Logic.Request());summary.Text="Kontrollerar aktuella uppgifter...";await Run(req,fresh,Path.Combine(folder,"before_log.txt"),true);
    var current=Logic.Read(fresh,Country);
    foreach(var a in selected){var match=current.Where(x=>x.Cow==a.Cow).ToList();if(match.Count!=1||!match[0].Eligible||match[0].Reg!=a.Reg||match[0].Proposed!=a.Proposed)throw new Exception("Djur "+a.Cow+" har ändrats eller är blockerat. Läs om uppgifterna och granska igen. Ingen import skickad.");}
    var requested=new HashSet<string>(selected.Select(x=>x.Cow));var candidates=current.Where(x=>requested.Contains(x.Cow)).ToList();
    // Decisions use the fresh B values, including a B number added since the preview.
    BTransponders.RequireKnown(current);int totalB=current.Count(x=>BTransponders.HasNumber(x.B));
    if(totalB>0){var answer=ChooseBTransponders(totalB,candidates.Count(x=>BTransponders.HasNumber(x.B)));if(!answer.HasValue)return;overwriteB=answer.Value;}
    selected=BTransponders.Select(candidates,overwriteB);if(selected.Count==0){summary.Text="B-transpondrarna behålls. Inga ändringar skickades till DelPro.";return;}
    if(!ConfirmImport(selected.Count))return;
    string import=Path.Combine(folder,"import.txt"),log=Path.Combine(folder,"import_log.txt");Logic.Write(import,Logic.Import(selected,overwriteB));summary.Text="Skickar import...";importStarted=true;await Run(import,Path.Combine(folder,"import_output.txt"),log,false);
    string after=Path.Combine(folder,"after.txt");summary.Text="Läser tillbaka och verifierar...";await Run(req,after,Path.Combine(folder,"after_log.txt"),true);var result=Logic.Read(after,Country);
    var failed=selected.Where(a=>result.Count(x=>x.Cow==a.Cow&&x.Old==a.Proposed)!=1).ToList();LoadFile(after);
    File.WriteAllText(Path.Combine(folder,"verification.txt"),"Begärda: "+selected.Count+"\r\nVerifierade: "+(selected.Count-failed.Count)+"\r\nEj verifierade: "+string.Join(",",failed.Select(x=>x.Cow)),Encoding.UTF8);
    summary.Text="Återläsning verifierade "+(selected.Count-failed.Count)+" av "+selected.Count+" ändringar. Importlogg: "+log;
#if MANUAL_LICENSING
    licensing.Verified(permit,failed.Count==0);UpdateLicenseStatus();
#endif
    if(failed.Count>0)ShowError("E008: Återläsning kunde inte verifiera "+failed.Count+" ändringar.\nDjur: "+string.Join(", ",failed.Select(a=>a.Cow))+"\nKontrollera importloggen: "+log+"\nGranska loggen innan du skickar importen igen.","Verifiering misslyckades");
   }catch(Exception e){ShowError(e.Message+"\nKörningsmapp: "+folder+(importStarted?"\nEn import kan ha behandlats delvis. Läs om uppgifterna före ett nytt försök.":"\nIngen import har skickats till DelPro."),"Återföring avbruten");}finally{if(locked)gate.ReleaseMutex();if(gate!=null)gate.Dispose();busy=false;bChoice.Enabled=true;
#if MANUAL_LICENSING
    UpdateLicenseStatus();
#endif
   }
  }
 }
 [STAThread] public static int Main(string[] args){try{Logic.Test();if(args.Length>0&&args[0]=="--check"){var r=Logic.Read(args[1]);Console.WriteLine("Djur="+r.Count+" Saknar="+r.Count(a=>Logic.Missing(a.Old))+" Godkända="+r.Count(a=>a.Eligible));var s=Logic.Import(r);File.WriteAllText(args[2],s,Encoding.ASCII);return 0;}Application.EnableVisualStyles();Application.Run(new MainForm());return 0;}catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}

public static class CombinedEntry {
 [STAThread]public static int Main(string[] args){
#if !STORE_MANUAL
  if(args.Length==1 && (args[0]=="--scheduled" || args[0]=="--monitor"))return AutoForm.Main(args);
#endif
  if(args.Length!=0){BrandMessage.Show("Okänt startargument.","HDX import");return 1;}
  return MainForm.Main(args);
 }
}


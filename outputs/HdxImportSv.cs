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
 sealed class BSelection {public int SelectedIndex;public bool Enabled{get;set;}}
 BSelection bChoice=new BSelection();RadioButton keepB=new RadioButton(),replaceB=new RadioButton();Panel bPanel=new TableLayoutPanel();Label bQuestion=new Label();
 Button read=new Button(),cancelRead=new Button(),testConnection=new Button(),settingsToggle=new Button();Panel settingsBody;Label pathSummary=new Label(),pathWarning=new Label();Label sendHint=new Label();FlowLayoutPanel steps=new FlowLayoutPanel();ToolTip hints=new ToolTip();
 System.Threading.CancellationTokenSource readCancel;System.Windows.Forms.Timer readTimer;Stopwatch readClock;Action readRetry;
 Panel workPanel=new FlowLayoutPanel();ProgressBar workProgress=new ProgressBar();Label workText=new Label(),workWarning=new Label();System.Windows.Forms.Timer workTimer;Stopwatch workClock;string workPhase="",workDone=null;System.Collections.Generic.Dictionary<Control,bool> workEnabled=new System.Collections.Generic.Dictionary<Control,bool>();
 int confirmExisting,confirmKeptB;int NextStep=1;
 DataGridView grid=new DataGridView(); Label summary=new Label();TextBox iso=new TextBox();
 const string Country="752";
 List<Animal> rows=new List<Animal>();string source=""; Button save=new Button(),send=new Button();bool busy=false;
#if !STORE_MANUAL
 ModeChoiceButton manualMode,automaticMode;TabControl modeTabs;SchedulePanel schedulePanel;
 Panel firstStartPanel;protected virtual string FirstStartStatePath{get{return FirstStartGuide.StatePath;}}
 protected void ShowFirstStartInfo(){firstStartPanel.Visible=FirstStartGuide.TakePending(FirstStartStatePath);}
 void DismissFirstStartInfo(){if(firstStartPanel!=null)firstStartPanel.Visible=false;}
#endif
 public MainForm(){Branding.Apply(this);
#if STORE_PACKAGE
  Text=ManualEdition.Name+" "+ManualEdition.Version+" - Svenska djur-ID";
#elif STORE_MANUAL
  Text=ManualEdition.Name+" "+ManualEdition.Version+" – testversion";
#else
  Text="HDX import "+SharedHelp.Version+" - Svenska djur-ID";
#endif
 Width=1180;Height=900;MinimumSize=new Size(980,760);BackColor=Color.FromArgb(242,245,248);StartPosition=FormStartPosition.CenterScreen;Font=new Font("Segoe UI",10);
#if STORE_MANUAL
  string config=ManualEdition.ConfigPath;
#else
  string config=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"HDXimport.ini");
#endif
  Shown+=delegate{try{LogRetention.Clean(ManualLogRoot(),DateTime.UtcNow);}catch(Exception e){summary.Text="Loggrensning: "+e.Message;}};
  int contentRow=3,settingsRow=4,footerRow=5;var root=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=6,Padding=new Padding(12)};
  root.RowStyles.Add(new RowStyle(SizeType.Absolute,62));root.RowStyles.Add(new RowStyle(SizeType.AutoSize));root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
#if MANUAL_LICENSING
  root.RowCount++;contentRow++;settingsRow++;footerRow++;root.RowStyles.Add(new RowStyle(SizeType.Absolute,52));
#endif
  root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.AutoSize));root.RowStyles.Add(new RowStyle(SizeType.Absolute,62));Controls.Add(root);root.Controls.Add(Branding.Header(),0,0);
  pathWarning.Text="Hittar inte DelPros ISO-program. Välj sökväg under Inställningar längst ner.";pathWarning.ForeColor=Color.DarkRed;pathWarning.AutoSize=true;pathWarning.Dock=DockStyle.Top;pathWarning.Visible=false;root.Controls.Add(pathWarning,0,1);
#if STORE_MANUAL
  var tabs=new TabControl{Dock=DockStyle.Fill,Padding=new Point(18,8)};
#else
  var tabs=new HiddenTabControl{Dock=DockStyle.Fill};modeTabs=tabs;
  var modeChoices=new FlowLayoutPanel{Dock=DockStyle.Top,AutoSize=true,WrapContents=false,Margin=new Padding(0)};manualMode=new ModeChoiceButton{Title="✋ Manuellt",Detail="Du läser, granskar och skriver själv"};automaticMode=new ModeChoiceButton{Title="⏰ Automatiskt",Detail="Programmet gör det varje dag\r\npå vald tid · Okänt"};modeChoices.Controls.Add(manualMode);modeChoices.Controls.Add(automaticMode);root.Controls.Add(modeChoices,0,2);manualMode.Click+=delegate{SelectMode(0);};automaticMode.Click+=delegate{SelectMode(1);};tabs.Selecting+=delegate(object sender,TabControlCancelEventArgs e){if(busy)e.Cancel=true;};tabs.SelectedIndexChanged+=delegate{UpdateModeChoices();};
#endif
  var manualPage=new TabPage("Manuell körning"){BackColor=Color.FromArgb(242,245,248),Padding=new Padding(6)};
  tabs.TabPages.Add(manualPage);tabs.SelectedIndex=0;
#if !STORE_MANUAL
  var autoPage=new TabPage("Automatik"){BackColor=Color.FromArgb(242,245,248),Padding=new Padding(16),AutoScroll=true};tabs.TabPages.Add(autoPage);tabs.SelectedIndex=0;
  var autoLayout=new TableLayoutPanel{Dock=DockStyle.Top,ColumnCount=1,RowCount=3,Height=600};autoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,64));autoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,390));autoLayout.RowStyles.Add(new RowStyle(SizeType.Percent,100));autoPage.Controls.Add(autoLayout);
  autoLayout.Controls.Add(new Label{Text="Programmet läser från DelPro varje dag på vald tid, lägger till ISO-nummer på godkända djur och kontrollerar resultatet. Det fungerar även när programmet är stängt.",Dock=DockStyle.Fill,Padding=new Padding(0,8,0,0)});
  schedulePanel=new SchedulePanel(()=>iso.Text){ObserveWhenHidden=true};schedulePanel.DisplayStatusChanged+=delegate{UpdateModeChoices();};autoLayout.Controls.Add(schedulePanel);var scheduleHandle=schedulePanel.Handle;autoLayout.Controls.Add(new Label{Text="Bakgrundskörningen läser, fyller i godkända saknade transpondernummer och verifierar automatiskt.\r\nBefintliga transpondernummer lämnas orörda. Problem visas i en felruta.\r\n\r\nPå / spara inställningar sparar även ISO-sökvägen ovan. Av stoppar kommande schemakörningar.",Dock=DockStyle.Fill,Padding=new Padding(0,16,0,0),ForeColor=Color.DimGray});
#endif
  var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=10,GrowStyle=TableLayoutPanelGrowStyle.FixedSize};layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));manualPage.Controls.Add(layout);
  foreach(int height in new[]{26,52,20,26})layout.RowStyles.Add(new RowStyle(SizeType.Absolute,height));layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,38));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,24));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,52));
  layout.RowStyles.Insert(2,new RowStyle(SizeType.AutoSize));
#if !STORE_MANUAL
  manualPage.Controls.Remove(layout);var manualWrap=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2};manualWrap.RowStyles.Add(new RowStyle(SizeType.AutoSize));manualWrap.RowStyles.Add(new RowStyle(SizeType.Percent,100));manualPage.Controls.Add(manualWrap);
  firstStartPanel=new FlowLayoutPanel{Dock=DockStyle.Top,AutoSize=true,Visible=false,BackColor=Color.FromArgb(220,236,252),Padding=new Padding(6),Margin=new Padding(0)};firstStartPanel.Controls.Add(new Label{Text="Innan du börjar: kontrollera att Use Command Line ISO är ibockad i DelPro.",AutoSize=true,MaximumSize=new Size(650,0),Margin=new Padding(0,8,6,0)});firstStartPanel.Controls.Add(Button("Så ställer du in DelPro",delegate{SharedHelp.ShowGuide(this);}));var dismiss=Button("×",DismissFirstStartInfo);dismiss.AutoSize=false;dismiss.Size=new Size(28,28);dismiss.Padding=new Padding(0);dismiss.Margin=new Padding(3,5,3,0);firstStartPanel.Controls.Add(dismiss);manualWrap.Controls.Add(firstStartPanel,0,0);manualWrap.Controls.Add(layout,0,1);Shown+=delegate{ShowFirstStartInfo();};root.RowStyles[root.RowStyles.Count-1].Height=92;
#endif
  steps.Dock=DockStyle.Fill;steps.WrapContents=false;
  var primary=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=false};read=Button("Läs från DelPro",delegate{ReadLive();});primary.Controls.Add(read);
  send=Button("Skriv till DelPro",delegate{SendLive();});send.Enabled=false;primary.Controls.Add(send);
  cancelRead=new Button{Text="Avbryt",AutoSize=true,Visible=false,Padding=new Padding(6)};cancelRead.Click+=delegate{if(readCancel!=null)readCancel.Cancel();};primary.Controls.Add(cancelRead);
  workPanel.AutoSize=true;workPanel.Dock=DockStyle.Top;workPanel.Visible=false;workPanel.Margin=new Padding(0);workProgress.Style=ProgressBarStyle.Marquee;workProgress.Size=new Size(300,20);workText.AutoSize=true;workText.Font=new Font(Font,FontStyle.Bold);workWarning.AutoSize=true;workWarning.ForeColor=Color.DarkRed;workWarning.Text="Avbryt inte och stäng inte programmet.";workPanel.Controls.Add(workProgress);workPanel.Controls.Add(workText);workPanel.Controls.Add(workWarning);layout.Controls.Add(workPanel,0,2);
  cancelRead.Cursor=Cursors.Default;
  sendHint.Dock=DockStyle.Fill;sendHint.ForeColor=Color.DimGray;sendHint.Text="Läs från DelPro först";
  bPanel.Dock=DockStyle.Top;bPanel.AutoSize=true;bPanel.AutoSizeMode=AutoSizeMode.GrowAndShrink;bPanel.Margin=new Padding(0);bPanel.Visible=false;bPanel.BackColor=Color.FromArgb(233,240,249);bQuestion.Dock=DockStyle.Top;bQuestion.Height=24;
  keepB.Text="Låt dem vara – de får inget ISO-nummer nu";replaceB.Text="Ge dem ISO-nummer – ISO-numret ersätter B-transpondern i DelPro";
  keepB.Dock=replaceB.Dock=DockStyle.Top;keepB.Height=replaceB.Height=26;keepB.Checked=true;bChoice.SelectedIndex=0;
  var bLayout=(TableLayoutPanel)bPanel;bLayout.ColumnCount=1;bLayout.RowCount=3;bLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));foreach(int height in new[]{24,26,26})bLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,height));bQuestion.Dock=keepB.Dock=replaceB.Dock=DockStyle.Fill;bQuestion.Margin=keepB.Margin=replaceB.Margin=new Padding(0);bLayout.Controls.Add(bQuestion,0,0);bLayout.Controls.Add(keepB,0,1);bLayout.Controls.Add(replaceB,0,2);replaceB.CheckedChanged+=delegate{if(!busy)ClearCompletion();bChoice.SelectedIndex=replaceB.Checked?1:0;if(rows.Count>0)ShowRows();};
  hints.SetToolTip(read,"Hämtar djurnummer, reg-ID och transpondernummer från DelPro. Ingenting ändras i DelPro.");hints.SetToolTip(send,"Skriver nya ISO-transpondernummer på de gröna raderna. Programmet läser om uppgifterna först och kontrollerar efteråt att allt blev rätt.");
  var paths=new FlowLayoutPanel{Dock=DockStyle.Fill};paths.Controls.Add(new Label{Text="Sökväg till DelPros ISO-program (iso32.exe):",AutoSize=true,Margin=new Padding(0,7,8,0)});
  iso.Text=@"C:\DelPro\ISO\iso32.exe";if(File.Exists(config))foreach(string line in File.ReadAllLines(config)){if(line.StartsWith("IsoProcessor="))iso.Text=line.Substring(13).Trim();}iso.Width=380;paths.Controls.Add(iso);var browse=Button("Bläddra…",delegate{using(var d=new OpenFileDialog{Filter="DelPros ISO-program|iso32.exe|Program|*.exe"})if(d.ShowDialog()==DialogResult.OK){iso.Text=d.FileName;
#if STORE_MANUAL
  ManualEdition.SaveIso(iso.Text);
#endif
  }});paths.Controls.Add(browse);paths.AutoSize=true;paths.Dock=DockStyle.Top;
  var settings=new TableLayoutPanel{Dock=DockStyle.Top,AutoSize=true,ColumnCount=1,RowCount=2,Margin=new Padding(0)};settings.RowStyles.Add(new RowStyle(SizeType.AutoSize));settings.RowStyles.Add(new RowStyle(SizeType.AutoSize));var settingsHeader=new FlowLayoutPanel{Dock=DockStyle.Top,AutoSize=true,WrapContents=false,Margin=new Padding(0)};settingsToggle.Text="Inställningar ▸";settingsToggle.AutoSize=true;Small(settingsToggle);settingsToggle.Cursor=Cursors.Hand;settingsToggle.Click+=delegate{if(busy)return;ClearCompletion();SetSettingsExpanded(!settingsBody.Visible);};settingsHeader.Controls.Add(settingsToggle);pathSummary.AutoSize=true;pathSummary.ForeColor=Color.DimGray;pathSummary.Margin=new Padding(8,7,0,0);settingsHeader.Controls.Add(pathSummary);settings.Controls.Add(settingsHeader,0,0);
  settingsBody=new TableLayoutPanel{Dock=DockStyle.Top,AutoSize=true,Visible=false,ColumnCount=1,RowCount=2,Margin=new Padding(0)};((TableLayoutPanel)settingsBody).RowStyles.Add(new RowStyle(SizeType.AutoSize));((TableLayoutPanel)settingsBody).RowStyles.Add(new RowStyle(SizeType.AutoSize));settingsBody.Controls.Add(paths);settingsBody.Controls.Add(new Label{Text="Normalt behöver du inte ändra detta. Standard är C:\\DelPro\\ISO\\iso32.exe.",AutoSize=true,ForeColor=Color.DimGray,Margin=new Padding(3,2,3,6)});settings.Controls.Add(settingsBody,0,1);root.Controls.Add(settings,0,settingsRow);iso.TextChanged+=delegate{RefreshPathState();};
#if MANUAL_LICENSING
  var licensePanel=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Padding=new Padding(0,3,0,3)};licensePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));licensePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,160));licensePanel.Controls.Add(licenseStatus);licensePanel.Controls.Add(Button("Licens / köp",delegate{using(var window=new ManualLicenseWindow(licensing))window.ShowDialog(this);UpdateLicenseStatus();}));root.Controls.Add(licensePanel,0,3);UpdateLicenseStatus();
  Shown+=async delegate{try{await licensing.RefreshForImport(Handle);}catch(Exception e){licensing.StoreDetail=e.Message;}if(!IsDisposed)UpdateLicenseStatus();};
#endif
  root.Controls.Add(tabs,0,contentRow);
  layout.Controls.Add(steps,0,0);layout.Controls.Add(primary,0,1);layout.Controls.Add(sendHint,0,3);
  summary.Text="";summary.Dock=DockStyle.Fill;layout.Controls.Add(summary,0,4);layout.Controls.Add(bPanel,0,5);
  layout.Controls.Add(new Label{Text="ⓘ "+SharedHelp.IsoNotice,Dock=DockStyle.Fill,Font=new Font(Font,FontStyle.Bold)},0,6);
  UpdateSteps();
  grid.BackgroundColor=Color.White;grid.BorderStyle=BorderStyle.None;grid.AlternatingRowsDefaultCellStyle.BackColor=Color.FromArgb(248,250,252);grid.ColumnHeadersDefaultCellStyle.BackColor=Color.FromArgb(226,233,241);grid.EnableHeadersVisualStyles=false;grid.Dock=DockStyle.Fill;grid.ReadOnly=true;grid.DefaultCellStyle.WrapMode=DataGridViewTriState.False;grid.AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.None;grid.RowTemplate.Height=24;grid.AllowUserToResizeRows=false;grid.ColumnHeadersDefaultCellStyle.WrapMode=DataGridViewTriState.True;grid.AllowUserToAddRows=false;grid.AllowUserToDeleteRows=false;grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;grid.RowHeadersVisible=false;grid.ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.AutoSize;
  foreach(string name in new[]{"Åtgärd","Djurnummer","Officiellt reg-ID","Nuvarande nummer","Nytt ISO-nummer","Förklaring"})grid.Columns.Add(name,name);
  int[] weights={80,60,90,90,90,200},minimums={90,100,110,110,110,170};for(int i=0;i<grid.Columns.Count;i++){grid.Columns[i].MinimumWidth=minimums[i];grid.Columns[i].FillWeight=weights[i];grid.Columns[i].SortMode=DataGridViewColumnSortMode.NotSortable;}layout.Controls.Add(grid,0,7);
  var legend=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=false};foreach(var entry in new[]{Tuple.Create(Color.MistyRose,"Fel – rättas i DelPro"),Tuple.Create(Color.FromArgb(230,245,230),"Får ISO-nummer"),Tuple.Create(Color.LightGray,"Ändras inte")}){legend.Controls.Add(new Panel{BackColor=entry.Item1,Width=16,Height=16,Margin=new Padding(2,5,4,0)});legend.Controls.Add(new Label{Text=entry.Item2,AutoSize=true,Margin=new Padding(0,4,14,0)});}layout.Controls.Add(legend,0,8);
  var actions=new FlowLayoutPanel{Dock=DockStyle.Fill};
  var footer=new Panel{Dock=DockStyle.Fill,Padding=new Padding(0,8,0,0)};var management=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=true};footer.Controls.Add(management);
#if STORE_MANUAL
  management.Controls.Add(Small(Button("Om programmet",delegate{BrandMessage.Show(ManualEdition.About,ManualEdition.Name);})));
#else
  management.Controls.Add(Button("Installation / avinstallation",delegate{Process.Start(new ProcessStartInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"HDXimport-Installation.exe")){UseShellExecute=true,Verb="runas"});}));management.Controls.Add(Button("Sök efter uppdateringar",delegate{OpenUpdater();}));management.Controls.Add(Button("Läs mig",delegate{string path=FirstStartGuide.ReadmePath(AppDomain.CurrentDomain.BaseDirectory);if(!File.Exists(path))throw new Exception("Läs mig-filen saknas.");Process.Start(new ProcessStartInfo(path){UseShellExecute=true});}));
#endif
  management.Controls.Add(Small(Button("Så ställer du in DelPro",delegate{SharedHelp.ShowGuide(this);})));testConnection=Small(Button("Testa anslutningen till DelPro",delegate{TestConnection();}));management.Controls.Add(testConnection);hints.SetToolTip(testConnection,"Kontrollerar att DelPro svarar. Ingenting ändras.");
  var close=Small(new Button{Text="Stäng",Dock=DockStyle.Right,Width=90});close.Click+=delegate{Close();};footer.Controls.Add(close);root.Controls.Add(footer,0,footerRow);FormClosing+=delegate(object sender,FormClosingEventArgs e){if(busy){e.Cancel=true;ShowError("En manuell läsning eller import pågår. Vänta tills den är klar innan du stänger programmet.","Körning pågår");}};
  actions.Controls.Add(Small(Button("Öppna svarsfil",delegate{using(var d=new OpenFileDialog{Filter="Textfiler|*.txt|Alla filer|*.*"})if(d.ShowDialog()==DialogResult.OK)LoadFile(d.FileName);})));
  actions.Controls.Add(Small(Button("Spara läsbegäran",delegate{SaveRequest();})));
  save=Small(Button("Spara importfil",delegate{SaveImport();}));save.Enabled=false;actions.Controls.Add(save);layout.Controls.Add(actions,0,9);
  RefreshPathState();iso.SelectionStart=0;iso.SelectionLength=0;ActiveControl=read.Enabled?(Control)read:settingsToggle;Shown+=delegate{iso.SelectionStart=0;iso.SelectionLength=0;if(read.Enabled)read.Focus();else settingsToggle.Focus();};
#if !STORE_MANUAL
  UpdateModeChoices();
#endif
 }
 void SetSettingsExpanded(bool expanded){settingsBody.Visible=expanded;settingsToggle.Text=expanded?"Inställningar ▾":"Inställningar ▸";pathSummary.Visible=!expanded;}
 void RefreshPathState(){if(settingsBody==null)return;bool valid=File.Exists(iso.Text);pathSummary.Text="DelPros ISO-program: "+iso.Text;hints.SetToolTip(pathSummary,pathSummary.Text);pathWarning.Visible=!valid;if(!valid)SetSettingsExpanded(true);read.Enabled=valid&&!busy;testConnection.Enabled=valid&&!busy;send.Enabled=valid&&!busy&&rows.Any(a=>Group(a)==2);if(!valid){StylePrimary(read,null);StylePrimary(send,null);read.ForeColor=send.ForeColor=testConnection.ForeColor=SystemColors.GrayText;}else if(!busy){ApplyPrimary();testConnection.ForeColor=SystemColors.ControlText;}}
 #if !STORE_MANUAL
 void SelectMode(int index){if(busy)return;modeTabs.SelectedIndex=index;UpdateModeChoices();}
 void UpdateModeChoices(){if(manualMode==null)return;manualMode.Selected=modeTabs.SelectedIndex==0;automaticMode.Selected=modeTabs.SelectedIndex==1;automaticMode.Detail="Programmet gör det varje dag\r\npå vald tid · "+(schedulePanel==null?"Okänt":schedulePanel.DisplayStatus);}
 void OpenUpdater(){
  if(busy)return;
  using(var window=new UpdateWindow())if(window.ShowDialog(this)==DialogResult.OK){
   try{Process.Start(new ProcessStartInfo(window.InstallerPath,"--update-from "+Process.GetCurrentProcess().Id){UseShellExecute=true,Verb="runas"});Close();}
   catch(System.ComponentModel.Win32Exception e){if(e.NativeErrorCode==1223)summary.Text="Uppdateringen avbröts. Programmet och schemat är oförändrade.";else ShowError(e.Message,"Uppdateringen kunde inte starta");}
   catch(Exception e){ShowError(e.Message,"Uppdateringen kunde inte starta");}
  }
 }
 #endif
 static readonly Color ActiveBlue=Color.FromArgb(24,95,165),ActiveGreen=Color.FromArgb(59,109,17),QuietBorder=Color.FromArgb(180,178,169),QuietText=Color.FromArgb(68,68,65);
 static Button Small(Button b){b.Font=new Font("Segoe UI",9);b.Padding=new Padding(2,0,2,0);b.MinimumSize=new Size(0,28);b.Height=28;b.FlatStyle=FlatStyle.Flat;b.UseVisualStyleBackColor=false;b.FlatAppearance.BorderColor=QuietBorder;b.BackColor=Color.White;b.ForeColor=QuietText;return b;}
 static void StylePrimary(Button b,Color? fill){b.FlatStyle=FlatStyle.Flat;b.UseVisualStyleBackColor=false;b.Margin=new Padding(3,3,8,3);if(fill.HasValue){b.Font=new Font("Segoe UI",11,FontStyle.Bold);b.MinimumSize=new Size(0,44);b.Padding=new Padding(12,2,12,2);b.BackColor=fill.Value;b.ForeColor=Color.White;b.FlatAppearance.BorderSize=0;b.FlatAppearance.MouseOverBackColor=ControlPaint.Dark(fill.Value,0.1f);}else{b.Font=new Font("Segoe UI",9);b.MinimumSize=new Size(0,30);b.Padding=new Padding(4,0,4,0);b.BackColor=Color.White;b.ForeColor=QuietText;b.FlatAppearance.BorderSize=1;b.FlatAppearance.BorderColor=QuietBorder;b.FlatAppearance.MouseOverBackColor=Color.FromArgb(241,239,232);}}
 void ApplyPrimary(){read.Text=NextStep==1||rows.Count==0?"Läs från DelPro":"Läs igen";StylePrimary(read,NextStep==1?ActiveBlue:(Color?)null);StylePrimary(send,NextStep==3?ActiveGreen:(Color?)null);}
 Button Button(string text,Action fn){var b=new Button{Text=text,AutoSize=true,Height=35,Padding=new Padding(6)};b.Click+=delegate{if(busy)return;try{ClearCompletion();fn();}catch(Exception e){BrandMessage.Show(e.Message,"HDX import",MessageBoxButtons.OK,MessageBoxIcon.Error);}};return b;}
 System.Collections.Generic.IEnumerable<Control> AllControls(Control parent){foreach(Control c in parent.Controls){yield return c;foreach(var child in AllControls(c))yield return child;}}
 void ClearCompletion(){workDone=null;workPanel.Visible=false;workProgress.Visible=true;workText.ForeColor=SystemColors.ControlText;}
 void ShowCompletion(string text){StopWork();workDone=text;workProgress.Visible=false;workWarning.Visible=false;workText.Text=text;workText.ForeColor=Color.DarkGreen;workPanel.Visible=true;cancelRead.Visible=false;}
 void StartWork(string phase,bool writing){ClearCompletion();summary.Text="";if(workTimer==null){workEnabled.Clear();foreach(var c in AllControls(this).Where(c=>c is Button||c is TextBox||c is RadioButton)){if(c==cancelRead)continue;workEnabled[c]=c.Enabled;c.Enabled=false;}read.BackColor=send.BackColor=SystemColors.Control;read.ForeColor=send.ForeColor=SystemColors.GrayText;Cursor=Cursors.WaitCursor;workTimer=new System.Windows.Forms.Timer{Interval=1000};workTimer.Tick+=delegate{workText.Text=workPhase+" "+(int)workClock.Elapsed.TotalSeconds+" s";};workTimer.Start();}workPhase=phase;workClock=Stopwatch.StartNew();workText.Text=phase+" 0 s";workWarning.Visible=writing;workPanel.Visible=true;}
 void StopWork(){if(workTimer!=null){workTimer.Stop();workTimer.Dispose();workTimer=null;}workPanel.Visible=workDone!=null;Cursor=Cursors.Default;foreach(var pair in workEnabled)if(!pair.Key.IsDisposed)pair.Key.Enabled=pair.Value;workEnabled.Clear();if(rows.Count>0)save.Enabled=send.Enabled=rows.Any(a=>Group(a)==2);ApplyPrimary();}
 protected virtual void ShowSuccess(string message,string title){BrandMessage.Show(message,title);}
 protected virtual void ShowError(string message,string title){ClearCompletion();StopWork();if(message.Contains("E009:")){using(var popup=new ErrorWindow(message,readRetry))popup.ShowDialog(this);}else BrandMessage.Show(message,title);}
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
 void LoadFile(string path){ClearCompletion();rows=Logic.Read(path,Country,true);source=path;ShowRows();}
 int Group(Animal a){return a.InvalidReg?1:a.Eligible&&(!BTransponders.HasNumber(a.B)||bChoice.SelectedIndex==1)?2:3;}
 string Explanation(Animal a){if(!Logic.Missing(a.Old))return "Har redan ISO-transpondernummer – ändras inte";if(!a.InvalidReg)return Group(a)==2?(BTransponders.HasNumber(a.B)?"Har B-transponder – ersätts med ISO-nummer enligt ditt val":"Saknar ISO-transpondernummer – får det nya numret"):"Har B-transponder – lämnas enligt ditt val";
  if(a.Status.Contains("E001"))return "Utländskt reg-ID – hanteras inte (E001)";if(a.Status.Contains("E002"))return Logic.Missing(a.Reg)?"Officiellt reg-ID saknas i DelPro (E002)":"Reg-ID har fel format – rätta i DelPro (E002)";if(a.Status.Contains("E003"))return "Reg-ID har fel kontrollsiffra – rätta i DelPro (E003)";if(a.Status.Contains("E004"))return "Djurnumret finns två gånger i DelPro (E004)";if(a.Status.Contains("E005"))return "Det nya numret krockar med ett annat djurs nummer (E005)";return a.Status;
 }
 void UpdateSteps(){steps.Controls.Clear();if(NextStep==0){ApplyPrimary();steps.Controls.Add(new Label{Text="Klart – inget att skriva",AutoSize=true,ForeColor=Color.DimGray});return;}foreach(int n in new[]{1,2,3})steps.Controls.Add(new Label{Text=n==1?"1 Läs från DelPro →":n==2?(NextStep==3?"2 Granska listan ✓ →":"2 Granska listan →"):"3 Skriv till DelPro",AutoSize=true,ForeColor=n==NextStep?(n==3?ActiveGreen:ActiveBlue):Color.Gray,Font=new Font(Font,n==NextStep?FontStyle.Bold:FontStyle.Regular)});ApplyPrimary();}
 string ShortExplanation(Animal a){return a.InvalidReg?Explanation(a):!Logic.Missing(a.Old)?"Har redan ISO-nummer":Group(a)==2?(BTransponders.HasNumber(a.B)?"B-transponder ersätts med ISO":"Saknar ISO-nummer"):"B-transponder behålls";}
 void ShowRows(){grid.Rows.Clear();foreach(var a in rows.OrderBy(a=>Group(a)).ThenBy(a=>int.Parse(a.Cow))){int group=Group(a);int i=grid.Rows.Add(group==1?"Fel – rättas i DelPro":group==2?"Får ISO-nummer":"Ändras inte",a.Cow,a.Reg,a.CurrentTransponder,group==2?a.Proposed:"",ShortExplanation(a));var row=grid.Rows[i];row.Cells[5].ToolTipText=Explanation(a);row.DefaultCellStyle.BackColor=group==1?Color.MistyRose:group==2?Color.FromArgb(230,245,230):Color.White;row.DefaultCellStyle.ForeColor=group==1?Color.DarkRed:group==3?Color.DimGray:Color.Black;if(group==1)row.ErrorText=a.Status;}
  int n=rows.Count(a=>Group(a)==2),errors=rows.Count(a=>Group(a)==1),b=rows.Count(a=>a.Eligible&&BTransponders.HasNumber(a.B));bPanel.Visible=b>0;
  bQuestion.Text=b+" djur har redan en B-transponder (äldre DeLaval-transponder) men inget ISO-transpondernummer.";
  summary.Text=rows.Count+" djur · "+n+" får ISO-nummer · "+(rows.Count-n-errors)+" ändras inte · "+errors+" fel";summary.ForeColor=SystemColors.ControlText;hints.SetToolTip(summary,source);save.Enabled=send.Enabled=n>0;send.Text=n>0?"Skriv "+n+" djur till DelPro":"Skriv till DelPro";sendHint.Text=n>0?"":"Inga djur att skriva";((TableLayoutPanel)sendHint.Parent).RowStyles[3].Height=n>0?0:20;NextStep=n>0?3:0;UpdateSteps();RefreshPathState();
 }

 void SaveRequest(){using(var d=new SaveFileDialog{FileName="request_djur.txt",Filter="Textfil|*.txt"})if(d.ShowDialog()==DialogResult.OK)Logic.Write(d.FileName,Logic.Request());}
#if MANUAL_LICENSING
 async void SaveImport(){if(busy)return;busy=true;try{await licensing.RefreshForImport(Handle);licensing.EnsureExport();SaveImportFile();}catch(Exception e){ShowError(e.Message,"Importfilen kunde inte sparas");}finally{busy=false;UpdateLicenseStatus();}}
#else
 void SaveImport(){SaveImportFile();}
#endif
 void SaveImportFile(){bool overwrite;var eligible=ChooseBImport(rows,out overwrite);if(eligible==null||eligible.Count==0)return;using(var d=new SaveFileDialog{FileName="import_hdx.txt",Filter="Textfil|*.txt"})if(d.ShowDialog()==DialogResult.OK){Logic.Write(d.FileName,Logic.Import(eligible,overwrite));File.Copy(source,d.FileName+".source.txt",true);summary.Text="Importfil sparad. Den har inte skickats till DelPro.";}}

 async System.Threading.Tasks.Task Run(string input,string output,string log,bool columns){
  if(!File.Exists(iso.Text))throw new Exception("Hittar inte DelPros ISO-program. Välj rätt sökväg.");
#if STORE_MANUAL
  ManualIsoFiles.Check(input,output,log);
#endif
  var start=new ProcessStartInfo(iso.Text,"-f \""+input+"\" -o \""+output+"\" -l \""+log+"\""+(columns?" -X":"")){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=Path.GetDirectoryName(iso.Text)};
  var cancel=readCancel==null?System.Threading.CancellationToken.None:readCancel.Token;
  using(var p=Process.Start(start)){try{while(!p.HasExited)await System.Threading.Tasks.Task.Delay(200,cancel);cancel.ThrowIfCancellationRequested();if(columns){if(p.ExitCode==0)await SharedHelp.WaitForResponse(output,log,p.ExitCode,cancel);else if(!File.Exists(output))throw new Exception(SharedHelp.MissingResponseHelp(output,log,p.ExitCode));}if(p.ExitCode!=0)throw new Exception("DelPro avslutades med kod "+p.ExitCode+". Kontrollera loggen: "+log);}catch(OperationCanceledException){if(!p.HasExited){p.Kill();p.WaitForExit();}throw;}}
 }
 async void ReadLive(){await ReadOnlyCore(false);}
 async void TestConnection(){await ReadOnlyCore(true);}
 async System.Threading.Tasks.Task ReadOnlyCore(bool test){if(busy)return;NextStep=1;UpdateSteps();busy=true;System.Threading.Mutex gate=null;bool locked=false;string folder="";readCancel=new System.Threading.CancellationTokenSource();cancelRead.Visible=true;StartWork("Läser från DelPro…",false);readClock=Stopwatch.StartNew();readTimer=new System.Windows.Forms.Timer{Interval=1000};readTimer.Tick+=delegate{summary.Text="Väntar på svar från DelPro… "+(int)readClock.Elapsed.TotalSeconds+" s";};summary.Text="Väntar på svar från DelPro… 0 s";readTimer.Start();try{gate=RunGate.Open();locked=RunGate.TryEnter(gate);if(!locked)throw new Exception("En annan körning pågår. Vänta tills den är klar.");folder=Folder();string req=Path.Combine(folder,"request.txt"),output=Path.Combine(folder,"output.txt"),log=Path.Combine(folder,"log.txt");Logic.Write(req,Logic.Request());await Run(req,output,log,true);readTimer.Stop();if(test){var result=Logic.Read(output,Country,true);
#if !STORE_MANUAL
DismissFirstStartInfo();
#endif
summary.Text="DelPro svarar – "+result.Count+" djur hittades.";summary.ForeColor=Color.DarkGreen;StopWork();cancelRead.Visible=false;ShowSuccess("Anslutningen till DelPro fungerar. "+result.Count+" djur hittades.\r\nIngenting har ändrats i DelPro.","DelPro svarar");}else {LoadFile(output);
#if !STORE_MANUAL
DismissFirstStartInfo();
#endif
ShowCompletion("✔ Klart – "+rows.Count+" djur lästa från DelPro.");}}catch(OperationCanceledException){summary.Text="Läsningen avbröts. Ingenting har ändrats i DelPro.";}catch(Exception e){readTimer.Stop();readRetry=()=>BeginInvoke(new Action(()=>{if(test)TestConnection();else ReadLive();}));try{ShowError(e.Message+"\r\nKörningsmapp: "+folder,"Läsning misslyckades");}finally{readRetry=null;}}finally{StopWork();readTimer.Stop();readTimer.Dispose();readTimer=null;readCancel.Dispose();readCancel=null;cancelRead.Visible=false;if(locked)gate.ReleaseMutex();if(gate!=null)gate.Dispose();busy=false;RefreshPathState();}}

 protected virtual bool? ChooseBTransponders(int total,int affected){return bChoice.SelectedIndex==1;}
 List<Animal> ChooseBImport(List<Animal> current,out bool overwrite){overwrite=false;BTransponders.RequireKnown(current);int total=current.Count(x=>BTransponders.HasNumber(x.B));if(total>0){var answer=ChooseBTransponders(total,current.Count(x=>x.Eligible&&BTransponders.HasNumber(x.B)));if(!answer.HasValue)return null;overwrite=answer.Value;}var chosen=BTransponders.Select(current,overwrite);if(chosen.Count==0)summary.Text="B-transpondrarna behålls. Det finns inga andra godkända ändringar att skriva.";return chosen;}
 protected virtual bool ConfirmImport(int count){using(var popup=new ConfirmImportWindow(count,confirmExisting,confirmKeptB))return popup.ShowDialog(this)==DialogResult.OK;}

 async void SendLive(){await SendLiveCore();}
 async System.Threading.Tasks.Task SendLiveCore(){
  {
   var selected=rows.Where(a=>a.Eligible).ToList();
   if(selected.Count==0)return;
   bool overwriteB=false;
   busy=true;StartWork("1 av 3: Läser om uppgifterna från DelPro…",true);bChoice.Enabled=false;bPanel.Enabled=false;cancelRead.Visible=false;summary.Text="";string folder="";System.Threading.Mutex gate=null;bool locked=false,importStarted=false;
#if MANUAL_LICENSING
   ImportPermit permit=null;
#endif
   try{
    gate=RunGate.Open();locked=RunGate.TryEnter(gate);if(!locked)throw new Exception("En annan körning pågår. Vänta tills den är klar.");
#if MANUAL_LICENSING
    summary.Text="Kontrollerar licens...";await licensing.RefreshForImport(Handle);permit=licensing.BeginImport();UpdateLicenseStatus();
#endif
    folder=Folder();File.Copy(source,Path.Combine(folder,"preview_source.txt"));
    string req=Path.Combine(folder,"request.txt"),fresh=Path.Combine(folder,"before.txt");Logic.Write(req,Logic.Request());summary.Text="";await Run(req,fresh,Path.Combine(folder,"before_log.txt"),true);
    var current=Logic.Read(fresh,Country);
    foreach(var a in selected){var match=current.Where(x=>x.Cow==a.Cow).ToList();if(match.Count!=1||!match[0].Eligible||match[0].Reg!=a.Reg||match[0].Proposed!=a.Proposed)throw new Exception("Djur "+a.Cow+" har ändrats eller är blockerat. Läs om uppgifterna och granska igen. Ingen import skickad.");}
    var requested=new HashSet<string>(selected.Select(x=>x.Cow));var candidates=current.Where(x=>requested.Contains(x.Cow)).ToList();
    // Decisions use the fresh B values, including a B number added since the preview.
    BTransponders.RequireKnown(current);int totalB=current.Count(x=>BTransponders.HasNumber(x.B));
    if(totalB>0){var answer=ChooseBTransponders(totalB,candidates.Count(x=>BTransponders.HasNumber(x.B)));if(!answer.HasValue)return;overwriteB=answer.Value;}
    selected=BTransponders.Select(candidates,overwriteB);if(selected.Count==0){summary.Text="B-transpondrarna behålls. Inga ändringar skickades till DelPro.";return;}
    confirmExisting=current.Count(a=>!Logic.Missing(a.Old));confirmKeptB=overwriteB?0:current.Count(a=>a.Eligible&&BTransponders.HasNumber(a.B));
    if(!ConfirmImport(selected.Count))return;
    string import=Path.Combine(folder,"import.txt"),log=Path.Combine(folder,"import_log.txt");Logic.Write(import,Logic.Import(selected,overwriteB));summary.Text="";importStarted=true;StartWork("2 av 3: Skriver "+selected.Count+" djur till DelPro…",true);await Run(import,Path.Combine(folder,"import_output.txt"),log,false);
    string after=Path.Combine(folder,"after.txt");StartWork("3 av 3: Kontrollerar att allt blev rätt…",true);summary.Text="";await Run(req,after,Path.Combine(folder,"after_log.txt"),true);var result=Logic.Read(after,Country);
    var failed=selected.Where(a=>result.Count(x=>x.Cow==a.Cow&&x.Old==a.Proposed)!=1).ToList();LoadFile(after);
    File.WriteAllText(Path.Combine(folder,"verification.txt"),"Begärda: "+selected.Count+"\r\nVerifierade: "+(selected.Count-failed.Count)+"\r\nEj verifierade: "+string.Join(",",failed.Select(x=>x.Cow)),Encoding.UTF8);
    summary.Text="Återläsning verifierade "+(selected.Count-failed.Count)+" av "+selected.Count+" ändringar. Importlogg: "+log;
#if MANUAL_LICENSING
    licensing.Verified(permit,failed.Count==0);UpdateLicenseStatus();
#endif
    if(failed.Count==0){StartWork("Läser från DelPro…",false);string refreshed=Path.Combine(folder,"refreshed.txt");await Run(req,refreshed,Path.Combine(folder,"refreshed_log.txt"),true);LoadFile(refreshed);string done="Klart – "+selected.Count+" djur har fått ISO-transpondernummer";NextStep=1;UpdateSteps();steps.Controls.Clear();steps.Controls.Add(new Label{Text=done,AutoSize=true,ForeColor=Color.DarkGreen});ShowCompletion("✔ "+done+". Kontrollerat i DelPro.");}
    if(failed.Count>0)ShowError("E008: Återläsning kunde inte verifiera "+failed.Count+" ändringar.\nDjur: "+string.Join(", ",failed.Select(a=>a.Cow))+"\nKontrollera importloggen: "+log+"\nGranska loggen innan du skickar importen igen.","Verifiering misslyckades");
   }catch(Exception e){ShowError(e.Message+"\nKörningsmapp: "+folder+(importStarted?"\nEn import kan ha behandlats delvis. Läs om uppgifterna före ett nytt försök.":"\nIngen import har skickats till DelPro."),"Återföring avbruten");}finally{StopWork();if(locked)gate.ReleaseMutex();if(gate!=null)gate.Dispose();busy=false;bChoice.Enabled=true;bPanel.Enabled=true;RefreshPathState();
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


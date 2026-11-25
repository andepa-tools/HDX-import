using System;using System.IO;using System.Drawing;using System.Diagnostics;using System.Windows.Forms;
public static class SharedHelp {
 public const string TechnicalSeparator="--- Teknisk information ---";
 public const string IsoNotice="Djur som redan har ett ISO-transpondernummer ändras aldrig. De står längst ner i listan som \"Ändras inte\".";
 public const string Steps="Steg 1. Öppna fliken Devices i DelPro och öppna ISOProcessor (Device Type: ISOController).\r\nDet gör inget att den står som inaktiv. Bocka INTE i Active.\r\nSteg 2. Under Command Line ISO: bocka i Use Command Line ISO.\r\nSteg 3. Kontrollera att Location Of The ISO Files är C:\\Delpro\\ISO.\r\nSteg 4. Spara med diskettikonen längst upp till vänster.\r\nSteg 5. Klicka på den blå knappen Jag har ändrat – försök igen.\r\n\r\nInställningen kan försvinna när en DelPro-databas återställs eller byts.";
 public static string Describe(string path){
  try{using(var f=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite)){return path+": "+(f.Length==0?"tom":"finns, "+f.Length+" byte (kan fortfarande vara låst eller ofärdig)");}}
  catch(FileNotFoundException){return path+": saknas";}catch(DirectoryNotFoundException){return path+": mappen saknas";}
  catch(UnauthorizedAccessException e){return path+": åtkomst nekad. "+e.Message;}catch(IOException e){return path+": kunde inte öppnas (låst eller annat filfel). "+e.Message;}
 }
 public static string MissingResponseHelp(string output,string log,int exitCode){return MissingResponseHelp(output,log,exitCode,false);}
 public static string MissingResponseHelp(string output,string log,int exitCode,bool noFiles){return
  (noFiles?"E009: HDX Import får inget svar från DelPro\r\n\r\nDet beror nästan alltid på att kommandoradsfunktionen för ISO är avstängd i DelPro.":"E009: HDX Import fick inget färdigt svar från DelPro\r\n\r\nKontrollera först inställningen i DelPro:")+"\r\n\r\n"+Steps+
  (noFiles?"":"\r\n\r\nOm rutan redan är ibockad: öppna Visa teknisk information och granska loggen.")+"\r\n\r\nDelPros åtkomst är inte bekräftad om svaret saknas.\r\n\r\n"+TechnicalSeparator+"\r\nFelkod: E009 ("+(noFiles?"kommandoraden avstängd":"inget färdigt svar")+")\r\nSvar: "+Describe(output)+"\r\nLogg: "+Describe(log)+"\r\nAvslutningskod: "+exitCode+"."+(noFiles?" DelPro avslutade utan att skriva svar eller logg.":"")+"\r\nKörmapp: "+Path.GetDirectoryName(output)+"\r\nTid: "+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");}
 public static string TechnicalInformation(string result){int at=result.IndexOf(TechnicalSeparator,StringComparison.Ordinal);if(at<0)return result;var lines=result.Substring(at+TechnicalSeparator.Length).TrimStart('\r','\n').Split(new[]{"\r\n","\n"},StringSplitOptions.None);return string.Join("\r\n",lines,0,Math.Min(6,lines.Length));}

 public static async System.Threading.Tasks.Task WaitForResponse(string output,string log,int exitCode,System.Threading.CancellationToken cancel=default(System.Threading.CancellationToken)){
  var clock=Stopwatch.StartNew();while(!File.Exists(output)&&!File.Exists(log)&&clock.Elapsed<TimeSpan.FromSeconds(5))await System.Threading.Tasks.Task.Delay(200,cancel).ConfigureAwait(false);
  cancel.ThrowIfCancellationRequested();if(!File.Exists(output)&&!File.Exists(log))throw new Exception(MissingResponseHelp(output,log,exitCode,true));
  if(!await ResponseWait.Ready(output,null,null,cancel).ConfigureAwait(false))throw new Exception(MissingResponseHelp(output,log,exitCode));
 }
 public static void ShowGuide(IWin32Window owner){using(var window=new ErrorWindow("Inställning i DelPro för HDX Import\r\n\r\n"+Steps,null,true))window.ShowDialog(owner);}
 public static string Version {get{var v=System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;return v.Major+"."+v.Minor;}}

}
public class ErrorWindow:Form {
 public ErrorWindow(string result,Action retry=null,bool guideOnly=false,string heading=null,string retryLabel="Jag har ändrat – försök igen"){
  if((guideOnly||result.Contains("E009:"))&&BuildGuide(result,retry,guideOnly,heading,retryLabel))return;
  Branding.Apply(this);Text="HDX import - fel vid tillägg av transpondernummer";Width=950;Height=530;MinimumSize=new Size(650,350);StartPosition=FormStartPosition.CenterScreen;Font=new Font("Segoe UI",10);TopMost=true;
  var label=new Label{Text="Följande kunde inte hanteras. Befintliga transpondernummer lämnas orörda.",Dock=DockStyle.Top,Height=48,Padding=new Padding(12)};
  var details=new TextBox{Text=result,ReadOnly=true,Multiline=true,ScrollBars=ScrollBars.Both,WordWrap=false,Dock=DockStyle.Fill};
  var close=new Button{Text="Stäng",Dock=DockStyle.Bottom,Height=40};close.Click+=delegate{Close();};Controls.Add(details);Controls.Add(label);Controls.Add(close);var brand=Branding.Header();brand.Dock=DockStyle.Top;brand.Height=62;Controls.Add(brand);AcceptButton=close;CancelButton=close;details.SelectionStart=0;details.SelectionLength=0;ActiveControl=close;
 }
 static Image Resource(string name){try{using(var stream=System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(name)){if(stream==null)return null;using(var original=Image.FromStream(stream))return new Bitmap(original);}}catch{return null;}}
 bool BuildGuide(string result,Action retry,bool guideOnly,string heading,string retryLabel){
  Image tree=Resource("CmdTree"),setting=Resource("CmdSetting");if(tree==null||setting==null){if(tree!=null)tree.Dispose();if(setting!=null)setting.Dispose();return false;}
  Branding.Apply(this);Text=guideOnly?"Inställning i DelPro för HDX Import":"HDX Import – hjälp med DelPro";Size=new Size(1000,820);MinimumSize=new Size(760,600);StartPosition=FormStartPosition.CenterParent;Font=new Font("Segoe UI",10);
  var scroll=new Panel{Dock=DockStyle.Fill,AutoScroll=true,Padding=new Padding(18)};var content=new TableLayoutPanel{Dock=DockStyle.Top,AutoSize=true,ColumnCount=1};content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));scroll.Controls.Add(content);Controls.Add(scroll);
  Action<string,bool> add=(value,bold)=>{var label=new Label{Text=value,AutoSize=true,MaximumSize=new Size(880,0),Margin=new Padding(0,0,0,10)};if(bold)label.Font=new Font(Font,FontStyle.Bold);content.Controls.Add(label);};
  int at=result.IndexOf("E009:");string title=heading??(guideOnly?"Inställning i DelPro för HDX Import":result.Substring(at+5).TrimStart().Split(new[]{'\r','\n'})[0]);
  var titleLabel=new Label{Text=title,Font=new Font("Segoe UI",13,FontStyle.Bold),AutoSize=true,MaximumSize=new Size(880,0),Margin=new Padding(0,0,0,12)};content.Controls.Add(titleLabel);
  if(!guideOnly)add(result.Contains("nästan alltid")?"Det beror nästan alltid på att kommandoradsfunktionen för ISO är avstängd i DelPro.":"Kontrollera först inställningen i DelPro:",false);
  Action<string> delpro=value=>{var line=new Label{Text=value,AutoSize=true,MaximumSize=new Size(880,0),Font=new Font(Font,FontStyle.Bold),Margin=new Padding(0,0,0,10)};content.Controls.Add(line);};
  delpro("Steg 1. Öppna fliken Devices i DelPro och öppna ISOProcessor (Device Type: ISOController).\nDet gör inget att den står som inaktiv. Bocka INTE i Active.");
  var first=new PictureBox{Image=tree,Size=tree.Size,SizeMode=PictureBoxSizeMode.Normal,Margin=new Padding(0,0,0,5)};content.Controls.Add(first);add("Steg 1: ISOProcessor under Devices",false);
  delpro("Steg 2. Under Command Line ISO: bocka i Use Command Line ISO.");var second=new PictureBox{Image=setting,Size=setting.Size,SizeMode=PictureBoxSizeMode.Normal,Margin=new Padding(0,0,0,5)};content.Controls.Add(second);add("Steg 2–4: Use Command Line ISO ibockad",false);
  delpro("Steg 3. Kontrollera att Location Of The ISO Files är C:\\Delpro\\ISO.");add("Steg 4. Spara med diskettikonen längst upp till vänster.",false);
  add(retry!=null?(retryLabel=="Jag har ändrat – försök igen"?"Steg 5. Klicka på den blå knappen Jag har ändrat – försök igen.":"Steg 5. Klicka på "+retryLabel+"."):"Steg 5. Stäng guiden och läs från DelPro igen.",false);
  Func<Button> retryButton=()=>{var b=new Button{Text=retryLabel,AutoSize=true,MinimumSize=new Size(280,44),BackColor=Color.FromArgb(25,91,160),ForeColor=Color.White,UseVisualStyleBackColor=false};b.Click+=delegate{Close();retry();};return b;};
  if(retry!=null&&retryLabel=="Jag har ändrat – försök igen")content.Controls.Add(retryButton());
  add("Inställningen kan försvinna när en DelPro-databas återställs eller byts.",false);
  if(!guideOnly){if(!result.Contains("nästan alltid"))add("Om rutan redan är ibockad: öppna Visa teknisk information och granska loggen.",false);string technicalText=SharedHelp.TechnicalInformation(result);var technical=new Panel{Width=880,Height=215,Visible=false};var details=new TextBox{Text=technicalText,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Both,WordWrap=false,Dock=DockStyle.Fill};var copy=new Button{Text="Kopiera",Dock=DockStyle.Bottom,Height=34};copy.Click+=delegate{Clipboard.SetText(technicalText);};technical.Controls.Add(details);technical.Controls.Add(copy);var toggle=new Button{Text="Visa teknisk information ▸",AutoSize=true};toggle.Click+=delegate{technical.Visible=!technical.Visible;toggle.Text=technical.Visible?"Dölj teknisk information ▾":"Visa teknisk information ▸";if(technical.Visible)scroll.ScrollControlIntoView(technical);};content.Controls.Add(toggle);content.Controls.Add(technical);}
  var actions=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=68,Padding=new Padding(8),FlowDirection=FlowDirection.RightToLeft};var close=new Button{Text="Stäng",AutoSize=true,DialogResult=DialogResult.Cancel};actions.Controls.Add(close);close.Click+=delegate{Close();};CancelButton=close;if(retry!=null){var again=retryButton();actions.Controls.Add(again);AcceptButton=again;}Controls.Add(actions);Shown+=delegate{ActiveControl=close;scroll.AutoScrollPosition=Point.Empty;};Disposed+=delegate{tree.Dispose();setting.Dispose();};return true;
 }

}
public class ReadmeWindow:Form {
 public ReadmeWindow(string title,string readme){
  Branding.Apply(this);Text=title;Width=900;Height=700;MinimumSize=new Size(650,400);StartPosition=FormStartPosition.CenterParent;Font=new Font("Segoe UI",10);
  var content=new TextBox{Multiline=true,ReadOnly=true,Dock=DockStyle.Fill,ScrollBars=ScrollBars.Vertical,Text=File.ReadAllText(readme)};
  var close=new Button{Text="Stäng",Dock=DockStyle.Bottom,Height=40};close.Click+=delegate{Close();};Controls.Add(content);Controls.Add(close);var brand=Branding.Header();brand.Dock=DockStyle.Top;brand.Height=62;Controls.Add(brand);var pdf=new Button{Text="Öppna som PDF",Dock=DockStyle.Bottom,Height=36};pdf.Click+=delegate{try{Process.Start(new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(readme),"README.pdf")){UseShellExecute=true});}catch(Exception e){BrandMessage.Show(e.Message,"Kunde inte öppna PDF");}};Controls.Add(pdf);AcceptButton=close;CancelButton=close;content.SelectionStart=0;content.SelectionLength=0;ActiveControl=close;
 }
}

public static class Branding {
 public static string ProductName {get{var values=System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyProductAttribute),false);return values.Length>0?((System.Reflection.AssemblyProductAttribute)values[0]).Product:"HDX import";}}

 public static void Apply(Form form){using(var stream=System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AppIcon")){if(stream!=null)using(var icon=new Icon(stream))form.Icon=(Icon)icon.Clone();}}
 public static Control Header(){
  var header=new Panel{Dock=DockStyle.Fill};
  Image logo;using(var stream=System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AppLogo")){using(var original=Image.FromStream(stream))logo=new Bitmap(original);}
  var image=new PictureBox{Image=logo,SizeMode=PictureBoxSizeMode.Zoom,Dock=DockStyle.Left,Width=68,Padding=new Padding(0,2,10,2)};image.Disposed+=delegate{logo.Dispose();};
  var title=new Label{Text=ProductName,Dock=DockStyle.Top,Height=29,Font=new Font("Segoe UI",16,FontStyle.Bold)};
  var subtitle=new Label{Text="Lägg till ISO-transpondernummer på djur som saknar det",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,AutoEllipsis=true};
  var words=new Panel{Dock=DockStyle.Fill};words.Controls.Add(subtitle);words.Controls.Add(title);header.Controls.Add(words);header.Controls.Add(image);return header;
 }
}

public static class BrandMessage {
 public static DialogResult Show(string message,string caption="HDX import",MessageBoxButtons buttons=MessageBoxButtons.OK,MessageBoxIcon icon=MessageBoxIcon.None){
  using(var form=new Form{Text=caption,Width=660,Height=380,MinimumSize=new Size(500,300),StartPosition=FormStartPosition.CenterScreen,Font=new Font("Segoe UI",10)}){
   Branding.Apply(form);var header=Branding.Header();header.Dock=DockStyle.Top;header.Height=68;
   var text=new TextBox{Text=message,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,Dock=DockStyle.Fill,BorderStyle=BorderStyle.None,BackColor=SystemColors.Control,Margin=new Padding(12)};
   var panel=new Panel{Dock=DockStyle.Fill,Padding=new Padding(12)};panel.Controls.Add(text);form.Controls.Add(panel);form.Controls.Add(header);
   var actions=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=48,FlowDirection=FlowDirection.RightToLeft,Padding=new Padding(6)};form.Controls.Add(actions);
   Action<string,DialogResult> add=(label,result)=>{var b=new Button{Text=label,AutoSize=true,Height=32,DialogResult=result};actions.Controls.Add(b);if(result==DialogResult.No||result==DialogResult.Cancel)form.CancelButton=b;if(result==DialogResult.OK||result==DialogResult.Yes)form.AcceptButton=b;};
   if(buttons==MessageBoxButtons.YesNo){add("Nej",DialogResult.No);add("Ja",DialogResult.Yes);}else {add("OK",DialogResult.OK);form.CancelButton=form.AcceptButton;}
   text.SelectionStart=0;text.SelectionLength=0;form.Shown+=delegate{text.SelectionStart=0;text.SelectionLength=0;form.ActiveControl=(Control)form.AcceptButton;((Control)form.AcceptButton).Focus();};return form.ShowDialog();
  }
 }
}

public static class ResponseWait {
 public static async System.Threading.Tasks.Task<bool> Ready(string path,TimeSpan? timeout=null,TimeSpan? quiet=null,System.Threading.CancellationToken cancel=default(System.Threading.CancellationToken)){
  var clock=Stopwatch.StartNew();TimeSpan limit=timeout??TimeSpan.FromSeconds(30),stable=quiet??TimeSpan.FromSeconds(2);long size=-1,stamp=-1;TimeSpan unchanged=TimeSpan.Zero;
  while(clock.Elapsed<limit){cancel.ThrowIfCancellationRequested();
   try{using(var file=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.None)){
    long current=file.Length,modified=File.GetLastWriteTimeUtc(path).Ticks;
    if(current>0){if(current!=size||modified!=stamp){size=current;stamp=modified;unchanged=clock.Elapsed;}else if(clock.Elapsed-unchanged>=stable)return true;}
    else {size=-1;stamp=-1;}
   }}catch(IOException){size=-1;stamp=-1;}catch(UnauthorizedAccessException){size=-1;stamp=-1;}
   await System.Threading.Tasks.Task.Delay(200,cancel).ConfigureAwait(false);
  }
  return false;
 }
}


public sealed class ConfirmImportWindow:Form {
 public ConfirmImportWindow(int count,int existing,int keptB){Branding.Apply(this);Text="Skriv ISO-transpondernummer till DelPro?";Width=720;Height=360;MinimumSize=new Size(680,340);StartPosition=FormStartPosition.CenterParent;Font=new Font("Segoe UI",10);
  var text=new Label{Dock=DockStyle.Fill,Padding=new Padding(22),Text="Skriv ISO-transpondernummer till DelPro?\r\n\r\n"+count+" djur får nytt ISO-transpondernummer.\r\n"+existing+" djur har redan ISO-transpondernummer och ändras inte.\r\n"+(keptB>0?keptB+" djur med B-transponder lämnas enligt ditt val.\r\n":"")+"\r\nProgrammet läser om uppgifterna från DelPro först och avbryter om något har ändrats."};Controls.Add(text);var actions=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=54,FlowDirection=FlowDirection.RightToLeft,Padding=new Padding(10)};var cancel=new Button{Text="Avbryt",AutoSize=true,DialogResult=DialogResult.Cancel};var send=new Button{Text="Skriv till DelPro",AutoSize=true,DialogResult=DialogResult.OK};actions.Controls.Add(cancel);actions.Controls.Add(send);Controls.Add(actions);AcceptButton=send;CancelButton=cancel;
 }
}

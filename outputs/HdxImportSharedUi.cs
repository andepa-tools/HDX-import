using System;using System.IO;using System.Drawing;using System.Diagnostics;using System.Windows.Forms;
public static class SharedHelp {
 public static string MissingResponseHelp(string output,string log,int exitCode){
#if STORE_MANUAL
  return ManualIsoFiles.Missing(output,log,exitCode);
#else
  return "E009: Ingen färdig svarsfil kunde läsas.\r\n"+
   "En möjlig orsak är att ISO-processorns kommandoradsfunktion inte är aktiverad.\r\n\r\n"+
   "Gör så här i DelPro:\r\n"+
   "1. Öppna utrustningsträdet.\r\n"+
   "2. Välj ISO-processorn.\r\n"+
   "3. Bocka i \"Använd kommandorad ISO\" och spara.\r\n\r\n"+
   "ISO-processorn behöver inte vara aktiv. Om kommandoradsbocken redan är ibockad: kontrollera sökvägen och granska loggfilen.\r\n"+
   "Svarsfil saknas, är tom eller kunde inte läsas färdigt: "+output+".\r\n"+
   "ISO-processorns avslutningskod: "+exitCode+".\r\nLoggfil: "+log;
#endif
 }
}
public class ErrorWindow:Form {
 public ErrorWindow(string result){
  Branding.Apply(this);Text="HDX import - fel vid tillägg av transpondernummer";Width=950;Height=530;MinimumSize=new Size(650,350);StartPosition=FormStartPosition.CenterScreen;Font=new Font("Segoe UI",10);TopMost=true;
  var label=new Label{Text="Följande kunde inte hanteras. Befintliga transpondernummer lämnas orörda.",Dock=DockStyle.Top,Height=48,Padding=new Padding(12)};
  var details=new TextBox{Text=result,ReadOnly=true,Multiline=true,ScrollBars=ScrollBars.Both,WordWrap=false,Dock=DockStyle.Fill};
  var close=new Button{Text="Stäng",Dock=DockStyle.Bottom,Height=40};close.Click+=delegate{Close();};Controls.Add(details);Controls.Add(label);Controls.Add(close);var brand=Branding.Header();brand.Dock=DockStyle.Top;brand.Height=62;Controls.Add(brand);AcceptButton=close;CancelButton=close;details.SelectionStart=0;details.SelectionLength=0;ActiveControl=close;
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
   return form.ShowDialog();
  }
 }
}

public static class ResponseWait {
 public static async System.Threading.Tasks.Task<bool> Ready(string path,TimeSpan? timeout=null,TimeSpan? quiet=null){
  var clock=Stopwatch.StartNew();TimeSpan limit=timeout??TimeSpan.FromSeconds(30),stable=quiet??TimeSpan.FromSeconds(2);long size=-1,stamp=-1;TimeSpan unchanged=TimeSpan.Zero;
  while(clock.Elapsed<limit){
   try{using(var file=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.None)){
    long current=file.Length,modified=File.GetLastWriteTimeUtc(path).Ticks;
    if(current>0){if(current!=size||modified!=stamp){size=current;stamp=modified;unchanged=clock.Elapsed;}else if(clock.Elapsed-unchanged>=stable)return true;}
    else {size=-1;stamp=-1;}
   }}catch(IOException){size=-1;stamp=-1;}catch(UnauthorizedAccessException){size=-1;stamp=-1;}
   await System.Threading.Tasks.Task.Delay(200).ConfigureAwait(false);
  }
  return false;
 }
}


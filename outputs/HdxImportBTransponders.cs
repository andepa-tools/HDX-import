using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Drawing;using System.Windows.Forms;
public static class BTransponders {
 public const string ConfigKey="OverwriteBTransponders";
 public static bool HasNumber(string value){return !Logic.Missing(value??"");}
 public static bool ReadSetting(string config){return BTransponderSettings.Read(config);}
 public static string WithSetting(string config,bool overwrite){return BTransponderSettings.With(config,overwrite);}
 public static void RequireKnown(IEnumerable<Animal> rows){if(rows.Any(x=>!x.BKnown))throw new Exception("E010: B-transponderuppgifter saknas i svarsfilen. Läs från DelPro igen med den nya versionen innan något skrivs. Ingen import skickad.");}
 public static List<Animal> Select(IEnumerable<Animal> rows,bool overwrite){var eligible=rows.Where(x=>x.Eligible).ToList();RequireKnown(eligible);return eligible.Where(x=>overwrite||!HasNumber(x.B)).ToList();}
}
public sealed class BTransponderWindow:Form {
 public bool? Overwrite;
 public BTransponderWindow(int farmCount,int affected){Branding.Apply(this);Text="Välj hur B-transpondrar ska hanteras";Width=800;Height=390;MinimumSize=new Size(760,360);Font=new Font("Segoe UI",10);StartPosition=FormStartPosition.CenterParent;BackColor=Color.FromArgb(242,245,248);
  var root=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(20),ColumnCount=1,RowCount=3};root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,52));Controls.Add(root);root.Controls.Add(Branding.Header());
  root.Controls.Add(new Label{Dock=DockStyle.Fill,Text="B-transpondrar finns på "+farmCount+" djur i den inlästa listan.\r\n"+affected+" av djuren i den här importen har B-transponder.\r\n\r\nBehåll: djur med B-transponder hoppas över och lämnas helt orörda.\r\nSkriv över: de får ISO-transpondernummer, som ersätter B-transpondern i DelPro.\r\n\r\nÖvriga godkända djur kan importeras som vanligt."});var actions=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=false};
  var keep=new Button{Text="Behåll B-transpondrar",AutoSize=true,Height=38};keep.Click+=delegate{Overwrite=false;DialogResult=DialogResult.OK;};var replace=new Button{Text="Skriv över med ISO",AutoSize=true,Height=38};replace.Click+=delegate{Overwrite=true;DialogResult=DialogResult.OK;};var cancel=new Button{Text="Avbryt",AutoSize=true,Height=38,DialogResult=DialogResult.Cancel};actions.Controls.Add(keep);actions.Controls.Add(replace);actions.Controls.Add(cancel);root.Controls.Add(actions);AcceptButton=keep;CancelButton=cancel;
 }
}

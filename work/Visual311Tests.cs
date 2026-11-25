using System;using System.IO;using System.Linq;using System.Reflection;using System.Windows.Forms;using System.Drawing;
class VisualForm:MainForm{protected override void OnShown(EventArgs e){}}
class Visual311Tests{
 static System.Collections.Generic.IEnumerable<Control> All(Control root){foreach(Control c in root.Controls){yield return c;foreach(var x in All(c))yield return x;}}
 static void Snap(Form form,string path){form.Show();for(int i=0;i<8;i++)Application.DoEvents();if(path.EndsWith("E009.png")){var inline=All(form).OfType<Button>().FirstOrDefault(b=>b.Text=="Jag har ändrat – försök igen"&&b.Parent is TableLayoutPanel);if(inline!=null){((ScrollableControl)inline.Parent.Parent).ScrollControlIntoView(inline);Application.DoEvents();}}using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(Point.Empty,bitmap.Size));bitmap.Save(path);}}
 [STAThread]static int Main(){try{Application.EnableVisualStyles();string error=SharedHelp.MissingResponseHelp("test/output.txt","test/log.txt",0,true);
#if NO_PICTURES
 using(var f=new ErrorWindow(error,()=>{})){f.Show();Application.DoEvents();if(All(f).OfType<PictureBox>().Count()!=1||All(f).OfType<TextBox>().Count()!=1)throw new Exception("image-free fallback");f.Close();}
#else
 using(var f=new ErrorWindow(error,()=>{})){Snap(f,"outputs/preview-1.7-E009.png");if(!All(f).OfType<Label>().Any(l=>l.Text=="HDX Import får inget svar från DelPro"))throw new Exception("guide heading");All(f).OfType<Button>().Single(b=>b.Text.StartsWith("Visa teknisk")).PerformClick();Snap(f,"outputs/preview-1.7-E009-teknisk.png");f.Close();}
 int retried=0;using(var f=new ErrorWindow(error,()=>retried++)){f.Show();Application.DoEvents();All(f).OfType<Button>().First(b=>b.Text=="Jag har ändrat – försök igen").PerformClick();if(retried!=1||f.Visible)throw new Exception("retry callback after close");}
 using(var f=new ErrorWindow(error,null,true)){f.Show();Application.DoEvents();if(All(f).OfType<TextBox>().Any()||All(f).OfType<Button>().Any(b=>b.Text.StartsWith("Visa teknisk")))throw new Exception("guide only hides diagnostics");f.Close();}
 using(var f=new VisualForm()){f.Size=new Size(980,760);var flags=BindingFlags.NonPublic|BindingFlags.Instance;((TextBox)typeof(MainForm).GetField("iso",flags).GetValue(f)).Text=Path.GetFullPath("work/test311-bin/MockB.exe");typeof(MainForm).GetMethod("SetSettingsExpanded",flags).Invoke(f,new object[]{false});
#if STORE_MANUAL
 Snap(f,"outputs/preview-1.7-huvudfonster-fore-lasning.png");string input=Path.GetFullPath("work/visual311-mixed.txt");File.WriteAllText(input,"000009\tSE-020201-1234\t?\t12345\n000002\tSE-020201-1235\t?\t0\n000007\tSE-020201-1236\t752002020112363\t23456\n000001\t?\t?\t0\n000003\tSE-020201-1237\t?\t0\nZN\n");typeof(MainForm).GetMethod("LoadFile",flags).Invoke(f,new object[]{input});Snap(f,"outputs/preview-1.7-huvudfonster-efter-lasning.png");if(((DataGridView)typeof(MainForm).GetField("grid",flags).GetValue(f)).Height<100)throw new Exception("minimum table space");
#else
 var tabs=All(f).OfType<TabControl>().Single();tabs.SelectedIndex=1;Snap(f,"outputs/preview-3.11-automatik.png");
#endif
 f.Close();}
#endif
 Console.WriteLine("PASS: guide layout, retry callback, help-only/fallback, minimum-size window.");return 0;}catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}

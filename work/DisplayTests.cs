using System;using System.IO;using System.Reflection;using System.Windows.Forms;
class DisplayTests {
 [STAThread]static int Main(){try {
 string[] missing={"","?","???????????????","0","00000","-",null};
 foreach(string value in missing){var a=new Animal{Old=value,B="12345"};if(a.CurrentTransponder!="12345")throw new Exception("B fallback");foreach(string b in missing){a.B=b;if(a.CurrentTransponder!="-")throw new Exception("missing marker");}}
 var iso=new Animal{Old="752002020112349",B="12345"};if(iso.CurrentTransponder!=iso.Old)throw new Exception("ISO priority");
 string file=Path.GetFullPath("work/transponder-display-test.txt");File.WriteAllText(file,"000001\tSE-020201-1234\t???????????????\t12345\n000002\tSE-020201-1235\t???????????????\t?????\n000003\tSE-020201-1236\t752002020112363\t23456\nZN\n");
 using(var form=new MainForm()){var flags=BindingFlags.NonPublic|BindingFlags.Instance;typeof(MainForm).GetMethod("LoadFile",flags).Invoke(form,new object[]{file});var grid=(DataGridView)typeof(MainForm).GetField("grid",flags).GetValue(form);var choice=(ComboBox)typeof(MainForm).GetField("bChoice",flags).GetValue(form);if(choice.SelectedIndex!=0||!grid.Rows[0].Cells[4].Value.ToString().Contains("behålls"))throw new Exception("Default keep");choice.SelectedIndex=1;if(grid.Rows[0].Cells[4].Value.ToString().Contains("behålls"))throw new Exception("Overwrite selection");form.Show();Application.DoEvents();using(var img=new System.Drawing.Bitmap(form.Width,form.Height)){form.DrawToBitmap(img,new System.Drawing.Rectangle(0,0,img.Width,img.Height));img.Save("outputs/HDXimport-B-val-i-manuell-flik.png");}string[] expected={"12345","-","752002020112363"};for(int i=0;i<3;i++)if((string)grid.Rows[i].Cells[2].Value!=expected[i])throw new Exception("Grid row "+i);}
 Console.WriteLine("PASS: ISO priority, B fallback, all missing markers, actual grid rendering data.");return 0;}catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}


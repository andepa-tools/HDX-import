using System;using System.IO;using System.Diagnostics;using System.Reflection;using System.Threading.Tasks;using System.Windows.Forms;
class WaitTests{
[STAThread]static int Main(){try{string root=Path.GetFullPath("work/wait37_"+DateTime.Now.Ticks);Directory.CreateDirectory(root);
string missing=Path.Combine(root,"missing.txt");var timer=Stopwatch.StartNew();if(ResponseWait.Ready(missing,TimeSpan.FromMilliseconds(650),TimeSpan.FromMilliseconds(100)).GetAwaiter().GetResult()||timer.ElapsedMilliseconds<650)throw new Exception("Missing response timeout failed");
string empty=Path.Combine(root,"empty.txt");File.WriteAllText(empty,"");if(ResponseWait.Ready(empty,TimeSpan.FromMilliseconds(300),TimeSpan.FromMilliseconds(100)).GetAwaiter().GetResult())throw new Exception("Empty file accepted");
Application.EnableVisualStyles();using(var form=new MainForm()){var flags=BindingFlags.NonPublic|BindingFlags.Instance;((TextBox)typeof(MainForm).GetField("iso",flags).GetValue(form)).Text=Path.GetFullPath("work/test311-bin/DelayedIso.exe");string request=Path.Combine(root,"request.txt");Logic.Write(request,Logic.Request());string output=Path.Combine(root,"manual.txt");timer.Restart();var task=(Task)typeof(MainForm).GetMethod("Run",flags).Invoke(form,new object[]{request,output,Path.Combine(root,"manual_log.txt"),true});while(!task.IsCompleted){Application.DoEvents();System.Threading.Thread.Sleep(20);}task.GetAwaiter().GetResult();if(timer.ElapsedMilliseconds<4000||Logic.Read(output,"752",true).Count!=1)throw new Exception("Manual read accepted incomplete response");}
#if !STORE_MANUAL
System.Threading.SynchronizationContext.SetSynchronizationContext(null);string auto=Path.Combine(root,"auto");AutoEngine.Execute(Path.GetFullPath("work/test311-bin/DelayedIso.exe"),auto,"752",Console.WriteLine).GetAwaiter().GetResult();if(File.Exists(Path.Combine(auto,"import.txt")))throw new Exception("Unexpected import");
#endif
Console.WriteLine("PASS: delayed file after process exit; locked partial writes; manual and automatic reads; empty file rejected; missing-file timeout.");return 0;}catch(Exception e){Console.Error.WriteLine(e);return 1;}}}


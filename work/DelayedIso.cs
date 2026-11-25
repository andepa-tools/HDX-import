using System;using System.IO;using System.Diagnostics;using System.Reflection;using System.Threading;
class DelayedIso{
static int Main(string[] args){if(args.Length==2&&args[0]=="--writer"){Thread.Sleep(1200);using(var f=new FileStream(args[1],FileMode.Create,FileAccess.Write,FileShare.Read))using(var w=new StreamWriter(f)){w.Write("000001\tSE-020201-1234\t");w.Flush();Thread.Sleep(1400);w.Write("752002020112349\nZN\n");}return 0;}
string output=args[Array.IndexOf(args,"-o")+1];Process.Start(new ProcessStartInfo(Assembly.GetExecutingAssembly().Location,"--writer \""+output+"\""){UseShellExecute=false,CreateNoWindow=true});return 0;}}

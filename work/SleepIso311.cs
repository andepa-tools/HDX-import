using System;using System.IO;using System.Diagnostics;using System.Threading;
class SleepIso311{static int Main(string[] args){string output=args[Array.IndexOf(args,"-o")+1];File.WriteAllText(Path.Combine(Path.GetDirectoryName(output),"pid.txt"),Process.GetCurrentProcess().Id.ToString());Thread.Sleep(60000);return 0;}}

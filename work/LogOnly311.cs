using System;using System.IO;
class LogOnly311{static int Main(string[] args){File.WriteAllText(args[Array.IndexOf(args,"-l")+1],"mock log only");return 0;}}

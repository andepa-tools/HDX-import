using System;using System.IO;using System.Reflection;
class AccessTests{
 static void Check(bool ok,string msg){if(!ok)throw new Exception(msg);}
 static bool Fails(Action action){try{action();return false;}catch(IOException e){return e.Message.Contains("E012:");}}
 static int Main(){try{
 Check(ManualEdition.Version=="1.6"&&ManualEdition.RunsRoot==@"C:\DelPro\ISO\HDXImport\Runs","version/root");
 string root=Path.GetFullPath("work/access-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);string req=Path.Combine(root,"request.txt"),o=Path.Combine(root,"output.txt"),l=Path.Combine(root,"log.txt");Logic.Write(req,Logic.Request());
 ManualIsoFiles.Check(req,o,l);Check(!File.Exists(o)&&!File.Exists(l),"probes removed before launch");
 File.WriteAllText(o,"preserve");Check(Fails(()=>ManualIsoFiles.Check(req,o,l))&&File.ReadAllText(o)=="preserve","existing output protected");File.Delete(o);
 using(var f=new FileStream(req,FileMode.Open,FileAccess.Read,FileShare.None)){Check(Fails(()=>ManualIsoFiles.Check(req,o,l)),"locked request");}
 Check(Fails(()=>ManualIsoFiles.Check(req,Path.Combine(root,"absent","o.txt"),l)),"missing destination");
 File.WriteAllText(req,"");Check(Fails(()=>ManualIsoFiles.Check(req,o,l)),"empty request");File.Delete(req);Check(Fails(()=>ManualIsoFiles.Check(req,o,l)),"missing request");
 Check(ManualIsoFiles.Describe(o).Contains("saknas"),"missing diagnostic");File.WriteAllText(o,"");Check(ManualIsoFiles.Describe(o).Contains("tom"),"empty diagnostic");
 using(var f=new FileStream(o,FileMode.Open,FileAccess.ReadWrite,FileShare.None)){Check(ManualIsoFiles.Describe(o).Contains("kunde inte"),"locked diagnostic");}
 Check(SharedHelp.MissingResponseHelp(o,l,0).Contains("inte bekräftad"),"no false external-access guarantee");
 Logic.Test();Console.WriteLine("PASS: version/root, preflight success, existing output preserved, locked/missing/empty request, inaccessible destination, detailed response diagnostics, checksum rules.");return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}

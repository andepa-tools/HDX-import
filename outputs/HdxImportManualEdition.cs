using System;
using System.IO;
using System.Text;
using System.Reflection;
[assembly: AssemblyVersion("1.6.0.0")]
[assembly: AssemblyFileVersion("1.6.0.0")]
[assembly: AssemblyInformationalVersion("1.6")]
[assembly: AssemblyProduct("HDX Import Manual")]
[assembly: AssemblyTitle("HDX Import Manual")]
public static class ManualEdition {
 public const string Name="HDX Import Manual";
 public static string Version {get{var values=typeof(ManualEdition).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute),false);return values.Length>0?((AssemblyInformationalVersionAttribute)values[0]).InformationalVersion:"";}}
#if STORE_PACKAGE
 const string Channel="Utgivare: AndePa\r\nDistributionskanal: Microsoft Store\r\nUppdateringar hanteras av Microsoft Store.\r\n\r\n";
#else
 const string Channel="Testversion. Inte installerad via Microsoft Store.\r\n\r\n";
#endif
#if MANUAL_LICENSING
 const string Terms="Tre verifierade importer gratis. Obegränsad användning med Store-köp eller signerad gårdslicens.\r\n";
#else
 const string Terms="Gratis och utan begränsning av antalet importer.\r\n";
#endif
 public static string About {get{return Name+" "+Version+"\r\n"+Channel+"Manuell import till DelPro. Svenska SE-ID och landskod 752.\r\n"+Terms+"Ingen schemaläggning. Befintliga transpondernummer lämnas orörda.";}}
 public static string DataRoot {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"HDXimportManuell","DelProHdx");}}
 public static string RunsRoot {get{return @"C:\DelPro\ISO\HDXImport\Runs";}}
 public static string ConfigPath {get{return Path.Combine(DataRoot,"HDXimport.ini");}}
 public static void SaveIso(string path){Directory.CreateDirectory(DataRoot);File.WriteAllText(ConfigPath,"IsoProcessor="+path+Environment.NewLine,Encoding.UTF8);}
}


public static class ManualIsoFiles {
 public static string CreateRun(){
  string path=Path.Combine(ManualEdition.RunsRoot,DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")+"_"+Guid.NewGuid().ToString("N"));
  try{Directory.CreateDirectory(path);return path;}
  catch(Exception e){throw new IOException("E012: Kan inte skapa ISO-arbetsmappen: "+path+". Kontrollera mappens åtkomst för Windows-användaren. Ingen ISO-process startad. "+e.Message,e);}
 }
 public static void Check(string input,string output,string log){
  try{
   using(var f=new FileStream(input,FileMode.Open,FileAccess.Read,FileShare.Read)){if(f.Length==0)throw new IOException("Begäran är tom.");}
   foreach(string target in new[]{output,log}){
    // CreateNew prevents overwriting any previous response. Remove only our own probe.
    using(var f=new FileStream(target,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None)){f.WriteByte(65);f.Flush();f.Position=0;if(f.ReadByte()!=65)throw new IOException("Åtkomstprovet kunde inte läsas.");}
    File.Delete(target);
   }
  }catch(Exception e){throw new IOException("E012: Filåtkomstkontrollen misslyckades. Ingen ISO-process startad.\r\nBegäran: "+input+"\r\nSvar: "+output+"\r\nLogg: "+log+"\r\nKontrollera läs- och skrivrättigheter. Befintliga svar/loggar skrivs inte över.\r\n"+e.Message,e);}
 }
 public static string Describe(string path){
  try{using(var f=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite)){return path+": "+(f.Length==0?"tom":"finns, "+f.Length+" byte (kan fortfarande vara låst eller ofärdig)");}}
  catch(FileNotFoundException){return path+": saknas";}catch(DirectoryNotFoundException){return path+": mappen saknas";}
  catch(UnauthorizedAccessException e){return path+": åtkomst nekad. "+e.Message;}
  catch(IOException e){return path+": kunde inte öppnas (låst eller annat filfel). "+e.Message;}
 }
 public static string Missing(string output,string log,int code){return "E009: Ingen färdig svarsfil kunde läsas inom 30 sekunder.\r\n"+Describe(output)+"\r\n"+Describe(log)+"\r\nISO-processorns avslutningskod: "+code+" (kod 0 bevisar inte att begäran behandlats).\r\nAppens filåtkomst kontrollerades före start. ISO-processorns åtkomst är inte bekräftad om svaret saknas.\r\nKontrollera att ISO-processorn kan läsa och skriva i körningsmappen, att rätt iso32.exe valts och att Använd kommandorad ISO är sparat i DelPro. Granska loggen om den finns. Läs om uppgifterna före ett nytt importförsök.";}
}

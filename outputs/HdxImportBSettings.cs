using System;using System.Linq;
public static class BTransponderSettings {
 public const string Key="OverwriteBTransponders";
 static bool IsSetting(string raw){string line=raw.Trim();if(line.StartsWith("#"))return false;int eq=line.IndexOf('=');return eq>=0&&line.Substring(0,eq).Trim()==Key;}
 public static bool Read(string config){bool found=false,result=false;foreach(string line in config.Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries).Where(IsSetting)){if(found||!bool.TryParse(line.Substring(line.IndexOf('=')+1).Trim(),out result))throw new Exception("Ogiltig inställning för B-transpondrar. Använd OverwriteBTransponders=true eller false.");found=true;}return result;}
 public static string With(string config,bool overwrite){var lines=config.Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries).Where(x=>!IsSetting(x)).ToList();lines.Add(Key+"="+(overwrite?"true":"false"));return string.Join("\r\n",lines)+"\r\n";}
}

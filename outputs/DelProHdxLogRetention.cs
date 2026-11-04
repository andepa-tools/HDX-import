using System;using System.IO;using System.Linq;
public static class LogRetention {
 public static readonly string[] KnownFiles={"request.txt","before.txt","fresh.txt","after.txt","read_log.txt","refresh_log.txt","verify_log.txt","review.txt","automatic_log.txt","import.txt","import_output.txt","import_log.txt","verification.txt","output.txt","log.txt","before_log.txt","after_log.txt","preview_source.txt"};
 public static int Clean(string root,DateTime utcNow){
  root=Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar);
#if STORE_MANUAL
  if(!string.Equals(root,ManualEdition.RunsRoot,StringComparison.OrdinalIgnoreCase))throw new Exception("Otillåten loggmapp.");
#else
  if(Path.GetFileName(root)!="Runs"||Path.GetFileName(Path.GetDirectoryName(root))!="DelProHdx")throw new Exception("Otillåten loggmapp.");
#endif
  if(!Directory.Exists(root))return 0;if((File.GetAttributes(root)&FileAttributes.ReparsePoint)!=0)return 0;
  DateTime cutoff=utcNow.AddDays(-30);int removed=0;
  foreach(string directory in Directory.GetDirectories(root)){
   string target=Path.GetFullPath(directory);
   if(!target.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))continue;
#if STORE_MANUAL
   if(!System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(target),@"^[0-9]{8}_[0-9]{6}_[0-9]{3}_[0-9a-f]{32}$"))continue;
#else
   if(!System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(target),@"^[0-9]{8}_[0-9]{6}_[0-9]{3}(?:_(?:auto|scheduled))?$"))continue;
#endif
   if((File.GetAttributes(target)&FileAttributes.ReparsePoint)!=0||Directory.GetLastWriteTimeUtc(target)>=cutoff||Directory.GetDirectories(target).Length>0)continue;
   var files=Directory.GetFiles(target);
   if(files.Any(f=>!KnownFiles.Contains(Path.GetFileName(f)) || File.GetLastWriteTimeUtc(f)>=cutoff || (File.GetAttributes(f)&FileAttributes.ReparsePoint)!=0))continue;
   foreach(string file in files)File.Delete(file);Directory.Delete(target,false);removed++;
  }
  return removed;
 }
}

"""Kör endast simulerade tester. Anropar aldrig en installerad ISO-processor."""
from pathlib import Path
import subprocess
import sys

root = Path(__file__).resolve().parent.parent
if Path.cwd() != root:
    sys.exit('Kör från projektroten: python work/run_tests.py')
csc = Path(r'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe')
sources = [str(p) for p in sorted((root/'outputs').glob('*.cs'))]
common = [str(csc), '/nologo', '/target:exe', '/platform:x64',
          '/r:System.Windows.Forms.dll', '/r:System.Drawing.dll', '/r:System.Core.dll',
          '/resource:'+str(root/'outputs/HDXimport.ico')+',AppIcon',
          '/resource:'+str(root/'outputs/HDXimport-logo.png')+',AppLogo',
          '/resource:'+str(root/'outputs/bilder/kommandorad-1-installning.png')+',CmdSetting',
          '/resource:'+str(root/'outputs/bilder/kommandorad-2-utrustningstrad.png')+',CmdTree']
def compile_one(main, extra, target, defines):
    subprocess.run(common + ['/main:'+main, '/define:'+defines, '/out:'+str(target)] + sources + [str(root / p) for p in extra], check=True)
try:
    (root/'work/test311-bin').mkdir(exist_ok=True)
    for main, file, name in [('MockB','MockB.cs','MockB'), ('DelayedIso','DelayedIso.cs','DelayedIso'), ('MockNoResponse','MockNoResponse.cs','MockNoResponse311'), ('SleepIso311','SleepIso311.cs','SleepIso311'), ('LogOnly311','LogOnly311.cs','LogOnly311')]:
        compile_one(main, ['work/'+file], root/'work/test311-bin'/(name+'.exe'), 'STORE_MANUAL')
    for suffix, defines in [('Test', 'STORE_MANUAL'), ('Store', 'STORE_MANUAL,STORE_PACKAGE')]:
        for main, file in [('AccessTests', 'AccessTests.cs'), ('BTests', 'ManualBTests.cs'), ('DisplayTests', 'DisplayTests.cs'),
                           ('Round3Tests','Round3Tests.cs'), ('Round2Tests','Round2Tests.cs'),
                           ('Review311Tests','Review311Tests.cs'), ('Ui311Tests','Ui311Tests.cs'),
                           ('Retry311Tests','Retry311Tests.cs'), ('Visual311Tests','Visual311Tests.cs'),
                           ('Response311Tests','Response311Tests.cs'), ('WaitTests','WaitTestsFinal.cs')]:
            target = root/'work'/(suffix+main+'.exe')
            compile_one(main, ['work/'+file], target, defines)
            print(suffix+': '+main, flush=True)
            subprocess.run([str(target)], check=True, timeout=150)
except (subprocess.CalledProcessError, subprocess.TimeoutExpired) as e:
    sys.exit(str(e))
print('PASS: samtliga simulerade tester för Test och Store')

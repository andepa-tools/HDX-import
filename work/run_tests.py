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
          '/resource:'+str(root/'outputs/HDXimport-logo.png')+',AppLogo']
def compile_one(main, extra, target, defines):
    subprocess.run(common + ['/main:'+main, '/define:'+defines, '/out:'+str(target)] + sources + [str(root / p) for p in extra], check=True)
try:
    compile_one('MockB', ['work/MockB.cs'], root/'work/MockB.exe', 'STORE_MANUAL')
    for suffix, defines in [('Test', 'STORE_MANUAL'), ('Store', 'STORE_MANUAL,STORE_PACKAGE')]:
        for main, file in [('AccessTests', 'AccessTests.cs'), ('BTests', 'ManualBTests.cs'), ('DisplayTests', 'DisplayTests.cs')]:
            target = root/'work'/(suffix+main+'.exe')
            compile_one(main, ['work/'+file], target, defines)
            subprocess.run([str(target)], check=True, timeout=120)
except (subprocess.CalledProcessError, subprocess.TimeoutExpired) as e:
    sys.exit(str(e))
print('PASS: samtliga simulerade tester för Test och Store')

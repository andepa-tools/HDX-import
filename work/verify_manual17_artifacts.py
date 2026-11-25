"""Compare a fresh build with preserved local 1.7 deliveries; never modifies originals."""
from pathlib import Path
import subprocess,struct,hashlib,zipfile,re
import sys
if len(sys.argv)!=2: sys.exit('Usage: python work/verify_manual17_artifacts.py ORIGINAL_PROJECT_ROOT')
r=Path(sys.argv[1]).resolve(); dest=Path(__file__).resolve().parent.parent
pairs=[(r/'outputs/HDX-Import-Manual-Test-1.7.exe',dest/'outputs/HDX-Import-Manual-Test-1.7.exe'),(r/'work/store-manual-1.7/HDXImportManual.exe',dest/'work/store-manual-1.7/HDXImportManual.exe')]
def normalized(p):
 b=bytearray(p.read_bytes());pe=struct.unpack_from('<I',b,0x3c)[0];b[pe+8:pe+12]=bytes(4)
 cmd="[Reflection.Assembly]::ReflectionOnlyLoadFrom('"+str(p).replace("'","''")+"').ManifestModule.ModuleVersionId.ToString()"
 import uuid
 guid=uuid.UUID(subprocess.check_output(['powershell','-NoProfile','-Command',cmd],text=True).strip()).bytes_le
 assert b.count(guid)==1
 assert len(re.findall(rb"<PrivateImplementationDetails>\{[0-9A-F-]{36}\}",b))==1
 b=b.replace(guid,bytes(16))
 return re.sub(rb"(<PrivateImplementationDetails>\{)[0-9A-F-]{36}(\})",lambda m:m[1]+b"0"*36+m[2],b)
expected={'HDX-Import-Manual-Test-1.7.exe':'7006b511ce2967f183c9fb6fb7f7e7192fe61ee44fdfd5c8d5022bb94107f690','HDX-Import-Manual-1.7.0.0-x64.msix':'6faa389efb762779a09b284418a9ee7b2ed215ccf54cc98779daad32e8433634'}
for name,digest in expected.items():
 assert hashlib.sha256((r/'outputs'/name).read_bytes()).hexdigest()==digest, name
print('PASS: original delivery SHA256 checksums')
with zipfile.ZipFile(r/'outputs/HDX-Import-Manual-1.7.0.0-x64.msix') as z:
 assert z.read('HDXImportManual.exe')==pairs[1][0].read_bytes()
print('PASS: original package embeds exact original Store EXE')
for old,new in pairs:
 a,b=normalized(old),normalized(new)
 print(old.name,len(a),len(b),'equal after PE timestamp, MVID and generated PrivateImplementationDetails GUID normalization:',a==b)
 if a!=b:print('first differences',[(i,a[i],b[i]) for i in range(min(len(a),len(b))) if a[i]!=b[i]][:30])
 assert a==b
 print('normalized SHA256:',hashlib.sha256(a).hexdigest())

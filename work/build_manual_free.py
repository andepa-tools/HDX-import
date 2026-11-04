# Bygger HDX Import Manual utan provgräns och köp.
# Kör från projektmappen:
#   python work/build_manual_free.py
# Versionen läses från outputs\HdxImportManualEdition.cs.
# Skriptet skriver aldrig över ett befintligt MSIX-paket och rör inte work\store-manual-1.0.
import hashlib
import os
import re
import shutil
import subprocess
import sys
from pathlib import Path

root = Path.cwd()
out = root / "outputs"
work = root / "work"
csc = Path(r"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe")
makeappx = work / "windows-sdk-buildtools-26100" / "bin" / "10.0.26100.0" / "x64" / "makeappx.exe"
if os.environ.get("MAKEAPPX"):
    makeappx = Path(os.environ["MAKEAPPX"])
elif not makeappx.exists():
    found = shutil.which("makeappx.exe")
    if found:
        makeappx = Path(found)
    else:
        sdk = Path(os.environ.get("ProgramFiles(x86)", r"C:\Program Files (x86)")) / "Windows Kits/10/bin"
        candidates = sorted(sdk.glob("*/x64/makeappx.exe"), key=lambda p: tuple(int(n) for n in p.parent.parent.name.split(".")))
        if candidates:
            makeappx = candidates[-1]
base = work / "store-manual-1.0"
manifest = work / "store-manual-asInvoker.manifest"

SOURCES = ["HdxImportSv.cs", "HdxImportSharedUi.cs", "DelProHdxRunGate.cs",
           "DelProHdxLogRetention.cs", "HdxImportManualEdition.cs", "HdxImportBTransponders.cs", "HdxImportBSettings.cs"]


def fail(message):
    print("FEL: " + message)
    sys.exit(1)


for required in [csc, makeappx, base / "AppxManifest.xml", manifest, out / "HDXimport.ico",
                 out / "HDXimport-logo.png"] + [out / s for s in SOURCES]:
    if not required.exists():
        fail("Saknar " + str(required) + ". Kör skriptet från projektmappen.")

edition = (out / "HdxImportManualEdition.cs").read_text(encoding="utf-8-sig")
m = re.search(r'AssemblyVersion\("(\d+)\.(\d+)\.(\d+)\.(\d+)"\)', edition)
if not m:
    fail("Hittar inte AssemblyVersion i HdxImportManualEdition.cs.")
version4 = ".".join(m.groups())
short = m.group(1) + "." + m.group(2)
if (int(m.group(1)), int(m.group(2))) <= (1, 1):
    fail("Versionen måste vara högre än 1.1 (1.0 är publicerad, 1.1 är den parkerade licensversionen).")

msix = out / ("HDX-Import-Manual-" + version4 + "-x64.msix")
test_exe = out / ("HDX-Import-Manual-Test-" + short + ".exe")
stage = work / ("store-manual-" + short)
for existing in [msix, test_exe]:
    if existing.exists():
        fail(existing.name + " finns redan. Höj versionen i HdxImportManualEdition.cs i stället för att bygga om.")


def compile_exe(defines, target):
    args = [str(csc), "/nologo", "/target:winexe", "/platform:x64", "/optimize+",
            "/main:CombinedEntry", "/define:" + defines, "/out:" + str(target),
            "/win32icon:" + str(out / "HDXimport.ico"), "/win32manifest:" + str(manifest),
            "/resource:" + str(out / "HDXimport.ico") + ",AppIcon",
            "/resource:" + str(out / "HDXimport-logo.png") + ",AppLogo",
            "/r:System.Windows.Forms.dll", "/r:System.Drawing.dll", "/r:System.Core.dll"]
    args += [str(out / s) for s in SOURCES]
    result = subprocess.run(args, capture_output=True, text=True)
    if result.returncode != 0:
        print(result.stdout + result.stderr)
        fail("Kompileringen misslyckades (" + defines + ").")
    data = target.read_bytes()
    # Säkerställ att ingen licenskod följt med.
    for marker in ["hdx-import-unlimited", "Gratisimporter kvar", "Licens / köp"]:
        if marker.encode("utf-16-le") in data:
            fail(target.name + " innehåller licenskod (" + marker + ").")


print("Version " + version4 + " (gratis, utan provgräns)")
compile_exe("STORE_MANUAL", test_exe)
print("Test-EXE: " + str(test_exe))

if stage.exists():
    if stage.resolve() == base.resolve():
        fail("Staging får inte vara 1.0-mappen.")
    if stage.resolve().parent != work.resolve():
        fail("Staging ligger utanför work.")
    allowed={"AppxManifest.xml", "HDXImportManual.exe", "HDXImportManual.exe.config", "StoreLogo.png", "Square44x44Logo.png", "Square150x150Logo.png"}
    if any(f.name not in allowed for f in stage.rglob("*") if f.is_file()):
        fail("Staging har oväntade filer. Bevara och undersök dem.")
shutil.copytree(base, stage, dirs_exist_ok=True, ignore=shutil.ignore_patterns("HDXImportManual.exe"))
compile_exe("STORE_MANUAL,STORE_PACKAGE", stage / "HDXImportManual.exe")

appx = stage / "AppxManifest.xml"
text = appx.read_text(encoding="utf-8-sig")
new_text, count = re.subn(r'(<Identity\b[^>]*?\bVersion=")[^"]+(")', r"\g<1>" + version4 + r"\g<2>", text)
if count != 1 or "AndePa.HDXImportManual" not in new_text:
    fail("Kunde inte uppdatera versionen i AppxManifest.xml.")
appx.write_text(new_text, encoding="utf-8")

log = work / ("store-manual-" + short + "-pack.log")
result = subprocess.run([str(makeappx), "pack", "/d", str(stage), "/p", str(msix), "/o"],
                        capture_output=True, text=True)
log.write_text(result.stdout + result.stderr, encoding="utf-8")
if result.returncode != 0:
    fail("MakeAppx misslyckades. Se " + str(log))
print("MSIX: " + str(msix))

lines = []
for f in [test_exe, msix]:
    lines.append(hashlib.sha256(f.read_bytes()).hexdigest() + "  " + f.name)
sha = out / ("HDX-Import-Manual-" + short + "-SHA256.txt")
sha.write_text("\r\n".join(lines) + "\r\n", encoding="ascii")
print("Kontrollsummor: " + str(sha))
print("KLART")

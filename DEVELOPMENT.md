# Utveckling av HDX Import Manual

Detta repo innehåller nu utvecklingsunderlag för **Manual 1.6**. Fullversion 3.10 har separat versionsserie och dess kompletta källkod och installationsprojekt ingår inte i denna överföring. Befintliga releaser bevaras.

## Kom igång
1. Hämta repot och läs AGENTS.md, STATUS.md och DECISIONS.md. Claude läser också CLAUDE.md.
2. Använd Windows x64 med .NET Framework 4.8 och Python 3. Bygget använder Windows-kompilatorn `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`.
3. Installera Windows SDK med MakeAppx för MSIX-bygge. Skriptet söker i Windows Kits/10/bin, PATH och den äldre lokala SDK-mappen. Sätt miljövariabeln MAKEAPPX till full sökväg vid behov. SDK och Microsofts ISO-processor distribueras inte här.
4. Kör från projektroten:

```powershell
python work/run_tests.py
python work/build_manual_free.py
```

Testerna använder en lokal simulerad ISO-processor. De ska inte ansluta till DelPro eller ändra verkliga djurdata. Windows Forms-fönster kan visas kort under testerna.

Bygget skapar en fristående test-EXE, ett osignerat MSIX-paket och SHA256-kontrollsummor i outputs. Inget skickas till Store. Befintliga EXE/MSIX skrivs inte över; använd en ren arbetskopia vid verifiering av samma version. Höj versionen i HdxImportManualEdition.cs endast när en ny version avses.

## Filer
| Plats | Innehåll |
| --- | --- |
| outputs/*.cs | Befintlig Manual-källkod, bevarad från det lokala 1.6-underlaget |
| outputs/HDXimport.ico och HDXimport-logo.png | Appens befintliga grafik |
| work/build_manual_free.py | EXE- och MSIX-bygge |
| work/run_tests.py | Kompilerar och kör tester för Test och Store |
| work/AccessTests.cs | Filåtkomst och E009/E012-diagnostik |
| work/ManualBTests.cs och MockB.cs | Simulerad import, B-val, färska uppgifter och återläsning |
| work/DisplayTests.cs | Visning av ISO/B och säkert standardval |
| work/store-manual-1.0 | Paketeringsmall och grafik; namnet är historiskt, Identity-versionen uppdateras av byggskriptet |
| docs/VERIFICATION-1.6.md | Historisk rapport med användarbekräftade tester och begränsningar |

## Avgränsningar
Manual byggs med STORE_MANUAL och Store-paketet även med STORE_PACKAGE. MANUAL_LICENSING används inte. Automatikkoden för fullversionen saknas; bygg inte utan STORE_MANUAL och anta inte att detta underlag kan återskapa fullversion 3.10.

Incheckad SHA256-fil avser de ursprungliga 1.6-leveranserna. Ett nytt bygge kan skilja sig binärt, bland annat genom kompilatorns tidsstämplar. Det är inte en verifiering av den redan inskickade Store-filen.

Tester bevisar inte faktisk DelPro-kommunikation, Store-distribution eller lyckad verklig import. Läsning på DelPro-datorn är användarbekräftad; verklig import är inte bekräftad som testad.

## Samarbete
Ta en avgränsad GitHub-uppgift, arbeta på en egen gren och lämna en pull request med ändring, motiv och kontrollresultat. Läs öppna pull requests före arbete i samma filer. Uppdatera STATUS.md vid överlämning och DECISIONS.md vid nya beslut. Läs alltid om aktuell GitHub-status när du byter assistent.

En assistent behöver egen åtkomst till GitHub genom sin tjänst eller en lokal klon. Dessa instruktioner ger inte automatiskt en AI skrivbehörighet. Publicering till Store och ändringar av verkliga DelPro-data kräver ett särskilt uppdrag.

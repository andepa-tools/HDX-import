# Manual 1.7 – verifierad källkodsöverföring

Datum: 2026-11-25. Windows x64, Python 3.12 och .NET Framework-kompilatorn csc v4.0.30319. Ursprung: användarens lokala projekt, tagg `manual-1.7.0.0`, commit `9dba65b`. GitHub-bas: main `4ffc29662e51fd7e483f2fee2512e5092415e2ce`. Ingen produktkod har skapats på nytt eller ändrats i överföringen.

## Källor
Samtliga sju produktkällor, originaltester och de två guidebilderna överfördes byteidentiskt från taggen. SHA256 för produktkällorna före Git-hantering av radslut:

| Fil i outputs | SHA256 |
|---|---|
| HdxImportSv.cs | `829e4c14066a6edec8ac46a6f8dcebea6e499f4f0f53b546fa907d4c8f9476b0` |
| HdxImportSharedUi.cs | `f4f02a829b6f5d0cae0155b63b53aa03317d30c03d548e53118ff710f5f41a78` |
| DelProHdxRunGate.cs | `962b8ac0540c8ef98f72a9dfb9007d5d3a863efdf9593e53114a4a62b80e1e5a` |
| DelProHdxLogRetention.cs | `4362ffdbdc2f49b28755f980ac06d261700ae1df68e65a4d3159dbb7eb004243` |
| HdxImportManualEdition.cs | `dfe9c83e114842c930729d0c00b0db285bbc5d5036e35e31ebf38e3b9535cef9` |
| HdxImportBTransponders.cs | `ec30456661541a6b46f9b89ffd7543ba12dd257d903778a41eca2c038e1df8d9` |
| HdxImportBSettings.cs | `21cbd2c564720fd69d5e63dbbf1c23972df18fb6e75b911b7ce31a422030f725` |

## Koppling till bevarade leveranser
Originalets SHA256 verifierades utan att originalfilerna ändrades:

- Test-EXE: `7006b511ce2967f183c9fb6fb7f7e7192fe61ee44fdfd5c8d5022bb94107f690`.
- MSIX: `6faa389efb762779a09b284418a9ee7b2ed215ccf54cc98779daad32e8433634`.

Original-MSIX innehåller byteidentiskt den bevarade Store-EXE:n. `python work/build_manual_free.py` lyckades i en separat ren arbetskopia, med MAKEAPPX satt till den befintliga lokala SDK-installationen. Ett nytt osignerat MSIX skapades med MakeAppx-validering; originalleveranserna skrevs aldrig över.

`python work/verify_manual17_artifacts.py ..` lyckades: hela Test- och Store-EXE:n är identiska med originalen efter att endast PE-tidsstämpeln, modulens MVID och kompilatorns GUID i PrivateImplementationDetails-namnet nollställts. Det omfattar även IL-kod, övrig metadata och inbäddade resurser. Inga andra byte ignoreras. Storlekar: 1 794 048 respektive 1 794 560 byte. Normaliserade SHA256:

- Test: `15c4058d068fd22cb3518d3b5373d343cf1f0eb726cf95fdf9e89f8b37a46397`.
- Store: `f3b13ad2b6bc84107d288ca5c597c981fa99279438837005dc1249b1ff800b00`.

Användarens tidigare bekräftelse är källan för att Manual 1.7 har skickats till Store. Ingen direkt kontroll av Partner Centers uppladdade fil eller publiceringsstatus har gjorts. Verifieringen knyter källan till de bevarade lokala leveranserna; den bevisar inte Store-uppladdning eller funktion mot verklig DelPro.

## Överföringens avgränsning
Byggskriptet behåller main-grenens SDK-sökning och lägger till 1.7-guidens två bildresurser. Testskriptet kör endast Manual-definitionerna med lokala mockprogram. De gemensamma originalkällorna innehåller även villkorligt utesluten fullversionskod, men fullversionens kompletta underlag överförs inte. EXE, MSIX, SDK, certifikat, nycklar, testutdata och verkliga gårds-/djurdata ingår inte. Originaltesternas syntetiska exempeldata är bevarade.

## Testresultat
`python work/run_tests.py`: GODKÄND (exitkod 0) på Windows med vanlig lokal skrivbordsåtkomst. Elva testgrupper för vardera Test och Store, totalt 22 godkända körningar: AccessTests, BTests, DisplayTests, Round3Tests, Round2Tests, Review311Tests, Ui311Tests, Retry311Tests, Visual311Tests, Response311Tests och WaitTests.

Kontrollerna omfattar bland annat ISO-skydd, B-behåll/ersätt/avbryt, färska uppgifter, återläsning, filåtkomst, sortering/färggrupper, knappar, layout, DelPro-guide, anslutningstest, avbrytning av egen mockprocess, E009, återförsök och fördröjda/låsta svarsfiler. Inga riktiga ISO-program eller verkliga DelPro-data användes.

Den första körningen med begränsad skrivbordsåtkomst stannade efter att Review311Tests skrivit PASS; processen avslutades inte inom 150 sekunder. Hela sviten kördes därefter om med vanlig Windows-åtkomst och passerade. Originaltesterna ändrades inte för att kringgå stoppet.

Slutlig filgranskning: 35 relevanta filer i ändringen, inga genererade program/paket, certifikat, nycklar eller testutdata. Nyckel-/GitHub-tokenmarkörer kontrollerade utan träff. Guidebilderna granskade: endast DelPro-inställningar/utrustningsträd, inga gårds- eller djuruppgifter. Git-diffkontroll godkänd med CR-radslut och originaltestets befintliga tomma slutrad tillåtna, för att bevara originalbytes. Fjärrkörning i GitHub Actions redovisas separat i PR:ns kontroller.

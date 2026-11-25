# Status – HDX import

Senast uppdaterad: 2026-11-04 av ChatGPT/Codex.

## Verifierat utgångsläge
- Repo: [andepa-tools/HDX-import](https://github.com/andepa-tools/HDX-import), offentligt.
- Huvudgren: main. Inventerad commit: `443233b3c0e397e193efb58aff5ad0d59caf7150`.
- Vid inventeringen innehöll main endast README.md. Ingen källkod, byggkonfiguration, testsvit eller AI-instruktionsfil fanns i filträdet.
- README beskriver ett Windows-program för att komplettera saknade ISO-transpondernummer i DelPro och anger repots roll som distribution och dokumentation.
- Publicerade releaser vid inventeringen: v3.8, v3.9 och v3.10. Senast publicerad var [v3.10](https://github.com/andepa-tools/HDX-import/releases/tag/v3.10), 2026-10-31, med installationsfiler, PDF-guide och SHA256-fil.
- Releasetexterna har lästs. Installationsfiler har inte körts och PDF-guiderna har inte granskats.

## Pågående överlämning
Uppgift: skapa gemensam dokumentation för ChatGPT och Claude.
Gren: `docs/ai-collaboration-2026-11-04`.
Leverans: utkast till pull request mot main med AGENTS.md, CLAUDE.md, STATUS.md och DECISIONS.md.
README och befintliga releaser lämnas orörda. Filerna är ett förslag tills pull requesten har slagits samman.

## Kontroller
Kontrollera i pull requestens diff att endast de fyra nya Markdown-filerna tillkommer och att README är oförändrad. Ingen programtestning ingår i denna dokumentationsändring.

## Nästa steg
1. Granska och ta ställning till samarbetsfilerna.
2. Lokalisera originalkällkod, installationsprojekt och bygginstruktioner med användaren innan utveckling planeras. Plats och eventuell separat lagring är ännu okända.
3. Verifiera Claude-åtkomst till repot och låt nästa assistent läsa dessa filer. Åtkomsten är ännu inte verifierad.

## Mall för nästa överlämning
- Datum och assistent:
- Uppgift och gren/pull request:
- Genomfört:
- Kontroller och resultat:
- Hinder eller öppna frågor:
- Nästa konkreta steg:

## Aktuell överlämning – 2026-11-04
Detta avsnitt ersätter de tidigare nästa stegen om att lokalisera Manual-källkoden. Den ursprungliga inventeringen ovan bevaras som historik.

- Manual/Store 1.6 och fullversion 3.10 är separata versionsspår.
- Användaren bekräftar att **Manual 1.6 är inskickad och väntar på publicering**. Faktisk publicering och Store-uppdatering på kunddator är ännu inte bekräftade. Partner Center har inte kontrollerats direkt i denna uppgift.
- Manual-källkod, grafik, paketeringsmall, byggskript och simulerade tester har lokaliserats och ingår i denna överföring. Se [DEVELOPMENT.md](DEVELOPMENT.md).
- Befintlig C#-programkod har överförts utan ändring. Byggskriptet har kompletterats med sökning efter MakeAppx och ett gemensamt testskript har lagts till.
- Säkerhets- och importtester med simulerad ISO gick igenom för både Test- och Store-definitionerna. Visningstester gick också igenom. EXE och osignerat MSIX 1.6.0.0 kunde byggas från det förberedda underlaget. Inget skrevs till verkliga DelPro-data.
- Tidigare användarbekräftad läsning med test-EXE och lokalt signerad MSIX på tidigare felande DelPro-dator finns beskriven i [verifieringsrapporten](docs/VERIFICATION-1.6.md). Verklig import är fortfarande inte bekräftad som testad.
- Binärer, SDK, privata nycklar, certifikat och lokala körningsdata ingår inte i källkodsöverföringen.
- Den kompletta källkoden och installationsprojektet för fullversion 3.10 återstår att inventera. Manual-underlaget ska inte användas för att bygga fullversionen.
- GitHub ger gemensamt underlag; Claude och andra assistenter behöver anslutas med egen GitHub-åtkomst eller hämta repot. Deras anslutning har inte verifierats här.

### Fortsätt här
1. Läs AGENTS.md och DEVELOPMENT.md; hämta aktuell huvudgren och kontrollera öppna pull requests.
2. Välj en avgränsad uppgift och kör simulerade tester innan ändringar lämnas för granskning.
3. Uppdatera publiceringsläget när användaren bekräftar att Store 1.6 är publicerad.
4. Inventera fullversionens separata utvecklingsunderlag när arbete med den beställs.

Överföringen levereras genom pull request #1. Kontrollera dess GitHub-status för att avgöra om den är införd på main. Automatisk kontroll finns i .github/workflows/manual.yml; lokala kontroller ovan är genomförda, fjärrkörningens resultat måste läsas separat.

## Bekräftat införande – 2026-11-04
Pull request #1 är sammanfogad med main (mergecommit `019a2b89424542c27b3157596181f03875746b64`). [GitHubs Windows-kontroll](https://github.com/andepa-tools/HDX-import/actions/runs/41803055311) passerade både samtliga simulerade tester och EXE/MSIX-bygget från en ny hämtning. Manual-underlaget är därmed tillgängligt på huvudgrenen. Uppgiften att införa samarbetsunderlaget är klar. Store-publicering och andra assistenters egna anslutningar ingår inte i denna verifiering.

## Publiceringsstatus – 2026-11-04
Användaren bekräftar att **HDX Import Manual 1.6 är publicerad**. Detta ersätter tidigare uppgifter ovan om att versionen väntar på publicering. Uppgiften bygger på användarens bekräftelse; Store-sidan och uppdatering på kunddator har inte kontrollerats direkt i denna uppdatering. Fullversion 3.10 har fortsatt separat versionsserie.

## Källkodsöverföring Manual 1.7 – 2026-11-25
- Uppdrag: överför den Store-inskickade Manual 1.7-källan för fortsatt UI-arbete. Gren: `codex/manual-1.7-source`, från main `4ffc29662e51fd7e483f2fee2512e5092415e2ce`.
- Faktisk läs-/skrivåtkomst till originalprojektet på användarens Windows-dator verifierad. De sju produktkällorna och relevanta originaltester hämtades byteidentiskt från lokal tagg `manual-1.7.0.0` (`9dba65b`). Originalprojektets pågående 3.12-arbete lämnas orört.
- Öppna PR:er kontrollerade före ändring: #2, `codex/release-3.11-handoff`, ändrar README och STATUS. Den lämnas öppen och orörd. Denna PR kompletterar samma dokumentation; läs båda och bevara respektive historik vid senare sammanslagning.
- Manual 1.7 är inskickad till Store enligt användarens tidigare uppgift. Partner Center har inte kontrollerats. Lokal original-MSIX har rätt bevarad SHA256 och innehåller exakt originalets Store-EXE. Ett rent bygge av överförd källa matchar båda originalprogrammen efter enbart dokumenterade kompilatoridentifierare/tidsstämpel. Se docs/VERIFICATION-1.7.md.
- Inga EXE/MSIX, SDK, certifikat, privata nycklar eller verkliga gårds-/djurdata ingår. Två nödvändiga guidebilder ingår som dokumentationsresurser.
- Nästa steg: granska denna PR och samordna dokumentationen med #2. Claude ska utgå från denna 1.7-gren för den separat beställda knappändringen. Ingen merge, release eller Store-publicering har gjorts i denna uppgift.

### Slutkontroller 2026-11-25
- `python work/run_tests.py`: exitkod 0, alla 22 simulerade körningar godkända (11 grupper × Test/Store). Första begränsade UI-körningen fick timeout vid processavslut; hela omkörningen med vanlig lokal Windows-åtkomst passerade.
- `python work/build_manual_free.py`: exitkod 0, Test-EXE och osignerat MSIX byggda och paketvaliderade i den separata arbetskopian. MAKEAPPX pekade på den befintliga lokala SDK-installationen.
- `python work/verify_manual17_artifacts.py ..`: exitkod 0; ursprungliga leveranshashar stämmer och båda ombyggda programmen matchar originalen efter endast dokumenterade kompilatorvariationer.
- De sju produktkällornas Git-indexinnehåll är byteidentiskt med originaltaggen. Inga oavsiktliga binärer/hemligheter/testdata finns bland de 35 överförda filerna. GitHub Actions-resultat ska läsas i PR:ns kontroller; de lokala resultaten ovan är separata.

## Bekräftad Store-publicering – 2026-11-25
Patrik bekräftar att **HDX Import Manual 1.7 redan är publicerad i Microsoft Store**. Detta ersätter tidigare uppgifter om enbart inskickad version eller okänd publiceringsstatus. Datumet avser bekräftelsen; det faktiska publiceringsdatumet har inte angetts. Källkodsöverföringen ligger i [PR #3](https://github.com/andepa-tools/HDX-import/pull/3), gren `codex/manual-1.7-source`, utan merge. Endast dokumentationen uppdateras med denna uppgift; produktkod och tidigare testresultat är oförändrade. Store-sidan, Partner Centers uppladdade fil och uppdatering på kunddator har inte kontrollerats direkt. Nästa steg är fortsatt granskning av PR #3 och samordning med PR #2.

## Överlämning – 2026-11-25 (Codex, Windows/CI för PR #3 och #4)
- Uppgift och grenar: felsök Review311Tests i `codex/manual-1.7-source` ([PR #3](https://github.com/andepa-tools/HDX-import/pull/3)) och testa Claudes knappar i `claude/manual-1.7-knappar` ([PR #4](https://github.com/andepa-tools/HDX-import/pull/4)). Separata rena arbetskopior skapades från GitHub; ursprungsprojektet och PR #2 lämnades orörda.
- Orsak verifierad i Windows CI: skrivbord 1024×768, arbetsyta 1024×720 och 96 DPI. WinForms begränsade det begärda 1180×900-fönstret till `SystemInformation.MaxWindowTrackSize`, 1044×788. Testet räknade alltså rader vid fel storlek. Lokalt är skrivbordet 1920×1080 vid samma DPI. Se [diagnostisk körning](https://github.com/andepa-tools/HDX-import/actions/runs/45593094126).
- Genomfört: endast testunderlaget och CI ändrades. ReviewForm får den uttryckliga renderingsstorleken genom testlokal SetWindowPos/WM_GETMINMAXINFO-hantering; verklig storlek kontrolleras och skärmmått/DPI loggas. Kravet `displayed >= 8` är oförändrat. Både 980×760 och 1180×900 kontrolleras, inklusive att sista av 30 simulerade djur går att nå med rullning. Appens utseende, knappar och importlogik är oförändrade.
- Kontroller och resultat: [grön Windows CI för PR #3, commit 597a4ef](https://github.com/andepa-tools/HDX-import/actions/runs/45593495271): samtliga 22 körningar (11 grupper × Test/Store) och EXE/osignerat MSIX 1.7.0.0 godkända. CI mäter före fixering 1044×788, efter fixering 1180×900, därefter 12 kompletta rader och tabellhöjd 319. Samma 12 rader mäts lokalt. Inga krav sänkta och ingen produktlayout ändrad.
- Windows lokalt: `python work/run_tests.py` godkänt i både PR #3- och PR #4-kopian; slutlig Review311Tests-hantering dessutom körd separat för Test och Store i PR #3. Samtliga grupper: AccessTests, BTests, DisplayTests, Round3Tests, Round2Tests, Review311Tests, Ui311Tests, Retry311Tests, Visual311Tests, Response311Tests och WaitTests. `python work/build_manual_free.py` godkänt i båda rena kopiorna, med MAKEAPPX pekande på befintlig lokal SDK. Ingen SDK installerades och ingen version höjdes.
- Skärmbilder: CI sparar `preview-1.7-huvudfonster-fore-lasning.png` och `preview-1.7-huvudfonster-efter-lasning.png` som artefakten `manual-1.7-preview` (30 dagar). Bilder och byggda binärer checkas inte in. PR #4:s kommentar ska innehålla länk till dess egen bildartefakt och fullständig grupprapport. Webbläsaruppladdning av inline-bilder kunde inte användas eftersom datorstyrningen inte säkert kunde fastställa aktuell URL.
- Avgränsning: endast simulerad ISO användes. Ingen verklig DelPro-import, ingen PR-merge till main, release, publicering eller versionshöjning. Fixcommittarna har förts till PR #4 genom vanliga merges mellan arbetsgrenarna; det är inte en sammanslagning av PR:erna.
- Nästa konkreta steg: läs aktuella head-committars kontroller i PR #3/#4 och PR #4:s testkommentar; Patrik granskar knapparna i bildartefakten. Efter granskning får Patrik besluta separat om PR-merge. Äldre överlämningar ovan bevaras som historik.

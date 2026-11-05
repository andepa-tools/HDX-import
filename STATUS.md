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

## Aktuell överlämning – 2026-11-05

Datum och assistent: 2026-11-05, ChatGPT/Codex.
Uppgift: publicera fullversion 3.11 efter Patriks godkännande och lämna dokumentationen för granskning.
Gren: `codex/release-3.11-handoff`, pull request mot main. Dokumentationen är ett förslag tills PR:n har slagits samman.

### Genomfört och verifierat

- **HDX import 3.11 är publicerad som Latest**: [release v3.11](https://github.com/andepa-tools/HDX-import/releases/tag/v3.11). Mål: main. Inte pre-release.
- Patrik har enligt sitt uttryckliga publiceringsuppdrag testat och godkänt 3.11 på DelPro-datorn. Ingen ny installation eller DelPro-import kördes vid publiceringen.
- Releasetexten motsvarar den godkända lokala releasetexten för 3.11; GitHub använder CRLF-radslut.
- Exakt fyra filer har laddats upp: HDXimport-Installation-v3.11.exe, HDXimport-Installation.exe, HDXimport-v3.11-LasMig.pdf och HDXimport-v3.11-SHA256.txt.
- [GitHubs latest-API](https://api.github.com/repos/andepa-tools/HDX-import/releases/latest) returnerade `tag_name: v3.11`. Båda EXE-filerna har storleken **5491200 byte** och `asset.digest: sha256:aac58e8fc4cf92bc70911f17b42cfd0b5953f1ee914bffc0d5f80cf7ffb8733f`, identiskt med de lokala filerna.
- [Den fasta nedladdningslänken](https://github.com/andepa-tools/HDX-import/releases/latest/download/HDXimport-Installation.exe) hämtades efter publiceringen. Nedladdade bytes har samma SHA-256 och storlek som 3.11-filen. Filen kördes inte.
- Äldre releaser v3.8, v3.9 och v3.10 har jämförts före och efter. Releasetexter, filidentiteter, digest, storlekar och ändringstider är oförändrade.
- **HDX Import Manual 1.7 är inskickad till Microsoft Store**, enligt Patriks uppgift i detta uppdrag. Publicering av 1.7 och uppdatering hos Store-kunder har inte verifierats; Partner Center kontrollerades inte i publiceringsarbetet.
- README:s Förutsättningar har förtydligats: Use Command Line ISO under Devices → ISOProcessor ska vara ibockad; ISOProcessor behöver inte vara aktiv.
- Denna PR ändrar endast README.md och STATUS.md. Ingen källkod, binär eller privat fil ingår i PR:n.

### Hinder och nästa steg

Publiceringen är klar utan kvarstående hinder. Granska dokumentations-PR:n; den ska inte slås samman automatiskt. Följ upp Store-beskedet för Manual 1.7. Nästa assistent ska läsa GitHub-läget på nytt och hålla Manual/Store och fullversionens versionsspår åtskilda.

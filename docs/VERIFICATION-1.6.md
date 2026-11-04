# HDX Import Manual 1.6 – bygg- och verifieringsrapport

2026-11-04

Leveranser i denna mapps outputs:
- HDX-Import-Manual-Test-1.6.exe (1.6.0.0)
- HDX-Import-Manual-1.6.0.0-x64.msix (osignerat Store-underlag)
- HDX-Import-Manual-1.6-SHA256.txt

## Ändring
Manualens ISO-kommunikation använder C:\DelPro\ISO\HDXImport\Runs\<tid>_<unik identifierare> för request, output, log, före-läsning, import och efter-läsning. INI-filen ligger kvar i appens lokala datamapp. ISO-processorns WorkingDirectory och argument har inte ändrats.

Före varje ISO-start öppnas begäran för läsning. Appen provar att skapa, skriva och läsa svar- och loggfilerna och tar bort sina provfiler före start. Befintliga filer skrivs inte över. Fel ger E012 och stoppar processstart; fel vid skapande av körningsmapp anger exakt sökväg. Ingen automatisk reservmapp, rättighetsändring eller administratörshöjning.

E009 visar om svar/logg saknas, är tomma eller inte kan öppnas och redovisar avslutningskoden. Appens åtkomstprov bevisar inte den externa ISO-processorns åtkomst. Svarskontrollen behåller 30 sekunders väntan och 2 sekunders stabilitet. Importens resultat verifieras fortfarande genom återläsning.

Loggrensningen är begränsad till den exakta nya Runs-mappen, 30 dagar, kända filer, rätt format på körningsmappar och befintliga skydd mot länkar/okända filer. Äldre körningar i appens tidigare datamapp lämnas kvar.

## Verifierat
- Båda EXE-varianterna kompilerade till x64, version 1.6.0.0, utan licensbegränsning.
- MakeAppx pack-validering godkänd, se work/store-manual-1.6-pack.log.
- MSIX ZIP-integritet, oförändrad Identity/Publisher, version och exakt Store-EXE i paketet kontrollerade. Inga källfiler, nycklar eller gårdsdata i paketet.
- Åtkomsttester mot båda EXE-varianterna: lyckat prov, skydd av befintligt svar, låst/saknad/tom begäran, saknad målmapp samt saknat/tomt/låst svar.
- Manuella importtester mot båda EXE-varianterna med simulerad ISO: Behåll/Skriv över/avbryt, befintliga ISO-nummer, färska B-uppgifter, saknad B-kolumn och verifierande återläsning. Alla testfiler ligger under work.
- Originalleveranserna för 1.5 har oförändrade kontrollsummor. Originalprojektet och fullversion 3.10 har inte ändrats eller byggts.

## Användarbekräftat test på DelPro-datorn – 2026-11-04
Användaren bekräftar i chatten att både test-EXE 1.6 och den lokalt testsignerade, installerade MSIX-versionen 1.6 lyckas hämta djurlistan på samma riktiga DelPro-dator som tidigare hade E009. Testcertifikatet installerades innan MSIX-testet.

Detta verifierar att läsningen fungerar i den testade miljön med den nya arbetsmappen. Den exakta grundorsaken till det tidigare felet är inte tekniskt fastställd; filomdirigering ska inte beskrivas som bevisad orsak. Resultatet är användarbekräftat, utan separat insamlade körningsloggar eller oberoende kontroll av filerna på DelPro-datorn.

## Återstående begränsningar och publiceringsläge
Verklig import till DelPro har inte bekräftats som testad. Importflödet har testats med simulerad ISO. Ingen ändring av DelPro-data har gjorts av agenten.

Den lokalt signerade MSIX-installationen är testad. Distribution, signering och uppdatering via Microsoft Store är ännu inte verifierade för 1.6.

Store-underlaget är det ursprungliga osignerade paketet HDX-Import-Manual-1.6.0.0-x64.msix. Testsignerat paket, certifikat och privata nycklar ska inte laddas upp. Inget har laddats upp, skickats för certifiering eller publicerats av agenten. Användarens uttryckliga godkännande krävs före inskickning/publicering.

Källor: outputs/*.cs. Bygg: work/build_manual_free.py från projektroten. Avgränsad jämförelse mot 1.5: work/manual-1.6.patch.

## Uppdaterat publiceringsläge 2026-11-04
Användaren har efter rapportens tidigare publiceringsnotering bekräftat att 1.6 är inskickad och väntar på publicering. Publicering är ännu inte bekräftad. Partner Center har inte kontrollerats direkt i dokumentationsarbetet.

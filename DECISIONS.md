# Beslut – HDX import

Här dokumenteras beslut med datum, status, motiv och konsekvens. Ändra inte äldre beslut i tysthet; lägg till ett nytt beslut som ersätter det gamla.

## D001 – Gemensam projektinformation på GitHub
Datum: 2026-11-04.
Status: användarens inriktning; praktiskt arbetsflöde föreslås i denna pull request.
Underlag: användarens uppdrag att göra HDX Import till ett gemensamt GitHub-baserat projekt för ChatGPT och Claude.
Beslut: samla gemensamma arbetsregler i AGENTS.md, en ingång för Claude i CLAUDE.md, överlämningar i STATUS.md och beslut i DECISIONS.md.
Motiv: nästa assistent ska kunna fortsätta med versionshanterad information utan att behöva hela chatthistoriken.
Konsekvens: båda assistenterna behöver aktuell läsåtkomst; den som gör ändringar behöver skrivåtkomst eller lämnar ett granskningsbart förslag.

## D002 – Bevara befintligt material vid införandet
Datum: 2026-11-04.
Status: genomförande av användarens krav.
Beslut: lägg enbart till de fyra saknade filerna på en separat gren för granskning. Bevara README och publicerade releaser.
Motiv: samarbetsstödet ska införas försiktigt utan att skriva över befintligt innehåll.
Konsekvens: huvudgrenen får samarbetsfilerna först efter sammanslagning.

## Öppen fråga – Var finns källkoden?
Vid inventeringen fanns endast README på main. Det är inte fastställt var originalkällkod, byggfiler och tidigare utvecklingsunderlag finns eller om de ska publiceras här. Inget beslut om flytt eller publicering av källkod har fattats.

## D003 – Gemensamt utvecklingsunderlag för Manual 1.6
Datum: 2026-11-04.
Status: godkänt genom användarens uppdrag att ordna GitHub så andra AI kan delta fullt ut.
Beslut: versionshantera befintlig Manual-källkod, grafik, paketeringsmall, byggskript, simulerade tester och relevant status på GitHub. Behåll nuvarande katalogstruktur för att minska risken vid överföringen.
Motiv: andra assistenter ska kunna läsa, ändra, testa och bygga utan tillgång till privata chattar eller den ursprungliga datorns arbetsmappar.
Konsekvens: den tidigare öppna frågan om Manual-källkodens plats är löst. Fullversionens kompletta utvecklingsunderlag återstår. Privata signeringsnycklar, certifikat, SDK, byggda paket och verkliga gårdsdata ingår inte. Ingen ny öppen källkodslicens tilldelas genom denna överföring.

## D004 – Separera utvecklingskontroller från publicering
Datum: 2026-11-04.
Status: infört i utvecklingsunderlaget.
Beslut: automatiska kontroller bygger och testar Manual med simulerad ISO och läsbehörighet till repot. Store-publicering, signering med privata nycklar och verklig import ingår inte.
Motiv: en kodändring ska kunna kontrolleras utan att ändra verkliga djurdata eller publicerade programversioner.

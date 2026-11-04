# Gemensamma instruktioner för HDX import

## Börja här
Läs denna fil, [README.md](README.md), [STATUS.md](STATUS.md) och [DECISIONS.md](DECISIONS.md) före arbete. Claude läser även [CLAUDE.md](CLAUDE.md). Kontrollera aktuell gren, senaste ändringar och öppna pull requests innan du ändrar något.

## Projektets omfattning
Repot är enligt README avsett för installationsfiler, dokumentation och versionsinformation. Vid inventeringen 2026-11-04 fanns ingen programkällkod på main. Anta inte språk, byggkommandon eller testmetod innan källkoden har lokaliserats.

## Arbetssätt för båda assistenterna
- Skriv och förklara på svenska. Arbeta stegvis inom användarens uppdrag.
- Bevara befintligt innehåll och andras ändringar. Läs alltid aktuell fil före ändring; komplettera i stället för att ersätta historik.
- Gör avgränsade ändringar på en separat gren och lämna en pull request för granskning. Slå inte samman eller publicera en release utan användarens uppdrag.
- Arbeta med en uppgift i taget. Kontrollera om en annan assistent redan arbetar med samma filer. Använd separata grenar och samordna överlappande ändringar före sammanslagning.
- Dokumentera viktiga beslut i DECISIONS.md och lämna aktuell överlämning i STATUS.md. Märk förslag som förslag och ange källa för verifierade uppgifter.
- Redovisa vad som ändrats, vad som kontrollerats och vad som återstår. Påstå inte att programmet är testat när bara dokumentation har granskats.
- Repot är offentligt. Lägg inte in hemligheter eller verkliga kund-, gårds- eller djurdata.

## Funktionsuppgifter som måste bevaras
README anger stöd för svenska SE-ID och landskod 752. Release v3.10 beskriver skydd av befintliga ISO-nummer och ett separat val för att behålla eller ersätta B-transpondrar, där behåll är förvalt. Läs aktuell versionsinformation innan beteendet beskrivs eller ändras.

## Överlämning
Uppdatera STATUS.md med datum, uppgift, gren/pull request, genomförda ändringar, kontrollresultat, hinder och ett konkret nästa steg. Nästa assistent ska läsa GitHub-läget på nytt; en chatt eller gammal status är inte bevis på att en ändring är införd.

## Uppdatering 2026-11-04 – utvecklingsunderlag
Den ursprungliga inventeringen ovan avser läget före denna överföring. Manual 1.6-källkod, byggunderlag och simulerade tester finns nu här. Följ [DEVELOPMENT.md](DEVELOPMENT.md). Kör `python work/run_tests.py` och, när byggunderlag ändras, `python work/build_manual_free.py` på Windows i en ren arbetskopia. Ingen automatisk Store-publicering eller verklig DelPro-import ingår i testerna. Fullversionens kompletta källkod ingår ännu inte.

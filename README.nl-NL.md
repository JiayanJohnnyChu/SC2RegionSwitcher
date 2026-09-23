[English](README.md) · [简体中文](README.zh-CN.md) · [Français](README.fr-FR.md) · [Deutsch](README.de-DE.md) · [Nederlands](README.nl-NL.md)

# SC2 Region Switcher

**Downloads:** [v3.5.3-preview.1](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.5.3-preview.1) · [Alle versies](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases)

SC2 Region Switcher maakt het mogelijk om afzonderlijke Chinese en internationale installaties van StarCraft II te gebruiken met één Battle.net-app. Het programma wisselt de aanmeldregio van Battle.net en de gedeelde taalinstellingen van het spel.

De toepassing ondersteunt elf interfacetalen en een keuze uit geïnstalleerde internationale speltalen.

![SC2 Region Switcher met gesimuleerde configuratie](assets/screenshots/sc2-switcher-3.5.1.png)

## Installatie

Windows x64 en **[Microsoft .NET 10 Desktop Runtime x64](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)** zijn vereist. De SDK is niet nodig om te spelen. De beschikbare pakketten zijn niet ondertekend.

| Pakket | Gebruik |
| --- | --- |
| MSI | Installatie voor alle Windows-gebruikers, met beheerderstoestemming, een Startmenu-item en de normale Windows-verwijderfunctie. De app zelf draait met gewone gebruikersrechten. |
| Draagbaar ZIP | Volledig uitpakken in een afzonderlijke map, met alle meegeleverde bestanden bij elkaar. `SC2Switcher.Wpf.exe` start de app; er komt geen snelkoppeling of verwijdervermelding. |

De “Source code”-downloads van GitHub zijn geen direct uitvoerbare toepassingspakketten.

## Eerste configuratie

1. Beide spelinstallaties zijn voltooid in Battle.net en staan in afzonderlijke mappen, bijvoorbeeld `D:\Games\StarCraft II CN` en `D:\Games\StarCraft II Global`. China vereist tekst en spraak in vereenvoudigd Chinees; de internationale installatie vereist beide in de gekozen taal.
2. De uiteindelijke installatiemap wordt vóór installatie in Battle.net gecontroleerd, ook als **Installeren** wordt getoond. Battle.net kan een submap `StarCraft II` toevoegen of de andere installatie herkennen.
3. De instellingen bevatten de Battle.net-map, beide spelmappen en het daadwerkelijk gebruikte bestand `StarCraft II\Variables.txt` in de Documenten-map van het spel. Normaal starten en afsluiten van het spel maakt dit bestand aan als het ontbreekt.
4. **Opslaan en controleren** controleert de paden en slaat ze op, zonder de speltaal te wijzigen. De volledige voorbereiding en padvoorbeelden staan in de Engelstalige [gebruikershandleiding](docs/SETUP.md).

## Wisselen en taal

Wisselen vereist dat StarCraft II en de editor gesloten zijn en dat Battle.net-downloads, updates en reparaties voltooid zijn. Het doel is China of Internationaal; Internationaal biedt ook de aanmeldregio's EU, US en KR. Het programma sluit Battle.net normaal af, maakt een back-up, past de taalinstellingen toe en opent de gevraagde aanmeldregio. De bedieningselementen blijven geblokkeerd totdat de bewerking is voltooid.

Aanmelden, de spelserver kiezen en het spel starten gebeuren in Battle.net. **EU, US en KR zijn Battle.net-aanmeldregio's en bevestigen niet welke spelserver is geselecteerd.** De interfacetaal staat los van de speltaal. De toepassing past de opgeslagen internationale speltaal bij de volgende wissel toe; tekst en spraak moeten al via Battle.net zijn geïnstalleerd.

## Spelvoortgang, back-ups en updates

Het programma wijzigt in de gedeelde spelinstellingen alleen de taalvelden. Het kopieert, verwijdert of beheert geen campagnesaves, replays of `Accounts`-bestanden. Spelaccounts en regio's bepalen de toegang tot voortgang; het programma biedt geen cloudsynchronisatie van saves.

De programma-instellingen en back-ups staan in `%LOCALAPPDATA%\SC2RegionSwitcherV2` en blijven na verwijdering behouden. Een openstaande herstelprocedure heeft de oorspronkelijke configuratie en back-ups nodig. Een MSI-update gebruikt een nieuwer installatieprogramma terwijl de app gesloten is; een draagbare update gebruikt een nieuwe map en een aangepaste snelkoppeling. Geen van beide verplaatst de spelinstallaties.

## Veelgestelde vragen

| Vraag | Antwoord |
| --- | --- |
| De app start niet | Desktop Runtime 10 x64 en alle meegeleverde toepassingsbestanden zijn nodig. |
| Een spelmap of taal is niet beschikbaar | De map moet de hoofdmap van het spel zijn; de installatie en de vereiste tekst- en spraakbestanden moeten compleet zijn. |
| Wisselen mislukt of herstel staat open | De foutdetails geven de volgende controle aan. De oorspronkelijke instellingen en back-ups blijven nodig; de handleiding beschrijft het herstel. |

De uitgebreide documentatie is beschikbaar in het Engels en vereenvoudigd Chinees; de volgende links verwijzen naar de Engelse versies.

| Meer informatie | Links |
| --- | --- |
| Handleiding voor spelers | [Gebruik en probleemoplossing](docs/SETUP.md) |
| Versies en ontwikkeling | [Wijzigingsoverzicht](CHANGELOG.md), [Ontwikkeling](docs/DEVELOPMENT.md), [Validatie](docs/VALIDATION.md) |

De oorspronkelijke projectmaterialen gebruiken de [MIT-licentie](LICENSE); de [vermeldingen van derden](THIRD-PARTY-NOTICES.md) beschrijven de meegeleverde lettertypen en andere materialen onder licentie.

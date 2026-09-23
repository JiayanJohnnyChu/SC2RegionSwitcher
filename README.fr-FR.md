[English](README.md) · [简体中文](README.zh-CN.md) · [Français](README.fr-FR.md) · [Deutsch](README.de-DE.md) · [Nederlands](README.nl-NL.md)

# SC2 Region Switcher

**Téléchargements :** [v3.4.2-preview.1](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.4.2-preview.1) · [Toutes les versions](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases)

SC2 Region Switcher permet d'utiliser deux installations distinctes de StarCraft II, Chine et International, avec une seule application Battle.net. Il change la région de connexion de Battle.net et les paramètres de langue partagés du jeu.

La version publique est **3.4.2**, avec une interface en anglais ou en chinois simplifié et des langues de jeu fixes : chinois pour la Chine, anglais pour l'installation internationale. La capture montre la **version de développement 3.5.1** avec une configuration simulée. Le code source 3.5.1 prend en charge onze langues d'interface et le choix d'une langue de jeu internationale déjà installée ; aucun paquet de cette version n'est publié.

![Interface de développement de SC2 Region Switcher 3.5.1 avec une configuration simulée](assets/screenshots/sc2-switcher-3.5.1.png)

## Installation

Windows x64 et **[Microsoft .NET 10 Desktop Runtime x64](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)** sont nécessaires. Le SDK n'est pas nécessaire pour jouer. Les paquets disponibles ne sont pas signés.

| Paquet | Utilisation |
| --- | --- |
| MSI | Installation pour tous les utilisateurs Windows, avec autorisation administrateur, entrée dans le menu Démarrer et désinstallation habituelle de Windows. L'application fonctionne avec les droits d'un utilisateur ordinaire. |
| ZIP portable | Extraction complète dans un dossier distinct, avec tous les fichiers fournis conservés ensemble. `SC2Switcher.Wpf.exe` lance l'application ; aucun raccourci ni entrée de désinstallation n'est créé. |

Les téléchargements « Source code » de GitHub ne sont pas des paquets directement exécutables.

## Première configuration

1. Les deux installations du jeu sont terminées dans Battle.net et occupent des dossiers distincts, par exemple `D:\Games\StarCraft II CN` et `D:\Games\StarCraft II Global`. La Chine nécessite les textes et les voix en chinois ; la version publique nécessite les deux ressources en anglais pour l'installation internationale.
2. Le dossier final est vérifié dans Battle.net avant l'installation, même si **Installer** est affiché. Battle.net peut ajouter un sous-dossier `StarCraft II` ou détecter l'autre installation.
3. Les paramètres indiquent le dossier Battle.net, les deux dossiers du jeu et le fichier `StarCraft II\Variables.txt` réellement utilisé dans le dossier Documents du jeu. Un lancement normal du jeu suivi d'une fermeture normale crée ce fichier s'il est absent.
4. **Enregistrer et vérifier** valide et enregistre les chemins sans modifier la langue du jeu. La préparation complète et les exemples de chemins figurent dans le [guide d'utilisation](docs/SETUP.md), en anglais.

## Changement de région et langue

Le changement nécessite que StarCraft II et son éditeur soient fermés et que les téléchargements, mises à jour et réparations de Battle.net soient terminés. La destination est Chine ou International ; cette dernière propose aussi les régions de connexion EU, US ou KR. L'outil ferme Battle.net normalement, applique les paramètres de langue après sauvegarde et ouvre la région demandée. Les commandes restent verrouillées jusqu'à la fin de l'opération.

La connexion au compte, la sélection du serveur de jeu et le lancement du jeu se font dans Battle.net. **EU, US et KR sont des régions de connexion Battle.net, pas une confirmation du serveur de jeu sélectionné.** La langue de l'interface est indépendante de celle du jeu. La version de développement applique la langue internationale enregistrée au prochain changement ; les ressources de texte et de voix doivent déjà être installées par Battle.net.

## Sauvegardes, copies de sécurité et mises à jour

L'outil ne modifie que les paramètres de langue dans le fichier partagé du jeu. Il ne copie, ne supprime et ne gère ni les sauvegardes de campagne, ni les replays, ni les fichiers `Accounts`. L'accès à la progression dépend des comptes et des régions du jeu ; l'outil ne synchronise pas les sauvegardes dans le cloud.

Les paramètres et copies de sécurité de l'outil sont conservés dans `%LOCALAPPDATA%\SC2RegionSwitcherV2`, même après désinstallation. Une récupération en attente nécessite la configuration et les copies d'origine. Une mise à jour MSI utilise un installateur plus récent, application fermée ; une mise à jour portable utilise un nouveau dossier et un raccourci adapté. Aucune de ces méthodes ne déplace les installations du jeu.

## Questions courantes

| Question | Réponse |
| --- | --- |
| L'application ne démarre pas | Desktop Runtime 10 x64 et tous les fichiers fournis sont nécessaires. |
| Un dossier ou une langue est indisponible | Le dossier doit être la racine du jeu ; l'installation et les ressources de texte et de voix doivent être complètes. |
| Le changement échoue ou une récupération est en attente | Les détails de l'erreur indiquent la vérification suivante. La configuration et les copies d'origine restent nécessaires ; le guide décrit la récupération. |

La documentation détaillée est disponible en anglais et en chinois simplifié ; les liens suivants mènent aux versions anglaises.

| Informations complémentaires | Liens |
| --- | --- |
| Guide du joueur | [Utilisation et dépannage](docs/SETUP.md) |
| Versions et développement | [Historique](CHANGELOG.md), [Développement](docs/DEVELOPMENT.md), [Validation](docs/VALIDATION.md) |

Les éléments originaux du projet utilisent la [licence MIT](LICENSE) ; les [mentions relatives aux tiers](THIRD-PARTY-NOTICES.md) décrivent les polices et autres éléments sous licence.

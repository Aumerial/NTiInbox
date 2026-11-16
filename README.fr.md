# NTi Inbox

**Une boîte de réception des files de messages IBM i, en .NET.**
Projet de démonstration (POC) fourni par [Aumerial](https://www.aumerial.com/) avec la licence d'essai du fournisseur de données **NTi** (`Aumerial.Data.Nti`).

[Read in English](README.md)

NTi Inbox transpose les files de messages IBM i, telles qu'on les voit sur un écran 5250 avec `WRKMSG`, `SNDMSG` et `SNDBRKMSG`, dans une interface web présentée comme une messagerie : les files dans une colonne latérale, les messages de la file choisie sous forme de liste, et un volet de lecture. Les messages en attente de réponse passent en premier, les messages répétés sont regroupés, et la réponse, la suppression ou le nouveau message sont à un clic.

C'est aussi un **projet d'exemple** : tout le code est fourni pour montrer comment piloter un IBM i en C# avec NTi (appels d'API système et espaces utilisateur), et pour être modifié ou étendu.

> ## Avertissement important
>
> **NTi Inbox agit directement sur la partition IBM i à laquelle elle est connectée.** Elle peut envoyer des messages, répondre à des messages d'interrogation (une réponse peut relancer ou arrêter le job qui l'attend), supprimer des messages et envoyer des messages d'interruption qui coupent des sessions utilisateur, avec les droits du profil utilisé pour la connexion.
>
> - Ce projet est un **POC de démonstration**, fourni **en l'état, sans garantie** d'aucune sorte, y compris d'adéquation à un usage particulier ou d'absence d'erreur.
> - **Aumerial décline toute responsabilité** pour toute perte de données, interruption de service ou dommage direct ou indirect résultant de l'utilisation, de la modification ou du déploiement de ce projet, notamment du fait des actions exécutées.
> - L'utilisateur est **seul responsable** des actions qu'il exécute et de l'environnement sur lequel il les exécute.
> - **Utilisez une partition de test ou de développement**, jamais un environnement de production, sauf si vous en maîtrisez pleinement les conséquences. Gardez des sauvegardes à jour.
> - Utilisez un profil IBM i aux **droits limités** : l'application n'élève jamais les privilèges, donc un profil restreint limite mécaniquement ce qu'une action peut faire.
>
> Chaque modification demande d'abord une confirmation, et un bandeau signale la production et les droits élevés (voir [Sécurité](#7-sécurité)), mais c'est une aide à la vigilance, pas une protection.

## Sommaire

1. [Fonctionnalités](#1-fonctionnalités)
2. [Prérequis](#2-prérequis)
3. [Mise en route pas à pas](#3-mise-en-route-pas-à-pas)
4. [Configuration](#4-configuration)
5. [Langue](#5-langue)
6. [Parcours de test](#6-parcours-de-test)
7. [Sécurité](#7-sécurité)
8. [Architecture du code](#8-architecture-du-code)
9. [Étendre le projet](#9-étendre-le-projet)
10. [Résolution de problèmes](#10-résolution-de-problèmes)
11. [Limites connues](#11-limites-connues)
12. [Support et conditions d'utilisation](#12-support-et-conditions-dutilisation)

## 1. Fonctionnalités

- **Découverte des files** : les files de messages d'une bibliothèque ou d'une valeur spéciale (`*LIBL`, `*ALLUSR`, `*CURLIB`, `*USRLIBL`), avec un filtre sur la liste et le nombre de messages en attente de réponse pour les files déjà lues.
- **Vue boîte de réception** : les messages les plus récents d'une file, du plus récent au plus ancien, avec les messages en attente de réponse dans leur propre section en haut.
- **Filtres et recherche** : statut de réponse, gravité minimale, type de message, période d'envoi, une heure choisie sur le graphique d'activité, et une recherche texte sur l'identifiant, le texte, les données de substitution, le job émetteur et le profil émetteur.
- **Regroupement des messages répétés** : les messages identiques se replient en une ligne avec leur nombre et leur période, et se déplient à la demande.
- **Graphique d'activité** : les 24 dernières heures, heure par heure, empilées par niveau de gravité.
- **Volet de lecture** : texte, gravité, type, job et profil émetteurs, données de substitution, et aide du message (texte de second niveau) lue sur le système à la demande.
- **Fiche du job émetteur** : état, fonction en cours, consommation et attentes de verrou du job qui a envoyé le message.
- **Réponse** à un message d'interrogation, avec la réponse par défaut rappelée à côté du champ.
- **Envoi** : un message immédiat ou prédéfini (`*INFO`, `*COMP`, `*DIAG` ou `*INQ`) vers cinquante files au plus, `*SYSOPR` et `*ALLACT` compris. Un message `*INQ` vise une seule file et nomme une file de réponse.
- **Message d'interruption** vers une file de messages de poste, confirmé en saisissant le nom du poste.
- **Suppression** d'un message, d'une sélection, ou de tous les messages affichés.
- **Attributs de la file** : taille, mode de livraison, filtre de gravité, alertes et plus.
- **Catalogue de messages** : texte, aide et variables de substitution d'un message prédéfini à partir de son identifiant.
- **Page du profil connecté** : droits spéciaux, file de messages et mode de livraison.
- **Lecture automatique** de la file ouverte toutes les 30 secondes, avec le compteur dans le titre de l'onglet et une notification navigateur facultative pour les nouveaux messages en attente de réponse.
- **Connexions enregistrées** (sans mot de passe), avec un marquage rouge de production.
- **Thèmes clair et sombre** (sombre par défaut) et une **interface en anglais et en français**.

Rien n'est installé sur l'IBM i : tout passe par des appels d'API système depuis .NET, plus une commande CL (`DLTUSRSPC`) qui supprime un espace utilisateur temporaire dans `QTEMP`.

## 2. Prérequis

### Côté IBM i

| Élément | Détail |
|---|---|
| Version | IBM i V5R4 minimum, V7R4 ou ultérieure recommandée (exigence de NTi) |
| Services TCP | `*DATABASE`, `*RMTCMD` et `*SIGNON` démarrés |
| Ports | 8471, 8475 et 8476 ouverts depuis la machine qui exécute l'application (9471, 9475 et 9476 avec SSL/TLS) |
| Profil utilisateur | Un profil et son mot de passe. Ses droits déterminent ce que l'application peut lire et faire |
| Licence NTi | Une clé de licence installée sur la partition, dans la bibliothèque `KNTI` par défaut (voir l'étape 1 ci-dessous) |

### Côté poste

| Élément | Détail |
|---|---|
| [SDK .NET 10](https://dotnet.microsoft.com/download) | À vérifier avec `dotnet --version` (doit afficher `10.x`) |
| Git | Pour récupérer le projet |
| Accès à NuGet | Pour restaurer le paquet `Aumerial.Data.Nti` |
| Éditeur (facultatif) | Visual Studio, Rider ou VS Code |
| Réseau | La machine doit joindre l'IBM i sur les ports ci-dessus |

## 3. Mise en route pas à pas

### 1. Obtenir et installer la licence NTi

Une licence est liée à **une partition** (numéro de série et identifiant de partition) et valable pour une durée limitée. Une licence d'essai gratuite de 30 jours est disponible. Une fois la licence expirée, les connexions à l'IBM i sont refusées.

1. Créez un compte sur le portail client : <https://portal.aumerial.com/>.
2. Déclarez la partition IBM i (LPAR) sur laquelle l'application sera testée.
3. Téléchargez le **script SQL de licence** généré automatiquement. Il contient la clé et les valeurs propres à la partition.
4. Exécutez-le sur l'IBM i, par exemple avec **Run SQL Scripts** dans IBM i Access Client Solutions (ACS). Le script crée la bibliothèque `KNTI` et y enregistre la licence.

La documentation de référence est ici : <https://www.documentation.aumerial.com/en/docs/installation>.

### 2. Démarrer les services TCP de l'IBM i

Si ces services ne sont pas déjà démarrés, lancez-les depuis une session 5250 ou depuis ACS :

```
STRTCPSVR SVR(*DATABASE)
STRTCPSVR SVR(*RMTCMD)
STRTCPSVR SVR(*SIGNON)
```

### 3. Récupérer le projet

```
git clone https://github.com/Aumerial/NTiInbox.git
cd NTiInbox
```

### 4. Vérifier le SDK et compiler

```
dotnet --version
dotnet build
```

La première compilation restaure automatiquement le paquet NuGet `Aumerial.Data.Nti`. Elle doit se terminer sans erreur. Sinon, voir [Résolution de problèmes](#10-résolution-de-problèmes).

### 5. Lancer l'application

```
dotnet run --project NTiInbox
```

Ouvrez ensuite l'adresse affichée dans la console (par défaut <http://localhost:5115>).

Pour utiliser HTTPS (recommandé dès que l'application n'est plus seulement locale), faites approuver une fois le certificat de développement, puis lancez le profil `https` (<https://localhost:7263>) :

```
dotnet dev-certs https --trust
dotnet run --project NTiInbox --launch-profile https
```

Avec Visual Studio, ouvrez `NTiInbox.slnx` et lancez le projet (F5).

#### Alternative : lancer avec Docker

Aucun SDK .NET n'est nécessaire. L'image est publiée sur Docker Hub :

```
docker run -p 127.0.0.1:8080:8080 -v ntiinbox-data:/app/App_Data aumerial/ntiinbox
```

Ouvrez ensuite <http://localhost:8080>. Le `127.0.0.1` limite l'accès à votre machine : l'application n'a pas d'authentification propre, ne l'exposez donc ni sur un réseau ni sur Internet. Le volume conserve vos connexions enregistrées entre deux redémarrages. Pour construire l'image vous-même, lancez `docker build -t ntiinbox .` depuis le dossier du projet `NTiInbox` (celui qui contient `Dockerfile`).

### 6. Se connecter

L'application s'ouvre sur l'écran de connexion. Renseignez :

- le **système** (nom DNS ou adresse IP de l'IBM i),
- le **profil utilisateur** et son **mot de passe**,
- la **bibliothèque** explorée pour trouver les files de messages (facultatif, `*LIBL` par défaut),
- l'option **SSL/TLS** si les serveurs hôtes sont configurés pour le chiffrement,
- l'option **production** si la partition est une partition de production, ce qui marque toute l'interface en rouge.

Une fois la connexion établie, elle est mémorisée **sans le mot de passe** : la fois suivante, un clic sur la connexion enregistrée ne demande que le mot de passe.

## 4. Configuration

Tout est facultatif : le formulaire de connexion fonctionne sans configuration. La section `Ibmi` de `NTiInbox/appsettings.json` ne fait que préremplir le formulaire. Toutes les valeurs sont vides par défaut, et le mot de passe n'est jamais lu depuis la configuration.

| Clé | Rôle |
|---|---|
| `Ibmi:Host` | Nom ou adresse du système proposé dans le formulaire |
| `Ibmi:User` | Profil utilisateur proposé dans le formulaire |
| `Ibmi:Library` | Bibliothèque (ou valeur spéciale) explorée pour trouver les files de messages. `*LIBL` si vide |
| `Ibmi:UseSsl` | Coche l'option TLS par défaut |
| `Ibmi:ConnectionOptions` | Mots-clés supplémentaires de la chaîne de connexion NTi, ajoutés à chaque connexion |

Les mots-clés supplémentaires permettent toute option du fournisseur, par exemple une autre bibliothèque de licence :

```
license library=MYLIB
```

La liste des mots-clés est dans la [documentation de la chaîne de connexion](https://www.documentation.aumerial.com/en/docs/connection-string).

> **N'écrivez jamais un mot de passe dans `appsettings.json` ni dans le dépôt.** Utilisez les secrets utilisateur .NET ou les variables d'environnement.

Avec les secrets utilisateur (stockés hors du dépôt, dans le profil de l'utilisateur de la machine) :

```
dotnet user-secrets init --project NTiInbox
dotnet user-secrets set "Ibmi:Host" "myibmi.example.com" --project NTiInbox
dotnet user-secrets set "Ibmi:User" "MYPROFILE" --project NTiInbox
```

Avec les variables d'environnement (le double souligné remplace les deux-points) :

```
# PowerShell
$env:Ibmi__Host = "myibmi.example.com"
$env:Ibmi__User = "MYPROFILE"
```

Les connexions enregistrées depuis l'écran de connexion sont écrites sans mot de passe dans le dossier `App_Data` du projet (un fichier JSON par utilisateur, plus les clés de protection des cookies). Ce dossier est ignoré par Git.

## 5. Langue

L'interface est disponible en **anglais** et en **français**. À la première ouverture, l'application utilise la langue du navigateur (l'anglais si ce n'est ni l'une ni l'autre). Le sélecteur **EN / FR** de l'écran de connexion et du rail de gauche change la langue immédiatement, sans perdre la connexion à l'IBM i, et le choix est mémorisé pendant un an (cookie `.AspNetCore.Culture`).

Deux sortes de textes apparaissent à l'écran :

- **Les textes de l'application** (libellés, boutons, messages) sont traduits par l'application. Ils vivent dans deux dictionnaires simples, `Localization/Strings.En.cs` et `Localization/Strings.Fr.cs`. En développement, l'application refuse de démarrer si une clé existe dans une langue mais pas dans l'autre.
- **Les textes qui viennent de l'IBM i** (texte et aide des messages, noms de files et de jobs, descriptions) sont renvoyés par le système **dans la langue du job IBM i**. Ils sont affichés tels quels et ne sont pas traduits par l'application.

Pour ajouter une langue, créez un troisième dictionnaire à côté des deux existants, enregistrez sa culture dans `Localizer.Cultures`, et sélectionnez-le dans `Localizer.Texts` et `Strings.Get`.

## 6. Parcours de test

Ce parcours valide toute la chaîne, de la lecture à la réponse. Utilisez une partition de test et un profil qui vous est propre.

1. Connectez-vous.
2. Ouvrez `QSYSOPR` dans la colonne de gauche : les messages sont listés, du plus récent au plus ancien. Ouvrez-en un et cliquez sur **Afficher** pour lire son aide.
3. Ouvrez la page **Profil** et cliquez sur **Ouvrir ma file de messages**.
4. Cliquez sur **Nouveau message**, gardez votre propre file comme destinataire, saisissez un texte et envoyez. Confirmez. Le message apparaît dans votre file.
5. Supprimez-le et confirmez.
6. Envoyez un message `*INQ` vers votre file, avec une autre de vos files comme file de réponse. Il apparaît dans la section **En attente de réponse**. Répondez-y et confirmez.
7. Passez la langue en EN puis revenez en FR : tous les textes de l'application changent, pas ceux des messages.
8. Cliquez sur **Se déconnecter**, puis reconnectez-vous : la connexion enregistrée ne demande que le mot de passe.

## 7. Sécurité

Le code est fait pour être lu, testé et adapté par les clients : les choix de sécurité sont donc explicites. Voici ce que l'application garantit, et ce qu'elle ne garantit pas.

| Point | État |
|---|---|
| Mot de passe IBM i | Conservé uniquement dans la mémoire du serveur, pour la durée de la session. Jamais écrit sur disque, jamais dans une connexion enregistrée, jamais dans un journal. |
| Messages d'erreur | Débarrassés du mot de passe courant et de toute paire `password=` ou `pwd=` avant affichage (`IbmiSession.Sanitize`), car un pilote peut citer la chaîne de connexion dans ses erreurs. |
| Contenu affiché | Les textes de messages, noms de files et de jobs viennent de l'IBM i, donc d'une source non maîtrisée. Ils sont encodés par Razor et jamais rendus en HTML brut. |
| Droits sur le système | Ceux du profil IBM i utilisé pour la connexion, et rien d'autre. L'application n'élève jamais les privilèges. |
| Modifications | L'envoi, la réponse, la suppression et l'envoi d'un message d'interruption ouvrent chacun une fenêtre de confirmation. Un message d'interruption exige en plus de saisir le nom du poste. |
| Bandeau d'avertissement | Les fenêtres de confirmation signalent les suppressions, une connexion de production, et un profil qui détient `*ALLOBJ` ou `*SECADM`. |
| Espace de travail temporaire | Un espace utilisateur dans `QTEMP`, propre au job IBM i de la connexion et supprimé après usage. |
| Serveur web | Hors développement : redirection HTTPS, HSTS, et page d'erreur sans détail technique. |

### Limites à connaître avant tout déploiement partagé

**L'application n'a pas d'authentification propre.** Quiconque atteint l'URL peut saisir une connexion. La seule barrière est le profil IBM i et son mot de passe. Cela convient à un poste ou à une démonstration, **pas à un serveur exposé** sur un réseau.

**Les connexions enregistrées ne sont pas cloisonnées par personne.** Elles sont stockées par clé d'utilisateur (`App_Data/{clé}.json`), mais l'implémentation fournie (`LocalCurrentUser`) renvoie une constante : sur un serveur partagé, tous les utilisateurs verraient les mêmes connexions enregistrées (hôte, profil et options, jamais les mots de passe).

**Le formulaire de connexion accepte n'importe quel nom d'hôte.** Sur un serveur accessible à d'autres, n'importe qui pourrait s'en servir pour tenter d'atteindre toute machine que le serveur lui-même peut joindre.

Le point de remplacement pour les deux premières limites est fourni : `ICurrentUser` est une interface enregistrée dans `Program.cs`. Pour un usage partagé, placez l'application derrière une vraie authentification et fournissez une implémentation qui renvoie l'identité de l'utilisateur. Aucun autre fichier n'a besoin de changer.

## 8. Architecture du code

Un seul projet, `NTiInbox` (Blazor Server, .NET 10). Chaque dossier répond à une question, et les dépendances vont dans un seul sens : l'interface appelle `Messages` et `UserInterface`, qui appellent `Connection` et `Storage`, qui s'appuient sur `SystemApis`. `Localization` est lu par toutes les couches. Chaque fichier contient un seul type et porte son nom.

```
NTiInbox/
├── Connection/     La connexion IBM i : la session de l'utilisateur, les connexions enregistrées
├── SystemApis/     Les API système IBM i appelées via NTi, une classe par API
├── Messages/       La logique des messages, sans écran : façade, mise en forme, filtres, état de la boîte
├── UserInterface/  Notifications, demandes de dialogue, notifications navigateur, thème
├── Storage/        Fichiers JSON locaux, et qui est l'utilisateur courant
├── Localization/   Service de langue, dictionnaires anglais et français, textes d'erreur
├── Components/     Interface Blazor (pages, composants partagés, mises en page)
├── wwwroot/        app.css (une seule feuille de style, thèmes clair et sombre par variables CSS, toutes les classes préfixées nti-), logo, petits scripts
├── Program.cs
├── appsettings.json
└── Dockerfile
```

### Connection : où vit la connexion

| Fichier | Rôle |
|---|---|
| `IbmiSession.cs` | La connexion IBM i de l'utilisateur (NTi), ses identifiants en mémoire, et le chemin unique par lequel passe tout appel système |
| `ConnectionRequest.cs` | Paramètres d'une connexion. Le mot de passe reste en mémoire uniquement |
| `ConnectionException.cs` | Échec de connexion, débarrassé de tout identifiant |
| `ConnectionProfile.cs`, `ConnectionProfileStore.cs` | Connexions enregistrées (jamais le mot de passe) |
| `IbmiOptions.cs` | Valeurs par défaut facultatives du formulaire (section `Ibmi`) |

### SystemApis : les appels système

| Fichier | API IBM i | Rôle |
|---|---|---|
| `MessageListApi.cs`, `QueueMessage.cs` | `QMHLSTM` | Lister les messages d'une file |
| `MessageDescriptionApi.cs`, `MessageDescription.cs` | `QMHRTVM` | Lire un message prédéfini (texte, aide, variables) |
| `SendMessageApi.cs`, `SendMessageRequest.cs` | `QMHSNDM` | Envoyer un message vers une ou plusieurs files |
| `BreakMessageApi.cs`, `BreakMessageRequest.cs` | `QMHSNDBM` | Envoyer un message d'interruption |
| `ReplyMessageApi.cs` | `QMHSNDRM` | Répondre à un message d'interrogation |
| `RemoveMessageApi.cs` | `QMHRMVM` | Retirer un message d'une file |
| `QueueAttributesApi.cs`, `QueueAttributes.cs` | `QMHRMQAT` | Lire les attributs d'une file |
| `QueueListApi.cs`, `QueueListEntry.cs` | `QUSLOBJ` | Lister les objets `*MSGQ` d'une bibliothèque |
| `JobInfoApi.cs`, `JobInformation.cs`, `JobRef.cs` | `QUSRJOBI` | Lire un job, et la référence à un job |
| `UserInfoApi.cs`, `UserInformation.cs` | `QSYRUSRI` | Lire le profil utilisateur connecté |
| `UserSpaceApi.cs` | `QUSCRTUS`, `QUSRTVUS` | Créer et lire les espaces utilisateur temporaires |
| `MessageQueueRef.cs` | n/a | Nom et bibliothèque d'une file, clé de toute opération sur une file |
| `ApiErrorCode.cs`, `IbmiApiException.cs` | n/a | Structure d'erreur des API, et l'exception qui en découle |
| `NTiConnectionExtensions.cs` | n/a | `EnsureOpen()` : ouvre la connexion seulement si elle n'est pas déjà ouverte |

Les API qui renvoient une liste passent par un espace utilisateur, comme l'exige l'IBM i.

### Messages : la logique

| Fichier | Rôle |
|---|---|
| `MessageQueueService.cs` | Façade unique sur la connexion : découverte, lecture, aide, envoi, réponse, suppression |
| `MessageFormatter.cs`, `JobFormatter.cs` | Libellés, gravité et classes de style des messages et des jobs |
| `MessageGrouping.cs`, `MessageGroup.cs` | Regroupement des messages identiques |
| `QueueActivity.cs`, `ActivityBucket.cs` | Activité horaire d'une file, pour le graphique |
| `MessageFilter.cs`, `PeriodFilter.cs`, `ReplyStatusFilter.cs` | Filtres de la liste des messages |
| `SeverityLevel.cs` | Niveaux de gravité |
| `InboxState.cs` | Files, compteurs et file courante de la session |

### UserInterface, Storage et Localization

| Fichier | Rôle |
|---|---|
| `UserInterface/ToastService.cs`, `Toast.cs`, `ToastKind.cs` | Notifications de succès et d'erreur |
| `UserInterface/BrowserNotifier.cs`, `NotificationPermission.cs` | Notification navigateur pour les nouveaux messages en attente de réponse |
| `UserInterface/ThemeService.cs`, `AppTheme.cs` | Thème sombre et clair |
| `UserInterface/DialogService.cs` | Demandes d'ouverture des dialogues partagés |
| `Storage/JsonUserStore.cs` | Fichier JSON par utilisateur dans `App_Data`, générique selon la forme du fichier |
| `Storage/ICurrentUser.cs`, `LocalCurrentUser.cs` | Identité de l'utilisateur, une constante dans ce POC |
| `Localization/Localizer.cs` | Langue, recherche de traduction, pluriels, dates |
| `Localization/LocalizedComponentBase.cs` | Classe de base des composants, avec `L()` et `Plural()` |
| `Localization/Strings.cs`, `Strings.En.cs`, `Strings.Fr.cs` | Tables de traduction, mêmes clés dans les deux langues |
| `Localization/ErrorText.cs` | Texte traduit d'une exception, sans identifiant |
| `Localization/UserMessageException.cs` | Erreur métier portant une clé de traduction |

### Components

| Dossier | Rôle |
|---|---|
| `Layout/` | `MainLayout`, `SignInLayout`, `AppRail`, `AppHeader`, `QueueSidebar`, `ReconnectModal` |
| `Pages/` | `SignInPage`, `InboxPage`, `CatalogPage`, `ProfilePage`, `ErrorPage`, `NotFoundPage` |
| `Shared/` | Composants réutilisables : dialogues, liste et détail des messages, graphique, icônes, bascules |

### Principes de conception

**Une session par circuit Blazor.** `IbmiSession` porte la connexion, émet `Changed` et ferme la connexion à sa libération.

**Un seul chemin vers l'IBM i.** Tout appel passe par `IbmiSession.RunAsync`, qui l'exécute hors du thread de rendu et un à la fois, car une connexion NTi ne supporte pas deux appels simultanés.

**`EnsureOpen()` au début de chaque appel système**, car NTi 5 lève une exception si `Open()` est appelé sur une connexion ouverte.

**Les erreurs reviennent sous forme d'identifiants de message.** Le paramètre de code erreur de chaque API est une structure binaire dont « bytes provided » vaut 16 : l'API renvoie l'identifiant du message (`CPF...`) au lieu de remonter une exception.

**Les espaces utilisateur sont temporaires.** Ils vivent dans `QTEMP` sous des noms propres au produit et sont supprimés après usage. Une liste qui ne tient pas est relue avec un espace utilisateur plus grand.

**Le texte affiché est construit au moment du rendu.** Les formateurs reçoivent les valeurs brutes du système et le service de langue : un changement de langue n'affiche donc jamais un texte périmé.

## 9. Étendre le projet

Le projet est conçu pour recevoir de nouvelles briques. Le schéma habituel pour ajouter une fonction qui interroge l'IBM i :

1. **Appel système** : ajoutez une classe statique dans `SystemApis/` avec une méthode d'extension de `NTiConnection` (voir `UserInfoApi.cs` comme modèle). Commencez par `conn.EnsureOpen()`, puis appelez l'API avec `CallProgram` et contrôlez la structure d'erreur avec `ApiErrorCode.ThrowIfError`.
2. **Type** : si l'appel renvoie des données, ajoutez une classe à côté de l'API dans `SystemApis/`, dans son propre fichier.
3. **Façade** : exposez l'appel dans `Messages/MessageQueueService.cs`, via `connection.RunAsync(...)`, pour que l'interface n'accède jamais directement à la connexion.
4. **Interface** : créez une page ou un composant dans `Components/`. Les composants héritent de `LocalizedComponentBase` : utilisez `L("clé")` pour chaque texte visible.
5. **Textes** : ajoutez les clés dans `Strings.En.cs` et `Strings.Fr.cs`. Utilisez `clé.one` et `clé.other` pour les pluriels.

Pour les mots-clés de connexion, les types de paramètres et des exemples d'appels de programmes et d'API système, reportez-vous à la [documentation NTi](https://www.documentation.aumerial.com/) (en particulier le tutoriel « Call a system API »).

## 10. Résolution de problèmes

| Symptôme | Cause probable | Piste |
|---|---|---|
| La connexion échoue avec une erreur de connexion | Services TCP arrêtés, port bloqué ou mauvais nom d'hôte | Vérifier `STRTCPSVR` (étape 2) et les pare-feu sur 8471, 8475 et 8476 |
| La connexion échoue avec un message de licence | Licence NTi absente, expirée ou liée à une autre partition | Vérifier la licence dans la bibliothèque `KNTI`, la renouveler depuis le portail client |
| La connexion échoue avec l'option SSL/TLS | Aucun certificat configuré pour les serveurs hôtes, ou ports 94xx bloqués | Configurer DCM sur l'IBM i et ouvrir 9471, 9475 et 9476, ou décocher l'option sur un réseau de confiance |
| Mot de passe refusé | Profil désactivé, mot de passe expiré ou erroné | Vérifier le profil avec `DSPUSRPRF` |
| `CPF2433` sur une file comme `QHST` | L'API de listage refuse certaines files système | Utiliser `DSPLOG` sur un écran 5250 |
| `CPF2451` à la suppression d'un message | La file est allouée à un job interactif (`*BREAK` ou `*NOTIFY`) | Vérifier le mode de livraison de la file, ou fermer la session interactive |
| `CPF9810` à la recherche des files | La bibliothèque n'existe pas | Vérifier le champ bibliothèque de l'écran de connexion ou de la colonne de gauche |
| Aucune file trouvée | Bibliothèque vide ou aucun droit dessus | Essayer `*LIBL` ou une autre bibliothèque, vérifier les droits du profil |
| « Non connecté » sur une page | La session a été perdue (redémarrage, longue inactivité) | Se reconnecter |
| `dotnet build` échoue sur le paquet NuGet | Pas d'accès à NuGet | Configurer l'accès à `nuget.org` ou à un flux interne |
| `dotnet --version` n'affiche pas 10.x | SDK .NET 10 absent | Installer le SDK depuis <https://dotnet.microsoft.com/download> |
| Des clés brutes comme `inbox.refresh` s'affichent | Une clé manque dans un dictionnaire | L'ajouter dans `Strings.En.cs` et `Strings.Fr.cs` |
| Les textes des messages sont dans la mauvaise langue | Ils viennent de la langue du job IBM i | Vérifier les paramètres de langue du profil sur le système |

## 11. Limites connues

- **Messages les plus récents seulement** : une file est lue jusqu'à ses 300 messages les plus récents. Le compteur et le graphique d'activité l'indiquent quand la file en contient davantage.
- **Certaines files système ne se listent pas** : l'API de listage refuse des files comme `QHST` (voir [Résolution de problèmes](#10-résolution-de-problèmes)).
- **La lecture est périodique** : la file ouverte est relue toutes les 30 secondes, pas en temps réel.
- **Suppression de nombreux messages** : l'API système retire un message par appel, donc supprimer une grande sélection fait un appel par message.
- **Un seul destinataire pour un message `*INQ`**, et cinquante files au plus pour les autres types.
- **Une réponse laisse le message dans la file**, marqué comme répondu. L'application ne le retire pas.
- **Pas d'authentification applicative** ni de cloisonnement des données par utilisateur (voir [Sécurité](#7-sécurité)).
- **Langues mélangées à l'écran** : les textes renvoyés par l'IBM i suivent la langue du job IBM i, pas la langue choisie dans l'interface (voir [Langue](#5-langue)).

## 12. Support et conditions d'utilisation

Ce projet est une démonstration fournie par Aumerial. Ses conditions d'utilisation sont celles communiquées avec la licence d'essai NTi, complétées par l'avertissement en tête de ce document.

Le code source de ce projet est publié sous [licence MIT](LICENSE). Le paquet `Aumerial.Data.Nti` et la clé de licence NTi n'en font pas partie et restent soumis à leurs propres conditions.

- Documentation NTi : <https://www.documentation.aumerial.com/>
- Portail client et licences : <https://portal.aumerial.com/>
- Paquet NuGet : <https://www.nuget.org/packages/Aumerial.Data.Nti>
- Contact : <contact@aumerial.com>

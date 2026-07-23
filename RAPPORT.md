# Le Codex de l'Architecte
## Rapport de projet — Jeu vidéo éducatif pour l'apprentissage de la programmation

*[Nom, établissement, année académique — à compléter]*

---

## Sommaire

1. Introduction — Pourquoi ce projet ?
2. Recherche et conception
3. Développement du jeu
4. Difficultés rencontrées
5. Points d'amélioration
6. Conclusion
7. Références

---

## 1. Introduction — Pourquoi ce projet ?

L'apprentissage de la programmation est l'un des plus grands obstacles rencontrés
par les étudiants en première année d'informatique. Non pas parce que la syntaxe
d'un langage serait insurmontable, mais parce que les **concepts fondamentaux
restent abstraits** : qu'est-ce qu'une variable ? Où « vit » réellement une
donnée ? Que se passe-t-il, concrètement, lorsqu'on écrit `int x = 4;` ou
`z = Int32.Parse(y);` ? Pour beaucoup, le code reste une suite d'incantations
dont le fonctionnement interne demeure invisible.

L'idée de ce projet est née de ce constat. Nous avons voulu créer un outil qui
**rende visible et tangible** ce qui se passe à l'intérieur de la machine
lorsqu'un programme s'exécute. Plutôt que d'expliquer la mémoire vive avec un
schéma au tableau, pourquoi ne pas laisser l'étudiant **s'y promener** ? Plutôt
que de décrire le rôle du processeur, pourquoi ne pas l'y faire **entrer** ?

C'est ainsi qu'est né *Le Codex de l'Architecte* : un **jeu vidéo éducatif en 3D**
dans lequel le joueur incarne un agent miniaturisé, parachuté sur une **carte
mère géante**. Sa mission : exécuter physiquement un programme, ligne après
ligne, en se déplaçant entre les différents composants de l'ordinateur — la
**RAM** où sont rangées les variables, le **CPU** qui lit et calcule, l'**écran**
qui affiche, le **clavier** qui reçoit la saisie de l'utilisateur.

L'objectif pédagogique est double :

- **Ancrer les concepts par l'action.** En transportant lui-même une valeur du
  clavier jusqu'à la mémoire, l'étudiant comprend viscéralement la différence
  entre une *variable* (un contenant qui reste en mémoire) et une *valeur* (le
  contenu, qui voyage). Cette distinction, souvent source de confusion, devient
  une évidence physique.

- **Dédramatiser l'erreur et motiver.** Le jeu guide, corrige avec bienveillance
  (l'« humour de l'OS »), récompense la progression par des badges et un système
  de notation, et transforme un cours parfois intimidant en une aventure.

Le public cible est l'**étudiant débutant** en informatique, mais l'approche se
veut suffisamment ludique pour intéresser un public plus large et plus jeune.

*[Capture d'écran : l'écran-titre du jeu]*

---

## 2. Recherche et conception

Avant d'écrire la première ligne de code, une phase de recherche et de réflexion
a été nécessaire pour poser des bases solides.

### 2.1 Étude de l'existant

Nous avons d'abord étudié les jeux et outils éducatifs qui abordent la
programmation, afin d'en tirer les bonnes idées et d'en éviter les écueils :

- **Human Resource Machine** et **7 Billion Humans** (Tomorrow Corporation) — des
  puzzles où l'on programme de petits personnages avec des instructions
  élémentaires. Ils démontrent qu'il est possible d'enseigner l'algorithmique
  (variables, boucles, conditions) sans jamais afficher de code intimidant, en
  passant par la manipulation d'objets. Ce fut une inspiration majeure pour
  l'idée de **manipuler physiquement les données**.

- **Shenzhen I/O** et **TIS-100** (Zachtronics) — des jeux qui plongent le joueur
  dans l'architecture matérielle (registres, mémoire, entrées/sorties). Ils nous
  ont confortés dans l'idée qu'une **métaphore matérielle** (la carte mère, le
  CPU, la RAM) est à la fois juste et pédagogiquement puissante.

- **CodeCombat**, **Scratch** — des approches plus scolaires, qui montrent
  l'importance d'une **progression graduelle** et d'un accompagnement constant du
  débutant.

De cette étude, nous avons retenu trois principes directeurs :
1. rendre les concepts **physiques et manipulables** ;
2. **cacher la complexité** du langage derrière des actions concrètes ;
3. **accompagner** le joueur pas à pas, sans jamais le laisser bloqué.

### 2.2 Choix de la plateforme de développement

Plusieurs moteurs de jeu ont été envisagés :

| Moteur | Avantages | Raisons du choix / rejet |
|---|---|---|
| **Unity** | Écosystème mûr, langage C# (cohérent avec le sujet enseigné), immense documentation, gratuit pour un usage éducatif, multiplateforme | **Retenu** |
| Unreal Engine | Rendu photoréaliste | Surdimensionné pour un jeu éducatif ; C++ plus lourd à prendre en main |
| Godot | Léger, open source | Écosystème et documentation moins fournis à l'époque du choix |

Le choix s'est porté sur **Unity** (version 6000.4.3f1, dite « Unity 6 »), avec le
pipeline de rendu **URP** (Universal Render Pipeline) et le nouveau système
d'entrées (*New Input System*). Un argument décisif : Unity utilise le **C#**,
c'est-à-dire précisément le langage que le jeu cherche à enseigner — une cohérence
appréciable.

### 2.3 Choix des ressources (assets)

Pour le personnage et ses déplacements, nous avons adopté le pack officiel
**Starter Assets — ThirdPersonController** de Unity (contrôleur à la troisième
personne, animations de marche/course/saut, caméra **Cinemachine**). Cela a
permis de disposer immédiatement d'un personnage jouable et de concentrer l'effort
sur le cœur pédagogique du jeu.

Pour les éléments de décor et les objets manipulés (les boîtes-variables, le
modèle de RAM, le clavier, l'écran plat, le processeur, le portail), des modèles
3D libres ont été importés. **Un principe fort a toutefois guidé le
développement : tout ce qui pouvait être généré par code l'a été** — l'interface,
le décor de la carte mère, les sons, la musique, la clôture de l'environnement.
Ce choix a rendu le projet **quasi sans dépendances externes** : il suffit de
cloner le dépôt et d'ouvrir Unity pour que tout fonctionne, sans réglage manuel.

### 2.4 Maquettes et validation

Avant le développement complet, des **maquettes** (prototypes fonctionnels) ont
été réalisées pour matérialiser l'idée : une première scène où le joueur se
déplace, un premier prototype de « boîte-variable », un embryon de RAM. Ces
maquettes ont été présentées au professeur encadrant afin de **valider la
direction** avant d'investir des mois de développement. Cette validation obtenue,
le développement à proprement parler a pu commencer.

*[Capture d'écran : une des premières maquettes]*

---

## 3. Développement du jeu

Cette partie constitue le cœur du projet. Elle s'est étalée sur **plusieurs
mois** et représente la charge de travail la plus importante. Nous la
détaillerons selon quatre axes : l'architecture technique, le concept pédagogique
central, la structure du jeu (scènes et stations), et enfin le contenu (les
chapitres et les systèmes annexes).

### 3.1 Architecture technique générale

Le projet est organisé autour d'un **script central, `GameState`**, implémenté en
*singleton* persistant (patron *singleton* + `DontDestroyOnLoad`). Ce composant
survit aux changements de scène et centralise **tout l'état du jeu** : ce que le
joueur porte, le contenu de la mémoire, la progression dans le programme, le
score, la sauvegarde. Toutes les autres briques du jeu consultent et modifient cet
état unique, ce qui évite les incohérences.

Autour de ce noyau gravitent des **composants autonomes**, chacun responsable
d'un système précis et créé automatiquement au lancement :

- **Interface (HUD, marqueur d'objectif, journal, notifications, menu pause,
  minimap)** — entièrement générée par code ;
- **Audio (voix radio, humour de l'OS, musique, file d'attente vocale)** ;
- **Décor (clôture, composants de carte mère, easter egg)** ;
- **Aide (le drone d'assistance)**.

Chaque système suit le même patron : une méthode statique `Ensure()` qui le crée
s'il n'existe pas, et un `Awake()` qui garantit l'unicité. Cette régularité rend
le code **prévisible et facile à étendre** — un point important pour un projet
destiné à être repris et poursuivi.

Le jeu est découpé en **cinq scènes** reliées entre elles :

| Scène | Rôle |
|---|---|
| `MainScene` | Le monde ouvert (la carte mère) : le joueur s'y déplace entre les stations |
| `RAM` | La mémoire vive : déclaration, lecture et écriture des variables |
| `CPU` | L'**unité de contrôle** : lit le programme, révèle les instructions |
| `Calculateur` | L'**unité arithmétique et logique** : conversions, calculs, tests |
| `Clavier` | Le terminal de saisie (`Console.ReadLine`) |

*[Capture d'écran : vue d'ensemble de la carte mère (MainScene)]*

### 3.2 Le concept pédagogique central : VARIABLE ≠ VALEUR

Toute la conception du gameplay repose sur une **distinction fondamentale**,
exigée par la rigueur pédagogique :

| | **Variable** (une boîte) | **Valeur** (un contenu) |
|---|---|---|
| **Où vit-elle ?** | Dans la RAM, qu'elle ne quitte jamais | Elle voyage **sur la tête** du joueur |
| **Représentation** | Une boîte en carton, avec son nom, son type et sa valeur | Juste le texte de la valeur, coloré selon le type |
| **Comment naît-elle ?** | Par une **déclaration** (on choisit un type, un nom) | Par la lecture d'une variable, la saisie clavier, ou un calcul |
| **Lire** | Cliquer la boîte à mains vides → on emporte une **copie de sa valeur** | — |
| **Écrire (affectation)** | Arriver avec une valeur et cliquer la variable cible → une confirmation, puis la valeur y est rangée | — |

Ce modèle rend concrets des mécanismes qui restent d'ordinaire abstraits :
- une **déclaration** réserve une case mémoire (on voit la boîte apparaître dans
  la RAM) ;
- **lire** une variable ne la détruit pas (la boîte reste, on n'emporte qu'une
  copie) ;
- une **affectation** remplace le contenu (la valeur portée entre dans la boîte) ;
- le **CPU ne manipule que des valeurs**, jamais des boîtes — car un processeur
  calcule sur des données, pas sur des emplacements mémoire.

Chaque **type** possède sa couleur (échantillonnée sur les boîtes de la scène :
`int` en rouge, `float` en magenta, `string` en bleu, `bool` en noir, `char` en
bleu clair), et cette couleur suit la valeur partout où elle va — sur la tête du
joueur comme dans la mémoire.

*[Capture d'écran : le joueur portant une valeur ; une variable dans la RAM]*

### 3.3 Le CPU en deux unités

Fidèle à l'architecture réelle d'un processeur, le CPU est scindé en **deux unités
visuellement distinctes** :

- l'**unité de contrôle** (thème cyan) — c'est là que le joueur **lit le
  programme** ; chaque instruction lui est révélée une à une, l'obligeant à
  revenir consulter le CPU avant de pouvoir agir (comme un processeur qui charge
  l'instruction suivante) ;

- l'**unité arithmétique et logique (UAL)** (thème orange) — c'est là que
  s'effectuent les **conversions, additions et tests**. Fait notable : l'UAL
  **ignore la provenance et la destination** des données ; elle affiche seulement
  `... + ...`, jamais `somme = ...`, car un additionneur ne fait qu'additionner —
  c'est le programme (l'unité de contrôle) qui sait où ira le résultat.

Cette séparation, matérialisée par des couleurs, des bandeaux et des filigranes
opposés, ancre une notion d'architecture des ordinateurs souvent négligée.

*[Capture d'écran : l'unité de contrôle et l'unité arithmétique]*

### 3.4 Le programme du chapitre 1 (les six lignes de base)

Le premier chapitre fait exécuter au joueur un programme complet de six lignes,
qui couvre les fondamentaux :

| Ligne | Concept enseigné |
|---|---|
| `int x = 4;` | La **déclaration** d'une variable |
| `Console.WriteLine(x);` | L'**affichage** d'une valeur |
| `string y = Console.ReadLine();` | La **saisie** utilisateur au clavier |
| `int z = Int32.Parse(y);` | La **conversion** de type (texte → entier) |
| `int somme = x + z;` | Le **calcul** arithmétique |
| `Console.WriteLine(somme);` | L'affichage du résultat |

Chaque ligne se décompose en **étapes internes** que le joueur franchit
physiquement. Par exemple, la ligne 4 (`int z = Int32.Parse(y);`) se joue ainsi :
(1) déclarer la variable `z` dans la RAM ; (2) aller lire la valeur de `y` (on en
emporte une copie) et l'apporter à l'unité arithmétique, qui la convertit et rend
un entier ; (3) revenir ranger cet entier dans `z`. Le joueur *vit* la conversion
au lieu de la lire.

Un détail pédagogique important : à la ligne 3, la valeur récupérée au clavier
arrive **sans nom** — c'est seulement en la rangeant dans `y` qu'elle acquiert son
identité, illustrant que `y = Console.ReadLine()` est une **affectation**.

*[Capture d'écran : le programme affiché sur le CPU]*

### 3.5 Le chapitre 2 : la condition `if` — les deux portes

Le second chapitre introduit la **structure conditionnelle** avec une mise en
scène marquante. La ligne à exécuter est :

```csharp
if (somme > 50) { Console.WriteLine("grand"); } else { Console.WriteLine("petit"); }
```

Le déroulé pédagogique :
1. Le joueur apporte la valeur de `somme` à l'unité arithmétique, qui **évalue le
   test** sous ses yeux (`67 > 50 → VRAI`) et lui rend un **booléen** (`true` ou
   `false`) — matérialisant que *le résultat d'une condition est une valeur de
   type `bool`*.
2. De retour dans le monde, **deux portes** se dressent devant l'écran : l'une
   pour la branche `VRAI → "grand"`, l'autre pour `FAUX → "petit"`.
3. **Seule la porte correspondant au booléen s'ouvre.** L'autre est murée :
   s'en approcher rappelle qu'*« une branche dont le test échoue ne s'exécute
   jamais »*.
4. Traverser la bonne porte **exécute la branche** : l'écran affiche le message.

Comme la valeur de `somme` dépend du nombre aléatoire saisi au chapitre 1,
**chaque partie emprunte une branche différente** — la meilleure démonstration
possible de ce qu'est une condition.

*[Capture d'écran : les deux portes de la condition]*

### 3.6 Les systèmes annexes

Autour de ce cœur pédagogique, de nombreux systèmes enrichissent l'expérience :

- **La voix radio.** Un « centre de contrôle » guide le joueur à chaque
  instruction, avec une **voix neuronale de synthèse** (fr-FR, générée hors ligne
  via *edge-tts*), bien plus naturelle qu'une voix robotique classique. Les
  consignes ne se déclenchent qu'à la **sortie du CPU**, une fois l'instruction
  lue.

- **Le guidage et le verrouillage.** Le joueur ne peut pas « sauter » une étape :
  tant qu'il n'a pas lu la prochaine instruction au CPU, les stations refusent
  d'interagir. S'il se rend à la mauvaise station, un message le **réoriente**
  (« Rien à faire ici, va plutôt vers… »). Le HUD, un **marqueur d'objectif 3D**
  et une **minimap** indiquent en permanence où aller.

- **Le drone d'aide.** Un drone flottant, activable à la demande, explique le
  **concept** de la ligne en cours et rappelle les règles du jeu — un filet de
  sécurité pour l'étudiant qui bloque.

- **La progression.** Un système de **badges** (un par notion apprise), de
  **notation** par ligne (rapidité, absence d'erreur) et de **skins** débloquables
  récompense l'apprentissage. Un **rapport élève** exportable permet au professeur
  de suivre les progrès.

- **L'ambiance.** Écran-titre avec caméra orbitale, cinématique d'introduction
  survolant les stations, musique composée par code, décor procédural de carte
  mère (puces, condensateurs, pistes de cuivre), clôture de l'environnement, et
  même un **easter egg pédagogique** : un insecte caché rappelant le premier
  « bug » de l'histoire de l'informatique (Grace Hopper, 1947).

- **L'accessibilité.** Un « mode Zen » retire la pression du chronomètre, les
  messages d'erreur sont bienveillants, et l'ensemble est en français.

*[Capture d'écran : le HUD, la minimap, une notification, le drone d'aide]*

### 3.7 Sauvegarde et robustesse

La progression du joueur (ligne courante, contenu de la mémoire, score, badges,
réglages) est **sauvegardée automatiquement** (via `PlayerPrefs`). Le joueur peut
quitter et **reprendre exactement où il s'était arrêté**, sans revoir la
cinématique d'introduction. Une grande attention a été portée à la robustesse :
gestion des changements de scène, périodes de « grâce » pour éviter des messages
intempestifs, compatibilité des anciennes sauvegardes lorsque le programme
s'allonge.

---

## 4. Difficultés rencontrées

Le développement, étalé sur plusieurs mois, n'a pas été sans obstacles. Les
principales difficultés ont été :

- **La traduction d'un concept abstrait en gameplay.** La distinction
  variable/valeur, évidente sur le papier, a demandé de **nombreuses itérations**
  avant de trouver sa forme jouable. Les premières versions faisaient transporter
  des boîtes au joueur, ce qui brouillait le message ; la solution — les
  *variables restent en RAM, seules les valeurs voyagent* — n'a émergé qu'après
  plusieurs refontes.

- **La gestion de l'état entre cinq scènes.** Faire communiquer proprement le
  monde, la RAM, le CPU, l'UAL et le clavier, tout en gardant un état cohérent, a
  nécessité une architecture centralisée et de nombreux ajustements (bugs de
  synchronisation, objets détruits au changement de scène, etc.).

- **Les enchaînements audio et cinématiques.** Faire coïncider l'écran-titre, la
  cinématique, la musique et la voix radio sans qu'ils se chevauchent a demandé la
  mise en place d'une **file d'attente vocale** et d'un séquencement précis.

- **L'adaptation à l'affichage.** Le passage en plein écran révélait des problèmes
  de cadrage (scène RAM trop zoomée), corrigés par une adaptation dynamique de la
  caméra au format de l'écran.

- **Des problèmes techniques ponctuels** : un fichier source Blender bloquant
  l'import faute de Blender installé, des chevauchements de texte dans l'interface,
  des messages d'erreur mal synchronisés — autant de petits obstacles résolus un à
  un.

Chacune de ces difficultés a été l'occasion d'**apprendre** : sur Unity, sur
l'architecture logicielle, et sur la conception pédagogique.

---

## 5. Points d'amélioration

Le jeu, bien que pleinement fonctionnel pour son objectif (le chapitre 1 et le
début du chapitre 2), offre de nombreuses perspectives d'évolution :

- **Le design visuel.** L'esthétique actuelle, largement générée par code, est
  fonctionnelle mais perfectible. Un travail artistique (textures, éclairage,
  effets) rendrait l'univers plus immersif et plus proche d'un « vrai » jeu.

- **Le chapitre 2 et au-delà.** La condition `if` est en place ; il reste à
  développer pleinement les **boucles** (`for`, `while`), dont toute la logique est
  déjà présente dans le code en réserve, prête à être réactivée. Au-delà, on
  pourrait aborder les **tableaux**, les **fonctions**, les **objets**.

- **Enrichissement pédagogique.** Ajout de niveaux « bac à sable » où l'étudiant
  compose librement son programme, de défis chronométrés, d'un mode « débogueur »
  permettant d'avancer pas à pas dans l'exécution.

- **Diffusion.** Un export **WebGL** permettrait de jouer directement dans un
  navigateur, sans installation — idéal pour un usage en salle de classe.

- **Accessibilité et internationalisation.** Sous-titres, mode daltonien,
  traduction en plusieurs langues.

Ces pistes sont documentées afin qu'un futur développeur puisse **reprendre et
poursuivre** le projet aisément.

---

## 6. Conclusion

*Le Codex de l'Architecte* est né d'une conviction : **on apprend mieux en
faisant**. En transformant les concepts abstraits de la programmation — variables,
mémoire, conversion, condition — en actions physiques au sein d'une carte mère
géante, le jeu offre à l'étudiant débutant une compréhension **intuitive et
durable** de ce qui se passe réellement lorsqu'un programme s'exécute.

Le projet a représenté un travail conséquent, étalé sur plusieurs mois, mêlant
**conception pédagogique**, **développement logiciel** et **création
d'expérience**. Au-delà du résultat — un jeu jouable, guidé, récompensant —, il
aura été une formidable occasion d'apprentissage : de Unity et du C#, de
l'architecture d'une application complète, et de l'art délicat de rendre
l'abstrait tangible.

Le socle est solide, l'architecture pensée pour l'extension, et les fondations du
chapitre 2 posées. *Le Codex de l'Architecte* ne demande qu'à grandir — chapitre
après chapitre, concept après concept — pour accompagner toujours plus loin celles
et ceux qui font leurs premiers pas dans le monde du code.

*[Capture d'écran : l'écran de fin / le rating]*

---

## 7. Références

### Documentation technique
- **Unity — Documentation officielle**, Unity Technologies. https://docs.unity3d.com
- **Unity — Universal Render Pipeline (URP)**, documentation. https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest
- **Unity — Input System**, documentation. https://docs.unity3d.com/Packages/com.unity.inputsystem@latest
- **Unity — Starter Assets: ThirdPerson**, Unity Asset Store.
- **Unity — Cinemachine**, documentation.
- **Microsoft — Documentation C#** (types, `Console`, `Int32.Parse`…). https://learn.microsoft.com/dotnet/csharp
- **edge-tts** — bibliothèque de synthèse vocale neuronale. https://pypi.org/project/edge-tts

### Jeux et outils ayant inspiré le projet
- **Human Resource Machine** / **7 Billion Humans** — Tomorrow Corporation.
- **Shenzhen I/O** / **TIS-100** — Zachtronics.
- **CodeCombat** — CodeCombat Inc.
- **Scratch** — MIT Media Lab.

### Culture informatique
- L'anecdote du premier « bug » (papillon dans le Harvard Mark II, équipe de
  **Grace Hopper**, 1947), reprise comme easter egg pédagogique.

---

*Rapport rédigé dans le cadre du projet Le Codex de l'Architecte. Les captures
d'écran illustrant chaque section sont insérées aux emplacements indiqués.*

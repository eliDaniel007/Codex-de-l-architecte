# Le Codex de l'Architecte — Documentation développeur

Jeu éducatif Unity (3D, troisième personne) : le joueur, agent sur une carte
mère géante, exécute **physiquement** un programme C# de 6 lignes (chapitre 1) —
déclaration, affichage, saisie, conversion, calcul. Chaque concept a un lieu :
la **RAM** (les variables), le **CPU** (contrôle + calcul), l'**écran**
(l'affichage), le **clavier** (la saisie).

- **Moteur** : Unity `6000.4.3f1` (Unity 6), rendu **URP**, New Input System
- **Contrôleur joueur** : StarterAssets ThirdPersonController (Cinemachine)
- **Dépôt** : https://github.com/eliDaniel007/Codex-de-l-architecte
- **Langue du code** : français (noms, commentaires) — garder cette convention.

---

## 1. Installer l'environnement de développement

1. Installer **Unity Hub** puis, depuis le Hub, Unity **6000.4.3f1**
   (cocher le module *Windows Build Support — IL2CPP/Mono*).
2. `git clone https://github.com/eliDaniel007/Codex-de-l-architecte`
3. Unity Hub → **Add project from disk** → choisir le dossier cloné.
4. Ouvrir `Assets/Scenes/MainScene.unity` → **Play**. C'est tout :
   **aucune dépendance externe** (UI, décor, sons et musique sont générés par
   code ; les voix sont des .mp3 déjà présents dans le dépôt).

## 2. Exporter un build jouable (Unity 6 : « Build Profiles »)

1. **File ▸ Build Profiles** (Ctrl+Shift+B).
2. Dans *Platforms*, choisir **Windows** (doit afficher `Active` —
   sinon *Switch Platform*).
3. Vérifier la **Scene List** (partagée) :
   `MainScene (0), RAM, Clavier, CPU, Calculateur` — **MainScene doit être à
   l'index 0** (c'est la scène de démarrage). L'ordre des autres est libre.
4. Bouton **Build** (en bas à droite) → choisir un dossier **vide**
   (ex : `Builds/Windows`). *Build And Run* = pareil + lance le jeu.
5. Distribuer **le dossier entier** (le `.exe` + le dossier `..._Data` +
   `UnityPlayer.dll`). Un zip suffit — aucune installation requise côté élève.

> **Web (navigateur)** : dans *Platforms*, choisir **Web** (installer le module
> si besoin) → Switch Platform → Build. La sauvegarde (PlayerPrefs) fonctionne ;
> l'export du rapport élève écrit alors dans le stockage du navigateur — à
> tester avant un déploiement en classe.

---

## 3. LA règle de conception : VARIABLE ≠ VALEUR

Tout le gameplay repose sur cette distinction (exigence pédagogique) :

| | Variable (boîte en carton) | Valeur (texte flottant) |
|---|---|---|
| **Où ?** | Vit dans la **RAM**, ne la quitte jamais | Voyage **sur la tête** du joueur |
| **Visuel** | Boîte carton, textes nom/valeur/type colorés | Juste le texte de la valeur, coloré par type |
| **Naît** | Formulaire de déclaration (clic sur une boîte de type) | Lecture d'une variable, ReadLine au clavier, résultat du CPU |
| **Lecture** | Clic mains vides → **copie de la valeur** part avec toi | — |
| **Écriture** | Valeur en main + **clic sur la variable cible** → dialogue **OUI/NON** (c'est l'affectation) → sauvegarde auto | — |

- Le **CPU ne manipule que des VALEURS** (jamais de boîtes).
- **Couleurs des types** : échantillonnées au chargement de la scène RAM sur le
  **texte des boîtes de type** (bool, char, int, float, string) →
  `GameState.DefinirCouleurType()` / `CouleurType()`. Variables et valeurs
  partagent exactement ces RGB (palette de secours codée en dur si absent).
- Le CPU a **deux unités visuellement distinctes** :
  - **UNITÉ DE CONTRÔLE** (scène `CPU`, thème **cyan**) : lit le programme,
    révèle les lignes (briefing), affiche l'état.
  - **UNITÉ ARITHMÉTIQUE ET LOGIQUE** (scène `Calculateur`, thème **orange**) :
    convertit (Parse) et calcule. Elle **ignore la provenance et la
    destination** des valeurs : elle affiche `... + ...`, jamais `somme = `.

## 4. Le programme (chapitre 1 — 6 lignes) et son déroulé physique

| # | Ligne | Déroulé (missionEtape entre parenthèses) |
|---|---|---|
| 1 | `int x = 4;` | RAM : clic boîte **int** → formulaire nom=x, valeur=4 → sauvegarde |
| 2 | `Console.WriteLine(x);` | RAM : clic **x** (copie de valeur) → l'écran l'affiche |
| 3 | `string y = Console.ReadLine();` | (0) déclarer y vide → (1) **clavier principal** `[E]` → valeur **SANS NOM** → (2) RAM : clic **y** (affectation) |
| 4 | `int z = Int32.Parse(y);` | (0) déclarer z vide → (1) copie de y → **UAL** convertit → entier nu → (2) RAM : clic **z** |
| 5 | `int somme = x + z;` | (0) déclarer somme vide → (1) copie de x → UAL `4 + ...` → (2) copie de z → résultat nu → (3) RAM : clic **somme** |
| 6 | `Console.WriteLine(somme);` | RAM : clic **somme** (copie) → écran → **CHAPITRE 1 TERMINÉ** + rating |

**Le clavier** ne sert QUE au `Console.ReadLine()` de la ligne 3, et seul le
**clavier principal** (le plus éloigné du portail RAM) répond. L'ancienne scène
`Clavier` (terminal de déclaration) n'est **plus jamais chargée** — conservée
dans le build par compatibilité, supprimable.

### Chapitres 2-3 : retirés mais prêts
`if / for / while` (lignes 7-9) ont été retirés de la campagne (demande du
professeur). **Leur logique complète est dormante dans le code** :
`QuestKind.ConditionIf / Boucle / TantQue`, branches de `CpuRecevoir`,
`BoucleCpu()`, routage `CPUZone`, affichages `CalculateurController`, badges
`Logicien`/`Boucleur`, voix `m7/m8/m9.mp3`. Le commentaire dans
`GameState.InitQuests()` explique la réactivation : rajouter les quêtes et
**migrer leurs échanges vers les valeurs nues** (`boxEstValeur`), sur le modèle
des lignes 3-5.

## 5. Architecture du code (`Assets/Scripts`)

### 5.1 Le cœur : `Mission/GameState.cs`
Singleton `GameState.I` (`DontDestroyOnLoad`, auto-créé au premier accès).

**État principal :**
| Champ | Rôle |
|---|---|
| `boxExists` | quelque chose est porté |
| `boxEstValeur` | vrai = valeur nue (texte), faux = boîte-variable |
| `boxValue`, `boxType` | contenu porté |
| `boxVariable` | provenance (`"x"`, `"y"`... ; `""` = clavier ou calcul CPU) |
| `boxVientDeRam` | la valeur vient d'une lecture RAM (exigé par l'UAL) |
| `ramSlots` | la mémoire (List\<RamSlot\> : filled, variable, value, type, color) |
| `quests`, `questIndex` | le programme et la ligne active |
| `missionEtape` | l'étape interne de la ligne active (voir tableau §4) |
| `missionRevelee` | dernière ligne révélée au CPU (briefing) |

**Méthodes clés :**
- `DeclarerEnRam(type, nom, valeur)` — déclaration (valeur vide permise) ;
  contient les hooks d'étape (y→ligne 3, z→ligne 4, somme→ligne 5).
- `PrendreSlot(i)` — LIRE : copie de la valeur en main (nue).
- `DeposerValeurDansSlot(i)` — ÉCRIRE : vérifie la cible attendue par la
  mission, écrit, complète la ligne. Retourne `(ok, message)`.
- `SaisirLigne(valeur)` — ReadLine du clavier → valeur nue sans nom.
- `CpuRecevoir()` — l'UAL : Parse (ligne 4) et addition (ligne 5).
- `IndicationActuelle()` — consigne du HUD par ligne + étape.
- `CompleterQueteActuelle()` — rating, badges, bannière, sauvegarde.
- `Sauvegarder()/Charger()` — PlayerPrefs (voir §7).

### 5.2 Ajouter une ligne au programme (recette)
1. Nouveau `QuestKind` + la quête dans `InitQuests()` (titre = la ligne de
   code ; description = le commentaire `//`).
2. Les étapes dans `IndicationActuelle()` + le commentaire d'étapes (§ « Étapes
   internes »).
3. Selon le besoin : hook de déclaration (`DeclarerEnRam`), logique UAL
   (`CpuRecevoir`), cible d'écriture (`DeposerValeurDansSlot`), affichage
   (`ConsoleScreen`), saisie (`KeyboardTerminal`).
4. Ciblage du losange (`ObjectiveMarker.Retrouver`), routage CPU (`CPUZone` :
   quelle box/étape part vers l'UAL).
5. Badge éventuel (`Badges.LigneTerminee`), voix `Assets/Resources/Voix/mN.mp3`
   (§8) — `VoiceOver` la joue automatiquement au briefing (nom = `m` + numéro).

### 5.3 Les singletons UI (créés par `GameState.Awake`)
| Script | Rôle |
|---|---|
| `MissionHUD` | bandeau LIGNE n/6 + consigne (F9 = reset) |
| `ObjectiveMarker` | losange 3D au-dessus de la cible + distance |
| `VoiceOver` | radio (briefings m1..m6, intro, fin) |
| `EcranTitre` | menu titre + caméra orbitale ; **REPRENDRE ne rejoue pas la cinématique** |
| `BriefingCinematic` | survol CPU→clavier→RAM→écran (nouvelles campagnes uniquement) |
| `PauseMenu` | pause, **Mode Zen** (note sans chrono), sélecteur de skins |
| `NotificationsUI` | toasts + **voix off** (badge, note1-3, chap1) |
| `JournalMissions` | touche J : programme, stats par ligne, badges, **EXPORTER LE RAPPORT** |
| `HumourOS` | répliques ironiques sur erreur (err1-3, cooldown 25 s) |
| `MiniCarte` | minimap : CPU/RAM/ÉCRAN/CLAVIER + flèche joueur + objectif |
| `SkinRobot` | teinte du robot débloquée par badges (2/4/6/8/10) |
| `BanniereChapitre` | bannière dorée de fin de chapitre |
| `MusiqueOuverture` | musique du titre + cinématique (composée par code) |
| `ScreenShake` | secousse caméra à la validation |

### 5.4 Composants auto-créés dans la MainScene (`RuntimeInitializeOnLoadMethod`)
| Script | Rôle |
|---|---|
| `ClotureEnvironnement` | enceinte sur les 4 bords réels de la carte mère + murs invisibles |
| `DecorCarteMere` | ~190 composants électroniques (puces, condos, pistes...) placés hors des zones de jeu |
| `DroneAide` | **l'aide** : drone flottant près du spawn, `[E]` → concept + consigne + règles |
| `BugDeHopper` | easter egg : insecte caché (badge « Chasseur de bugs ») |

### 5.5 Scripts de scène (posés dans les .unity)
| Script | Scène | Rôle |
|---|---|---|
| `CPUZone` | MainScene | entrée CPU : contrôle (défaut) ou UAL (si valeur attendue) |
| `LoadSceneOnPlayerEnter` | MainScene | portail vers la RAM |
| `KeyboardTerminal` | MainScene | `Console.ReadLine()` de la ligne 3 (clavier principal) |
| `ConsoleScreen` | MainScene | `Console.WriteLine` : affiche la valeur posée |
| `RAMSceneController` | RAM | disposition `___1___2___3___` (3 cases égales/tablette), échantillonnage couleurs, messages, **dialogue OUI/NON** |
| `RAMBoxSelector` | RAM | clic boîte : type→formulaire, variable→lire ou écrire |
| `RamDeclarationUI` | RAM | formulaire de déclaration (type présélectionné) |
| `CPUSceneController` | CPU | unité de contrôle (le programme) |
| `CalculateurController` | Calculateur | l'UAL (thème orange, `... + ...`) |
| `PlayerHolder` / `PickupItem` / `DataBox` | — | portage sur la tête |

## 6. Les badges (chapitre 1)
Un badge **par notion** + performance + secret — voir `Badges.Tous` :
`Première variable`, `Afficheur`, `À l'écoute`, `Convertisseur`, `Calculateur`,
`Sans faute`, `Exécution éclair`, `Programme exécuté`, `Compilation parfaite`,
`Chasseur de bugs` (10). Débloqués via `Badges.LigneTerminee(kind)` /
`MissionTerminee` / `CampagneTerminee` / `ChasseurDeBug`.
Ils alimentent les **skins** (`SkinRobot.Skins`). `Recommencer` efface tout.

## 7. Sauvegarde (PlayerPrefs, clés `cda_*`)
| Clé | Contenu |
|---|---|
| `cda_actif`, `cda_version` (=2) | sauvegarde présente / format |
| `cda_questIndex`, `cda_completes`, `cda_etape`, `cda_revelee` | progression |
| `cda_ram` | cases RAM sérialisées (`filled|nom|valeur|type|couleur` par ligne) |
| `cda_erreurs`, `cda_temps`, `cda_stat_0..5` | score, temps, meilleur score par ligne |
| `cda_calc` | valeurs mémorisées par l'UAL (cpuX, cpuZ, cpuSomme, cpuY) |
| `cda_badge_*`, `cda_skin`, `cda_zen` | badges, skin choisi, mode Zen |

Au chargement, si le programme s'est allongé depuis la sauvegarde, on avance
automatiquement à la première ligne non terminée.

## 8. Les voix (edge-tts, gratuites)
Voix neuronale **fr-FR-DeniseNeural**, `.mp3` dans `Assets/Resources/Voix/` :
`intro`, `m1`…`m6` (+ `m7-m9` en réserve), `fin`, `err1-3`, `badge`,
`note1-3`, `chap1-3`. Régénérer :
```bash
pip install edge-tts
python -m edge_tts --voice fr-FR-DeniseNeural --rate=+4% \
       --text "Nouvelle réplique." --write-media "Assets/Resources/Voix/m1.mp3"
```
`VoiceOver` charge par nom — remplacer le fichier suffit.

## 9. Dépannage
- **Rien ne se passe au clavier pendant la ligne 3** → c'est le clavier
  PRINCIPAL qui répond (le plus loin du portail RAM), pas le doublon.
- **Les couleurs des types semblent fausses** → elles sont échantillonnées dans
  la scène RAM : vérifier que chaque boîte de type a un TMP dont le texte est
  exactement `int`, `float`, `string`, `bool` ou `char`.
- **La cinématique rejoue à chaque reprise** → elle ne doit jouer que si
  `missionRevelee < 0` (voir `BriefingCinematic.CampagneDejaCommencee`).
- **Réinitialiser complètement un poste** → touche F9 en jeu, ou supprimer les
  clés `cda_*` (regedit : `HKCU\Software\<Company>\<Product>`).
- **Erreur UnityConnect au démarrage de l'éditeur** → problème de compte
  Unity/services, pas du projet (se reconnecter ou ignorer).

## 10. Touches
| Touche | Action |
|---|---|
| ZQSD/WASD + souris | déplacement / caméra |
| E | interagir (aide, ReadLine, examiner) |
| J | journal + export rapport élève |
| Échap | pause (Mode Zen, skins) / fermer un panneau |
| Espace | passer la cinématique |
| F9 | réinitialiser la campagne |

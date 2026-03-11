# Env_Multiplayer_Unity

> 🔗 **Dépôt GitHub du projet** : [https://github.com/Rachidjunior/Env_Multiplayer_Unity](https://github.com/Rachidjunior/Env_Multiplayer_Unity)

## Description

Mise en place d'un environnement virtuel Unity multijoueur dans lequel plusieurs utilisateurs peuvent se connecter simultanément, naviguer dans une scène partagée et interagir avec des objets en temps réel — les actions de chaque utilisateur étant visibles par les autres. Le projet intègre également un support VR pour les casques Meta Quest 2.

---

## Technologies utilisées

- **Unity 6000.3.6f1**
- **C#**
- **Netcode for GameObjects** `2.9.2` — gestion du réseau multijoueur
- **Unity Services Multiplayer** `2.1.2` — gestion des sessions cloud
- **XR Interaction Toolkit** — interaction et navigation en réalité virtuelle
- **Oculus XR Plugin** — support des casques Meta Quest
- **Unity Cloud / Distributed Authority** — topologie réseau décentralisée

---

## Étapes réalisées

### 1 — Projet en mode Distributed Authority
Création d'un projet Unity 3D connecté à Unity Cloud. Mise en place d'un `NetworkManager` configuré en topologie **Distributed Authority**. Création d'un script `ConnectionManager.cs` permettant à un utilisateur de choisir son nom de profil et le nom de la session à rejoindre. Création d'un prefab `PlayerCube` avec `NetworkObject` et `PlayerCubeController.cs` (dérivant de `NetworkTransform`) pour synchroniser les déplacements entre clients.

### 2 — Navigation dans l'environnement partagé
Remplacement du `PlayerCube` par un prefab `NetworkedPlayer` plus évolutif (objet vide + capsule 3D + `NetworkObject`). Création du script `PlayerController.cs` gérant :
- La **navigation** (translation avant/arrière, rotation gauche/droite via clavier)
- L'**attachement automatique de la caméra** au joueur local à l'apparition (`CatchCamera`)
- La **récupération manuelle de la caméra** via la touche **C** (utile lors de la connexion d'un client VR)

### 3 — Interaction avec des objets partagés
Ajout d'un **curseur 3D** (`Cursor`) à l'intérieur du prefab `NetworkedPlayer`, contrôlable à la souris (mouvements gauche/droite/haut/bas + molette pour la profondeur) via `CursorDriver.cs`. Création du script `NetworkedInteractionTool.cs` pour préparer les interactions réseau. Ajout d'un composant `XRDirectInteractor` sur le curseur pour attraper des objets interactifs.

Création d'un prefab `SharedCube` avec `NetworkObject`, `NetworkTransform` et `XRGrabInteractable` pour rendre des objets manipulables dans l'environnement partagé. Implémentation d'un mécanisme de **spawn dynamique** d'objets partagés (touche **P**).

### 4 — Awareness d'interaction
Amélioration du comportement lors de la saisie d'objets : création de `NetworkedXRDirectInteractor.cs` (dérivant de `XRDirectInteractor`) pour préserver l'**offset de saisie** entre le curseur et l'objet attrapé.

Création de `NetworkedXRGrabInteractable.cs` (dérivant de `NetworkBehaviour`) pour fournir un **retour visuel** par changement de couleur :
- **Cyan** → curseur en intersection avec l'objet (hover)
- **Jaune** → objet saisi

Ces changements de couleur sont propagés à tous les clients via des appels **RPC** (`ShowCaughtRpc`, `ShowReleasedRpc`, `ShowCatchableRpc`, `HideCatchableRpc`).

### 5 — Support VR (Meta Quest 2)
Import du package **Oculus XR Plugin**. Création d'une scène dédiée `VRScene` avec un `XR Origin (XR Rig)` transformé en prefab **VRPlayer** (avec `NetworkObject` et `NetworkTransform`). Création d'un `VRConnectionManager.cs` (basé sur `ConnectionManager.cs`) avec une méthode `JoinSharedWorld()` permettant de rejoindre le monde partagé sans interface clavier. Ajout d'un objet interactif dans la `VRScene` déclenchant cette connexion par désignation.

Configuration d'un profil de build **Android** (IL2CPP, ARM64, API 29+) pour déployer l'application sur casque Meta Quest 2 via SideQuest ou ADB.

---

## Scripts créés

| Script | Rôle |
|---|---|
| `ConnectionManager.cs` | Connexion à une session partagée (desktop) |
| `PlayerCubeController.cs` | Déplacement d'un cube synchronisé sur le réseau |
| `PlayerController.cs` | Navigation + gestion caméra du joueur local |
| `CursorDriver.cs` | Pilotage du curseur 3D à la souris |
| `NetworkedInteractionTool.cs` | Base réseau pour l'outil d'interaction |
| `NetworkedXRDirectInteractor.cs` | Interacteur XR avec préservation de l'offset de saisie |
| `NetworkedXRGrabInteractable.cs` | Objet interactif avec retour visuel synchronisé (RPC) |
| `VRConnectionManager.cs` | Connexion au monde partagé depuis un casque VR |

---

## Lancer le projet

### Mode Desktop (PC)
1. Ouvrir la scène principale dans Unity
2. Lancer le projet
3. Renseigner un **nom de profil** et un **nom de session** puis cliquer sur *Create or Join Session*
4. Répéter sur une autre instance pour simuler un second utilisateur

### Mode VR (Meta Quest 2)
1. Installer le APK sur le casque via un build depuis le PC
2. Lancer d'abord une session sur le PC
3. Dans le casque, désigner l'objet **JoinButton** pour rejoindre la session
4. Les deux clients se voient et peuvent interagir dans le monde partagé
5. Sur le desktop, appuyer sur **C** pour récupérer la caméra si nécessaire

> ⚠️ Le PC et le casque doivent être sur le **même réseau WiFi**. L'adresse IP du PC doit être renseignée dans tous les `NetworkManager` du projet.

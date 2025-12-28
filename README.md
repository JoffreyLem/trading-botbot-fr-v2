# RobotBlazorApp - Version 2

> ⚠️ **Note importante** : Il s'agit de la **Version 2** du projet. Une **Version 3** plus récente existe et peut être disponible sur demande.

## Description

RobotBlazorApp V2 est une plateforme de trading algorithmique complète développée en .NET avec une interface Blazor. Le système permet de concevoir, tester et exécuter des stratégies de trading automatisées avec support de backtesting et trading en temps réel.

## Architecture

Le projet suit une architecture modulaire avec les composants suivants :

### Backend (.NET)
- **Robot.Server** : Serveur principal avec API REST et SignalR Hubs
- **Robot.DataBase** : Couche d'accès aux données avec Entity Framework
- **Robot.Mail** : Service de notification par email

### Libraries de Trading
- **RobotAppLibraryV2** : Bibliothèque principale de trading
- **RobotAppLibraryV2.Api.Xtb** : Intégration avec le broker XTB
- **RobotAppLibraryV2.ApiConnector** : Connecteurs API génériques
- **RobotAppLibraryV2.ApiHandler** : Gestion des communications API
- **RobotAppLibraryV2.Backtest** : Moteur de backtesting
- **RobotAppLibraryV2.Indicators** : Indicateurs techniques
- **RobotAppLibraryV2.CandleList** : Gestion des données de chandeliers
- **RobotAppLibraryV2.MoneyManagement** : Gestion du risque et du capital
- **RobotAppLibraryV2.PositionHandler** : Gestion des positions
- **RobotAppLibraryV2.StrategyDynamiqCompiler** : Compilation dynamique de stratégies

### Frontend
- **frontv2.client** : Application React/TypeScript avec Vite

## Prérequis

- .NET 6.0 ou supérieur
- Node.js 16+ et npm/yarn
- Docker et Docker Compose (pour le déploiement)
- SQL Server ou PostgreSQL (pour la base de données)

## Installation

### Développement local

1. **Cloner le repository**
```bash
git clone https://gitlab.com/tradingbot3941224/robotapp.git
cd robotapp
```

2. **Configuration de la base de données**
```bash
cd Robot.DataBase
dotnet ef database update
```

3. **Configuration des variables d'environnement**
Créer un fichier `appsettings.Development.json` dans Robot.Server avec vos paramètres.

4. **Lancer le backend**
```bash
cd Robot.Server
dotnet run
```

5. **Lancer le frontend**
```bash
cd frontv2.client
npm install
npm run dev
```

### Déploiement avec Docker

```bash
# Développement
docker-compose -f docker-compose-dev.yml up

# Production
docker-compose up -d
```

## Fonctionnalités principales

- ✅ Création et gestion de stratégies de trading personnalisées
- ✅ Backtesting avec données historiques
- ✅ Trading en temps réel via API XTB
- ✅ Indicateurs techniques intégrés
- ✅ Gestion du risque et money management
- ✅ Interface utilisateur Blazor moderne
- ✅ Notifications par email
- ✅ Communication temps réel via SignalR
- ✅ Compilation dynamique de stratégies

## Structure des données

Le système utilise Entity Framework Core avec migrations pour gérer la structure de la base de données. Les modèles principaux se trouvent dans `Robot.DataBase/Modeles/`.

## Tests

```bash
# Exécuter tous les tests
dotnet test

# Tests spécifiques
cd RobotAppLibraryV2.Tests
dotnet test
```

## Technologies utilisées

- **Backend** : .NET 6+, ASP.NET Core, Entity Framework Core
- **Frontend** : React, TypeScript, Vite
- **Temps réel** : SignalR
- **Base de données** : SQL Server / PostgreSQL
- **Conteneurisation** : Docker
- **API Trading** : XTB API

## API et Intégrations

Le système est conçu pour s'intégrer avec plusieurs brokers. Actuellement, l'intégration XTB est pleinement implémentée via le module `RobotAppLibraryV2.Api.Xtb`.

## Contribution

Ce projet est en version 2. Pour les nouvelles fonctionnalités, veuillez consulter la Version 3.

## Licence

**Copyright © 2025 - Tous droits réservés / All Rights Reserved**

⚠️ **CE DÉPÔT EST PUBLIC POUR CONSULTATION UNIQUEMENT** ⚠️

Bien que ce dépôt soit accessible publiquement, **AUCUNE licence d'utilisation n'est accordée**. L'accès public ne confère AUCUN droit d'utilisation, de copie, de modification ou de distribution du code.

### Termes légaux

Ce logiciel est protégé par les lois sur le droit d'auteur et la propriété intellectuelle. **Tous les droits sont expressément réservés**.

**UTILISATIONS STRICTEMENT INTERDITES sans autorisation écrite préalable :**

- ❌ Copier, reproduire ou dupliquer le code (en tout ou partie)
- ❌ Modifier, adapter ou créer des œuvres dérivées
- ❌ Distribuer, publier ou partager le code
- ❌ Utiliser à des fins commerciales
- ❌ Utiliser à des fins personnelles ou non commerciales
- ❌ Incorporer dans d'autres projets ou logiciels
- ❌ Forker ou cloner le repository
- ❌ Rétro-ingénierie, décompilation ou désassemblage
- ❌ Extraire ou réutiliser des portions du code

### Utilisation autorisée

La **SEULE** utilisation autorisée est :
- ✅ Consultation du code à des fins éducatives ou de référence
- ✅ Lecture de la documentation à des fins d'information

### Conséquences légales

Toute violation de ces termes constitue une violation du droit d'auteur et peut entraîner :
- Des poursuites judiciaires civiles et/ou pénales
- Des réclamations pour dommages et intérêts
- Des mesures d'injonction
- Tout autre recours juridique disponible

### Demande de licence

Pour toute demande d'autorisation d'utilisation ou de licence commerciale, veuillez contacter le propriétaire via les canaux officiels du repository.

**Voir le fichier [LICENSE](LICENSE) pour les termes complets en français et anglais.**
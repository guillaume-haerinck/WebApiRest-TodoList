# TP3 Sécurité réseau UQAC

Web API de TodoList avec authentification par Token JWT (ne possède pas d'interface graphique ou de client)

## Mise en route
### Build serveur

Il est possible de compiler depuis Visual Studio ou par ligne de commande.
Dans ce second cas, avec dotnet d'installé, utilisez cette commande à l'intérieur de /src/TodoApi

```
dotnet run
```

### Requettes clients

Il faut simuler un client en utilisant un logiciel comme [Postman](https://www.getpostman.com/).
Un fichier Json de postman est présent à la racine avec les requêttes déja crées.

Il faut commencer par enregistrer un utilisateur
```
POST /api/users
```

L'application est conçue pour répondre aux requettes suivantes pour l'utilisateur loggé :

Pour lister les utilisateurs
```
GET /api/users
```

Pour ajouter un item todo à l'utilisateur
```
POST /api/todos
```

Pour récuperrer tous les todos de l'utilisateur
```
GET /api/users
```

Pour récuperrer tous les todos de l'utilisateur
```
DELETE /api/todos/GUID
```

Pour récuperrer tous les todos de l'utilisateur
```
PATCH /api/todos/GUID
```

## Dévellopé avec

* [.NET Core 2.0](https://www.microsoft.com/net/download/windows) - Le framework C# utilisé
* [Visual Studio 2017](https://www.visualstudio.com/) - IDE utilisé

## Auteurs

* **Guillaume Haerinck** 
* **Azis Tekaya**
* **Alexandre Noret**

## License

Ce projet est licencié sous la licence MIT

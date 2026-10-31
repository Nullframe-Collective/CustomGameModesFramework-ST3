# CustomGameModesFramework

## 1. Overview

Framework automatically scans loaded assemblies in the `AppDomain`, discovers classes inheriting from `CustomGameMode`, and registers them.

To develop a mode:

* Reference `CustomGameModesFramework.dll`.
* Create a class inheriting from `CustomGameMode`.
* Decorate the class with the `GameModeInfo` attribute.

---

## 2. GameModeInfo Attribute

### Constructor and Parameters

```csharp
[GameModeInfo("mode_id", "Mode Name", "12", "4", "8", "12", BaseMode = "SBX")]
```

1. **`modeID`** — Identifier (must not conflict with reserved native IDs: `COOP`, `VS`, `SUR`, `INF`, `SBX`, `THR`, `DM`, `TDM`).
2. **`modeName`** — Display name shown in game.
3. **`players`** — Maximum player limit (range: `"1"` to `"32"`).
4. **`smallMapPlayerCount`** — Player limit for small maps.
5. **`mediumMapPlayerCount`** — Player limit for medium maps.
6. **`largeMapPlayerCount`** — Player limit for large maps.
7. **`BaseMode`** — Underlying native mode behavior. Allowed values: `COOP`, `VS`, `SUR`, `INF`, `SBX`, `THR`, `DM`, `TDM`. Default: `SBX`.

---

## 3. CustomGameMode Base Class

### Properties

* `GameModeContext Context` — Current match and network context.
* `GameModeSettings Settings` — Mode configuration settings.
* `bool UseNativeHUD` — Whether to use the standard HUD (default: `true`).
* `bool UseNativeRespawnGUI` — Whether to use the standard respawn UI (default: `true`).

### Lifecycle Hooks

* **Configuration:**
* `public virtual void ConfigureSettings(GameModeSettings settings)` — Called during registration to configure custom in-game UI settings.

* **Match and Room Events:**
* `public virtual void OnStart(RoomMultiplayerMenu instance)`
* `public virtual void OnSetupRoom(RoomMultiplayerMenu instance)`
* `public virtual void OnStartGame(RoomMultiplayerMenu instance)`
* `public virtual void OnGUI(RoomMultiplayerMenu instance)`
* `public virtual void OnFixedUpdate(RoomMultiplayerMenu instance)`
* `public virtual void OnSpawnPlayer(RoomMultiplayerMenu instance)`
* `public virtual void OnQuitRoom(RoomMultiplayerMenu instance)`
* `public virtual void OnRoundEnded(RoomMultiplayerMenu instance)`
* `public virtual void OnRestart(RoomMultiplayerMenu instance)`
* `public virtual void LobbyMenuOptions(LobbyMenu instance)`

* **Player Events:**
* `public virtual void OnRespawnPlayerGUI(RagdollController instance)`
* `public virtual void OnRespawnPlayer(RagdollController instance)`

---

## 4. GameModeContext

### Properties

* `string ModeID` — Current mode id.
* `string BaseMode` — Underlying base mode.
* `bool IsHost` — Returns true if the local player is the room host or playing in offline mode.
* `GameModeSettings Settings`
* `RoomMultiplayerMenu Room`
* `NetworkManager Network`
* `RelayTransport Relay`

### Methods

* `void SpawnLocalPlayer(string teamName = "")`

---

## 5. GameModeCatalog

Static helper class for checking framework status:

* `int ApiVersion` — Current API version (returns `1`).
* `bool IsReady` — Returns true if the framework is fully initialized.
* `bool Enabled` — Gets or sets whether the game mode catalog is enabled.

---

## 6. Example Mode

```csharp
using CustomGameModesFramework;
using Il2Cpp;

[GameModeInfo("my_custom_mode", "My Custom Mode", "16", "4", "8", "16", BaseMode = "SBX")]
public class MyCustomGameMode : CustomGameMode
{
    public override void ConfigureSettings(GameModeSettings settings)
    {
        // Custom settings displayed in the game UI here
    }

    public override void OnStartGame(RoomMultiplayerMenu instance)
    {
        MelonLoader.MelonLogger.Msg("Custom game mode started!");
        
        if (Context.IsHost)
        {
            // Host only logic
        }
    }

    public override void OnSpawnPlayer(RoomMultiplayerMenu instance)
    {
        // Player spawn logic
    }
}
```

This is only source, the .dll mod on our [Discord](https://discord.com/invite/B4KR8Ae4te) server.

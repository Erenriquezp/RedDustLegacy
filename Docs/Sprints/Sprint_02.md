# Sprint 02 — Level01 jugable, enemigo bioluminiscente y audio

> **Estado:** 🔄 En progreso · **Revisión:** 2026-07-06 (T4 núcleo de audio cerrado: Mixer + `MusicState`) · **Ref. GDD:** §8, §9.2, §13, §14

Geometría de Level01, animaciones del rover, Ser Bioluminiscente y audio.

## Progreso

| Tarea | Estado |
|-------|--------|
| T3 — Ser Bioluminiscente | ✅ Completo (ahora **derrotable con dash**) |
| T4 — Audio (capa SFX) | ✅ Rover, enemigos y UI |
| Deuda S01 — bug `OnDashed` | ✅ Resuelto |
| T2 — Flip de dirección del rover | ✅ Resuelto (por escala) |
| T5 — Geometría Level01 | 🔄 En `Level01.unity`; faltan marcadores/zonas y arreglos de capas/Tag |
| T4 — Audio (Mixer + MusicState) | ✅ Núcleo hecho (2026-06-30/07-02); los disparadores de gameplay quedan en S04 T4.2 |
| T2 — Animaciones Land/Damage | 🔴 Faltan sprites + clips |
| T1 — Parallax | ✅ Hecho — scroll por offset de textura (2 capas) en Level01; centrado/cobertura/orden corregidos; scripts muertos eliminados |
| Deuda S01 — bindings | 🔴 Jump/Scan sin ajustar |

## Hecho

- **Ser Bioluminiscente** (`BioluminescentAI`): FSM Idle/Alert/Chase, ataque, `ApplyStun`, HP/muerte, aturdimiento al escanear. SFX 3D vía `EnemyAudioController`. Ahora **daña al contacto con i-frames+knockback** y es **derrotable embistiéndolo con dash** (ver S03 — combate). Corregido el AnimationEvent vacío de `BiolAttack.anim`.
- **Flip de dirección del rover**: `PlayerAnimatorController` voltea por **escala** del sprite (no `flipX`, que no se reflejaba) usando el input en vivo. Gira con A/D.
- **Geometría** consolidada en `Level01.unity` (Grid + 6 tilemaps, Player, Biol, Drone) — escena promovida desde `Level01_2.0`.
- **Mecánicas de nivel** (`Scripts/leveo01/`): `PlataformaMovil` (arrastra al Player), `GiroCompleto`, `OsciladorGiro`, trampas `CaidaCristal`/`CaidaPorCercania` y `HazardDamage` (pinchos/obstáculos) — todas dañan con i-frames+knockback.
- **Audio SFX**: `PlayerAudioController` (dash/landing/daño-por-fase/muerte), `EnemyAudioController`, `UIAudioController`.
- **Deuda S01**: bug `OnDashed→PlayDamageSound` corregido (`sfxDash` propio).
- **Audio Mixer + música adaptativa (núcleo T4)** (2026-06-30/07-02, `feature/audio-system` PRs #22/#23): `Assets/Audio/MainMixer.mixer` con `Master → {Music, SFX, Ambient, UI}` y volúmenes expuestos (`MusicVol`/`SfxVol`/`AmbientVol`/`UiVol`) + setters 0–1→dB en `AudioManager` (`SetMusicVolume`, etc.). `enum MusicState { Silence, Exploration, Tension, Combat, Cinematic }` y `SetMusicState()` con doble `AudioSource` A/B y crossfade según GDD §14.1 (1,5 / 0,8 / 0,5 s) + pistas `Bgm_Tension`/`Bgm_Combat`/`Bgm_Cinematic`. Extras: `RestartExplorationMusicOnCheckpoint()` (restaura la mezcla del editor al reaparecer), `PauseAudioTrigger` (silencia SFX/Ambient a −80 dB en pausa) y sonidos de Game Over.
- **SFX de escaneo del rover**: `PlayerController` ahora emite `OnScanStarted`/`OnScanStopped` y `PlayerAudioController` reproduce el loop del escáner (`sfxScanStart`/`sfxScanStop`) sin pisar el motor Idle/Walk.
- **Parallax de fondo (Level01)** (`ParallaxBackground.cs`, clase `ParallaxMovement`): scroll por **offset de textura** sobre 2 capas hijas del objeto `Background`; el contenedor sigue a la cámara (X e Y) para mantenerse centrado. Corregidos los 4 bugs visuales — descentrado (antes fijaba la Y propia y restaba `-1` en X), cobertura (planos centrados `Position X/Y = 0` y agrandados `Scale Z = 1.8`), huecos al saltar/caer (`[DefaultExecutionOrder(1000)]` para correr **después** de Cinemachine y no quedar un frame atrás) y profundidad (`sortingOrder = -10` por código + `OnValidate` para previsualizar en editor, porque el `MeshRenderer` no expone *Order in Layer* en el Inspector). Eliminados los scripts muertos `Parallax.cs`, `ParallaxCamera.cs`, `ParallaxLayer.cs`.
  - **Desviación del GDD §9.1** (3 capas por *factor de profundidad*): se implementó con **2 capas por offset de textura**, que es lo que ya estaba montado y funciona con la cámara **ortográfica** (la Z no da profundidad en orto). Si se quiere fidelidad al GDD, añadir una 3.ª capa hija a `Background`. **Tuning:** `parallaxSpeed` (intensidad global), `verticalParallax`, `textureProperty` (`_MainTex` legacy / `_BaseMap` URP) y `sortingOrder`. Las texturas de fondo deben estar en **Wrap = Repeat**. Reutilizable tal cual para el fondo de **Level02** (S05 T2).

## Pendiente

### T4 — Audio Mixer y estados de música — ✅ núcleo cerrado; queda 1 fleco (→ S04 T4.2)

El Mixer, el ruteo y el crossfade adaptativo están hechos (ver **Hecho**). Único pendiente, que se cierra en **S04 T4.2**: **conectar los disparadores de `SetMusicState` desde gameplay** — hoy nadie lo llama fuera del arranque (`Start → Exploration`). IA Biol/Drone en `Alert`/`Chase` → `Tension`; boss activo o varios en `Chase` → `Combat`; SI ≥41 % y sin enemigos → `Exploration`.

### T2 — Animaciones Land / Damage (el flip ya está hecho)
- Sprites `Opportunity-land` (4f) y `Opportunity-damage` (5f).
- Clips `Rover_Land`/`Rover_Damage` en `RoverAC` (Duration 0, Has Exit Time OFF, Write Defaults OFF). El trigger `IsDamaged` ya se dispara desde código (S03); falta el clip. `Wall_Slide`/`Wall_Jump` → V2.
- **Deuda del controlador:** `Rover_Death` no tiene transición de salida; hoy se sale por código (`Animator.Play("Rover_Idle")` al revivir). Conviene añadir la transición `Rover_Death → Rover_Idle` con condición `IsDead = false`.

### T5 — Geometría (cierre)
- Colocar `LevelMarker` (SC-01/02/03, Checkpoints, BlockedZone, BossRoom) y cerrar zonas Z1→Z5. Guía: [T5_Geometria_Level01.md](./T5_Geometria_Level01.md).
- **Arreglar config de la escena** (Tag del Player, capas de plataformas, máscara de suelo) → ver **[Integracion_Level01.md](./Integracion_Level01.md)**.

### Deuda S01 — bindings
- `RoverInputActions_Local.inputactions`: sin cambios — Jump sigue en `W`/`↑` (GDD pide `Espacio`) y Scan sigue en `E` (GDD pide `F`). Ojo: el audio de escaneo nuevo ya se construyó asumiendo la tecla `E`; decidir si se actualiza el GDD o el binding antes de tocar nada.

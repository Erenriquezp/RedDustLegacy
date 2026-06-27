# Sprint 02 — Level01 jugable, enemigo bioluminiscente y audio

> **Estado:** 🔄 En progreso · **Revisión:** 2026-06-27 (T1 Parallax cerrado y corregido) · **Ref. GDD:** §8, §9.2, §13, §14

Geometría de Level01, animaciones del rover, Ser Bioluminiscente y audio.

## Progreso

| Tarea | Estado |
|-------|--------|
| T3 — Ser Bioluminiscente | ✅ Completo (ahora **derrotable con dash**) |
| T4 — Audio (capa SFX) | ✅ Rover, enemigos y UI |
| Deuda S01 — bug `OnDashed` | ✅ Resuelto |
| T2 — Flip de dirección del rover | ✅ Resuelto (por escala) |
| T5 — Geometría Level01 | 🔄 En `Level01.unity`; faltan marcadores/zonas y arreglos de capas/Tag |
| T4 — Audio (Mixer + MusicState) | 🔴 0% — sin `.mixer` ni `MusicState` (detalle abajo) |
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
- **Parallax de fondo (Level01)** (`ParallaxBackground.cs`, clase `ParallaxMovement`): scroll por **offset de textura** sobre 2 capas hijas del objeto `Background`; el contenedor sigue a la cámara (X e Y) para mantenerse centrado. Corregidos los 4 bugs visuales — descentrado (antes fijaba la Y propia y restaba `-1` en X), cobertura (planos centrados `Position X/Y = 0` y agrandados `Scale Z = 1.8`), huecos al saltar/caer (`[DefaultExecutionOrder(1000)]` para correr **después** de Cinemachine y no quedar un frame atrás) y profundidad (`sortingOrder = -10` por código + `OnValidate` para previsualizar en editor, porque el `MeshRenderer` no expone *Order in Layer* en el Inspector). Eliminados los scripts muertos `Parallax.cs`, `ParallaxCamera.cs`, `ParallaxLayer.cs`.
  - **Desviación del GDD §9.1** (3 capas por *factor de profundidad*): se implementó con **2 capas por offset de textura**, que es lo que ya estaba montado y funciona con la cámara **ortográfica** (la Z no da profundidad en orto). Si se quiere fidelidad al GDD, añadir una 3.ª capa hija a `Background`. **Tuning:** `parallaxSpeed` (intensidad global), `verticalParallax`, `textureProperty` (`_MainTex` legacy / `_BaseMap` URP) y `sortingOrder`. Las texturas de fondo deben estar en **Wrap = Repeat**. Reutilizable tal cual para el fondo de **Level02** (S05 T2).

## Pendiente

### T4 — Audio Mixer y estados de música (núcleo, 0%)

**Estado del código:** `AudioManager` (namespace `Core`) solo tiene `musicSource`, `sfxGlobalSource`, `PlayMusic(clip, loop)`, `TriggerGameOverMusic()` y `PlayGlobalSFX(clip)`. No hay `.mixer` ni `MusicState`.

**1 — Crear el Mixer** `Assets/Audio/MainMixer.mixer`:
- Grupos: `Master → { Music, SFX, Ambient }` (añade `UI` cuando llegue Sprint 04 T4).
- Expón el volumen de cada grupo (clic derecho en el slider del *Attenuation* → *Expose ... to script*) y renómbralos `MusicVol`, `SfxVol`, `AmbientVol`. El panel de Opciones (S04 T1) los moverá con `mixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Clamp(v01, 0.0001f, 1f)) * 20f)` (slider 0–1 → dB).
- Enrutar salidas (`outputAudioMixerGroup`): `musicSource` → **Music**; `sfxGlobalSource` + los `AudioSource` del rover (`PlayerAudioController`) y enemigos (`EnemyAudioController`) → **SFX**; el ambiente de nivel → **Ambient**.

**2 — Música adaptativa en `AudioManager`:**
- Añade un **segundo `AudioSource` (`musicSourceB`)** para cruzar A↔B sin cortes, y un clip serializado por estado.
- `enum MusicState { Silence, Exploration, Tension, Combat, Cinematic }` + `SetMusicState(MusicState s)`: si el estado cambia, arranca el clip nuevo en el source inactivo a volumen 0 y lerpea los volúmenes en una corutina durante el crossfade; al terminar, detén el source viejo.
- Tiempos de crossfade (**GDD §14.1**, tiempo para *entrar* a cada estado): `Exploration` **1.5 s** · `Tension` **0.8 s** · `Combat` **0.5 s** · `Cinematic` lo gestiona la cinemática (S04/S05).
- Disparadores (los conecta cada sistema): IA Biol/Drone en `Alert`/`Chase` → `Tension`; boss activo o varios en `Chase` → `Combat`; SI ≥41 % y sin enemigos → `Exploration`.

**DoD parcial T4:** los 3 buses se ajustan por separado desde código y la música cruza entre Exploration/Tension/Combat sin corte audible.

### T2 — Animaciones Land / Damage (el flip ya está hecho)
- Sprites `Opportunity-land` (4f) y `Opportunity-damage` (5f).
- Clips `Rover_Land`/`Rover_Damage` en `RoverAC` (Duration 0, Has Exit Time OFF, Write Defaults OFF). El trigger `IsDamaged` ya se dispara desde código (S03); falta el clip. `Wall_Slide`/`Wall_Jump` → V2.
- **Deuda del controlador:** `Rover_Death` no tiene transición de salida; hoy se sale por código (`Animator.Play("Rover_Idle")` al revivir). Conviene añadir la transición `Rover_Death → Rover_Idle` con condición `IsDead = false`.

### T5 — Geometría (cierre)
- Colocar `LevelMarker` (SC-01/02/03, Checkpoints, BlockedZone, BossRoom) y cerrar zonas Z1→Z5. Guía: [T5_Geometria_Level01.md](./T5_Geometria_Level01.md).
- **Arreglar config de la escena** (Tag del Player, capas de plataformas, máscara de suelo) → ver **[Integracion_Level01.md](./Integracion_Level01.md)**.

### Deuda S01 — bindings
- `RoverInputActions_Local.inputactions`: Jump `W`/`↑` → `Espacio`; Scan `E` → `F` (GDD §2).

# Sprint 02 — Level01 jugable, enemigo bioluminiscente y audio

> **Estado:** 🔄 En progreso · **Revisión:** 2026-06-26 · **Ref. GDD:** §8, §9.2, §13, §14

Geometría de Level01, animaciones del rover, Ser Bioluminiscente y audio.

## Progreso

| Tarea | Estado |
|-------|--------|
| T3 — Ser Bioluminiscente | ✅ Completo (ahora **derrotable con dash**) |
| T4 — Audio (capa SFX) | ✅ Rover, enemigos y UI |
| Deuda S01 — bug `OnDashed` | ✅ Resuelto |
| T2 — Flip de dirección del rover | ✅ Resuelto (por escala) |
| T5 — Geometría Level01 | 🔄 En `Level01.unity`; faltan marcadores/zonas y arreglos de capas/Tag |
| T4 — Audio (Mixer + MusicState) | 🔴 0% |
| T2 — Animaciones Land/Damage | 🔴 Faltan sprites + clips |
| T1 — Parallax | 🔴 Script listo, sin montar |
| Deuda S01 — bindings | 🔴 Jump/Scan sin ajustar |

## Hecho

- **Ser Bioluminiscente** (`BioluminescentAI`): FSM Idle/Alert/Chase, ataque, `ApplyStun`, HP/muerte, aturdimiento al escanear. SFX 3D vía `EnemyAudioController`. Ahora **daña al contacto con i-frames+knockback** y es **derrotable embistiéndolo con dash** (ver S03 — combate). Corregido el AnimationEvent vacío de `BiolAttack.anim`.
- **Flip de dirección del rover**: `PlayerAnimatorController` voltea por **escala** del sprite (no `flipX`, que no se reflejaba) usando el input en vivo. Gira con A/D.
- **Geometría** consolidada en `Level01.unity` (Grid + 6 tilemaps, Player, Biol, Drone) — escena promovida desde `Level01_2.0`.
- **Mecánicas de nivel** (`Scripts/leveo01/`): `PlataformaMovil` (arrastra al Player), `GiroCompleto`, `OsciladorGiro`, trampas `CaidaCristal`/`CaidaPorCercania` y `HazardDamage` (pinchos/obstáculos) — todas dañan con i-frames+knockback.
- **Audio SFX**: `PlayerAudioController` (dash/landing/daño-por-fase/muerte), `EnemyAudioController`, `UIAudioController`.
- **Deuda S01**: bug `OnDashed→PlayDamageSound` corregido (`sfxDash` propio).

## Pendiente

### T4 — Audio Mixer y estados de música (núcleo, 0%)
- `Assets/Audio/MainMixer.mixer`: Master → {Music, SFX, Ambient}; exponer `MusicVol`/`SfxVol`/`AmbientVol`; enrutar `AudioManager.musicSource`→Music y los AudioSources del rover/enemigos→SFX.
- `AudioManager`: `enum MusicState { Silence, Exploration, Tension, Combat, Cinematic }` + `SetMusicState(state)` con crossfade (Exploration→Tension 1.5 s · Tension→Combat 0.5 s · Combat→Exploration 3 s).

### T2 — Animaciones Land / Damage (el flip ya está hecho)
- Sprites `Opportunity-land` (4f) y `Opportunity-damage` (5f).
- Clips `Rover_Land`/`Rover_Damage` en `RoverAC` (Duration 0, Has Exit Time OFF, Write Defaults OFF). El trigger `IsDamaged` ya se dispara desde código (S03); falta el clip. `Wall_Slide`/`Wall_Jump` → V2.
- **Deuda del controlador:** `Rover_Death` no tiene transición de salida; hoy se sale por código (`Animator.Play("Rover_Idle")` al revivir). Conviene añadir la transición `Rover_Death → Rover_Idle` con condición `IsDead = false`.

### T1 — Parallax
- `ParallaxController` no está en ninguna escena. Montar 3 capas (factores 0.15 / 0.45 / 0.85) detrás del tilemap, con los fondos de `Art/Backgrounds/`.

### T5 — Geometría (cierre)
- Colocar `LevelMarker` (SC-01/02/03, Checkpoints, BlockedZone, BossRoom) y cerrar zonas Z1→Z5. Guía: [T5_Geometria_Level01.md](./T5_Geometria_Level01.md).
- **Arreglar config de la escena** (Tag del Player, capas de plataformas, máscara de suelo) → ver **[Integracion_Level01.md](./Integracion_Level01.md)**.

### Deuda S01 — bindings
- `RoverInputActions_Local.inputactions`: Jump `W`/`↑` → `Espacio`; Scan `E` → `F` (GDD §2).

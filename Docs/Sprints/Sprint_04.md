# Sprint 04 — Pantallas del sistema, Nivel 1 completo, Boss Leviatán y audio integral

> **Estado:** 🔄 En progreso · **Creado:** 2026-06-23 · **Revisión:** 2026-07-06 · **Prerequisito:** Sprint 03 · **Ref. GDD:** §8.3, §9.2, §10, §12.2, §14, §15.1 · **Ref. HUD:** §3, §4.1, §6, §10

Cierre del vertical slice del Nivel 1: pantallas de sistema (menú/carga/pausa), diseño completo del nivel (Z1→boss), Boss Leviatán y capa de audio integral.

## Progreso

| Tarea | Responsable | Estado |
|-------|-------------|--------|
| T1 — Pantallas (menú/carga/pausa) | Technical Director | 🟢 ~85% — flujo real menú→carga→nivel funcionando; falta panel de Opciones y portar el arte de pausa al HUD |
| T2 — Nivel 1 completo | Level Designer | 🟡 Base montada y probada; **es el bloqueante del sprint**: falta poblar todo el recorrido y montar la arena |
| T3 — Boss Leviatán | AI Programmer | 🟢 ~90% — lógica completa y compilando; falta colocarlo en la arena (depende de T2) y flecos de arte |
| T4 — Audio integral | Technical Director | 🟢 ~90% — núcleo, SFX y **música adaptativa completa** (Tension/Combat/Cinematic cableados); faltan clips del Leviatán y el ambiente |

## ✅ Hecho (resumen)

**T1 — Pantallas** (arte PR #25 + integración 2026-07-06): las 4 escenas (`MenuPrincipal`, `PantallaCargaNivel1/2`, `StopMenu`) funcionan de verdad — `SceneLoader` reescrito con **carga async** (barra real `AsyncOperation` + mínimo 2 s + auto-bootstrap), botones de menú y pausa **cableados por nombre** (`MainMenuController`/`PauseMenuController`, con SFX de UI y navegación teclado/gamepad), Build Settings corregidos (`MenuPrincipal → PantallaCargaNivel1 → Level01 → PantallaCargaNivel2 → Level02`), `GameManager` reconoce las escenas de UI y `MenuInicio.cs` (roto, cargaba `"Mundo1"`) eliminado.

**T3 — Boss Leviatán** (lógica completa 2026-07-06, valores GDD §8.3 en `LeviatanStatsSO`):
- `LeviatanAI`: FSM `Dormant → Intro (lockdown) → Idle/Track → Attack → Vulnerable → Death` con fallback por tiempo (no se atasca si faltan AnimationEvents); **enrage <50%** (velocidad ×1,5 + 4º golpe por ciclo); dispara música `Combat` al emerger y `Cinematic` al morir; SFX vía `EnemyAudioController`; barra de boss vía `HUDManager`; eventos `OnDefeated`/`OnHealthChanged`.
- `LeviatanCore`: recibe el **dash ofensivo** (patrón Biol/Drone, 1 golpe por dash) — `dashDamage` 25 ×2 = 50 ⇒ ~4 ventanas para 200 HP; solo daña durante `Vulnerable`.
- `LeviatanTentacle`: 15 SI con **i-frames de 0,8 s por fuente** (overload nuevo en `DegradationSystem`); los tentáculos no reciben daño.
- `BossArenaTrigger` (nuevo, `Scripts/Level/`): trigger de entrada → cierra la barrera + `StartEncounter()`; al morir el boss abre la barrera y activa la salida/cinemática. Para la arena: boss con `startDormant` ✔.
- `HUDManager`: API `ShowBossBar/UpdateBossBar/HideBossBar` (null-safe hasta que exista el arte).

**T4 — Audio (núcleo cerrado):** Mixer con 4 buses y volúmenes expuestos + setters; música adaptativa (`MusicState` + crossfade 1,5/0,8/0,5 s); SFX de Biol, Drone, rover (incl. escaneo), UI (código completo con back/pause/tick) y Game Over; `PauseAudioTrigger`; restauración de mezcla en checkpoint. **Disparadores completos (2026-07-06):** `AudioManager.ReportEnemyAlert` lleva un **contador de enemigos en Alert/Chase** (≥1 → `Tension`, 0 → `Exploration`; nunca pisa `Combat`/`Cinematic` ni suena fuera de gameplay) y `BioluminescentAI`/`DronePatrollerAI` reportan en sus transiciones, muerte y `OnDisable`; el boss pone `Combat` al emerger y `Cinematic` al morir.

---

## 🔴 Pendiente (por tarea)

### T1 — Pantallas: cierre
1. **Panel de Opciones** (menú y pausa comparten panel): sliders Music/SFX/Ambient/UI → ya existen `AudioManager.SetMusicVolume/SetSFXVolume/SetAmbientVolume/SetUIVolume` (0–1). Al crearlo, rehabilitar `Btn_Opciones` y `Btn_Configuracion` en los controladores.
2. **Pausa con el arte de `StopMenu`:** copiar el layout dentro del `PausePanel` del prefab `HUD_Canvas` (trabajo de editor/Artist) y añadirle el `PauseMenuController` — se cablea solo por nombre.
3. **Pantalla de carga:** silueta del rover avanzando con el progreso + `ui_loading_tick`.
4. **CONTINUAR** real cuando exista guardado (hoy es placeholder = Nueva Misión).

**DoD T1:** Nueva Misión muestra la carga con barra real ≥2 s y entra a Level01; Esc abre/cierra pausa y sus 5 opciones enrutan; navegable sin ratón; Opciones mueve los volúmenes del Mixer.

### T2 — Nivel 1 completo (bloqueante — Level Designer)
La base de `Level01.unity` está montada y probada (Grid + 6 tilemaps, HUD prefab, checkpoints ×4, hazards, 1 Biol + 1 Drone, parallax ✅). Falta **poblar el recorrido completo** (HUD §4.1.2, GDD §9.2):

| # | Segmento | Falta |
|---|----------|-------|
| 1 | Galería de Cristal (Z1) | SC-01 + 2 Biol |
| 2 | Bifurcación A (vertical) | pozo wall-jump, **Checkpoint 1**, upgrade Rueda Reforzada |
| 3 | Caverna Central (Z2) | SC-02, `EnergyCellPickup`, `BlockedZone` |
| 4 | Sala del Meteorito (Z3) | SC-03, **Drone ×2**, upgrade Escaneo Mejorado |
| 5 | Corredor de Arcilla (Z4) | **Checkpoint 2**, plataformas móviles, trampas `Caida*` |
| 6 | Sala Pre-Boss | **Checkpoint 3**, `EnergyCellPickup` |
| 7 | Arena del Leviatán | sala cerrada + **colocar `BossArenaTrigger` + Leviatán (`startDormant` ✔) — scripts listos, ver T3** |
| 8 | Salida | objeto `LevelExit`/trigger de cinemática (lo activa el `BossArenaTrigger` al morir el boss) |

- **Población objetivo:** 6 Biol + 2 Drones distribuidos por presupuesto de daño (llegar al boss con **SI 47–61%**; contacto Biol 5 SI/s, proyectil drone 8–12 SI). Checkpoints ≥8 u del enemigo más cercano.
- **Marcadores:** solo existe `SpawnPoint`; faltan Z2–Z5, `BlockedZone`, `BossRoom` (`LevelMarker`).
- **Escaneables SC-01/02/03:** dependen del `ScanSystem` (S05 T1) — colocar los objetos y marcar la posición aunque el sistema llegue después.
- **Config menor:** corregir el override de tag `Player` en la instancia `SerBioluminiscente`; asignar `rover` en cada enemigo; `Confiner 2D` por sala.

**DoD T2:** se recorre Sala de Reinicio → Arena sin atascos; 3 checkpoints registran; 6+2 enemigos persiguen; al entrar a la arena se dispara lockdown + spawn del boss; SI llega al boss en 47–61%.

### T3 — Leviatán: flecos (todos de editor/arte)
1. **Montar la instancia en la arena** (cuando T2 la tenga): boss con `stats`, `coreHitbox` (hijo trigger + `LeviatanCore.boss`), `tentacleHitboxes` (hijos trigger + `LeviatanTentacle.boss`), `startDormant` ✔, y `BossArenaTrigger` apuntándole (con `barrier` y `exitToUnlock`). El campo `rover` se auto-resuelve por tag si se deja vacío.
2. **SFX:** añadir `EnemyAudioController` al boss y asignar clips (rugido=attack, daño al núcleo=damage, muerte=death) — el código ya los dispara (cierra T4.4).
3. **Arte de la barra de boss** en `HUD_Canvas` y asignar `_bossPanel/_bossFill/_bossName` del `HUDManager`.
4. *(Opcional, mejora)* AnimationEvents en `LeviatanAtack.anim`: `AttackTentacle1/2/3` (SFX por golpe) y `FinishAttack` al final (si no, funciona el fallback por `attackDuration`); clips `Vulnerable`/`Enrage` si el Artist los produce (hoy reutiliza Idle con bools).

**DoD T3:** emerge con el lockdown; 3 tentáculos (15 SI, i-frames 0,8 s) + pausa de 2 s con núcleo dañable ×2 solo por dash; enrage <50% (×1,5 + 4º golpe); HP=0 → Death, abre la salida y cambia la música a Cinematic.

### T4 — Audio: cierre (solo assets/editor — el código del sprint está completo)
1. **Clips del Leviatán** (ver T3.2).
2. **Ambiente del Nivel 1** (bus Ambient): viento/goteo base, géiseres Z4, silencio pre-boss — loops espaciales por zona.
3. Verificar que los clips de UI (back/pause_open/pause_close/loading_tick) estén asignados en las escenas de pantallas, y que `AudioManager` tenga `clipTension`/`clipCombat`/`clipCinematic` asignados en las escenas de nivel (si falta uno, el crossfade funde a silencio).
4. *(Refinamiento V2, GDD §14.1)* `Tension` también con SI 20–40% aunque no haya enemigos — hoy solo dispara por enemigos.

**DoD T4:** la música cruza Exploration↔Tension↔Combat según enemigos/boss sin corte audible; cada botón suena; Biol/Drone/Leviatán emiten sus SFX; el ambiente de cueva suena en Level01.

---

## Equipo, ramas y reglas

| Rol | Rama | Tareas |
|-----|------|--------|
| Technical Director | `feature/ui-screens` / `feature/audio-system` | T1, T4 |
| Level Designer | `feature/level-design` | T2 |
| AI Programmer | `feature/enemy-ai` | T3 |
| Artist | arte | pantallas, Leviatán, SFX |

- Orden: **T2 desbloquea T3 (arena) y T4 (ambiente)**; T1 y T4.1 pueden ir en paralelo.
- No modificar `PlayerController.cs`, `RoverStatsSO.cs` ni `SceneLoader.cs` sin avisar al Gameplay Programmer (T1 ya tocó `SceneLoader` para la carga async — coordinado). Conventional Commits; rebase sobre `develop`; nunca push directo a `main`/`develop`; **siempre** incluir `.meta`.

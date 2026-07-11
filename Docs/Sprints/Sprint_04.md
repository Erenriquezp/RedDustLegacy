# Sprint 04 — Pantallas, Nivel 1 completo, Boss Leviatán y audio

> 🔄 En progreso · Revisión: 2026-07-10 · Prerequisito: S03 · Ref. GDD: §8.3, §9.2, §10, §12.2, §14

## Estado

| Tarea | Responsable | Estado |
|-------|-------------|--------|
| T1 — Pantallas (menú/carga/pausa) | Technical Director | 🟢 ~90% |
| T2 — Nivel 1 completo | Level Designer | 🟡 **bloqueante** — falta poblar el recorrido |
| T3 — Boss Leviatán | AI Programmer | 🟢 ~95% — jugable; flecos de editor/arte |
| T4 — Audio integral | Technical Director | 🟢 ~90% |

## ✅ Hecho

- **T1:** flujo real menú → carga async (barra real, mín. 2 s) → nivel; pausa y botones cableados por nombre; Build Settings corregidos.
- **T3:** boss completo y jugable en Level01 — FSM con enrage <50%, dash al núcleo (50×4 = 200 HP), tentáculos 15 SI (inofensivos en la ventana vulnerable), arena con `BossArenaTrigger` + `LevelExit`, barra de boss en `HUD_Canvas`, feedback visual (tinte vulnerable + flash de daño).
- **T4:** Mixer 4 buses, música adaptativa Exploration↔Tension↔Combat↔Cinematic (enemigos y boss reportan), SFX de Biol/Drone/rover/UI/GameOver.
- **T3.3/T4.1 (2026-07-07):** SFX del Leviatán (Attack/Damage/Death/Idle) y del Drone Detector (Alert/Damage/Death/Idle) asignados en los prefabs con `EnemyAudioController`.
- **T1.3 (2026-07-11):** pantalla de carga con silueta del rover (`bot_id`) avanzando por la barra + `ui_loading_tick` por segmento (`ControladorBarraSegmentada`; el clip ya estaba asignado en ambas escenas de carga).

## 🔴 Pendiente

### T1 — Pantallas
1. **Panel de Opciones** (menú y pausa): sliders → `AudioManager.SetMusicVolume/SetSFXVolume/SetAmbientVolume/SetUIVolume`; rehabilitar `Btn_Opciones`/`Btn_Configuracion`.
2. **Arte de pausa ✅ (2026-07-11):** ejecutado `Tools → Red Dust → Copiar arte de StopMenu al PausePanel` — el `PausePanel` de `HUD_Canvas` ya tiene el layout de `StopMenu` (12 elementos) + `PauseMenuController`. Re-ejecutable tras retocar StopMenu. Queda verificar en Play mode (Level01): Esc → Reanudar/Reiniciar/Abandonar operativos, Configuración/Archivo deshabilitados.
3. **CONTINUAR** real cuando exista guardado → S06 T5.

### T2 — Poblar Level01 (bloqueante · HUD §4.1.2)
- **Enemigos:** 6 Biol + 2 Drones repartidos para llegar al boss con **SI 47–61%**; checkpoints ≥8 u del enemigo más cercano.
- **Escaneables SC-01/02/03:** colocar posiciones (el `ScanSystem` llega en S05 T1).
- **Upgrades** Rueda Reforzada (Z2, pozo wall-jump) y Escaneo Mejorado (Z3) — esperan S05 T5; dejar `BlockedZone` marcada.
- **Celdas:** `EnergyCell` en Z3 y pre-boss.
- **Marcadores** Z2–Z5 (`LevelMarker`) y `Confiner 2D` por sala; corregir tag `Player` en la instancia `SerBioluminiscente`.
- Arena ✅ y salida ✅ (2026-07-07).

### T3 — Leviatán: flecos
1. Verificar el `BossArenaTrigger` (215, −2, collider 2×14) en la Scene view — que no quede dentro de una pared.
2. **`barrier`:** muro que cierre la arena durante el combate (hoy se puede huir); asignarlo en el trigger.
3. Ajustar hitboxes al sprite (cuerpo 1.2×1.1, tentáculos 0.35×1.2 — dimensionados a ojo). *(Opcional)* AnimationEvents en `LeviatanAtack.anim` y clips Vulnerable/Enrage.

### T4 — Audio
1. Ambiente del Nivel 1 (bus Ambient): viento/goteo, géiseres Z4, silencio pre-boss.
2. Verificar clips asignados: UI (back/pause/tick) y `clipTension/clipCombat/clipCinematic` en las escenas de nivel.

## Reglas

Ramas: `feature/ui-screens`·`feature/audio-system` (TD), `feature/level-design` (LD), `feature/enemy-ai` (AI). No tocar `PlayerController`/`RoverStatsSO`/`SceneLoader` sin coordinar. Conventional Commits; rebase sobre `develop`; nunca push directo a `main`/`develop`; siempre `.meta`.
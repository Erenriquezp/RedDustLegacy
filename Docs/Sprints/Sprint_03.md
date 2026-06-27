# Sprint 03 — Sistemas core, HUD, segundo enemigo y checkpoints

> **Estado:** Hecho y probado en `Level01` (T1–T5 + combate). Solo queda arte y pulido menor. · **Revisión:** 2026-06-27 (Drone: sensado de terreno + colisión física + daño solo por proyectil) · **Prerequisito:** Sprint 02 · **Ref. GDD:** §4, §7, §8.4, §10, §15

Integridad Estructural (SI), HUD, Drone Patrullero, Checkpoints, GameManager y combate por dash.

## Progreso

| Tarea | Estado |
|-------|--------|
| T1 — DegradationSystem (SI, fases, i-frames, knockback) | ✅ Hecho y probado |
| Combate — dash ofensivo (Biol + Drone) | ✅ Hecho y probado |
| T2 — HUD (barra SI, celdas, paneles Pausa/GameOver) | ✅ Hecho y probado |
| T3 — Drone Patrullero (terrestre + derrotable) | ✅ Hecho y probado |
| T4 — Checkpoints (reparan + respawn sin recargar) | ✅ Hecho y probado |
| T5 — GameManager (estados, pausa, Game Over) | ✅ Hecho y probado |


## Qué hace cada sistema

- **T1 `DegradationSystem`** (en el Player): SI 100 / inicio 74 / muerte <1, **fases 1–6** sobre una copia runtime del `RoverStatsSO` (modifican velocidad/dash/salto por fase). **i-frames** (`invulnDuration` 0.6 s, cortan el daño en cascada) + **knockback** al recibir golpe. Daño por caída medido desde el **despegue** (saltar no daña). Publica eventos para HUD/GameManager. Es la autoridad de daño.
- **Combate — dash ofensivo:** el rover derrota enemigos **embistiéndolos con dash** (`PlayerController.IsDashing`); Biol y Drone reciben `dashDamage` (1 golpe por dash). No hay melee. El modelo de contacto difiere por enemigo: **Biol** daña por **contacto** (collider trigger, i-frames + knockback); **Drone** tiene collider **sólido** (choca con el rover, no lo atraviesa) y **solo daña con proyectiles** — tocarlo o saltarle encima no resta SI.
- **T2 `HUDManager`:** barra de SI (color por umbral + parpadeo <40 %), slots de celda, pulso al recibir daño, `ShowAlert`, paneles Pausa/GameOver. Tool `Scaffold HUD Canvas` lo genera y cablea (+ EventSystem).
- **T3 Drone** (`DronePatrollerAI` + `EnemyProjectile` + `DronePatrollerStatsSO`): FSM Patrol/Chase/Attack/Return, dispara proyectil (8–12 SI). **Terrestre** (movimiento en X + pegado al suelo por raycast) con **sensado de terreno**: raycast de pared y de borde → gira al toparse con un muro o llegar a un precipicio, patrullando un **área acotada** (`patrolRange` desde el spawn) sin atravesar paredes ni caer al vacío. Collider **sólido** (bloquea físicamente al rover) y **daño solo por proyectil**. **Derrotable con dash** (el sólido dispara `OnCollisionStay2D`). Tool `Create DroneProjectile Prefab`.
- **T4 Checkpoints** (`Checkpoint` + `CheckpointManager`): al cruzar **reparan** la SI hasta `repairToSI` (74) y guardan ese valor; `RespawnPlayer` reubica y restaura **sin recargar** la escena. Tool `Create Checkpoint`.
- **T5 `GameManager`:** singleton autoarrancado; estados MainMenu/Playing/Paused/GameOver/Cinematic; pausa por `Esc`; muerte → Game Over + música; `RestartFromCheckpoint`, `ReturnToMainMenu`. Relay `GameMenuActions` para los botones.

## Pendiente (arte y pulido)

- **Arte:** clip `Rover_Damage` (el trigger `IsDamaged` ya se dispara; falta el clip — deuda S02 T2), sprite final del proyectil del Drone (hoy placeholder), iconos de celda y tipografía del HUD.
- **Confirmar** los 3 checkpoints colocados (inicio, media, pre-boss), ≥8 u del enemigo más cercano.

## Notas de diseño / tuning

- Daño por caída por **descenso neto desde el despegue**. Fase 3 desactiva dash aéreo · Fase 5 wall-jump · Fase 6 dash (GDD §4.2).
- **Combate:** el daño del rover a enemigos es solo por **dash**; cada golpe recibido aplica i-frames (0.6 s) + knockback corto.
- **Drone terrestre:** se mueve en X y se pega al suelo por raycast (no persigue en vertical). **Sensado de terreno** por raycast (pared al frente / borde delante de los pies) → gira y patrulla un área acotada a `patrolRange` desde el punto de aparición; ya no atraviesa muros ni cae al vacío. Collider **sólido** (bloquea al rover; la matriz Player↔Enemy ya lo permite) y **daño solo por proyectil** (sin daño por contacto). El medio ancho del raycast de pared se deriva de `SpriteRenderer.bounds`, así que respeta la escala del prefab.
- **Checkpoints = reparación:** suben la SI a 74 (no la bajan) al cruzar y guardan ese valor.
- Pausa por polling de `Esc`/Start (no se tocó el `.inputactions`). `DronePatrollerStatsSO` propio (no se amplió `EnemyStatsSO`).
- **Tuning ajustable:** `dashDamage` (Biol/Drone SO), `invulnDuration` + knockback (`DegradationSystem` / `RoverStatsSO`), `repairToSI` (`Checkpoint`). **Drone (nuevo en `DronePatrollerStatsSO`):** `wallCheckDistance`, `ledgeCheckDistance`, `patrolRange` (0 = patrulla solo limitada por paredes/bordes); el `m_Radius`/`IsTrigger` del `CircleCollider2D` del prefab definen la colisión física.

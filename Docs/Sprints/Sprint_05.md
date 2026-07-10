# Sprint 05 — Nivel 2 (El Relicto) y cierre del prototipo

> 🔄 En progreso · Revisión: 2026-07-07 · Prerequisito: S04 · Ref. GDD: §5, §6, §8.1, §8.5–8.7, §9.3, §11, §12

Objetivo: el segundo nivel + los sistemas que faltan para el prototipo completo (escaneo/lore, enemigos del Relicto + DDA, upgrades, cinemáticas con condición de victoria). Orden: T1 y T3 alimentan a T2; T2+T3 desbloquean T4; T6 al final.

## Estado

| Tarea | Responsable | Prioridad | Estado |
|-------|-------------|-----------|--------|
| T1 — Escaneo, lore y flashbacks | Gameplay Programmer | 🔴 | ⬜ 0% |
| T2 — Nivel 2: El Relicto | Level Designer | 🔴 | 🟡 base + Detectores + herencia de SI ✅ |
| T3 — Enemigos N2 + `AIManager`/DDA | AI Programmer | 🔴 | 🟡 Detector ✅; faltan Centinelas y DDA |
| T4 — Cinemáticas + victoria | Technical Director | 🔴 | ⬜ 0% |
| T5 — Upgrades + celdas | Gameplay Programmer | 🟡 | 🟡 celdas ✅; sin `UpgradeManager` |
| T6 — Audio N2 + cinemáticas | Technical Director | 🟡 | ⬜ 0% (desbloqueado) |

## ✅ Hecho (2026-07-07)

- **Drone Detector completo y jugable:** patrulla en vaivén, cono 8 u/60° con obstrucción, chase, proyectil 8–12 SI, daño por dash, comunicación con `AIManager` (reparte la posición a Patrulleros a ≤12 u) y música Tension; prefab cableado, ×3 en Level02.
- **Celdas de energía:** `EnergyCellPickup` + `Q` → +20 SI (máx. 2), slots del HUD reactivos.
- **Herencia de SI/celdas N1→N2:** `LevelExit` guarda → `GameManager.CarryOverToNextLevel` aplica al cargar; volver al menú la descarta. HUD re-vinculado y re-sincronizado en cada carga; respawn con 1 s de gracia.
- Base de `Level02.unity`: geometría, sistemas core (`HUD_Canvas`, checkpoints ×4, cámara), parallax 2 capas, 3 Patrulleros + 2 Biol, `AIManager` en escena.

## 🔴 Pendiente

### T1 — Escaneo, lore y flashbacks (0% — cierra también los SC del Nivel 1)
- `Scripts/Scan/ScanSystem.cs` (Player): mantener Scan → `OverlapCircle` radio 3 u (5 con upgrade) en capa `Interactable`; ficha en el HUD, fade 0,3 s al soltar. Los eventos `OnScanStarted/Stopped` y el stun del Biol ya existen.
- `Scannable.cs`: `id` SC-01…06, encabezado, texto (GDD §11.2), `flashbackId`, `unaVez`.
- Texto corrupto con SI ≤29% (GDD §11.1); terminal monoespaciada en `HUDManager`.
- Flashbacks FB-01…06 vía `CinematicManager` (T4).

### T2 — El Relicto (guía: [T2_Geometria_Level02.md](../HUD/T2_Geometria_Level02.md) · métricas: GDD §9.3)
- **Paleta N2:** `Global Light 2D` violeta-naranja (hoy blanca); tilemaps `#060610`/`#4A1A7A`/`#C0581A`.
- **3 alas semi-libres** con gating (ascensor ← SC-05; Ala B opcional) + marcadores `LevelMarker` (`SpawnPoint`, alas, `BlockedZone`, `BossRoom`).
- **Colocar:** 4 celdas restantes (2 Ala B, 2 pre-boss), SC-04/05/06, 2 upgrades (T5), arena del Centinela Principal (T3), `LevelExit` al final, trampas ambientales (6 SI).
- **Enemigos:** faltan Centinela Secundario ×2 (T3); limpiar waypoints obsoletos `WP_0/WP_1`.

### T3 — Enemigos N2 + IA central
- **Centinela Secundario** (GDD §8.6): HP 100, patrulla vertical en columna (≤8 u del origen), detección omnidireccional 6 u, proyectil recto 10 SI, cada 3er ataque con rastreo parcial (gira ≤30°); spawneable por el boss. Reusar collider sólido + dash + i-frames (S03).
- **Centinela Principal — boss final** (GDD §8.7): HP 400, 2 fases mínimas — F1: abanico ×3 (12 SI, 8 u/s), cd 2,5 s; F2: abanico ×5 + rastreo (18 SI, 45°) + spawn de un Secundario; anclado al centro (≤3 u); muerte → victoria (T4); barra vía `HUDManager` (ya construida). F3 (pulso de área) = stretch.
- **Detector — Flanking** (DDA activo): al perder de vista al rover, rodear por el lado opuesto a 3,5 u/s.
- **`AIManager` — DDA real:** registro de agentes y parámetros ×0,75–×1,25 según >2 muertes/5 min (baja) u 8 min sin morir (sube); API de spawn para el boss. Recordar: no se autoarranca (colocado en Level02 ✔, Level01 no).
- **Arte:** Detector solo tiene idle (faltan alerta/ataque/muerte).

### T4 — Cinemáticas + victoria (0%)
- `Scripts/Cinematics/CinematicManager.cs`: congela vía `GameManager.Cinematic` (ya existe) y gestiona timings.
- Flashbacks FB-01…06 (imagen + texto + audio); escena de Spirit (SC-06, pantalla dividida, no saltable, 45–55 s).
- **Victoria** (GDD §1.3): tras el Centinela Principal, secuencia de transmisión — enviar ≥1 fragmento antes de energía 0%.
- **Secuencia final** (GDD §12.3): 7 escenas; "El silencio" = negro absoluto 10 s exactos; el astronauta **nunca habla**.
- Enganchar el `LevelExit` de Level02 (hoy encadena al menú).

### T5 — Upgrades (celdas ✅)
- `UpgradeManager` + `UpgradePickup` + slots del HUD (GDD §10.1).
- **Rueda Reforzada** (N1 Z2): gatear el wall-jump (hoy siempre activo) → abre la `BlockedZone`.
- **Escaneo Mejorado** (N1 Z3): radio 3→5 u + ping pasivo <4 u (engancha T1).
- **Escudo de Plasma** (N2 entrada): absorbe 1 impacto, recarga 8 s. **Batería EMP** (N2 Ala B): aturde drones 2 s en radio 5 u, recarga 25 s, inútil contra el boss.
- Colocar los 4 pickups (T2) y SFX de recogida de celda (clip en el prefab).

### T6 — Audio N2 (núcleo S04 listo)
- Ambiente del Relicto por alas (bus Ambient); SFX de Detector/Centinelas/boss final.
- Disparadores N2: Detector/Secundario → Tension (Detector ✔); arena del boss → Combat; victoria → Cinematic.
- Estado `CINEMATIC` por escena desde `CinematicManager`; silencio absoluto en "El silencio"; SFX de la transmisión.

## Reglas

Ramas: `feature/scan-system`·`feature/upgrades` (GP), `feature/enemy-ai` (AI), `feature/level-design` (LD), `feature/cinematics`·`feature/audio-system` (TD). No tocar `PlayerController`/`RoverStatsSO`/`SceneLoader` sin coordinar; reusar dash + i-frames (S03), `HUDManager` y `GameManager.Cinematic`. Conventional Commits; rebase sobre `develop`; nunca push directo; siempre `.meta`.

# Sprint 05 — Nivel 2 (El Relicto) y cierre del prototipo

> 🔄 En progreso · Revisión: 2026-07-10 · Prerequisito: S04 · Ref. GDD: §5, §6, §8.1, §8.5–8.7, §9.3, §11, §12

Objetivo: el segundo nivel + los sistemas que faltan para el prototipo completo (escaneo/lore, enemigos del Relicto + DDA, upgrades, cinemáticas con condición de victoria). Orden: T1 y T3 alimentan a T2; T2+T3 desbloquean T4; T6 al final.

## Estado

| Tarea | Responsable | Prioridad | Estado |
|-------|-------------|-----------|--------|
| T1 — Escaneo, lore y flashbacks | Gameplay Programmer | 🔴 | 🟢 código + datos ✅; falta scaffold del terminal, colocar SC en niveles y handoff a T4 |
| T2 — Nivel 2: El Relicto | Level Designer | 🔴 | 🟡 base + Detectores + herencia de SI ✅ |
| T3 — Enemigos N2 + `AIManager`/DDA | AI Programmer | 🔴 | 🟢 Centinelas y Detector ✅; falta Flanking y consumo del DDA |
| T4 — Cinemáticas + victoria | Technical Director | 🔴 | ⬜ 0% — guía lista |
| T5 — Upgrades + celdas | Gameplay Programmer | 🟡 | 🟡 celdas ✅; sin `UpgradeManager` |
| T6 — Audio N2 + cinemáticas | Technical Director | 🟡 | ⬜ 0% (desbloqueado) |

## ✅ Hecho

- **Animaciones de muerte (2026-07-11):** los enemigos ya no se congelan en el frame 1 al morir — las transiciones Any State → Death tenían `Can Transition To Self` activo y el bool (`IsDead`) las re-disparaba cada frame (Leviatán, Drone Patroller; desactivado también en Centinelas como defensa). Los clips Dead de CP/CS/Drone ya no loopean (quedan en el último frame) y el despawn respeta la animación (~1,8 s): Drone 1,5→3 s, Centinela Secundario 1→3 s. Los `Die()` limpian triggers/bools de combate pendientes para que nada saque al animator del estado Death. (El Detector sigue sin clip de muerte — pendiente de arte T3.)
- **Combate N2 completo:** Centinelas Secundario y Principal jugables de punta a punta — FSM, daño por dash, proyectiles con impacto real, fases del boss (F1/F2 + invocación + `OnDefeated`), prefabs autosuficientes, valores GDD en los `.asset`.
- **Locomoción y detección de enemigos:** Centinelas terrestres (ya no flotan; el Secundario camina con gravedad, el boss se mueve solo en X y dispara desde su ancla); Detector con proximidad omnidireccional 3,5 u, gracia de alerta 1 s y lock-on <11 u.
- **Leviatán mejorado:** embestida telegrafiada que cae frente al rover + onda de impacto; persecución 2,2 u/s, cd 1,2 s; el enrage acelera todo ×1,5 (`LeviatanStatsSO`).
- **Muerte/respawn sólidos:** input cortado al morir, `DashSleep` respeta el Game Over, animación de muerte visible (1,5 s), reinicio de nivel a la 3.ª muerte por checkpoint, `SetSI` reaplica fases, muertes alimentan el DDA.
- **Bugs críticos de escena:** `DegradationSystem` duplicado sobre el Player en L01/L02 eliminado (barra desincronizada + muerte sin Game Over); daño por caída solo en caídas altas (mín. 5/4 u, fuerte ≥9 u, 10/25 SI); sprite idle del rover re-pivotado (flotaba en idle).
- **Base previa (07–08 jul):** Detector ×3 en Level02, celdas de energía + `Q`, herencia SI/celdas N1→N2, base de `Level02.unity`, `AIManager` (DDA base + `SpawnEnemy`), SFX Detector/Leviatán, plan de pruebas (`PRUEBAS.md`), PR #30 (shader de sprites, escena `Isometric`, `EditableColliderTool`).

## 🔴 Pendiente

### T1 — Escaneo, lore y flashbacks (código ✅ 2026-07-10 — guía: [ScanSystem.md](../Architecture/ScanSystem.md))
- **Hecho:** `Scripts/Scan/` completo — `ScanSystem` (en el prefab del Player: detección 10 Hz, adquisición 0,4 s, anillo procedimental, highlight), `Scannable` + `ScanDataSO`, `ScanTerminalUI` (typewriter, severidad, corrupción determinista con SI ≤29%), los 6 assets SC-01…06 con textos GDD, y `Tools → Red Dust → Scaffold Scan Terminal`.
- **Manual:** ejecutar el scaffolder dentro del prefab `HUD_Canvas` y guardarlo (recomendado: fuente TMP monoespaciada en Header/Body).
- **Colocación (T2/LD):** crear los objetos SC en los niveles — sprite + Collider2D trigger + `Scannable` (el `Reset` autoconfigura capa/trigger) + asset SC-0X + cablear `onScanned` (SC-01 ruta oculta, SC-03 puerta del Leviatán, SC-05 ascensor).
- **Primer escaneable ✅ — el Biol muerto:** al morir pasa a capa `Interactable` y su cadáver queda 10 s (`corpseDuration` en `EnemyStatsSO`) con fade final de 1,5 s; su `Scannable` (en el prefab) muestra la ficha `SC-B1` ("VIDA EXTRATERRESTRE CONFIRMADA", roja). Vivo no es escaneable (capa Enemy) — vivo se aturde, muerto se analiza.
- **Handoff a T4:** al completar escaneo con `flashbackId` hoy solo se loguea; `CinematicManager.PlayFlashback(id)` debe reemplazar ese log (valida SI mínimo, GDD §11.3).
- **Engancha T5:** el upgrade Escaneo Mejorado sube `ScanSystem.ScanRadius` 3→5.

### T2 — El Relicto (guía: [T2_Geometria_Level02.md](../HUD/T2_Geometria_Level02.md) · métricas: GDD §9.3)
- **Paleta N2:** `Global Light 2D` violeta-naranja (hoy blanca); tilemaps `#060610`/`#4A1A7A`/`#C0581A`.
- **3 alas semi-libres** con gating (ascensor ← SC-05; Ala B opcional) + marcadores `LevelMarker` (`SpawnPoint`, alas, `BlockedZone`, `BossRoom`).
- **Colocar:** 4 celdas restantes (2 Ala B, 2 pre-boss), SC-04/05/06, 2 upgrades (T5), arena del Centinela Principal (T3), `LevelExit` al final, trampas ambientales (6 SI).
- **Enemigos:** Centinela Secundario ×2 — el prefab ya es autosuficiente, colocarlo **sobre suelo** (ahora tiene gravedad); limpiar waypoints obsoletos `WP_0/WP_1`.

### T3 — Enemigos N2 + IA central (Centinelas ✅)
- **Verificar en Play mode** el combate de ambos Centinelas en Level02 (reemplazar el boss cableado a mano en `BiolAC1.unity` por el prefab); ajustar hitboxes/FirePoint a ojo.
- **Arena del boss:** replicar el patrón `BossArenaTrigger` del Leviatán suscrito a `CentinelaPrincipalAI.OnDefeated` (ya existe) → victoria (T4).
- **Detector — Flanking** (con DDA activo): al perder de vista al rover, rodear por el lado opuesto a 3,5 u/s.
- **DDA — consumo:** los agentes deben aplicar `GetDifficultyMultiplier` a velocidad/cooldowns; activar `ddaEnabled` en Level02 (las muertes ya se registran vía `GameManager`). `AIManager` no se autoarranca: está en Level02 ✔, Level01 no.
- **Arte:** Detector solo tiene idle (faltan alerta/ataque/muerte). F3 del boss (pulso de área) = stretch.
- **Audio:** asignar clips de Centinelas en `EnemyAudioController` de sus prefabs (los hooks de código ya están — T6).

### T4 — Cinemáticas + victoria (guía completa: [Cinematics.md](../Architecture/Cinematics.md))
- `Scripts/Cinematics/CinematicManager.cs`: overlay propio + congela vía `GameManager.SetCinematic` (ya existe); data-driven con `FlashbackSO`/`CinematicSO`.
- **Cinemática de introducción** (nueva, ver guía): contexto de la misión en <60 s, estilo terminal + imágenes estáticas, siempre saltable.
- **Flashbacks FB-01…06** (overlay 8–12 s, imagen + texto mono + audio) y **escena de Spirit** (SC-06: pantalla dividida, 45–55 s, no saltable — GDD §12.4).
- **Victoria** (GDD §1.3): tras `OnDefeated` del Centinela Principal, secuencia de transmisión — enviar ≥1 fragmento antes de energía 0% — y **secuencia final de 7 escenas** (GDD §12.3): "El silencio" = negro absoluto 10 s exactos; el astronauta **nunca habla**.
- Enganchar el `LevelExit` de Level02 (hoy encadena al menú) → la victoria pasa por la transmisión, no por la salida.

### T5 — Upgrades (celdas ✅)
- `UpgradeManager` + `UpgradePickup` + slots del HUD (GDD §10.1); persistencia entre niveles vía `GameManager` (mismo patrón que SI/celdas).
- **Rueda Reforzada** (N1 Z2): gatear el wall-jump (hoy siempre activo — `WallJumpEnabled` ya existe en `PlayerController`) → abre la `BlockedZone`.
- **Escaneo Mejorado** (N1 Z3): radio 3→5 u + ping pasivo <4 u (engancha con T1 — el radio debe leerse de aquí).
- **Escudo de Plasma** (N2 entrada): absorbe 1 impacto, recarga 8 s — interceptar en `DegradationSystem.TakeDamage`. **Batería EMP** (N2 Ala B): aturde drones 2 s en radio 5 u, recarga 25 s, inútil contra el boss.
- Colocar los 4 pickups (T2) y SFX de recogida de celda (clip en el prefab).

### T6 — Audio N2 (núcleo S04 listo)
- Ambiente del Relicto por alas (bus Ambient); SFX de Centinelas y boss final (hooks de código listos; Detector ✔).
- Disparadores N2: Detector/Secundario → Tension (Detector ✔); arena del boss → Combat; victoria → Cinematic.
- Estado `CINEMATIC` por escena desde `CinematicManager`; **silencio absoluto** en "El silencio" (parar todos los buses); SFX de la transmisión y del apagón de sistemas.

## Reglas

Ramas: `feature/scan-system`·`feature/upgrades` (GP), `feature/enemy-ai` (AI), `feature/level-design` (LD), `feature/cinematics`·`feature/audio-system` (TD). No tocar `PlayerController`/`RoverStatsSO`/`SceneLoader` sin coordinar; reusar dash + i-frames (S03), `HUDManager` y `GameManager.Cinematic`. Conventional Commits; rebase sobre `develop`; nunca push directo; siempre `.meta`.

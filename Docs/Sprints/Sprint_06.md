# Sprint 06 — Interludio isométrico, pantallas de sistema y cierre de producto

> ⬜ Planificado · Creado: 2026-07-10 · Prerequisito: S05 · Ref. GDD: §8.1, §9.1, §10, §12.2 (+ decisiones de diseño nuevas, fuera del GDD)

Objetivo: convertir el prototipo en un juego completo de cara al jugador — transición jugable entre niveles usando la vista isométrica (PR #30), las pantallas que todo juego necesita (opciones, dificultad, historia, códex), guardado con CONTINUAR real y la beta con jugadores. Orden sugerido: T5 (guardado) temprano porque T2/T3/T4 persisten en él; T1 en paralelo; T7 al final con build.

## Estado

| Tarea | Responsable | Prioridad | Estado |
|-------|-------------|-----------|--------|
| T1 — Interludio isométrico N1→N2 | Level Designer + Gameplay Programmer | 🔴 | 🟡 código + flujo ✅ 2026-07-11; falta el pase de escena del LD y el viento |
| T2 — Opciones: audio, video y dificultad | Technical Director | 🔴 | ⬜ 0% |
| T3 — Historia / Archivo de Misión | Technical Director | 🟡 | ⬜ 0% |
| T4 — Códex de objetos y enemigos | Gameplay Programmer | 🟡 | ⬜ 0% |
| T5 — Guardado + CONTINUAR real | Gameplay Programmer | 🔴 | 🟢 código ✅ 2026-07-11; falta verificación en Play y el borrado en la victoria (S05 T4) |
| T6 — Gamepad completo | Gameplay Programmer | 🟡 | ⬜ 0% |
| T7 — Build + beta (PRUEBAS.md) + balance | Technical Director + equipo | 🔴 | ⬜ 0% |

## 🔴 Pendiente

### T1 — Interludio isométrico N1→N2 (código ✅ 2026-07-11 — la escena `Isometric` del PR #30 deja de ser sandbox)
- **Hecho (código):** `SceneLoader.IsometricScene` + `LoadIsometric()` (comparte `PantallaCargaNivel2` — "rumbo al Relicto"; si el Artist hace una pantalla propia, cambiarla ahí); `LoadNextLevel()` ahora encadena Level01 → Isometric → Level02 → menú; `Isometric` añadida a Build Settings. `InterludeExit` (`Scripts/Level/`): trigger 3D que reconoce al `BasicCharacter` (va sin tag) y carga Level02. `GameManager`: el interludio autosava (persiste la herencia pendiente al no haber `DegradationSystem`) y CONTINUAR puede retomar en él. `HUDManager`: sin `DegradationSystem` oculta barra de SI y celdas — el HUD queda como contenedor de pausa (GDD: el interludio no tiene daño ni muerte). La herencia N1→N2 sobrevive el interludio por diseño (solo se consume donde hay `DegradationSystem`).
- **Manual (1 clic):** ejecutar `Tools → Red Dust → Preparar interludio (Isometric)` — instancia `HUD_Canvas` (pausa), crea el `EventSystem` y un `InterludeExit` placeholder junto al Player; idempotente.
- **Pendiente (LD):** mover el `InterludeExit` a la entrada del Relicto; recorrido de ~60–90 s con colliders (`EditableColliderTool`) y límites/confiner de cámara (la escena ya tiene `CinemachineCamera`); sin enemigos ni mecánicas nuevas.
- **Pendiente (TD):** viento marciano por el bus Ambient — **no hay clip en el repo** (`Assets/Audio/` no tiene ambientes); conseguir el asset y colocar un `AudioSource` en loop ruteado al grupo Ambient. Si T4 de S05 llega, aquí encaja FB/lore de transición.
- **Verificar en Play:** N1 completo → salida → interludio (pausa con Esc, sin barra de SI) → `InterludeExit` → N2 con la misma SI/celdas de la salida de N1; y CONTINUAR tras salir del juego en pleno interludio.

### T2 — Opciones: audio, video y dificultad (retoma S04 T1.1, que quedó pendiente)
- **Panel único** accesible desde menú principal y pausa (rehabilitar `Btn_Opciones`/`Btn_Configuracion`).
- **Audio:** sliders Música/SFX/Ambiente/UI → `AudioManager.SetMusicVolume/SetSFXVolume/SetAmbientVolume/SetUIVolume` (la API ya existe); persistir en `PlayerPrefs` y aplicar al arrancar.
- **Video (mínimo PC):** pantalla completa/ventana y resolución (dropdown con `Screen.resolutions`), VSync on/off. Nada más — sin calidad gráfica por ahora.
- **Dificultad:** selector Fácil/Normal/Difícil → mapea al `AIManager`: multiplicador inicial 0,75/1,0/1,25 y `ddaEnabled` activo en todas (el DDA ajusta desde esa base; muertes ya se registran vía `GameManager`). Cerrar aquí el pendiente de S05 T3: que los agentes consuman `GetDifficultyMultiplier` en velocidad/cooldowns.
- Persistencia de opciones va con T5 (o `PlayerPrefs` directo si T5 se retrasa).

### T3 — Historia / Archivo de Misión (pantalla del menú hoy deshabilitada)
- **Sinopsis jugable:** contexto de la misión real de Opportunity + premisa del juego (el "legado" — GDD §1); 2–3 pantallas de texto con arte, navegables, saltables.
- **Registro de lore:** las fichas SC-01…06 escaneadas (S05 T1) se releen aquí; las no descubiertas aparecen bloqueadas ("SEÑAL NO REGISTRADA").
- Rehabilitar `Btn_ArchivoMision` en `MainMenuController`; misma pantalla accesible desde pausa (solo lectura).

### T4 — Códex de objetos y enemigos (decisión de diseño nueva)
- `CodexEntrySO` (ScriptableObject, patrón del proyecto): id, nombre, sprite, tipo (Objeto/Enemigo), descripción corta, pista de combate (enemigos) o de uso (objetos).
- **Entradas mínimas:** enemigos — Biol, Drone Patrullero, Drone Detector, Leviatán, Centinela Secundario, Centinela Principal; objetos — Celda de energía, Checkpoint, los 4 upgrades (S05 T5).
- **Desbloqueo:** enemigos al escanearlos o derrotarlos por primera vez; objetos al recogerlos/usarlos. Entrada bloqueada = silueta + "???".
- UI con pestañas Objetos/Enemigos dentro del Archivo de Misión (T3) — una sola pantalla contenedora, no dos sistemas.
- Estado de desbloqueo persiste en el guardado (T5).

### T5 — Guardado + CONTINUAR real (código ✅ 2026-07-11)
- **Hecho:** `SaveSystem` estático + `SaveData` (JSON en `persistentDataPath/save.json`, un slot, sin encriptar) con campos ya reservados para upgrades (S05 T5), códex (T4) y dificultad/opciones (T2 — el autosave preserva los campos que no gestiona). `GameManager`: autosave al entrar a Level01/Level02 y al registrar checkpoint (vía `CheckpointManager.Register`), rastreo de fichas SC-XX escaneadas (`IsScanned`/`ScannedIds`, se suscribe a `ScanSystem.OnScanCompleted`), `ContinueFromSave()` (restaura lore y SI/celdas por la herencia pendiente) y `StartNewGame()`. Autosave también en cada `OnCellsChanged` (recoger/usar celda persiste al momento — una celda tomada tras el último checkpoint no se pierde al salir). Round-trip verificado headless con `Editor.SaveSmokeTest.Run` (⚠ borra el slot local). Menú: `Btn_Continuar` deshabilitado sin guardado (cierra BUG-003 de PRUEBAS.md); `NUEVA MISIÓN` con confirmación de dos pulsaciones ("¿SEGURO? SE PERDERÁ EL PROGRESO", timeout 4 s) si hay guardado.
- **Decisión de alcance:** no se guarda posición — CONTINUAR retoma el nivel guardado desde su inicio con la SI/celdas/lore del último autosave (el spec no lista posición). Las escenas sandbox (Dev/Enemy/Isometric) no escriben el slot; al volverse interludio (T1), añadir `Isometric` a `GameManager.AutoSave`.
- **Pendiente:** verificar en Play el ciclo menú → N1 → checkpoint → salir → CONTINUAR; la victoria (S05 T4) debe llamar a `SaveSystem.Delete()` para cerrar limpia; T2 aplica `difficulty` y opciones desde `SaveData` al arrancar.

### T6 — Gamepad completo (limitación conocida de la beta — PRUEBAS.md)
- Saltar/dash/escanear/celda con gamepad (hoy solo movimiento y pausa): completar los bindings del `InputActions` y el polling de `Gamepad.current` donde aplique (celda `Q` en `DegradationSystem`, escaneo).
- **Navegación de menús** con gamepad/teclado: `EventSystem` con primer botón seleccionado en cada panel (menú, pausa, Game Over, opciones).
- Remapeo de teclas = stretch, fuera del alcance del prototipo.

### T7 — Build + beta + balance (cierra el ciclo con PRUEBAS.md)
- Build Windows (IL2CPP o Mono según tiempo) con todas las escenas en Build Settings; smoke test del flujo completo menú → N1 → interludio → N2 → victoria.
- Ejecutar el plan de `PRUEBAS.md` (10–20 jugadores externos, escenarios E1–E11) y completar sus secciones 4–10 con los resultados.
- **Pase de balance** con los datos: SI al llegar al boss (objetivo 47–61%, HUD §4.1.2), muertes por zona, tiempos por nivel; ajustar StatsSO — no código.
- Triage de bugs de la beta: crítica/alta se corrigen en el sprint; media/baja a backlog.

## Reglas

Ramas: `feature/isometric-transition` (LD+GP), `feature/ui-screens` (TD), `feature/codex`·`feature/save-system`·`feature/gamepad` (GP), `release/beta` (TD). No tocar `PlayerController`/`RoverStatsSO`/`SceneLoader` sin coordinar (T1 toca `SceneLoader`: coordinar con TD). Reusar patrones existentes: StatsSO/`CodexEntrySO` para datos, eventos C# para reacciones, `PlayerPrefs`/JSON para persistencia. Conventional Commits; rebase sobre `develop`; nunca push directo; siempre `.meta`.

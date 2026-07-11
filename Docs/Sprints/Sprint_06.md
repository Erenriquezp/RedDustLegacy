# Sprint 06 — Interludio isométrico, pantallas de sistema y cierre de producto

> ⬜ Planificado · Creado: 2026-07-10 · Prerequisito: S05 · Ref. GDD: §8.1, §9.1, §10, §12.2 (+ decisiones de diseño nuevas, fuera del GDD)

Objetivo: convertir el prototipo en un juego completo de cara al jugador — transición jugable entre niveles usando la vista isométrica (PR #30), las pantallas que todo juego necesita (opciones, dificultad, historia, códex), guardado con CONTINUAR real y la beta con jugadores. Orden sugerido: T5 (guardado) temprano porque T2/T3/T4 persisten en él; T1 en paralelo; T7 al final con build.

## Estado

| Tarea | Responsable | Prioridad | Estado |
|-------|-------------|-----------|--------|
| T1 — Interludio isométrico N1→N2 | Level Designer + Gameplay Programmer | 🔴 | ⬜ 0% |
| T2 — Opciones: audio, video y dificultad | Technical Director | 🔴 | ⬜ 0% |
| T3 — Historia / Archivo de Misión | Technical Director | 🟡 | ⬜ 0% |
| T4 — Códex de objetos y enemigos | Gameplay Programmer | 🟡 | ⬜ 0% |
| T5 — Guardado + CONTINUAR real | Gameplay Programmer | 🔴 | ⬜ 0% |
| T6 — Gamepad completo | Gameplay Programmer | 🟡 | ⬜ 0% |
| T7 — Build + beta (PRUEBAS.md) + balance | Technical Director + equipo | 🔴 | ⬜ 0% |

## 🔴 Pendiente

### T1 — Interludio isométrico N1→N2 (decisión de diseño: la escena `Isometric` del PR #30 deja de ser sandbox)
- **Flujo:** `LevelExit` de Level01 → pantalla de carga → `Isometric` (recorrido corto en superficie: el rover cruza el terreno marciano hasta la entrada del Relicto) → trigger de entrada → pantalla de carga N2 → Level02.
- `SceneLoader`: constante `IsometricScene` + método `LoadIsometric()`; `LoadNextLevel()` pasa a Level01 → Isometric → Level02 → menú; añadir la escena a Build Settings.
- **Herencia de SI/celdas:** `GameManager.CarryOverToNextLevel` ya sobrevive escenas sin `DegradationSystem` (solo consume el pendiente donde hay uno) — verificar que N1 → Isometric → N2 conserva SI/celdas de punta a punta.
- **La escena es un interludio, no un nivel:** sin daño, sin HUD de SI, sin muerte; `GameManager` debe tratarla como gameplay (pausa disponible) aunque no encuentre `DegradationSystem` (hoy solo loguea un warning — validar que nada más se rompa).
- `BasicCharacter`: hoy solo camina (`OnMove`); necesita el trigger de salida, límites de cámara y colliders del recorrido (`EditableColliderTool` ya está para los meshes). Sin enemigos ni mecánicas nuevas: es un beat narrativo/visual de ~60–90 s.
- Ambiente sonoro de superficie (viento marciano — bus Ambient) y, si T4 de S05 llega, aquí encaja FB/lore de transición.

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

### T5 — Guardado + CONTINUAR real (limitación conocida de la beta — PRUEBAS.md)
- **Alcance mínimo:** un solo slot, autosave al completar nivel y al registrar checkpoint. Datos: escena actual, SI, celdas, upgrades (S05 T5), lore/códex desbloqueado, dificultad y opciones.
- JSON en `Application.persistentDataPath` (`SaveSystem` estático + `SaveData` serializable); sin encriptar — es un prototipo.
- **Menú:** `CONTINUAR` habilitado solo si existe guardado (carga escena + estado); `NUEVA MISIÓN` con confirmación si hay guardado previo ("se perderá el progreso").
- Borrar guardado al completar la secuencia final (S05 T4) para que la victoria cierre limpia.

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

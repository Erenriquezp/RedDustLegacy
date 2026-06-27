# Sprint 05 — Nivel 2 (El Relicto), sistemas transversales y cierre del prototipo

> **Estado:** ⬜ Pendiente · **Creado:** 2026-06-26 · **Prerequisito:** Sprint 04 (vertical slice del Nivel 1) · **Ref. GDD:** §5, §6, §8.1, §8.5–8.7, §9.3, §11, §12 · **Ref. HUD:** §4, §10

Construye el **segundo nivel** y los **sistemas que el GDD pide para un prototipo completo pero que aún no existen en código**: escaneo/lore + flashbacks, IA central (`AIManager` + DDA y los tres enemigos del Relicto), upgrades de combate (Escudo de Plasma, Batería EMP) y las cinemáticas con la **condición de victoria** (transmisión final). Cierra el arco Despertar → Silencio.

## Equipo y ramas

| Rol | Rama | Tareas en este sprint |
|-----|------|-----------------------|
| Technical Director (Líder) | `feature/cinematics` / `feature/audio-system` | T4 (cinemáticas + victoria), T6 (audio del Nivel 2) |
| Gameplay Programmer | `feature/scan-system` / `feature/upgrades` | T1 (escaneo/lore), T5 (upgrades + celdas) |
| AI Programmer | `feature/enemy-ai` | T3 (enemigos del Relicto + `AIManager`/DDA) |
| Level Designer | `feature/level-design` | T2 (Nivel 2 — El Relicto) |
| Artist | arte | bioma N2, sprites/animación de los enemigos N2 y boss final, assets de cinemáticas y flashbacks |

## Orden recomendado

```
T1 Escaneo/Lore ──┬─→ retro-llena los escaneables del Nivel 1 (deuda S04 T2)
                  └─→ T2 Nivel 2 (coloca SC-04/05/06 + flashbacks)
T3 Enemigos N2 ────→ T2 (poblar el Relicto)
T2 + T3 ──────────→ T4 Cinemáticas (final dispara tras el Centinela Principal)
T1 (flashbacks) ──→ T4 (CinematicManager reproduce FB-01…06)
todo ─────────────→ T6 Audio (ambiente violeta, SFX enemigos N2, música de cinemática)
```

---

## T1 — Sistema de Escaneo, Lore y Flashbacks
**Responsable:** Gameplay Programmer (+ TD para el enganche con cinemáticas) · **Rama:** `feature/scan-system` · **Ref. GDD:** §11, §13.3 · **Ref. HUD:** §10

**Estado actual:** no existe `ScanSystem` ni objetos escaneables; el rover ya tiene el input **Scan** (`IsScanning` en el Animator) pero sin lógica. Es un sistema **transversal** (Nivel 1 y 2): construirlo aquí también cierra los escaneables pendientes del Nivel 1 (deuda S04 T2).

Pasos:
- `Scripts/Scan/ScanSystem.cs` (en el Player): al mantener Scan, `Physics2D.OverlapCircle`/`CircleCast` en la capa `Interactable` con **radio 3 u** (5 u con upgrade Escaneo Mejorado). Resalta el objetivo más cercano y muestra su texto en el HUD mientras se mantiene; fade 0,3 s al soltar.
- `Scripts/Scan/Scannable.cs` (ScriptableObject o MonoBehaviour por objeto): `id` (SC-01…06), `encabezado` (`ANALISIS`/`REGISTRO AMBIENTAL`/`SENAL IDENTIFICADA`/`ARCHIVO HISTORICO`), `textoHUD`, `flashbackId` opcional, ` unaVez`. Textos definitivos en GDD §11.2.
- **Aturdir al Ser Bioluminiscente** con el pulso de escaneo (2 s, sin daño) — engancha con `BioluminescentAI.ApplyStun` ya existente (GDD §8.2).
- **Degradación del texto** desde SI ≤29%: corromper caracteres progresivamente (GDD §11.1).
- Terminal de escaneo en el HUD: panel monoespaciado (delegar en `HUDManager`); color por tipo (blanco/ámbar/rojo).
- Disparar **flashbacks** (FB-01…06, GDD §11.3) vía `CinematicManager` (T4): al escanear ciertos objetos con la SI mínima requerida.
- Engancha **upgrade Escaneo Mejorado** (T5): radio 3→5 u + ping pasivo (objetos brillan a <4 u).

**DoD T1:** mantener Scan junto a un objeto muestra su ficha en el HUD; los escaneables del Nivel 1 (SC-01/02/03) y del Nivel 2 (SC-04/05/06) funcionan; el pulso aturde al Biol 2 s; escanear los objetos clave dispara su flashback; con SI ≤29% el texto se corrompe.

---

## T2 — Nivel 2: El Relicto (diseño de nivel)
**Responsable:** Level Designer · **Rama:** `feature/level-design` · **Ref. GDD:** §9.3, §13.1 · **Ref. HUD:** §4

**Estado actual:** solo existe la referencia `SceneLoader.LoadLevel02()` y la entrada en Build Settings; **no hay escena construida**. Tomar `Level01.unity` como plantilla de sistemas (HUD, checkpoints, cámara, contenedores) y construir el bioma alienígena. **Guía de implementación detallada:** [T2_Geometria_Level02.md](../HUD/T2_Geometria_Level02.md).

Métricas objetivo (GDD §9.3):

| Parámetro | Valor |
|-----------|-------|
| Bioma | Estructura alienígena — luz violeta-naranja, metal + bioluminiscencia orgánica |
| Duración estimada | 35–50 min |
| Estructura | Metroidvania con **3 alas en orden semi-libre** |
| Checkpoints | **4** (entrada, Ala A, Ala B, pre-boss) |
| Enemigos | Drone Detector ×3, Drone Patrullero ×2, Centinela Secundario ×2, Centinela Principal ×1 (boss) |
| Escaneables | SC-04 (inicio), SC-05 (entrada, abre ascensor), SC-06 (Ala C, escena Spirit) |
| Upgrades | Escudo de Plasma (entrada, post-checkpoint), Batería EMP (Ala B, opcional) |
| Celdas de energía | **5** (1 entrada, 2 Ala B, 2 pre-Centinela) |
| SI al inicio | 47–61% (herencia del Nivel 1) |
| SI estimada al final | 9–29% |

Pasos:
- Crear `Level02.unity`: Grid + tilemaps (`Collision`, `Visual`, `OneWay`, `Danger`, `Markers`, `Front`) con la **paleta N2** (`#060610`/`#4A1A7A`/`#C0581A`); `Global Light 2D` violeta-naranja.
- Trazar **3 alas semi-libres** con gating: el **ascensor** se abre al escanear SC-05; Ala B opcional (Batería EMP); puerta al boss tras explorar lo requerido.
- Colocar **4 checkpoints** (≥8 u del enemigo más cercano), **5 celdas**, los **3 escaneables**, los **2 upgrades** y la **arena del Centinela Principal** (lockdown + spawn → T3).
- **Parallax de 4 capas** (GDD §9.1) con los scripts `Scripts/Level/Parallax*` (ya existen, sin montar): montar y ajustar factores.
- Heredar SI entre niveles: leer la SI con que se termina el Nivel 1 (coordinar con T4/`LevelManager`); no reiniciar a 74%.
- **Trampas ambientales** (objeto ambiental 6 SI, GDD §4.3, "solo en Nivel 2"): colocar con las mecánicas de `Scripts/leveo01/`.
- Asignar el campo `rover` de cada enemigo al Player de la escena.

**DoD T2:** se recorre entrada → 3 alas → arena del Centinela sin caer al vacío; los 4 checkpoints registran; los 8 enemigos están colocados y persiguen; SC-04/05/06 escaneables (SC-05 abre el ascensor); Escudo de Plasma y Batería EMP recogibles; el parallax de 4 capas se mueve; el rover entra al Relicto con la SI heredada del Nivel 1.

---

## T3 — Enemigos del Relicto + IA central (`AIManager` / DDA)
**Responsable:** AI Programmer (+ Artist para sprites/animación) · **Rama:** `feature/enemy-ai` · **Ref. GDD:** §8.1, §8.5–8.7, §13.4

**Estado actual:** existen `BioluminescentAI` y `DronePatrollerAI` (con el patrón **dash ofensivo**); no existe `AIManager` ni ningún enemigo del Nivel 2. Reusar el patrón de combate por dash (núcleo/cuerpo con trigger + `OnTrigger/CollisionStay2D` + `dashDamage`) y los **i-frames centrales** del `DegradationSystem`.

### 3.1 Drone Detector (GDD §8.5) — FSM Sensar-Pensar-Actuar
- `Scripts/AI/DroneDetectorAI.cs` + `DroneDetectorStatsSO`. HP 80; visión **cono 8 u / 60°** con **raycast de obstrucción** (paredes bloquean); FSM `Patrol → Alert (confirmación 1,5 s) → Chase → Attack`; `Search` 6 s; **Flanking** si DDA activo.
- En `Alert` **emite evento a `AIManager`** con la posición del rover (alimenta la comunicación de drones).

### 3.2 Centinela Secundario (GDD §8.6)
- `Scripts/AI/CentinelaSecundarioAI.cs` + SO. HP 100; patrullaje **vertical en columnas** (no se aleja >8 u del origen); detección omnidireccional 6 u; proyectil recto 10 SI; **cada 3er ataque** proyectil con rastreo parcial (gira hasta 30°). Spawneable por el boss.

### 3.3 Centinela Principal — Boss Final (GDD §8.7)
- `Scripts/AI/CentinelaPrincipalAI.cs` + `LeviatanStats`-style SO + `CentinelaPrincipalAC.controller`. HP **400 en 3 fases**, anclado al centro (máx. 3 u).
  - **Fase 1** (400–268): abanico de 3 proyectiles (12 SI, 8 u/s), cooldown 2,5 s.
  - **Fase 2** (267–134): abanico de 5 + 1 proyectil de rastreo (18 SI, gira 45°, 6 u/s), cooldown 2,0 s, **spawn de un Centinela Secundario** a los 10 s.
  - **Fase 3** (133–0) — *stretch goal, fuera del scope mínimo*: igual F2 + pulso de área radio 4 u c/8 s (25 SI).
- Animator: `Idle`, `Attack_F1`, `Attack_F2`, `Enrage` (transición de fase), `Death` (32f, **no interrumpible**). Su muerte **dispara la secuencia de victoria** (T4).
- Barra de vida del boss en el HUD (delegar en `HUDManager`).

### 3.4 `AIManager` + DDA (GDD §8.1)
- `Scripts/AI/AIManager.cs` (singleton): registro de agentes NPC; **DDA** que solo modifica parámetros de enemigos (rango ×0,75–×1,25) según muertes/SI perdida/tiempo en SI baja; **comunicación de drones** (Detector en Alert → reparte la posición a Patrulleros en 12 u); spawn de enemigos del boss.

**DoD T3:** el Detector ve por cono con obstrucción y avisa a los Patrulleros; el Centinela Secundario patrulla en columna y lanza su proyectil de rastreo cada 3er ataque; el Centinela Principal cumple sus fases 1 y 2 (abanicos + rastreo + spawn) y su muerte dispara la victoria; el DDA baja la presión tras >2 muertes en 5 min y la sube tras 8 min sin morir.

---

## T4 — Cinemáticas, narrativa y condición de victoria (`CinematicManager`)
**Responsable:** Technical Director (+ Artist para assets) · **Rama:** `feature/cinematics` · **Ref. GDD:** §1.3, §12 · **Ref. HUD:** §10

**Estado actual:** `GameManager` ya tiene el estado `Cinematic`; **no existe `CinematicManager`**. Crear `Scripts/Cinematics/CinematicManager.cs`: congela `GameManager` durante la cinemática y gestiona timings.

Pasos:
- **Flashbacks (FB-01…06, GDD §11.3):** reproducir el panel/secuencia al ser disparados por T1 (escaneo). Imagen + texto + audio según ficha; no saltables los marcados.
- **Escena de Spirit — Ala C (GDD §12.4):** pantalla dividida (escaneo Opportunity / telemetría Spirit cayendo a 0% en ~40 s), **no saltable**, 45–55 s. Disparada por SC-06.
- **Condición de victoria (GDD §1.3):** secuencia de transmisión en la sala final — enviar **≥1 fragmento** antes de que la energía llegue a 0% = victoria (resultado óptimo 341/891). Implementar el minijuego/QTE de transmisión y el conteo de fragmentos.
- **Secuencia final (GDD §12.3), 7 escenas:** último intento → transmisión interrumpida (1%→38%, `Fragmentos: 341/891`) → apagón de sistemas → **El silencio (negro absoluto, 10 s exactos, no saltable)** → el informe (JPL) → Marte 2037 (astronauta, **nunca habla**) → plano final + texto de cierre. Respetar la **regla de oro**: el astronauta no habla nunca.
- **`LevelManager`** (GDD §15.1) si hace falta para persistir SI/checkpoints/escaneados entre escenas y alimentar la herencia de SI Niv.1→Niv.2 (coordinar con T2).
- Enganchar el **arco de 4 actos** (GDD §12.1) a hitos (inicio N1, fin N1, N2, final).

**DoD T4:** escanear los objetos clave reproduce sus flashbacks; SC-06 dispara la escena de Spirit no saltable; derrotar al Centinela Principal abre la secuencia de transmisión; enviar ≥1 fragmento gana la partida; la secuencia final reproduce las 7 escenas con los 10 s de negro exactos y sin que el astronauta hable.

---

## T5 — Upgrades de combate y economía de celdas
**Responsable:** Gameplay Programmer · **Rama:** `feature/upgrades` · **Ref. GDD:** §5, §6

**Estado actual:** no existe sistema de upgrades ni uso de celdas; el HUD ya reserva los slots (S03). El Nivel 1 ya pide **Rueda Reforzada** (wall-jump, ya implementado en `PlayerController`) y **Escaneo Mejorado** (engancha T1) como pickups; el Nivel 2 añade los dos de combate.

Pasos:
- `Scripts/Player/UpgradeManager.cs` (o componente en el Player): registro de upgrades desbloqueados; pickups `UpgradePickup` que los activan; reflejar en los **slots de upgrade del HUD** (GDD §10.1).
- **Rueda Reforzada** (Niv.1 Z2): hoy el wall-jump está siempre activo; **gatearlo** tras recoger el pickup (abre rutas verticales). Engancha la `BlockedZone` de S04 T2.
- **Escaneo Mejorado** (Niv.1 Z3): radio 3→5 u + ping pasivo (objetos brillan a <4 u) — engancha T1.
- **Escudo de Plasma** (Niv.2 entrada): absorbe el **primer impacto**, recarga 8 s; indicador de cooldown en el slot del HUD.
- **Batería EMP** (Niv.2 Ala B, opcional): pulso radio 5 u que aturde drones 2 s, recarga 25 s; **ineficaz contra el Centinela Principal**.
- **Uso de celdas de energía (GDD §5):** input `Q` (no reasignable) → **+20 SI**, máx. 2 en reserva; `EnergyCellPickup` que rellena slots; reflejar slots en el HUD. Coordinar con `DegradationSystem` (curación) y `CheckpointManager`.

**DoD T5:** recoger Rueda Reforzada habilita el wall-jump (antes bloqueado) y abre la `BlockedZone`; Escaneo Mejorado sube el radio a 5 u; el Escudo de Plasma absorbe un golpe y recarga; la Batería EMP aturde drones y no afecta al boss; `Q` consume una celda y restaura +20 SI; los slots del HUD reflejan upgrades y celdas.

---

## T6 — Audio del Nivel 2 y de las cinemáticas
**Responsable:** Technical Director (+ Artist para assets) · **Rama:** `feature/audio-system` · **Ref. GDD:** §14 · **Ref. HUD:** §10

**Estado actual:** depende del **núcleo de audio de S04 T4** (Mixer + `MusicState`). Este sprint extiende la capa adaptativa al Relicto y suma la música de cinemática.

Pasos:
- **Ambiente del Relicto** (bus Ambient): zumbido metálico, goteo orgánico, pulsos de bioluminiscencia; capas por ala con volumen por proximidad.
- **SFX de los enemigos N2:** Drone Detector (cono/alerta, disparo, muerte), Centinela Secundario (cañón, proyectil de rastreo), **Centinela Principal** (abanico, rastreo, enrage de fase, muerte).
- **Estado `CINEMATIC`** (GDD §14.1): música original por escena gestionada desde `CinematicManager` (T4); **silencio absoluto** en "El silencio" (10 s) y al congelarse la transmisión.
- Disparadores de música del Nivel 2: Detector/Secundario en Alert/Chase → `TENSION`; arena del Centinela Principal → `COMBAT`; victoria → `CINEMATIC`.
- SFX de la **secuencia de transmisión** (tono de envío de fragmentos, error de energía a 0%).

**DoD T6:** el ambiente del Relicto suena por alas; cada enemigo del Nivel 2 emite sus SFX clave; la música transiciona Exploration↔Tension↔Combat según enemigos/boss; las cinemáticas usan el estado `CINEMATIC` y "El silencio" es realmente silencioso 10 s.

---

## Reglas del proyecto

- No modificar `PlayerController.cs`, `RoverStatsSO.cs` ni `SceneLoader.cs` sin avisar al Gameplay Programmer (T5 toca `PlayerController` para gatear el wall-jump y usar celdas; T2/T4 tocan `SceneLoader`/persistencia: coordinar). No tocar el Canvas de referencia 1920×1080 sin el Technical Director.
- Reusar lo ya construido: patrón **dash ofensivo** + **i-frames centrales** (S03) para todo enemigo nuevo; `HUDManager` para barra de boss y terminal de escaneo; `GameManager.Cinematic` para congelar durante cinemáticas.
- Conventional Commits; rebase sobre `develop` antes del PR; nunca push directo a `main`/`develop`; **siempre** incluir `.meta`; nunca subir `Library/`.

## Progreso

> Sprint creado el 2026-06-26. Depende del cierre de Sprint 04.

| Tarea | Responsable | Prioridad | Estado |
|-------|-------------|-----------|--------|
| T1 — Escaneo, lore y flashbacks | Gameplay Programmer | 🔴 Alta | ⬜ 0% (sin `ScanSystem`; input `IsScanning` ya existe; también cierra los escaneables del Nivel 1) |
| T2 — Nivel 2: El Relicto | Level Designer | 🔴 Alta | ⬜ 0% (solo existe `LoadLevel02()` + Build Settings; sin escena) |
| T3 — Enemigos N2 + `AIManager`/DDA | AI Programmer | 🔴 Alta | ⬜ 0% (sin Detector/Centinelas ni `AIManager`; reusar patrón dash) |
| T4 — Cinemáticas + victoria | Technical Director | 🔴 Alta | ⬜ 0% (sin `CinematicManager`; `GameManager.Cinematic` ya existe) |
| T5 — Upgrades + celdas | Gameplay Programmer | 🟡 Media | ⬜ 0% (sin `UpgradeManager`; slots del HUD reservados en S03) |
| T6 — Audio N2 + cinemáticas | Technical Director | 🟡 Media | ⬜ 0% (depende del núcleo de audio de S04 T4) |
</content>
</invoke>

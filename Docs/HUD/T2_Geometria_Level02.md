# Guía T2 (Sprint 05) — Geometría y poblado de Level02: El Relicto

> **Sprint:** 05 · **Responsable:** Level Designer · **Rama:** `feature/level-design` · **Ref. GDD:** §5, §6, §7, §8, §9.1, §9.3, §13.1 · **Ref. HUD:** §4.2, §5
> **Creado:** 2026-06-26 · **Objetivo:** construir `Level02.unity` (estructura alienígena, **metroidvania de 3 alas en orden semi-libre**) según la dirección del GDD §9.3 y la cronología visual del HUD §4.2.2.

## Progreso

| Bloque | Estado |
|--------|--------|
| `SceneLoader.LoadLevel02()` + entrada en Build Settings | ✅ Ya existe (solo la referencia; sin escena) |
| `Level02.unity` (Grid + tilemaps + colisión) | 🔴 No existe — construir desde cero |
| Bioma N2 (paleta violeta-naranja, luz artificial) | 🔴 Pendiente |
| Hub + 3 alas (A/B/C) + gating semi-libre | 🔴 Pendiente |
| 8 enemigos + 4 checkpoints + 5 celdas | 🔴 Pendiente |
| Escaneables (SC-04/05/06) + 2 upgrades (Escudo, EMP) | 🔴 Pendiente |
| Arena del Centinela Principal + `LevelExit` | 🔴 Pendiente (boss = S05 T3) |
| Parallax 4 capas + proyección isométrica 26° | 🔴 Pendiente |

> **Nota:** la lógica de escaneo (`ScanSystem`), upgrades (`UpgradeManager`), IA de los enemigos N2 y cinemáticas llegan en **Sprint 05 T1/T3/T4/T5**. Esta guía cubre **solo la geometría y la colocación**; deja los objetos posicionados y marcados para que esas tareas los cableen.

---

## 1. Punto de partida — clonar la plantilla de sistemas

`Level02.unity` no existe. **No rehagas los sistemas:** parte de `Level01.unity` como plantilla y reemplaza la geometría/arte.

- Duplica `Level01.unity` → `Assets/Scenes/Level02/Level02.unity` (o crea una escena nueva y copia los contenedores de sistemas).
- Conserva y reusa: HUD completo, `EventSystem`, `CheckpointManager`, `CinemachineCamera` (+ Brain), `Global Light 2D`, música, y los contenedores `Systems`/`Platforms`/`Hazards`/`Enemies`/`Markers`.
- **Vacía** la geometría heredada (tilemaps) y los enemigos/checkpoints del Nivel 1: se repintan para el Relicto.
- Confirma que la escena esté en Build Settings con el nombre `Level02` (ya referenciado por `SceneLoader`).

---

## 2. Métricas objetivo del Nivel 2 (GDD §9.3)

| Parámetro | Valor |
|-----------|-------|
| Bioma | Estructura alienígena — luz **violeta (`#4A1A7A`) / naranja oxidado (`#C0581A`)** sobre negro profundo `#060610`; acentos dorado `#B8860B` y cian `#00E5FF` |
| Duración estimada | 35–50 min |
| Estructura | Metroidvania con **3 alas (A/B/C) en orden semi-libre** que convergen al núcleo |
| Checkpoints | **4** (entrada, Ala A, Ala B, pre-boss) |
| Enemigos | Drone Detector ×3, Drone Patrullero ×2, Centinela Secundario ×2, Centinela Principal ×1 (boss) |
| Escaneables | SC-04, SC-05 (abre el ascensor al hub), SC-06 (Ala C, escena Spirit) |
| Upgrades | Escudo de Plasma (entrada), Batería EMP (Ala B, opcional) |
| Celdas de energía | **5** (1 entrada, 2 Ala B, 2 pre-Centinela) |
| SI al inicio | **47–61%** (heredada del Nivel 1, no reiniciar a 74%) |
| SI estimada al final | **9–29%** |

---

## 3. Medidas de referencia (tuning actual del rover)

> Tiles = unidades (1 tile = 1 u). Igual que en Nivel 1; **confírmalo en Play**.

| Concepto | Valor aprox. | Implicación al pintar |
|----------|--------------|-----------------------|
| Cápsula del Player | ~1.6 × 1.4 | Corredores **≥ 3 tiles de alto** |
| Altura de salto | ~4.5 u | Separación vertical a saltar: **≤ 4 tiles** |
| Salto con carrera | ~10 u | Hueco saltable normal: **≤ 8 tiles** |
| Dash | ~6.6 u (22 u/s · 0.3 s) | Foso que exige dash: más ancho que un salto con carrera |
| **Wall-jump** | disponible | El rover **ya tiene Rueda Reforzada** del Nivel 1 → puedes exigir escalada desde el inicio (Ala B vertical) |

> **Plano de juego ortogonal (GDD §9.1):** el playfield es 2D puro. Los **ángulos isométricos de 26°**, los pasillos "inclinados a 45°" y la "gravedad invertida" del HUD §4.2.2 son **dirección visual/parallax**, no colisión real. Implementa la colisión como suelos/rampas escalonadas ortogonales; cualquier mecánica real de gravedad invertida es coordinación con Gameplay (fuera de esta guía — márcalo si se decide implementar).

---

## 4. Estructura explícita y beat map (HUD §4.2.2 + GDD §9.3)

Topología semi-libre: **Entrada → (ascensor SC-05) → Hub** del que salen las **tres alas A/B/C**; las tres convergen en el **Núcleo Central**, que da paso al **boss**.

```
Descenso (cinemática) → Entrada → [SC-05 abre ascensor] → HUB
                                                           ├─→ Ala A (Superior → Profunda)
                                                           ├─→ Ala B (Inferior → Núcleo/Generador)
                                                           └─→ Ala C (Central/Biblioteca)
                          Ala A + Ala B + Ala C ─────────→ Núcleo Central (pre-boss) → Boss → LevelExit
```

Pinta la **colisión** en `Collision_Tilemap` (layer `Ground`); plataformas one-way en `OneWay_Tilemap` (layer `Platform`); decoración en `Visual`/`Front`; peligros en `Danger_Tilemap` (layer `Ground` + `HazardDamage`, como en N1).

| # | Segmento (HUD §4.2.2) | Geometría a construir | Contenido a colocar | Marcadores |
|---|-----------------------|-----------------------|---------------------|------------|
| 0 | **Descenso** | Rampa empinada de arena que frena en suelo metálico bronce (puede ser solo trigger de entrada / lo cubre la cinemática S05 T4) | trigger de spawn | `SpawnPoint` |
| 1 | **Entrada del Relicto** | Pasillo colosal horizontal, suelo de metal pulido; puerta circular al fondo | **Checkpoint 1** (terminal monolítico), **Escudo de Plasma** (pickup, frente al checkpoint), **SC-04** (registro de tormenta), **1 celda**, **SC-05** (glifo → abre el ascensor) | `Checkpoint`, `UpgradePickup`, `Scannable` ×2, `EnergyCellPickup` |
| 2 | **Hub / ascensor** | Sala-nodo con el ascensor desbloqueado por SC-05; 3 salidas hacia A/B/C | gate del ascensor (bloqueado hasta escanear SC-05) | `BlockedZone` (ascensor), nodos de ruta |
| 3 | **Ala A — Superior** | Corredores anchos con rampas/escalones (lectura "inclinada 45°"); plataformas flotantes | **Drone Detector ×1** (cono de visión), hazards opcionales | `EnemyPatrol` (Detector) |
| 4 | **Ala A — Profunda** | Pasadizos estrechos y oscuros (≥3 tiles alto); ruta al archivo | **Drone Detector ×1**, **Drone Patrullero ×1**, **Checkpoint 2** | `Checkpoint`, `EnemyPatrol` ×2 |
| 5 | **Ala B — Inferior** | **Pozos verticales** con plataformas metálicas suspendidas (separación ≤4 tiles); exige saltos precisos y wall-jump | **rayos de energía violeta** = hazards `Danger`/`HazardDamage` (objeto ambiental 6 SI, GDD §4.3); **Batería EMP** (pickup, zona profunda, opcional); **2 celdas** | `UpgradePickup` (EMP), `EnergyCellPickup` ×2, hazards |
| 6 | **Ala B — Núcleo (Generador)** | Bóveda cilíndrica amplia; núcleo central decorativo (anillos giratorios) | **Centinela Secundario ×1**, **Checkpoint 3**, escaneable del núcleo (ver nota SC-08) | `Checkpoint`, `EnemyPatrol`, `Scannable` |
| 7 | **Ala C — Central (Biblioteca)** | Sala solemne cerrada, paredes de cristales; terminal central | **SC-06** (archivo Spirit → escena split-screen S05 T4), ambiente sin enemigos o **Drone Detector ×1** vigilando la entrada | `Scannable` (SC-06), `EnemyPatrol` opcional |
| 8 | **Núcleo Central (pre-boss)** | Bóveda esférica grande donde **convergen las 3 alas**; dispositivo de transmisión central | **Checkpoint 4**, **2 celdas** (pre-Centinela), **Drone Patrullero ×1** opcional | `Checkpoint`, `EnergyCellPickup` ×2 |
| 9 | **Arena — Centinela Principal** | Sala amplia y cerrada (lockdown al entrar); suelo despejado para esquivar abanicos | **BossRoom** spawn del Centinela Principal (lógica S05 T3); un **Centinela Secundario** lo spawnea el boss en Fase 2 | `BossRoom` |
| 10 | **Salida / Cinemática final** | Trigger tras derrotar al boss → secuencia final (S05 T4) | `LevelExit` / trigger de cinemática | `LevelExit` |

### Reparto de enemigos (cierra el presupuesto)
- **Drone Detector ×3:** Ala A Superior, Ala A Profunda, y vigilancia del Ala C (o repartir según presupuesto).
- **Drone Patrullero ×2:** Ala A Profunda + Núcleo Central (reusa el prefab/IA de N1).
- **Centinela Secundario ×2:** uno colocado en Ala B Núcleo; el segundo lo **spawnea el boss** en Fase 2 (GDD §8.7) → coordina con T3 para no duplicar.
- **Centinela Principal ×1:** arena final.
- **Presupuesto de daño:** el recorrido debe dejar al rover en **9–29% SI** al terminar (parte de 47–61%). Distribuir proyectiles de drones (8–12 SI), Centinelas (10–18 SI) y rayos de Ala B (6 SI) sin saturar antes del boss.

> ⚠️ **Inconsistencia de IDs a reconciliar:** el GDD §11.2 lista para el Nivel 2 **SC-04, SC-05, SC-06**; el HUD §4.2.2(6) menciona además un **SC-08** (núcleo del Ala B). Antes de colocar, confirma con diseño si el núcleo del Ala B es un escaneable extra (SC-07/08) o si se omite. Esta guía usa los 3 del GDD como canónicos y deja el del núcleo como opcional.

---

## 5. Gating (orden semi-libre)
- **Ascensor:** bloqueado (`BlockedZone` + tiles) hasta escanear **SC-05**; al abrirse habilita el Hub con las 3 alas.
- **Ala B** exige **wall-jump** (ya disponible por Rueda Reforzada de N1) por su verticalidad.
- **Batería EMP** opcional (no bloquea progreso; herramienta anti-drones).
- **Puerta al boss:** se abre al converger las 3 rutas en el Núcleo Central (o tras un requisito mínimo a definir con diseño).

## 6. Parallax e isometría (HUD §4.2, §5)
- **4 capas** de parallax (GDD §9.1, vs 2 montadas en N1) con el componente `ParallaxMovement` (`Scripts/Level/ParallaxBackground.cs`, reutilizado de S02 T1 — N hijos con material en Wrap = Repeat): fondo lejano (megaestructuras/engranajes, escala 60–70%, saturación −30%), plano medio (columnas isométricas que se iluminan al paso), primer plano (fragmentos/cables, escala 110–120%, oscuros).
- **Proyección isométrica 26°** solo en estructuras y plataformas de **fondo** (caras superiores más claras); el plano jugable permanece ortogonal.
- Líneas de luz cian/violeta como decoración en `Visual`/`Front`.

## 7. Cámara, herencia de SI y cierre técnico
- `CinemachineCamera.Follow = Player`; un `Cinemachine Confiner 2D` por sala/ala para acotar el encuadre (sobre todo en los pozos del Ala B).
- **Herencia de SI:** el rover entra con la SI con que terminó el Nivel 1 (47–61%), **no** a 74%. Coordinar con `LevelManager`/T4: la escena no debe forzar SI inicial.
- Cerrar techos y paredes laterales en `Collision_Tilemap`.
- Asignar el campo `rover` de cada enemigo al Player de la escena (si no, `NullReferenceException`).

---

## 8. Pruebas (Definition of Done)

En **Play Mode**:

- [ ] La escena `Level02.unity` carga desde `SceneLoader.LoadLevel02()` con el HUD y la cámara funcionando.
- [ ] Se recorre **Entrada → 3 alas → Núcleo Central → Arena** sin caer al vacío ni atascarse.
- [ ] **SC-05 abre el ascensor**; antes de escanearlo el Hub está bloqueado.
- [ ] Los **4 checkpoints** registran (entrada, Ala A, Ala B, pre-boss).
- [ ] Los **8 enemigos** (3 Detector + 2 Patrullero + 2 Secundario + boss) están colocados con su `rover` asignado y persiguen (lógica IA = S05 T3).
- [ ] SC-04/05/06 están posicionados; Escudo de Plasma y Batería EMP recogibles; las 5 celdas colocadas.
- [ ] Los **rayos de energía del Ala B** hacen daño; el wall-jump permite subir los pozos.
- [ ] El **parallax de 4 capas** se mueve y las estructuras de fondo leen isométricas (26°).
- [ ] El rover entra con la **SI heredada del Nivel 1** (no se reinicia a 74%).
- [ ] Al entrar a la arena se dispara el lockdown y el spawn del boss (S05 T3); al derrotarlo, el `LevelExit` dispara la cinemática (S05 T4).

---

## 9. Cierre

```bash
git add Assets/Scenes/Level02 "Assets/**/*.meta"
git commit -m "feat(level): construir Level02 (El Relicto) — geometria, alas y poblado"
git push origin feature/level-design
# Abrir PR → base: develop
```

> Recuerda: **siempre** incluir los `.meta`; nunca push directo a `develop`; añade `Level02` a Build Settings si no aparece.
</content>

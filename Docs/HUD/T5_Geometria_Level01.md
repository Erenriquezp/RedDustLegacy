# Guía T5 — Geometría y poblado de Level01

> **Sprint:** 02 (cierre arrastrado a Sprint 04 T2) · **Responsable:** Level Designer · **Rama:** `feature/level-design` · **Ref. GDD:** §9.2, §16.1
> **Revisión:** 2026-06-26 · **Objetivo:** completar el recorrido íntegro del Nivel 1 sobre la escena **`Level01.unity`**, que ya está consolidada y es jugable.

## Progreso

| Bloque | Estado |
|--------|--------|
| Consolidación `Level01_2.0` → `Level01.unity` | ✅ Hecho (la escena oficial ya tiene la geometría) |
| Grid + 6 tilemaps + colisión + `Danger` (layer Ground + daño) | ✅ Hecho |
| Tileset (`piso_1_mundo`, 64 px, PPU 64) + Player jugable | ✅ Hecho |
| Geometría base recorrible + hazards + 4 checkpoints | ✅ Hecho (base; falta cerrar zonas) |
| Build Settings (`MainMenu → Level01 → Level02`) | ✅ Hecho |
| Marcadores `LevelMarker` de zona | 🔴 Solo `SpawnPoint` (Z1); faltan el resto |
| Poblar enemigos (6 Biol + 2 Drones) | 🔴 Hoy solo 1 + 1 |
| Escaneables, upgrades, arena del boss, parallax | 🔴 Pendiente (detalle abajo) |

---

## 1. Hecho (no lo rehagas)

`Level01.unity` ya es la escena oficial y jugable (se promovió desde `Level01_2.0`; ya no hay escenas divergentes):

- **Grid + 6 tilemaps** configurados: `Collision_Tilemap` (layer `Ground`, `CompositeCollider2D` Static), `OneWay_Tilemap` (layer `Platform`, `PlatformEffector2D`), `Visual`/`Markers`/`Front` (decoración) y **`Danger_Tilemap`** (layer `Ground` + `HazardDamage` → actúa como suelo y hace daño).
- **Tileset** `piso_1_mundo` sliceado a 64 px, **PPU 64** (1 tile = 1 u), tiles con Collider Type `Grid`.
- **Player** funcionando (corre, salta, dash, wall-jump) y **geometría base recorrible** con hazards (≈16 cristales, obstáculos giratorios, `PlataformaMovil`, `Caida*`), plataformas flotantes y **4 checkpoints**.
- **Sistemas cableados:** HUD completo, `CheckpointManager`, `CinemachineCamera` (Follow = Player), `Global Light 2D`, música. Todo en contenedores (`Systems`/`Platforms`/`Hazards`/`Enemies`/`Markers`).

---

## 2. Medidas de referencia (tuning actual del rover)

> Tiles = unidades (1 tile = 1 u). Valores de `RoverStats_Default`; **siempre confírmalo en Play**.

| Concepto | Valor aprox. | Implicación al pintar |
|----------|--------------|-----------------------|
| Cápsula del Player | ~1.6 ancho × 1.4 alto | Corredores **≥ 3 tiles de alto** |
| Altura de salto | ~4.5 u (jumpForce 13.5 / grav 20) | Separación vertical a saltar: **≤ 4 tiles** |
| Salto con carrera (horizontal) | ~10 u | Hueco saltable normal: **≤ 8 tiles** |
| Dash | 22 u/s · 0.3 s ⇒ ~6.6 u | Foso que **exige** dash: más ancho que un salto con carrera |

---

## 3. Pendiente — completar el Nivel 1

La geometría base existe; falta **poblar y cerrar el recorrido** Sala de Reinicio → Arena del Leviatán. El **beat map completo de 8 segmentos** vive en **[Sprint_04.md → T2](../Sprints/Sprint_04.md)** (úsalo como fuente). Resumen accionable:

### 3.1 Marcadores de zona (`LevelMarker`)
GameObject vacío → `Add Component → LevelMarker` → elige `Type` + `label`. Hoy solo hay `SpawnPoint` (Z1). Colocar los que faltan:
- `Scannable` SC-01 (Z1), SC-02 (Z2/caverna), SC-03 (Z4).
- `EnemyPatrol` por cada enemigo (ver 3.2).
- `Checkpoint` en los puntos de los 3 checkpoints de diseño (hoy hay 4 instancias colocadas; confirmar posiciones vs GDD §7: inicio, media, pre-boss).
- `BlockedZone` (exige Rueda Reforzada, ~Z2) y `BossRoom` (Z7, spawn del boss).
- `LevelExit` / trigger de cinemática al final (Z8).

### 3.2 Poblar enemigos (de 1+1 a 6+2)
- **6 Ser Bioluminiscente** y **2 Drone Patrullero** distribuidos por zonas según el presupuesto de daño (dejar al rover en **47–61% SI** al llegar al boss).
- Por cada instancia: **asignar el campo `rover`** al Player de la escena (si no, `NullReferenceException`).
- **Corregir la instancia actual del Biol:** tiene un override de tag `Player` equivocado — debe ser su tag de enemigo.

### 3.3 Escaneables y upgrades
- **SC-01/02/03:** colocar los objetos escaneables; la lógica de escaneo llega en **Sprint 05 T1** (`ScanSystem`). Dejar los `Scannable` posicionados para que T1 solo los cablee.
- **Upgrades:** pickup de **Rueda Reforzada** (Z2, abre la `BlockedZone`) y **Escaneo Mejorado** (Z3). El sistema de upgrades llega en **Sprint 05 T5**; aquí va la colocación y el gating de tiles.

### 3.4 Arena del boss y salida
- **Arena del Leviatán** (Z7): sala circular cerrada de basalto con **lockdown** al entrar; el spawn y la lógica del boss son **Sprint 04 T3**.
- **Trigger de cinemática / `LevelExit`** al derrotar al boss → transición a Nivel 2.

### 3.5 Parallax del fondo
- El `Background` es estático. Montar las **3 capas** (factores 0.15 / 0.45 / 0.85) con los scripts `Scripts/Level/Parallax*` (ya existen, sin montar) y los fondos de `Art/Backgrounds/`. Deuda de Sprint 02 T1.

### 3.6 Cierre de geometría
- Cerrar techo y paredes laterales en `Collision_Tilemap` para que el Player no salga del mapa.
- Confirmar que las paredes de wall-jump (Z2/Z5) están en layer `Ground`.
- Opcional: `Cinemachine Confiner 2D` por sala para acotar la cámara.

---

## 4. Pruebas (Definition of Done)

En **Play Mode**:

- [x] El Player aparece sobre el spawn y no atraviesa suelo ni paredes.
- [x] El `Danger_Tilemap` actúa como suelo y hace daño al contacto.
- [ ] Se recorre **Sala de Reinicio → Arena del Leviatán** sin caer al vacío ni atascarse.
- [ ] Los **6 Biol + 2 Drones** están colocados, tienen su `rover` asignado y persiguen.
- [ ] Los **3 checkpoints** registran y el rover llega al boss con SI dentro de **47–61%**.
- [ ] SC-01/02/03 están posicionados (escaneo cableado en S05 T1).
- [ ] El foso que exige dash no se cruza a la carrera; **sí** con dash; el wall-jump funciona en las paredes `Ground`.
- [ ] El parallax de 3 capas se mueve detrás del tilemap.
- [ ] La cámara sigue al Player y no se sale del mapa.

---

## 5. Cierre

```bash
git add Assets/Scenes/Level01 "Assets/**/*.meta"
git commit -m "feat(level): poblar Level01 (enemigos, marcadores, escaneables, parallax)"
git push origin feature/level-design
# Abrir PR → base: develop
```

> Recuerda: **siempre** incluir los `.meta`; nunca push directo a `develop`.
</content>

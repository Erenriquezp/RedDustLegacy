# Guía T5 — Pintar la geometría de Level01

> **Sprint:** 02 · **Responsable:** Level Designer · **Rama:** `feature/level-design` · **Ref. GDD:** §9.2, §16.1
> **Objetivo:** pintar el nivel recorrible (Z1→Z5) sobre el `Grid` que **ya está montado y funcionando**.

---

## 0. Lo que YA está listo (no lo rehagas)

> ⚠️ **Revisión 2026-06-23:** la geometría jugable real (Grid + 6 tilemaps + 2 `CompositeCollider2D`, con Player y Biol instanciados y mecánicas `PlataformaMovil`/`GiroCompleto`/`OsciladorGiro`) ya está construida en **`Assets/Scenes/Level01_2.0/Dev_PlayerMovement.unity`**. La `Level01.unity` "oficial" **sigue vacía** (solo la cámara Cinemachine). Lo que falta de esta guía: **decidir y ejecutar la consolidación** (promover `Level01_2.0` a `Level01.unity` o copiar su contenido), **colocar los marcadores `LevelMarker`** y las trampas `CaidaCristal`/`CaidaPorCercania`, y **cerrar el beat map** de las 5 zonas. Tras consolidar, actualiza `SceneLoader` + Build Settings si cambia el nombre/ruta de la escena.

- ✅ **Grid + tilemaps** montados y configurados (en la escena `Level01_2.0/Dev_PlayerMovement.unity`):
  - `Collision_Tilemap` → layer **`Ground`**, con `TilemapCollider2D` (Composite Operation = **Merge**) + `CompositeCollider2D` + `Rigidbody2D` (**Static**).
  - `OneWay_Tilemap` → layer **`Platform`**, con `PlatformEffector2D`.
  - `Visual_Tilemap` y `Markers_Tilemap` → sin collider (decoración / marcas).
- ✅ **Tileset real** (`piso_1_mundo`) sliceado a 64 px, **PPU = 64** (1 tile = 1 unidad), Tiles generados en `Assets/Art/Tiles/` con **Collider Type = `Grid`**, y paleta lista.
- ✅ **Player funcionando**: se apoya en el suelo, corre, salta y hace dash.

**Lo único que falta es esta guía: pintar la geometría de las 5 zonas** (y luego colocar marcadores y enemigos, §4).

> La geometría jugable actual está en `Assets/Scenes/Level01_2.0/Dev_PlayerMovement.unity`. El destino **final** debe ser `Assets/Scenes/Level01/Level01.unity` (la que carga `SceneLoader`); decide pronto si renombras/promueves la escena `Level01_2.0` o copias su contenido, para no mantener dos escenas divergentes.

---

## 2. Medidas de referencia (movimiento actual del rover)

> Las medidas están en **tiles = unidades** (1 tile = 1 u). Los saltos/dash dependen del tuning de `RoverStats_Default`; **al final siempre pruébalo en Play y ajusta**.

| Concepto | Valor aprox. | Implicación al pintar |
|----------|--------------|-----------------------|
| Cápsula del Player | ~1.6 ancho × 1.4 alto | Corredores **≥ 3 tiles de alto** para pasar cómodo |
| Altura de salto | ~4.5 u (jumpForce 13.5 / grav 20) | Plataformas a saltar: separación vertical **≤ 4 tiles** |
| Salto con carrera (horizontal) | ~10 u | Un hueco saltable normal: **≤ 8 tiles** |
| Dash | 28 u/s · 0.5 s ⇒ alcance largo | Foso que **exige** dash: más ancho que un salto con carrera |

---

## 3. Pintar el beat map (Z1 → Z5)

**Flujo de trabajo en la Scene view** (con el cursor sobre ella):
- **B** = pincel · **U** = relleno rectangular (clic-arrastra una caja, ideal para suelos largos) · **Shift+clic** = borrar · **I** = cuentagotas (copia un tile ya puesto).
- Pinta cada zona de **izquierda a derecha**.

> **Prueba rápida primero:** con `Active Tilemap = Collision_Tilemap`, pinta una línea de suelo de ~12 tiles **debajo** del Player y dale Play. Debe quedarse encima sin caer. Cuando eso funcione, sigue con las zonas.

| Zona | Geometría a pintar (en `Collision_Tilemap` salvo que se diga) | Reglas |
|------|--------------------------------------------------------------|--------|
| **Z1 — Entrada / tutorial** | Suelo plano largo (~25 tiles), 1–2 escalones bajos y un hueco simple de 2–3 tiles | Corredor ≥3 tiles de alto. Enseña correr + saltar |
| **Z2 — Primera tensión** | Un **foso ancho que obligue a dash**; sala lateral elevada cerrada; tramo plano para patrulla enemiga | El foso no debe cruzarse a la carrera: ensánchalo hasta que **solo el dash** lo pase |
| **Z3 — Respiro** | Sala cerrada y segura, sin enemigos; 1–2 plataformas en `OneWay_Tilemap` para alcanzar el pickup | Sin fosos ni peligros |
| **Z4 — Escalada vertical** | Pasillo **angosto vertical** (2–3 tiles de ancho) con paredes enfrentadas para wall-jump; plataformas a distintas alturas (≤4 tiles entre saltos) | Las paredes **deben** estar en `Collision_Tilemap` (layer `Ground`) o el wall-jump no funciona |
| **Z5 — Boss** | Sala amplia horizontal (~20×10), suelo plano y despejado | Sin obstáculos en el piso |

> **Cierra los bordes** del nivel (techo y paredes laterales) con tiles en `Collision_Tilemap` para que el Player no se salga del mapa.

**Decoración:** una vez tengas la colisión, cambia a `Active Tilemap = Visual_Tilemap` y pinta los cristales/fondo encima sin preocuparte por la física.

---

## 4. Después de pintar (breve)

Cuando la geometría esté recorrible:

- **Marcadores (`LevelMarker`):** GameObject vacío → `Add Component → LevelMarker` → elige `Type` y `label`. Mínimos: `SpawnPoint` (inicio Z1), `EnemyPatrol` (Z2), `Checkpoint` (fin Z2 y pre-boss), `Scannable` (Z1/Z3/Z4), `BossRoom` (Z5). Son solo guías visuales; la lógica llega en Sprint 03.
- **Player:** muévelo al `SpawnPoint`, sobre el suelo (no dentro de tiles). Confirma en su `PlayerController` que `Ground Layer` y `Wall Layer` = **`Ground`**.
- **Biol:** arrastra `Assets/Prefabs/Enemies/SerBioluminiscente.prefab` a cada `EnemyPatrol` y **asigna el campo `rover`** al Player de la escena (sin eso lanza `NullReferenceException`).
- **Cámara:** `CinemachineCamera` → `Follow` = Player. Opcional: `Cinemachine Confiner 2D` para que no muestre fuera del mapa.

---

## 6. Pruebas (Definition of Done)

En **Play Mode**:

- [ ] El Player aparece sobre el spawn y **no atraviesa** suelo ni paredes.
- [ ] Se recorre **Z1 → Z5** sin caer al vacío ni quedar atascado en la geometría.
- [ ] El **foso de Z2** no se cruza a la carrera; **sí** con dash.
- [ ] En **Z4** el wall-jump funciona (paredes en layer `Ground`).
- [ ] Las plataformas `OneWay` se atraviesan desde abajo y sostienen desde arriba.
- [ ] El `Collision_Tilemap` muestra **contorno verde** sobre los tiles sólidos.
- [ ] La cámara sigue al Player y no se sale del mapa.

---

## 7. Cierre

```bash
git add Assets/Scenes/Level01 Assets/Art/Tiles "Assets/**/*.meta"
git commit -m "feat(level): paint Level01 geometry (Z1-Z5) with collision tilemaps"
git push origin feature/level-design
# Abrir PR → base: develop
```

> Recuerda: **siempre** incluir los `.meta`; nunca push directo a `develop`.

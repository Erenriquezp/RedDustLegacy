# Sistema de escaneo y lore — Guía de implementación y diseño visual (S05 T1)

> Ref. GDD: §11 (parámetros, SC-01…06, flashbacks) · §1.1 pilar 01 · §5 (celdas) · Estado: por implementar

El escaneo es la mecánica de identidad del juego: Opportunity no dispara — **analiza**. Cada escaneo debe sentirse como ciencia real leída por una máquina, no como "recoger un coleccionable". Esta guía cubre arquitectura, contenido y presentación.

## 1. Qué existe ya (no reimplementar)

| Pieza | Dónde | Qué hace |
|-------|-------|----------|
| Input de escaneo | `PlayerController.OnScanInput` | `E` mantenida → `IsScanning = true`, eventos `OnScanStarted/OnScanStopped` |
| Frenado del rover | `PlayerController.HandleRun` | Con `IsScanning`, velocidad X = 0 (escanear = quedarse expuesto) |
| Stun del Biol | `BioluminescentAI.UpdateChase` | Si el rover escanea cerca, se aturde — **no tocar** |
| SFX del escáner | `PlayerAudioController` (S04) | Loop de escaneo ya cableado a los eventos |
| Alert strip del HUD | `HUDManager.ShowAlert` | Reusable para "NUEVO REGISTRO" |
| Congelado de cinemáticas | `GameManager.SetCinematic` | Lo usará el handoff a flashbacks (T4) |

## 2. Arquitectura

```
Player
 └─ ScanSystem.cs            (nuevo — Scripts/Scan/)
     ├─ escucha OnScanStarted/Stopped del PlayerController
     ├─ mientras escanea: OverlapCircle(radio) en capa Interactable
     ├─ objetivo = Scannable más cercano → muestra ficha en ScanTerminalUI
     └─ al completar: Scannable.Consume() → gating + flashback + registro

Escena
 └─ objetos con Scannable.cs (nuevo) + Collider2D (trigger) + capa Interactable
     └─ datos en ScanDataSO (nuevo SO — patrón del proyecto)

HUD_Canvas
 └─ ScanTerminalUI.cs (nuevo) — panel terminal, typewriter, corrupción por SI
```

### 2.1 `ScanDataSO` (ScriptableObject — menú `Opportunity/Scan Data`)

| Campo | Tipo | Notas |
|-------|------|-------|
| `id` | string | `SC-01`…`SC-06` (tabla GDD §11.2) |
| `encabezado` | enum | `ANALISIS` · `REGISTRO_AMBIENTAL` · `SENAL_IDENTIFICADA` · `ARCHIVO_HISTORICO` |
| `severidad` | enum | `Nominal` (blanco) · `Advertencia` (ámbar) · `Anomalia` (rojo) — color del texto |
| `texto` | string (multiline) | Literal del GDD §11.2 — **sin tildes ni ñ** (estética de terminal) |
| `flashbackId` | string | `FB-01`… o vacío; el SI mínimo lo valida el CinematicManager (GDD §11.3) |
| `unaVez` | bool | SC de historia = true; escaneables ambientales repetibles = false |

### 2.2 `Scannable.cs` (en el objeto de escena)

- `[SerializeField] ScanDataSO data` + `UnityEvent onScanned` — el gating se cablea en el Inspector **sin código nuevo**: SC-01 activa la ruta oculta, SC-03 abre la puerta del Leviatán, SC-05 activa el ascensor (T2 coloca los objetos).
- `bool YaEscaneado` — con `unaVez`, el segundo escaneo muestra la ficha pero no re-dispara `onScanned` ni el flashback.
- Los escaneados **permanecen tras el respawn** (GDD §1.3): el respawn no recarga la escena, así que el estado del componente sobrevive solo. La recarga completa de nivel (3.ª muerte) sí lo resetea — aceptable en prototipo.

### 2.3 `ScanSystem.cs` (en el Player)

- Radio: campo `scanRadius = 3f`; cuando exista `UpgradeManager` (T5) leerlo de ahí (3 → 5 u + ping pasivo <4 u). No hardcodear el 5.
- Bucle (solo mientras `IsScanning`): `Physics2D.OverlapCircleAll(pos, radio, LayerMask "Interactable")` a 10 Hz (no cada frame) → `Scannable` más cercano.
- **Adquisición de 0,4 s**: el objetivo debe mantenerse bajo el radio 0,4 s antes de mostrar la ficha (anillo de progreso). Evita que rozar un objeto dispare lore y hace del escaneo un *acto* con riesgo (estás quieto).
- Al soltar `E` o salir del radio: la ficha hace fade 0,3 s (GDD §11.1). Si la adquisición no terminó, se cancela sin efectos.
- Evento público `OnScanCompleted(ScanDataSO)` → lo consumen `CinematicManager` (flashback, T4), el códex (S06) y el registro del Archivo de Misión.

## 3. Diseño visual — el terminal

La ficha es un **terminal embebido del rover**, no un cuadro de diálogo. Referencia de tono: telemetría de la pantalla de carga (GDD §12.2).

| Elemento | Especificación |
|----------|---------------|
| Posición | Panel inferior-derecho del HUD, nunca tapa al rover ni la barra de SI; ancho ~40% de pantalla |
| Fondo | Negro `#060610` al 85% de opacidad, borde 1 px del color de severidad, esquinas rectas |
| Tipografía | Monoespaciada (TMP), tamaño reducido; encabezado en tamaño +2 y el color de severidad |
| Entrada del texto | **Typewriter ~40 caracteres/s** con blip de terminal cada 3–4 caracteres (no por carácter — satura) |
| Colores | Blanco = nominal · Ámbar = advertencia · Rojo = anomalía (GDD §11.1); cursor `▌` parpadeante al final |
| Salida | Fade 0,3 s del panel completo al soltar |

**Feedback en el mundo (tan importante como el panel):**
- Al mantener `E`: **anillo expandiéndose** desde el rover hasta el radio real (3/5 u) — honesto con la mecánica, el jugador aprende el alcance viéndolo.
- Objeto en adquisición: highlight (brillo/outline en el SpriteRenderer) + el anillo de progreso de 0,4 s sobre el objeto.
- Al completar: pulso breve en el objeto + `ShowAlert("NUEVO REGISTRO — SC-0X", 2f)` si es la primera vez.

## 4. Corrupción por degradación (SI ≤ 29%)

El hardware que muere también corrompe la ciencia — es narrativa mecánica, no un efecto gratuito.

- `CorruptText(string s, float severity)`: reemplaza un % de caracteres por glifos (`▓ █ ░ # @ /`) — severidad 0,10 en Fase 4 (SI ≤29%), 0,25 en Fase 5, 0,40 en Fase 6.
- **Determinista por posición** (semilla = índice del carácter + id del SC): el mismo texto se corrompe igual en cada lectura; con refresco a 4 Hz algunos glifos "parpadean" entre dos estados. Nunca aleatorio por frame (ilegible y epiléptico).
- Nunca corromper el encabezado ni la primera línea completa — el jugador siempre debe saber *qué* está leyendo, aunque pierda el detalle.

## 5. Contenido y gating (colocación: T2 de S04/S05)

SC-01…06 y sus funciones/textos: **GDD §11.2 es la fuente de verdad — copiar literal.** Recordatorio de gating: SC-01 ruta oculta (N1 Z1) · SC-03 puerta del Leviatán (N1 Z4) · SC-05 ascensor del Relicto (N2). FB por escaneo según GDD §11.3, con SI mínimo (p. ej. FB-06 requiere SI ≤9% — el CinematicManager lo valida, el ScanSystem solo notifica).

## 6. Orden de implementación

1. Capa `Interactable` + `ScanDataSO` + `Scannable` con los 6 assets SC (textos del GDD).
2. `ScanSystem`: detección + adquisición + `OnScanCompleted`.
3. `ScanTerminalUI`: panel + typewriter + colores + fade.
4. Corrupción por SI (leer `DegradationSystem.CurrentPhase`).
5. Gating por `UnityEvent` (probar SC-03 con la puerta del Leviatán, ya existente).
6. Handoff a flashbacks (cuando T4 exista; hasta entonces, `Debug.Log`).
7. Polish: anillo, highlight, blips.

## 7. Criterios de aceptación

- [ ] Mantener `E` a <3 u de SC-01 durante 0,4 s → ficha con typewriter; soltar → fade 0,3 s.
- [ ] El rover no se mueve mientras escanea; el Biol se sigue aturdiendo (regresión S03).
- [ ] SC-03 abre la puerta del Leviatán solo la primera vez; re-escanear muestra la ficha sin re-disparar el evento.
- [ ] Con SI 25% el texto sale corrupto pero el encabezado se lee limpio.
- [ ] Morir y respawnear no resetea los SC ya escaneados.
- [ ] Sin `CinematicManager` en escena, escanear no lanza excepciones (flashback pendiente ≠ crash).

# Sprint 02 — Level01 jugable, enemigo bioluminiscente y audio

> **Estado:** 🔄 En progreso · **Revisión:** 2026-06-26 · **Ref. GDD:** §8, §9.2, §13, §14

Geometría de Level01, animaciones del rover, Ser Bioluminiscente y audio.

## Progreso

| Tarea | Estado |
|-------|--------|
| T3 — Ser Bioluminiscente | ✅ Completo (ahora **derrotable con dash**) |
| T4 — Audio (capa SFX) | ✅ Rover, enemigos y UI |
| Deuda S01 — bug `OnDashed` | ✅ Resuelto |
| T2 — Flip de dirección del rover | ✅ Resuelto (por escala) |
| T5 — Geometría Level01 | 🔄 En `Level01.unity`; faltan marcadores/zonas y arreglos de capas/Tag |
| T4 — Audio (Mixer + MusicState) | 🔴 0% — sin `.mixer` ni `MusicState` (detalle abajo) |
| T2 — Animaciones Land/Damage | 🔴 Faltan sprites + clips |
| T1 — Parallax | 🔴 Scripts duplicados; falta el de unión + montar (detalle abajo) |
| Deuda S01 — bindings | 🔴 Jump/Scan sin ajustar |

## Hecho

- **Ser Bioluminiscente** (`BioluminescentAI`): FSM Idle/Alert/Chase, ataque, `ApplyStun`, HP/muerte, aturdimiento al escanear. SFX 3D vía `EnemyAudioController`. Ahora **daña al contacto con i-frames+knockback** y es **derrotable embistiéndolo con dash** (ver S03 — combate). Corregido el AnimationEvent vacío de `BiolAttack.anim`.
- **Flip de dirección del rover**: `PlayerAnimatorController` voltea por **escala** del sprite (no `flipX`, que no se reflejaba) usando el input en vivo. Gira con A/D.
- **Geometría** consolidada en `Level01.unity` (Grid + 6 tilemaps, Player, Biol, Drone) — escena promovida desde `Level01_2.0`.
- **Mecánicas de nivel** (`Scripts/leveo01/`): `PlataformaMovil` (arrastra al Player), `GiroCompleto`, `OsciladorGiro`, trampas `CaidaCristal`/`CaidaPorCercania` y `HazardDamage` (pinchos/obstáculos) — todas dañan con i-frames+knockback.
- **Audio SFX**: `PlayerAudioController` (dash/landing/daño-por-fase/muerte), `EnemyAudioController`, `UIAudioController`.
- **Deuda S01**: bug `OnDashed→PlayDamageSound` corregido (`sfxDash` propio).

## Pendiente

### T4 — Audio Mixer y estados de música (núcleo, 0%)

**Estado del código:** `AudioManager` (namespace `Core`) solo tiene `musicSource`, `sfxGlobalSource`, `PlayMusic(clip, loop)`, `TriggerGameOverMusic()` y `PlayGlobalSFX(clip)`. No hay `.mixer` ni `MusicState`.

**1 — Crear el Mixer** `Assets/Audio/MainMixer.mixer`:
- Grupos: `Master → { Music, SFX, Ambient }` (añade `UI` cuando llegue Sprint 04 T4).
- Expón el volumen de cada grupo (clic derecho en el slider del *Attenuation* → *Expose ... to script*) y renómbralos `MusicVol`, `SfxVol`, `AmbientVol`. El panel de Opciones (S04 T1) los moverá con `mixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Clamp(v01, 0.0001f, 1f)) * 20f)` (slider 0–1 → dB).
- Enrutar salidas (`outputAudioMixerGroup`): `musicSource` → **Music**; `sfxGlobalSource` + los `AudioSource` del rover (`PlayerAudioController`) y enemigos (`EnemyAudioController`) → **SFX**; el ambiente de nivel → **Ambient**.

**2 — Música adaptativa en `AudioManager`:**
- Añade un **segundo `AudioSource` (`musicSourceB`)** para cruzar A↔B sin cortes, y un clip serializado por estado.
- `enum MusicState { Silence, Exploration, Tension, Combat, Cinematic }` + `SetMusicState(MusicState s)`: si el estado cambia, arranca el clip nuevo en el source inactivo a volumen 0 y lerpea los volúmenes en una corutina durante el crossfade; al terminar, detén el source viejo.
- Tiempos de crossfade (**GDD §14.1**, tiempo para *entrar* a cada estado): `Exploration` **1.5 s** · `Tension` **0.8 s** · `Combat` **0.5 s** · `Cinematic` lo gestiona la cinemática (S04/S05).
- Disparadores (los conecta cada sistema): IA Biol/Drone en `Alert`/`Chase` → `Tension`; boss activo o varios en `Chase` → `Combat`; SI ≥41 % y sin enemigos → `Exploration`.

**DoD parcial T4:** los 3 buses se ajustan por separado desde código y la música cruza entre Exploration/Tension/Combat sin corte audible.

### T2 — Animaciones Land / Damage (el flip ya está hecho)
- Sprites `Opportunity-land` (4f) y `Opportunity-damage` (5f).
- Clips `Rover_Land`/`Rover_Damage` en `RoverAC` (Duration 0, Has Exit Time OFF, Write Defaults OFF). El trigger `IsDamaged` ya se dispara desde código (S03); falta el clip. `Wall_Slide`/`Wall_Jump` → V2.
- **Deuda del controlador:** `Rover_Death` no tiene transición de salida; hoy se sale por código (`Animator.Play("Rover_Idle")` al revivir). Conviene añadir la transición `Rover_Death → Rover_Idle` con condición `IsDead = false`.

### T1 — Parallax (3 capas)

**Estado del código (hay que limpiarlo primero):** existen **tres** scripts de parallax enredados:
- `ParallaxBackground.cs` contiene una clase llamada **`ParallaxController`** (el nombre no coincide con el archivo): hace scroll por *offset de textura* (`_MainTex`) sobre hijos con `Renderer`/`Material`, con una sola `parallaxSpeed`. **No** usa factores por capa.
- `ParallaxLayer.cs` (`parallaxFactor` por capa, mueve `localPosition`) + `ParallaxCamera.cs` (delegado `onCameraTranslate(delta)` al moverse la cámara): es el enfoque por **factores por capa** que pide el GDD, **pero le falta el "pegamento"** — ningún script suscribe `ParallaxLayer.Move` al delegado de `ParallaxCamera`.
- `Parallax.cs`: auto-scroll a velocidad constante (ignora la cámara) — no sirve aquí.

**Recomendado — usar el enfoque por factores y añadir el script de unión que faltó:**
```csharp
// Vive en el contenedor de capas; ParallaxCamera va en la Main Camera.
public class ParallaxBackgroundManager : MonoBehaviour {
    [SerializeField] ParallaxCamera parallaxCamera;
    readonly System.Collections.Generic.List<ParallaxLayer> layers = new();
    void Start() {
        if (parallaxCamera == null) parallaxCamera = Camera.main.GetComponent<ParallaxCamera>();
        parallaxCamera.onCameraTranslate += Move;
        GetComponentsInChildren(layers);
    }
    void Move(float delta) { foreach (var l in layers) l.Move(delta); }
}
```
> ⚠️ Antes de crearlo, resuelve el choque de nombres: renombra `ParallaxBackground.cs` → `ParallaxController.cs` (para que archivo y clase coincidan) o borra el enfoque de textura si no se va a usar.

**Montaje en `Level01.unity`:**
- `ParallaxCamera` en la **Main Camera** (la que mueve el `CinemachineBrain`).
- Contenedor `Parallax` con el `ParallaxBackgroundManager` y **3 hijos = capas**, cada uno con `SpriteRenderer` (fondos de `Assets/Art/Backgrounds/`) + `ParallaxLayer`:
  - Lejana `parallaxFactor ≈ 0.15`, media `≈ 0.45`, cercana `≈ 0.85` (GDD §9.1; N1 = 3 capas).
  - Sorting Layer/Order **detrás** del tilemap; escala lejana 60–70 %, cercana 110–120 % con saturación reducida en la lejana (HUD §5).
- Hoy `Background` es estático: reemplázalo por estas capas.

**DoD parcial T1:** al mover la cámara, las 3 capas se desplazan a distinta velocidad (profundidad legible) y ninguna tapa el plano de juego.

### T5 — Geometría (cierre)
- Colocar `LevelMarker` (SC-01/02/03, Checkpoints, BlockedZone, BossRoom) y cerrar zonas Z1→Z5. Guía: [T5_Geometria_Level01.md](./T5_Geometria_Level01.md).
- **Arreglar config de la escena** (Tag del Player, capas de plataformas, máscara de suelo) → ver **[Integracion_Level01.md](./Integracion_Level01.md)**.

### Deuda S01 — bindings
- `RoverInputActions_Local.inputactions`: Jump `W`/`↑` → `Espacio`; Scan `E` → `F` (GDD §2).

# Sprint 02 — Nivel 1 jugable, enemigo bioluminiscente y audio

> **Objetivo:** Level01 con geometría base y parallax, animaciones del rover completas, primer enemigo con IA (Ser Bioluminiscente) y sistema de audio Mixer/música.  
> **Estado:** 🔄 En progreso  
> **Ref. GDD:** §8 IA, §9.2 Nivel 1, §13 Animación, §14 Audio  
> **Prerequisito:** [Sprint 01 MVP](RedDustLegacy/Docs/Sprints/Sprint_01_MVP.md) completado

---

## Inventario del proyecto — Estado actual

### Assets existentes (verificados en el repositorio)

**Sprites del Rover** (`Assets/Art/Sprites/`):

| Archivo | Estado |
|---------|--------|
| `Opportunity-idle.png` | ✅ Integrado en RoverAC |
| `Opportunity-walk.png` | ✅ Integrado en RoverAC |
| `Opportunity-jump-v1.png` | ✅ Integrado en RoverAC |
| `Opportunity-run.png`  | ✅ Integrado en RoverAC |
| `Opportunity-scan_loop.png` | ✅ Integrado en RoverAC |
| `Opportunity-death.png` | ⬜ Existe, no integrado |
| `Opportunity-destruction.png` | ⬜ Existe, no integrado |
| `Opportunity-victory.png` | ⬜ Existe, no integrado |

**Sprites de Enemigos** (`Assets/Art/Sprites/`):

| Enemigo | Sprites existentes |
|---------|-------------------|
| Ser Bioluminiscente | `Biol-idle-v1.png`, `Biol-walk-v1.png` |
| Drone Patrullero | `Drone-idle-v1.png`, `Drone-walk-v1.png` |
| Drone Detector | `Drone Detector-idle-v1.png` |
| Centinela Secundario | `Centinela Secundario-idle.png`, `CentinelaSecundaria-morir.png`, `CentinelaSecundaria.png` |
| Centinela Principal | `CentinelaPrincipal-idle.png`, `CentinelaPrincipal-walk.png`, `CentinelaPrincipal-eletricAttack.png`, `CentinelaPrincipal-morir.png`, `CentinelaPrincipal.png` |
| Leviatán | `Leviatan-idle.png`, `Leviatan-walk.png`, `Leviatan-ataque.png`, `Leviatan-muerte.png`, `Leviatan.png` |

**Audio** (`Assets/Audio/`):

| Tipo | Archivos |
|------|----------|
| BGM | `Bgm_MainTheme.wav`, `Bgm_GameOver.wav` |
| SFX Rover | `Sfx_Rover_Idle.wav`, `Sfx_Rover_Walk.wav`, `Sfx_Rover_Jump.wav`, `Sfx_Rover_Damage.wav`, `Sfx_Rover_Death.wav` |

**Animaciones** (`Assets/Animations/Rover/`):

| Archivo | Integrado en RoverAC |
|---------|:---:|
| `Rover_Idle.anim` | ✅ |
| `Rover_walk.anim` | ✅ |
| `Rover_Jump.anim` | ✅ |
| `Rover_Run.anim` | ✅ |
| `Rover_Scan.anim` | ✅ |

**Prefabs** (`Assets/Prefabs/`):

| Prefab | Estado |
|--------|--------|
| `Player.prefab` | ✅ Creado e integrado |

**Scripts existentes:**

| Script | Ruta | Estado |
|--------|------|--------|
| `PlayerController.cs` | `Scripts/Player/` | ✅ Funcional |
| `PlayerAnimatorController.cs` | `Scripts/Player/` | ✅ Funcional |
| `PlayerAudioController.cs` | `Scripts/Player/` | ⚠️ Bug: `OnDashed → PlayDamageSound` |
| `RoverStatsSO.cs` | `Scripts/ScriptableObjects/` | ✅ Funcional |
| `SceneLoader.cs` | `Scripts/Core/` | ✅ Funcional |
| `AudioManager.cs` | `Scripts/Core/` | ⚠️ Básico, sin estados de música ni Audio Mixer |
| `MainMenuController.cs` | `Scripts/UI/` | ✅ Funcional |

**Lo que NO existe todavía:**
- ❌ Carpeta `Assets/Animations/Enemies/` — no hay Animator Controllers de enemigos
- ❌ Fondos/parallax (`Assets/Art/Backgrounds/` está vacío)
- ❌ Tilemap en Level01
- ❌ Scripts de IA (`Assets/Scripts/AI/` está vacío)
- ❌ Scripts de nivel (`Assets/Scripts/Level/` está vacío)

---

## Correcciones pendientes del Sprint 01

> Estas correcciones deben completarse **antes** de iniciar las tareas del Sprint 02.

| # | Corrección | Archivo | Detalle |
|---|-----------|---------|---------|
| C-02 | Corregir suscripción OnDashed | `PlayerAudioController.cs` | Línea 39: `OnDashed += PlayDamageSound` debería ser un SFX de dash o removerse |

---

## Tarea 1 — Fondos y Parallax del Nivel 1

**Rama git:** `feature/level-design`  
**Responsable:** Level Designer + Artist  
**Ref. GDD:** §9.1 Técnicas 2.5D, §9.2 Nivel 1, §13.1 Paleta de colores

### 1.1 Fondos parallax (Artist)

**Entregables:** 3 imágenes PNG de fondo para Nivel 1 (Cuevas Bioluminiscentes)

| Capa | Factor parallax | Descripción visual |
|------|----------------|-------------------|
| Fondo lejano | 0.15× | Pared de cristal lejana, niebla azulada |
| Formaciones medias | 0.45× | Cristales, columnas de roca, formaciones medianas |
| Foreground | 0.85× | Rocas en primer plano, bordes de cueva |

**Paleta de color Nivel 1 (GDD §13.1):**
- Negro cueva: `#0A0A12`
- Azul cristal: `#1A3A6E`
- Verde bioluminiscente: `#2ECC71`
- Turquesa detalle: `#00BCD4`
- Blanco destellos: `#E8F5E9`

**Guardar en:** `Assets/Art/Backgrounds/Level01/`

### 1.2 Tileset (Artist)

**Entregable:** Tileset de cuevas exportado como spritesheet

Piezas mínimas: suelo, pared, techo, borde, esquinas (interior y exterior), plataformas one-way.

**Guardar en:** `Assets/Art/Sprites/Tilesets/`

### 1.3 Parallax Controller (Technical Director)

**Entregable:** Script `ParallaxController.cs` en `Assets/Scripts/Level/`

```
Implementación:
- Crear GameObject ParallaxBackground en Level01
- Cada capa es un hijo con SpriteRenderer
- Desplazamiento = Camera.main.transform × factor
- Tiling horizontal para evitar bordes visibles
```

**Definition of Done:** Las 3 capas de parallax están importadas en la escena `Level01` y se desplazan a diferentes velocidades relativas a la cámara principal al moverse el jugador.

---

## Tarea 2 — Animaciones completas del Rover

**Rama git:** `feature/player-movement`  
**Responsable:** Artist + Gameplay Programmer  
**Ref. GDD:** §13.3 Animator Controller del Rover  
**Ref. Arquitectura:** [AnimatorSetup.md](file:///c:/Users/LENOVO/RedDustLegacy/Docs/Architecture/AnimatorSetup.md)

### 2.1 Sprites por crear (Artist)

| Sprite | Frames | FPS | Loop | Notas |
|--------|--------|-----|:----:|-------|
| `Opportunity-dash.png` | 6 | 24 | ☐ | Cuerpo inclinado, aceleración rápida |
| `Opportunity-wallslide.png` | 6 | 8 | ☑ | Rover apoyado en pared, deslizamiento lento |
| `Opportunity-walljump.png` | 5 | 12 | ☐ | Impulso desde pared |
| `Opportunity-land.png` | 4 | 12 | ☐ | Impacto de aterrizaje |
| `Opportunity-damage.png` | 5 | 12 | ☐ | Flash de daño |

### 2.2 Integración en RoverAC (Gameplay Programmer)

**Sprites existentes por integrar:**

| Sprite existente | Clip a crear/usar | Parámetros nuevos |
|-----------------|-------------------|-------------------|
| `Opportunity-run.png` | `Rover_Run.anim` (ya existe) | — (reemplazar Walk por Run en transiciones) |
| `Opportunity-scan_loop-v5.png` | `Rover_Scan.anim` (ya existe) | `IsScanning` (Bool) — ya creado |
| `Opportunity-death.png` | Crear `Rover_Death.anim` | `IsDead` (Bool) — nuevo |

**Nuevos estados en el Animator Controller:**

| Estado | Condición de entrada | Prioridad |
|--------|---------------------|-----------|
| `Run` | `Speed > 0.1 && IsGrounded` | MVP — reemplaza `Walk` |
| `Jump_Rise` | `IsJumping && VelocityY > 0` | MVP — reemplaza `Jump` unificado |
| `Jump_Fall` | `IsJumping && VelocityY ≤ 0` | MVP |
| `Land` | `IsGrounded (desde Jump_Fall)` | MVP |
| `Dash` | `IsDashing = true` | MVP |
| `Damage` | Trigger `IsDamaged` | MVP |
| `Death` | `IsDead = true` | MVP |
| `Scan_Loop` | `IsScanning = true` | MVP |
| `Wall_Slide` | `IsOnWall && VelocityY < 0` | V2 |
| `Wall_Jump` | Trigger `WallJumped` | V2 |

**Parámetros nuevos a agregar en RoverAC:**

| Parámetro | Tipo | Escrito por |
|-----------|------|-------------|
| `VelocityY` | Float | `PlayerAnimatorController.Update()` |
| `IsDashing` | Bool | `PlayerAnimatorController` vía evento `OnDashed` |
| `IsDamaged` | Trigger | `PlayerAnimatorController` vía evento futuro `OnDamageReceived` |
| `IsDead` | Bool | `PlayerAnimatorController` vía evento futuro `OnDeath` |
| `IsOnWall` | Bool | `PlayerAnimatorController` vía evento `OnWallSliding` |

**Definition of Done:** El rover reproduce correctamente Idle, Run, Jump_Rise, Jump_Fall, Land, Dash, Scan y Death al probar en Play Mode. Las transiciones son instantáneas (Duration = 0, Has Exit Time = OFF). Write Defaults = OFF en todos los estados.

---

## Tarea 3 — Primer enemigo con IA: Ser Bioluminiscente

**Rama git:** `feature/enemy-ai`  
**Responsable:** AI Programmer  
**Ref. GDD:** §8.2 Ser Bioluminiscente

### 3.1 Scripts a crear

| Script | Ruta | Función |
|--------|------|---------|
| `BioluminescentAI.cs` | `Scripts/AI/` | FSM de 3 estados + lógica de detección y daño |
| `EnemyStatsSO.cs` | `Scripts/ScriptableObjects/` | ScriptableObject base para stats de enemigos |
| `BiolStats.asset` | `Scripts/ScriptableObjects/` | Instancia con valores del GDD |

### 3.2 FSM — 3 estados

```
Idle (flotar aleatoriamente)
  ↓ rover en rango 5 u
Alert (orientarse al rover)
  ↓ confirmación 0.5 s
Chase (seguir al rover a 3.75 u/s)
  ↓ rover fuera de rango 8 u por 3 s
Idle
```

### 3.3 Valores del GDD

| Parámetro | Valor |
|-----------|-------|
| HP | 40 |
| `moveSpeed` base | 1,5 u/s |
| `moveSpeed` Chase | 3,75 u/s |
| Daño al contacto | 5 SI/s (temporalmente con placeholder o log, daño real en Sprint 03) |
| Rango detección | 5 u — sin línea de visión |
| Vulnerabilidad | Aturdido 2 s por pulso de escaneo |
| Comportamiento grupal | Individual, sin coordinación |

### 3.4 Animator Controller

**Guardar en:** `Assets/Animations/Enemies/Bioluminiscente/BiolAC.controller`

Sprites existentes: `Biol-idle-v1.png`, `Biol-walk-v1.png`

| Estado | Sprite | FPS | Loop | Condición |
|--------|--------|-----|:----:|-----------|
| `Float_Idle` | `Biol-idle-v1.png` | 8 | ☑ | Sin rover detectado |
| `Float_Alert` | (reusar idle con tint) | 12 | ☑ | Rover en rango |
| `Float_Chase` | `Biol-walk-v1.png` | 12 | ☑ | Chase activo |
| `Death` | ⬜ Por crear | 8 | ☐ | HP = 0 |

### 3.5 Prefab

**Guardar en:** `Assets/Prefabs/Enemies/Bioluminiscente.prefab`

**Componentes:**
```
Rigidbody2D (Kinematic o Dynamic según diseño)
CircleCollider2D (trigger para daño de contacto)
BioluminescentAI
Animator → BiolAC
SpriteRenderer
```

**Definition of Done:** El Ser Bioluminiscente patrulla en flotación. Al acercarse el rover a <5 u, se orienta y persigue. Al contacto, se registra un log o se reduce la SI (si el sistema ya está configurado). Al morir (HP = 0), reproduce animación Death y se desactiva.

---

## Tarea 4 — Audio: estados de música y Audio Mixer

**Rama git:** `feature/audio-system`  
**Responsable:** Technical Director  
**Ref. GDD:** §14 Audio

### 4.1 Mejoras al AudioManager.cs

**Estado actual:** Funcional pero básico. Solo `PlayMusic(clip, loop)` y `TriggerGameOverMusic()`.

**Mejoras necesarias:**

```csharp
// Agregar enum de estados
public enum MusicState { Silence, Exploration, Tension, Combat, Cinematic }

// Agregar crossfade entre estados
// Exploration → Tension:  1.5 s
// Tension → Combat:       0.5 s
// Combat → Exploration:   3.0 s
// Cualquiera → Cinematic: gestionado por CinematicManager
```

### 4.2 Unity Audio Mixer — 4 buses

| Bus | Uso |
|-----|-----|
| Master | Volumen global |
| Music | Música adaptativa (crossfade entre clips) |
| SFX | Efectos del rover y enemigos |
| Ambient | Sonido ambiental de cueva |

### 4.3 Correcciones en PlayerAudioController

- Corregir suscripción `OnDashed → PlayDamageSound` — crear `PlayDashSound()` o remover
- Agregar SFX de aterrizaje vía `OnLanded` (evento nuevo en PlayerController)
- Asegurar que los AudioSources usan el bus SFX del Mixer

### 4.4 SFX de enemigos

Cada enemigo necesita al mínimo:
- SFX de ataque (contacto para Biol)
- SFX de muerte

**Definition of Done:** La música transiciona con crossfade al detectar/eliminar enemigos. El Audio Mixer permite ajustar Music, SFX y Ambient independientemente.

---

## Tarea 5 — Tilemap del Nivel 1 (Level Designer)

**Rama git:** `feature/level-tilemap`  
**Responsable:** Level Designer  
**Ref. GDD:** §9.2 Nivel 1, §16.1 Collision Matrix

### 5.1 Configuración de Grid y Capas de Tilemap
- Crear o limpiar la escena `Level01.unity`.
- Crear un GameObject principal `Grid`.
- Configurar un Tilemap dedicado a las colisiones físicas: `Collision_Tilemap` (Layer: `Ground`, usar `TilemapCollider2D` junto con `CompositeCollider2D`, establecer el Rigidbody2D como `Static` y `Used by Composite`).
- Configurar un Tilemap para elementos netamente visuales: `Visual_Tilemap` (Layer: `Default`, sin colliders).

### 5.2 Diseño de Geometría por Zonas (Beat Map)
- **Zona 1 — Entrada (Tutorial implícito):**
  - Plataformas simples de piedra y suelo regular plano.
  - Diseñar el recorrido de manera que enseñe al jugador mecánicas básicas: movimiento lateral y salto.
  - Colocar un placeholder para el objeto escaneable `SC-01`.
- **Zona 2 — Primera tensión:**
  - Crear un foso/abismo con una anchura que exija el uso de Dash para cruzar con éxito.
  - Añadir una sala lateral elevada que resulte inaccesible por el momento (bloqueada con un marcador temporal de Rueda Reforzada).
  - Espacio plano para patrullaje de enemigos.
  - Colocar placeholder para el Checkpoint final de la zona.
- **Zona 3 — Respiro y Recurso (Sala del Meteorito):**
  - Crear una sala segura (sin spawns de enemigos).
  - Colocar marcador para la celda de energía (objeto escaneable `SC-02`) y el upgrade del scanner.
- **Zona 4 — Escalada vertical (Sala del Leviatán):**
  - Diseñar una sección angosta y vertical para forzar el uso de Wall Jump en las paredes.
  - Colocar plataformas a diferentes alturas y el objeto escaneable `SC-03`.
  - Añadir un checkpoint pre-boss.
- **Zona 5 — Boss (Leviatán):**
  - Sala amplia y horizontal, despejada de obstáculos terrestres para permitir los patrones de ataque de tentáculos.

### 5.3 Bloqueo de Rutas e Integración de Prefabs
- Marcar el 30% del nivel como inaccesible utilizando bloques visuales temporales identificados con una paleta de color distintiva, representando las zonas que requieren el upgrade `Rueda Reforzada`.
- Instanciar el prefab `Player.prefab` en el punto de spawn de la Zona 1 para pruebas de juego directas.

**Definition of Done:** El nivel se puede recorrer de principio a fin (Zona 1 a Zona 4) usando el prefab del jugador sin caer al infinito ni atascarse en la geometría. La colisión con la capa `Ground` funciona de forma estable. Las zonas inaccesibles están claramente delimitadas con bloques provisionales.

---

## Configuración de Unity — Pendiente para Sprint 02

### Layers por configurar

| Layer | Uso | Estado |
|-------|-----|--------|
| Ground | Suelo y plataformas | ✅ Configurado |
| Player | GameObject Player | ⬜ Configurar |
| Enemy | Todos los enemigos | ⬜ Configurar |
| Platform | Plataformas one-way | ⬜ Configurar |
| Interactable | Objetos escaneables | ⬜ Configurar |

### Layer Collision Matrix (GDD §16.1)

Configurar en `Edit → Project Settings → Physics 2D → Layer Collision Matrix` según la tabla del GDD.

---

## Reglas del proyecto

- No agregar assets de Asset Store sin aprobación del Technical Director
- No modificar `PlayerController.cs`, `RoverStatsSO.cs` ni `SceneLoader.cs` sin consultar al Gameplay Programmer
- No cambiar la resolución de referencia del Canvas (1920×1080)
- No cambiar la configuración de Physics 2D sin consultar al Technical Director
- Simulation Mode: `Fixed Update`
- Physics Material del Player y Ground: `NoFriction`
- Todo Rigidbody2D dinámico: `Collision Detection: Continuous`

---

## Flujo de trabajo Git

```
1. Antes de trabajar:
   git checkout feature/tu-rama
   git fetch origin
   git rebase origin/develop

2. Commits con Conventional Commits:
   feat(enemies): add Biol FSM with 3 states
   art(sprites): add Opportunity-dash spritesheet
   fix(audio): correct crossfade timing

3. Al terminar feature:
   git push origin feature/tu-rama
   Abrir Pull Request → base: develop
   Mínimo 1 approval antes de merge

4. NUNCA push directo a main o develop
5. SIEMPRE incluir archivos .meta en commits
6. NUNCA subir Library/ o Temp/
```

---

## Resumen de entregables por tarea

| Tarea | Responsable | Entregable clave | Prioridad |
|-------|------------|------------------|-----------|
| Correcciones Sprint 01 | Todos | Corregir suscripción OnDashed | 🔴 Bloqueante |
| T1 — Fondos/Parallax | Artist | Fondos y Parallax en Level01 | 🔴 Alta |
| T2 — Animaciones rover | Artist + Gameplay Prog. | RoverAC con estados MVP completos | 🔴 Alta |
| T3 — Ser Bioluminiscente | AI Programmer | Prefab con FSM 3 estados + daño | 🟡 Media |
| T4 — Audio Mixer | Technical Director | Crossfade estados música + Mixer | 🟡 Media |
| T5 — Tilemap del Nivel 1 | Level Designer | Level01 con geometría jugable | 🔴 Alta |
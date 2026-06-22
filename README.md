<div align="center">

# 🔴 Opportunity: Red Dust Legacy

**Un Metroidvania 2.5D de ciencia ficción inspirado en la historia real del rover Opportunity de la NASA (2004–2019).**

*Sobrevive. Explora. Adáptate.*

![Engine](https://img.shields.io/badge/Unity-6.4_(6000.4.8f1)-000000?logo=unity)
![Pipeline](https://img.shields.io/badge/Render-URP_17.4-2A6FDB)
![Language](https://img.shields.io/badge/C%23-Input_System-239120?logo=csharp)
![Platform](https://img.shields.io/badge/Plataforma-PC_Windows_10%2F11-0078D6?logo=windows)
![Status](https://img.shields.io/badge/Estado-Prototipo_en_desarrollo-orange)

</div>

---

## 📖 Sobre el juego

En **Opportunity: Red Dust Legacy** controlas a *Oppy*, el rover marciano de la NASA, mucho después de su último contacto. Despiertas con tu hardware degradado tras 736 soles de silencio y debes explorar cavernas bioluminiscentes y una instalación alienígena mientras tus sistemas fallan progresivamente.

No hay barra de vida tradicional: tu recurso es la **Integridad Estructural (SI)**. Cada caída, cada golpe y cada error desgasta al rover de forma **permanente** y altera cómo se mueve. La degradación no es solo cosmética — cambia el juego.

### Pilares de diseño

| Pilar | Definición |
|-------|------------|
| 🔍 **Exploración con propósito** | Cada sala recompensa la curiosidad con datos científicos reales. |
| 🔧 **Adaptación tecnológica** | Los upgrades son reparaciones con justificación narrativa. |
| ⚡ **Tensión sin frustración** | El peligro es real pero siempre legible: la IA telegrafía sus acciones. |

---

## ✨ Características

- **Movimiento de plataformas de alta precisión** — carrera con aceleración diferenciada, salto variable, *coyote time*, *jump buffer*, dash con *freeze-frames* y wall jump.
- **Sistema de Integridad Estructural (SI)** — 6 fases de degradación acumulativa que modifican velocidad, salto y dash a medida que el rover se daña.
- **Enemigos con IA telegrafiada** — desde el Ser Bioluminiscente hasta drones patrulleros y centinelas, cada uno con su propio comportamiento.
- **Escaneo y lore** — escanea el entorno para revelar datos científicos reales de la misión Opportunity.
- **Audio adaptativo** — música por estados y SFX del rover con *crossfade*.
- **Estética 2.5D** con Universal Render Pipeline e iluminación 2D.

---

## 🎮 Controles

| Acción | Teclado | Gamepad |
|--------|---------|---------|
| Moverse | `A` / `D` · `←` / `→` | Stick izquierdo |
| Saltar (variable) | `Espacio` (mantener) | `A` / `X` |
| Dash | `Shift izq.` | `B` / `O` |
| Escalar / Wall jump | `E` en pared + `Espacio` | `RB` / `R1` |
| Escanear | `F` (mantener) | `Y` / `△` |
| Usar celda de energía | `Q` | `LB` / `L1` |
| Pausa | `Esc` | `Start` / `Options` |

> Las teclas son reasignables excepto Pausa y Celda de energía. Configuración vía **Unity Input System**.

---

## 🛠️ Stack técnico

| Componente | Versión / Detalle |
|------------|-------------------|
| Motor | Unity **6.4** (`6000.4.8f1`) |
| Render | Universal Render Pipeline 17.4 |
| Lenguaje | C# (Unity Input System, Cinemachine 3) |
| 2D | Tilemap, Sprite Animation, 2D Lights |
| Plataforma | PC — Windows 10/11 |

---

## 🚀 Puesta en marcha

1. Instala **Unity 6.4 (`6000.4.8f1`)** desde Unity Hub.
2. Clona el repositorio:
   ```bash
   git clone https://github.com/Erenriquezp/RedDustLegacy.git
   ```
3. En Unity Hub, **Add project from disk** → selecciona la carpeta clonada.
4. Ábrelo en Unity (la primera importación tarda unos minutos).
5. Abre `Assets/Scenes/MainMenu/MainMenu.unity` y pulsa **Play**.

> No se incluyen `Library/`, `Temp/` ni `obj/` en el repo — Unity los regenera al abrir el proyecto.

---

## 📂 Estructura del proyecto

```
Assets/
├── Animations/      Animator Controllers y clips (rover, enemigos)
├── Art/             Sprites, fondos y tiles (placeholder)
├── Audio/           Música (BGM) y efectos (SFX)
├── Input/           Action Maps del Input System
├── Prefabs/         Player y enemigos
├── Scenes/          MainMenu · Level01 · Level02 · escenas Dev/Enemy
├── Scripts/
│   ├── Core/        AudioManager, SceneLoader (singletons)
│   ├── Player/      PlayerController + animator/audio (movimiento)
│   ├── AI/          IA de enemigos + EnemyStatsSO
│   ├── Level/       Marcadores y herramientas de nivel
│   ├── UI/          Menús y HUD
│   ├── ScriptableObjects/   RoverStatsSO (tuning del rover)
│   └── Editor/      Herramientas de Editor (tiles, andamiaje)
└── Settings/        Perfiles de URP y materiales de física

Docs/
├── GDD.md           Documento de diseño técnico (fuente de verdad)
├── Architecture/    PlayerController, AnimatorSetup
└── Sprints/         Planes y guías de sprint
```

📚 **Documentación clave:** [GDD](Docs/GDD.md) · [Arquitectura del PlayerController](Docs/Architecture/PlayerController.md) · [Sprints](Docs/Sprints/)

---

## 🗺️ Roadmap

| Sprint | Foco | Estado |
|--------|------|:------:|
| [Sprint 01](Docs/Sprints/Sprint_01_MVP.md) | Movimiento del rover, menú, audio base | ✅ Completado |
| [Sprint 02](Docs/Sprints/Sprint_02.md) | Level01 jugable, enemigo bioluminiscente, animaciones | 🔄 En progreso |
| [Sprint 03](Docs/Sprints/Sprint_03.md) | Sistema de SI, HUD, checkpoints, GameManager | ⬜ Pendiente |

---

## 🌿 Flujo de trabajo Git

Ramas: `main` (producción) ← `develop` (integración) ← `feature/*` (trabajo individual).

```bash
git checkout feature/tu-rama
git fetch origin && git rebase origin/develop   # antes de abrir PR
git push origin feature/tu-rama                  # luego: abrir PR hacia develop
```

**Reglas del repositorio:**

- ❌ **Nunca** hagas `push` directo a `main` ni a `develop`.
- ❌ **Nunca** apruebes ni mergees tu propio PR sin revisión de otro miembro.
- ✅ Haz `git rebase develop` antes de abrir un PR.
- ✅ Incluye **siempre** los archivos `.meta` de Unity en tus commits.
- ✅ Un PR por cada *feature*; commits con [Conventional Commits](https://www.conventionalcommits.org/) (`feat`, `fix`, `art`, `docs`…).
- ✅ Resuelve los conflictos **en tu propia rama**, nunca sobre `develop`.

---

## 👥 Equipo — Red Dust Team

| Rol | Rama principal |
|-----|----------------|
| Líder / Gameplay Programmer | `feature/player-movement` |
| UI / HUD | `feature/ui-hud` |
| Level Design | `feature/level-design` |
| Audio | `feature/audio-system` |
| Enemy AI | `feature/enemy-ai` |
| VFX / Shaders | `feature/vfx-shaders` |

---

<div align="center">

*Basado en la misión real del rover Opportunity (MER-B) de la NASA — 2004 a 2019.*
*"My battery is low and it's getting dark."*

</div>

# Opportunity: Red Dust Legacy — Technical Design Document

> **Motor:** Unity 6.4 (6000.4.8f1) — URP  
> **Género:** 2.5D Metroidvania / Sci-Fi Platformer  
> **Plataforma:** PC — Windows 10/11  
> **Alcance:** Prototipo funcional: 2 niveles + menú + cinemáticas  
> **Duración estimada (1.ª partida):** 60–85 min  
> **Versión del documento:** v5.0 — Technical Development Reference

---

## Tabla de contenidos

- [1. Resumen del proyecto](#1-resumen-del-proyecto)
- [2. Controles e Input](#2-controles-e-input)
- [3. Mecánicas del jugador](#3-mecánicas-del-jugador)
- [4. Sistema de Integridad Estructural (SI)](#4-sistema-de-integridad-estructural-si)
- [5. Economía: celdas de energía](#5-economía-celdas-de-energía)
- [6. Sistema de upgrades](#6-sistema-de-upgrades)
- [7. Sistema de checkpoints](#7-sistema-de-checkpoints)
- [8. Inteligencia artificial y enemigos](#8-inteligencia-artificial-y-enemigos)
- [9. Diseño de niveles](#9-diseño-de-niveles)
- [10. HUD e interfaz de usuario](#10-hud-e-interfaz-de-usuario)
- [11. Sistema de escaneo y lore](#11-sistema-de-escaneo-y-lore)
- [12. Narrativa y cinemáticas](#12-narrativa-y-cinemáticas)
- [13. Arte y animación](#13-arte-y-animación)
- [14. Audio](#14-audio)
- [15. Arquitectura de software](#15-arquitectura-de-software)
- [16. Configuración de Unity](#16-configuración-de-unity)
- [17. Requisitos de hardware](#17-requisitos-de-hardware)

---

## 1. Resumen del proyecto

Opportunity: Red Dust Legacy es un videojuego de plataformas 2.5D Metroidvania basado en la historia real del rover Opportunity de la NASA (2004–2019). El jugador controla al rover explorando cavernas marcianas y una instalación alienígena mientras su hardware se degrada progresivamente.

### 1.1 Pilares de diseño

| # | Pilar | Definición | Criterio de validación |
|---|-------|------------|----------------------|
| 01 | Exploración con propósito | Cada sala recompensa la curiosidad con datos científicos reales | ¿Existe razón narrativa o mecánica para esta sala? |
| 02 | Adaptación tecnológica | Los upgrades son reparaciones con justificación narrativa | ¿El jugador puede explicar de dónde vino este upgrade? |
| 03 | Tensión sin frustración | El peligro es real pero siempre legible. La IA telegrafía acciones | ¿El jugador que muere entiende por qué murió? |

### 1.2 Loop de juego

**Microciclo (3–15 s):** Moverse → Detectar objeto/enemigo → Decidir (escanear/evitar/combatir) → Resolver situación.

**Mesociclo (2–8 min):** Entrar a sala → Leer entorno → Ejecutar estrategia → Obtener recompensa → Avanzar con estado acumulado.

**Macrociclo (25–50 min):** Exploración inicial → Primer upgrade → Zona de tensión → Checkpoint pre-boss → Boss y transición.

### 1.3 Condiciones de victoria y derrota

**Victoria:**
1. Derrotar al Centinela Principal (Nivel 2, sala final)
2. Ejecutar la secuencia de transmisión: enviar ≥1 fragmento antes de que la energía llegue a 0%

| Parámetro | Valor |
|-----------|-------|
| Mínimo para victoria | 1 fragmento transmitido de 891 |
| Resultado óptimo | 341 fragmentos |
| SI al transmitir | 1% (Fase 6 — Extinción) |

**Derrota:**

| Parámetro | Comportamiento |
|-----------|---------------|
| Trigger de muerte | SI = 0. Animación Death (12 frames, no interrumpible) |
| Respawn | Último checkpoint activado, con la SI que tenía al activarlo |
| Estado del nivel | Enemigos derrotados y celdas recogidas **no** reaparecen. Objetos escaneados permanecen |
| Penalización | Ninguna. Sin pérdida de upgrades ni progreso |

---

## 2. Controles e Input

Gestión de input con **Unity Input System**. El jugador puede reasignar teclas excepto las marcadas como bloqueadas.

| Acción | Teclado | Gamepad (Xbox/PS) | Notas |
|--------|---------|-------------------|-------|
| Moverse | `A/D` o `←/→` | Stick izquierdo | Analógico en gamepad, digital en teclado |
| Salto completo | `Espacio` (mantener) | `A/X` (mantener) | Altura máxima: 6,4 u |
| Salto corto | Soltar `Espacio` antes del pico | Soltar `A/X` antes del pico | Altura ~2,4 u |
| Coyote jump | `Espacio` hasta 0,12 s tras borde | `A/X` | Automático |
| Dash | `Shift izq.` | `B/O` | Dirección del input horizontal o adelante |
| Escalar pared | `E` (mantener en pared) | `RB/R1` (mantener) | Requiere upgrade **Rueda Reforzada** |
| Wall jump | `Espacio` mientras se escala | `A/X` mientras se escala | Desactiva input horizontal 0,15 s |
| Escanear | `F` (mantener) | `Y/△` (mantener) | Radio: 3 u base, 5 u con upgrade |
| Usar celda de energía | `Q` | `LB/L1` | **No reasignable** |
| Pausa | `Esc` | `Start/Options` | **No reasignable** |

---

## 3. Mecánicas del jugador

### 3.1 Carrera

Aceleración de 0 a `maxRunSpeed` en 0,30 s. Frenado en 0,19 s. En aire: aceleración y deceleración al 65%.

### 3.2 Salto

Doble altura: mantener = 6,4 u, soltar temprano = ~2,4 u (via `jumpCutMultiplier`). Coyote time de 0,12 s. Jump buffer de 0,10 s.

### 3.3 Dash

Horizontal en dirección del input. Un dash aéreo máximo. `gravityScale = 0` durante el dash. `dashSleepTime` de 0,028 s para game feel.

### 3.4 Escalada de paredes

Requiere upgrade **Rueda Reforzada**. Desliza lentamente al aferrarse. Wall jump aplica fuerza horizontal + vertical. Input horizontal desactivado 0,15 s post wall jump.

### 3.5 Tabla maestra de parámetros de movimiento

> **Estos son los valores definitivos para `PlayerController.cs`.**

| Parámetro | Valor | Unidad | Nota / Fórmula |
|-----------|-------|--------|----------------|
| `maxRunSpeed` | 7,5 | u/s | Velocidad horizontal máxima en suelo |
| `runAcceleration` | 25,0 | u/s² | Tiempo a vel. máx: 7,5 ÷ 25 = **0,30 s** |
| `runDecceleration` | 40,0 | u/s² | Tiempo a detención: 7,5 ÷ 40 = **0,19 s** |
| `accelInAir` | 65% | % de `runAcceleration` | Control aéreo reducido |
| `deccelInAir` | 65% | % de `runDecceleration` | Resistencia aérea reducida |
| `jumpForce` | 16,0 | u/s | Velocidad vertical inicial |
| `jumpHeight` | 6,4 | u | `jumpForce² ÷ (2 × |gravity|)` = 256 ÷ 40 |
| `jumpCutMultiplier` | 0,5 | — | Multiplica velocidad Y al soltar botón |
| `jumpCoyoteTime` | 0,12 | s | Ventana post-borde |
| `jumpBufferTime` | 0,10 | s | Ventana pre-suelo |
| `fallGravityMultiplier` | 1,8 | — | Gravedad en caída: −20 × 1,8 = **−36 u/s²** |
| `fastFallGravityMult` | 2,5 | — | Al mantener ↓ durante caída |
| `maxFallSpeed` | −26,0 | u/s | Velocidad terminal |
| `dashSpeed` | 28,0 | u/s | Velocidad durante dash |
| `dashTime` | 0,18 | s | Distancia: 28 × 0,18 = **5,04 u** |
| `dashDistance` (nominal) | 5,0 | u | Valor de referencia de diseño |
| `dashCooldown` | 1,2 | s | Tiempo entre dashes |
| `dashSleepTime` | 0,028 | s | Congelamiento de frames al inicio |
| `dashesInAir` | 1 | — | Dashes disponibles sin tocar suelo |

---

## 4. Sistema de Integridad Estructural (SI)

### 4.1 Parámetros base

| Parámetro | Valor |
|-----------|-------|
| SI máxima | 100 puntos |
| SI al inicio del juego | **74** (consecuencia de 736 soles de silencio) |
| SI mínima funcional | 1 (debajo = muerte) |
| Checkpoints restauran SI | **No** — daño acumulado permanente |
| Celdas de energía | +20 SI por celda, máx. 2 en reserva |
| Fuentes de daño | Ataques de enemigos, caídas >3 u, objetos ambientales |

### 4.2 Tabla de degradación por fases

> **Efectos acumulativos:** entrar en Fase 4 = todos los efectos de Fases 1–3 siguen activos.

| Fase | SI | Efectos en `PlayerController` | Efectos HUD/Audio | Estado visual del rover |
|------|----|-------------------------------|-------------------|------------------------|
| **1 — NOMINAL** | 100–74% | Sin modificadores | Barra verde. Música de exploración | Sin daño visible |
| **2 — DESGASTE** | 73–61% | Daño por caída activo desde 2 u (en vez de 3 u) | Barra verde-ámbar. Antena tiembla cada 15 s | Polvo en paneles. Antena vibra en Idle |
| **3 — AVERÍA** | 60–47% | `dashCooldown × 1,5`. Sin dashes aéreos. Delay 0,04 s en input de dash | Barra ámbar. Sonido rueda raspando c/8–10 s | Rueda trasera derecha deformada. Panel solar con grieta |
| **4 — CRÍTICO** | 46–29% | `maxRunSpeed × 0,75`. `runAcceleration × 0,80`. Jump buffer = 0,06 s | Barra roja parpadeante. Tinte naranja. Alarma c/30 s | Rueda delantera doblada. Pancam con fisura. Antena caída |
| **5 — EMERGENCIA** | 28–9% | `jumpForce × 0,85`. Wall jump desactivado. Coyote time = 0,06 s | Tinte naranja intenso. Interferencia visual c/5 s | Brazo colgando. Dos ruedas dañadas. Marcas de impacto |
| **6 — EXTINCIÓN** | 8–1% | `maxRunSpeed × 0,50`. Movimiento asimétrico (arrastra rueda izq.). Sin dash | Estática en bordes. Minimapa con fallas. Apagones c/20 s | Deterioro máximo. Rover irreconocible vs arte base |

### 4.3 Tabla de daño — Fuentes y valores

| Fuente de daño | SI perdida | Condición | Notas de implementación |
|----------------|-----------|-----------|------------------------|
| Proyectil de drone | 8–12 | Contacto con hitbox del rover | Variación aleatoria ±4 |
| Contacto con ser bioluminiscente | 5/segundo | Colisión continua | `OnTriggerStay2D` |
| Tentáculo del Leviatán | 15 | Contacto único por ataque | Invincibility frames 0,8 s |
| Proyectil Centinela (Fase 1) | 12 | Contacto con hitbox | Recto, 8 u/s |
| Proyectil Centinela (Fase 2) | 18 | Contacto con hitbox | Seguimiento parcial, 6 u/s |
| Caída 3–5 u | 10 | Aterrizaje con velocidad > umbral | Fase 2+: activo desde 2 u |
| Caída ≥6 u | 25 (cap) | Aterrizaje | Daño no escala más allá del cap |
| Objeto ambiental (trampa) | 6 | Trigger | Solo en Nivel 2 |

---

## 5. Economía: celdas de energía

| Parámetro | Valor |
|-----------|-------|
| SI restaurada por celda | +20 puntos |
| Capacidad de reserva | 2 celdas simultáneas (slots visibles en HUD) |
| Activación | Input dedicado. No automática |
| Distribución Nivel 1 | 4 celdas: 2 en zonas opcionales, 2 pre-Leviatán |
| Distribución Nivel 2 | 5 celdas: 1 en entrada, 2 en Ala B, 2 pre-Centinela |
| Representación en HUD | 2 slots junto a barra SI. Icono lleno = disponible, vacío = sin celda |
| Respawn de celdas | **No** — únicas por sesión |

---

## 6. Sistema de upgrades

| Upgrade | Zona | Mecánica desbloqueada | Impacto en progresión |
|---------|------|----------------------|----------------------|
| **Rueda Reforzada** | Niv.1 — Zona 2 (sala lateral opcional) | Escalada de paredes + wall jump | Abre rutas verticales (~30% del Niv.1) |
| **Sistema de Escaneo Mejorado** | Niv.1 — Zona 3 (sala del meteorito) | Radio de escaneo: 3→5 u. Ping pasivo: objetos brillan a <4 u | Facilita encontrar lore opcional |
| **Escudo de Plasma** | Niv.2 — Entrada (post primer checkpoint) | Escudo de 1 uso que absorbe primer impacto. Recarga: 8 s | Permite absorber hit para contraatacar |
| **Batería EMP** | Niv.2 — Ala B (opcional) | Pulso EMP, radio 5 u, aturde drones 2 s. Recarga: 25 s | Herramienta anti-grupos. Ineficaz contra Centinela Principal |

---

## 7. Sistema de checkpoints

| Parámetro | Valor |
|-----------|-------|
| Activación | Automática al cruzar `BoxCollider2D`. Sin input |
| Dimensiones trigger | Ancho: ancho del pasillo (mín. 3 u). Alto: 4 u |
| Radio mínimo de seguridad | ≥8 u del enemigo más cercano (evitar loops spawn-muerte) |
| Efecto visual | Pulso de luz 1,2 s. Texto HUD: `CHECKPOINT REGISTRADO` |
| SI al revivir | **No se restaura** — misma SI que al activar el checkpoint |
| Distribución Niv.1 | 3: inicio, mitad (pre-Zona 4), pre-Leviatán |
| Distribución Niv.2 | 4: entrada, Ala A, Ala B, pre-Centinela |

---

## 8. Inteligencia artificial y enemigos

### 8.1 Sistema DDA — Dificultad Dinámica Adaptativa

Centralizado en `AIManager`. Solo modifica parámetros de enemigos, no del jugador.

| Parámetro | Valor |
|-----------|-------|
| Métricas monitoreadas | % dashes evasivos exitosos, muertes en últimos 5 min, SI perdida/min, tiempo en SI <29% |
| Trigger reducción | >2 muertes en 5 min → −15% velocidad detección y daño proyectiles (3 min) |
| Trigger aumento | 0 muertes y SI >60% por >8 min → flanqueo activo + patrullaje +10% |
| Comunicación drones | Drone Detector en Alert → evento a `AIManager` → Drones Patrulleros en 12 u reciben posición |
| Rango de modificadores | ×0,75 a ×1,25 de valores base |

---

### 8.2 Ser Bioluminiscente

| Parámetro | Valor |
|-----------|-------|
| HP | 40 |
| `moveSpeed` base | 1,5 u/s (flotación) |
| `moveSpeed` Chase | 3,75 u/s |
| Daño al contacto | 5 SI/s (`OnTriggerStay2D`) |
| Rango de detección | 5 u radio — sin línea de visión requerida |
| FSM | 3 estados: `Idle` (flotar) → `Alert` (orientarse) → `Chase` (seguir) |
| Vulnerabilidad | Aturdido 2 s por pulso del Sistema de Escaneo (sin daño) |
| Comportamiento grupal | Individual — sin coordinación |

### 8.3 Leviatán (Boss Nivel 1)

| Parámetro | Valor |
|-----------|-------|
| HP | 200 |
| `moveSpeed` | 0,8 u/s |
| Daño tentáculo | 15 SI — i-frames 0,8 s |
| Patrón de ataque | 3 tentáculos alternos, 2 s pausa entre ataques |
| Punto débil | Núcleo central — ×2 daño. Solo visible durante pausa post-ataque |
| Enfurecimiento | <50% HP: velocidad tentáculos ×1,5 + cuarto tentáculo al ciclo |
| Invulnerabilidad | Tentáculos no reciben daño — solo el núcleo |
| Proyectiles | No. Solo ataques de contacto |

### 8.4 Drone Patrullero

| Parámetro | Valor |
|-----------|-------|
| HP | 60 |
| `moveSpeed` patrullaje | 3,0 u/s |
| `moveSpeed` Chase | 5,0 u/s |
| Rango detección | 7 u radio |
| `attackRange` | 3,5 u |
| Daño proyectil | 8–12 SI (±4 aleatorio) |
| Velocidad proyectil | 8,0 u/s — recto, sin seguimiento |
| Tiempo vida proyectil | 2,0 s |
| Hitbox proyectil | Círculo radio 0,2 u |
| Cooldown ataque | 1,5 s |
| Comportamiento | Waypoints fijos → Chase al detectar rover → Regresa al waypoint si rover escapa >5 s |

### 8.5 Drone Detector — FSM completo (Sensar-Pensar-Actuar)

| Parámetro | Valor |
|-----------|-------|
| HP | 80 |
| `moveSpeed` patrullaje | 2,5 u/s |
| `moveSpeed` Chase | 4,5 u/s |
| `moveSpeed` Flanking | 3,5 u/s |
| Rango de visión | 8 u, cono 60° (ampliado a 90° por DDA si alta evasión) |
| Obstrucción | Sí — raycast. Paredes bloquean detección |
| `attackRange` | 4,0 u |
| Daño proyectil | 8–12 SI |
| Velocidad proyectil | 7,0 u/s — recto |
| Tiempo vida proyectil | 2,5 s |
| Cooldown ataque | 2,0 s |
| Comunicación | En Alert → emite evento a `AIManager` con posición del rover |

**FSM del Drone Detector:**

```
Patrol → [rover en cono] → Alert → [confirmación 1,5 s] → Chase → [attackRange] → Attack
                                                              ↓
                                                 [rover fuera rango 4 s] → Search → [no encontrado 6 s] → Patrol
                                                              ↓
                                                 [DDA activo] → Flanking (posición opuesta al rover)
```

### 8.6 Centinela Secundario

| Parámetro | Valor |
|-----------|-------|
| HP | 100 |
| `moveSpeed` base | 2,0 u/s |
| `moveSpeed` Chase | 3,5 u/s |
| Rango detección | 6 u — omnidireccional (sin cono) |
| `attackRange` | 5,0 u |
| Daño proyectil | 10 SI — recto |
| Velocidad proyectil | 7,5 u/s |
| Proyectil especial | Cada 3er ataque: rastreo parcial (gira hasta 30° hacia rover) |
| Tiempo vida proyectil | 3,0 s |
| Cooldown ataque | 1,8 s |
| Comportamiento | Patrullaje vertical en columnas. No se aleja >8 u de su punto de origen |

### 8.7 Centinela Principal (Boss Final)

| Parámetro | Valor |
|-----------|-------|
| HP total | 400 (divididos en 3 fases) |
| `moveSpeed` | 1,5 u/s — no persigue activamente |
| Posición | Anclado al centro. Máx. 3 u desde posición base |

| Fase | HP | Ataques | Notas |
|------|----|---------|-------|
| **1** | 400–268 (33%) | Abanico 3 proyectiles (12 SI c/u, 8 u/s). Cooldown: 2,5 s | Fase introductoria. Sin seguimiento |
| **2** | 267–134 (33%) | Abanico 5 proyectiles + 1 proyectil de rastreo (18 SI, gira 45°, 6 u/s). Cooldown: 2,0 s. Spawn de Centinela Secundario (×1) a los 10 s | Objetivo de extensión |
| **3** *(stretch goal)* | 133–0 (33%) | Igual Fase 2 + pulso de área radio 4 u c/8 s (25 SI) | Fuera del scope del prototipo |

---

## 9. Diseño de niveles

### 9.1 Técnicas 2.5D

| Técnica | Implementación |
|---------|---------------|
| **Parallax multi-capa** | Niv.1: 3 capas. Niv.2: 4 capas. `ParallaxBackground.cs` usando `Camera.main.transform` |
| **Proyección axonométrica** | Niv.2: ángulo isométrico 26° en elementos de fondo. Superficies superiores con tono más claro. Plano de juego ortogonal (2D puro) |
| **Escala eje Z** | Primer plano: 110–120%. Fondo lejano: 60–70% con saturación −30%. NPCs de fondo: 50% del rover |
| **Billboarding** | Partículas con componente Billboard de URP → orientación a `Camera.main` cada frame |
| **Ray casting** | `Physics2D.Raycast`: suelo, pared. `Physics2D.BoxCast`: detección avanzada. `CircleCast`: escaneo |
| **Sombras planares** | URP `Light 2D` + `Shadow Caster 2D`. Rover: sombra dinámica. Seres bioluminiscentes: sombra opacidad 0,4 |

### 9.2 Nivel 1 — Las Cuevas Bioluminiscentes

| Parámetro | Valor |
|-----------|-------|
| Bioma | Cueva de cristal — luz azul-verde ambiental |
| Duración estimada | 25–35 min |
| Checkpoints | 3 (inicio, media progresión, pre-boss) |
| Enemigos | Ser Bioluminiscente (×6), Drone Patrullero (×2), Leviatán (×1 boss) |
| Objetos escaneables | SC-01, SC-02, SC-03 |
| Upgrades | Rueda Reforzada (Zona 2, opcional), Escaneo Mejorado (Zona 3) |
| SI al inicio | 74% — Fase 1 |
| SI estimada al final | 47–61% |

### 9.3 Nivel 2 — El Relicto

| Parámetro | Valor |
|-----------|-------|
| Bioma | Estructura alienígena — luz violeta-naranja, materiales metálicos con bioluminiscencia orgánica |
| Duración estimada | 35–50 min |
| Checkpoints | 4 (entrada, Ala A, Ala B, pre-boss) |
| Enemigos | Drone Detector (×3), Drone Patrullero (×2), Centinela Secundario (×2), Centinela Principal (×1 boss) |
| Objetos escaneables | SC-04, SC-05, SC-06 |
| Upgrades | Escudo de Plasma (entrada), Batería EMP (Ala B, opcional) |
| SI al inicio | 47–61% (herencia Niv.1) |
| SI estimada al final | 9–29% |
| Estructura | Metroidvania con 3 alas en orden semi-libre |

---

## 10. HUD e interfaz de usuario

### 10.1 Elementos del HUD

| Elemento | Posición | Componente Unity | Comportamiento |
|----------|---------|-----------------|---------------|
| **Barra de SI** | Top-left `(32, −32)` | `Slider` + `Image` (fill) + `TextMeshPro` | Valor numérico siempre visible. Pulso al recibir daño: scale 1.0→1.08→1.0 en 0,12 s |
| **Slots de celda** (×2) | Top-left, debajo de barra SI | `Image` por slot | Lleno: icono brillante. Vacío: gris 40% opacidad. Pulsante al activar |
| **Contador de Sol** | Top-center `(0, −28)` | `TextMeshPro` monoespacio | Texto: `SOL [N]`. Incrementa al entrar en nueva zona. Fase 5+: parpadeo |
| **Estado transmisión** | Top-right `(−32, −32)` | `TextMeshPro` + `Image` (antena) | Ping cada 8 s. Sin respuesta: `SENAL PERDIDA` rojo. Fase 6: `BATERIA CRITICA` rojo parpadeante |
| **Slots upgrades** | Bottom-left `(32, 32)` | `Image` por slot (máx. 4) | Gris hasta desbloquearse. Brillo de cooldown para EMP/Escudo. Shake si no disponible |
| **Minimapa** | Bottom-right `(−32, 32)` | `RenderTexture` de cámara ortogonal + `RawImage` | Radio 10 u. Degradado circular en bordes. Fase 4+: ruido en 15% píxeles. Fase 5+: apagado c/20 s |
| **Alert Strip** | Bottom-center | `TextMeshPro` + fondo semitransparente | Solo con alerta activa. Mayúsculas monoespacio. Fade 0,5 s al terminar |

### 10.2 Rangos de color del HUD

| Rango SI | Estado | Color barra | Color texto | Efectos |
|----------|--------|-------------|-------------|---------|
| 100–61% | NOMINAL | Verde `#4CAF50` | Blanco | Ninguno |
| 60–41% | ADVERTENCIA | Ámbar `#FFA726` | Ámbar | Antena vibra en Idle |
| 40–21% | CRÍTICO | Rojo `#F44336` | Rojo parpadeante | Tinte naranja. Alarma c/30 s |
| 20–1% | EXTINCIÓN | Rojo oscuro `#8B0000` | Rojo oscuro parpadeante rápido | Tinte naranja intenso. Estática. Apagones |

---

## 11. Sistema de escaneo y lore

### 11.1 Parámetros del sistema

| Parámetro | Valor |
|-----------|-------|
| Radio base | 3 u |
| Radio con upgrade | 5 u |
| Ping pasivo (con upgrade) | Objetos brillan a <4 u |
| Activación | Mantener botón de escaneo |
| Duración texto | Mientras se mantiene el botón. Fade 0,3 s al soltar |
| Degradación | A partir de SI 29%: caracteres corruptos progresivos |
| Tipografía | Monoespaciada, tamaño reducido (terminal embebido) |
| Color texto | Blanco = nominal. Ámbar = advertencia. Rojo = anomalía |
| Encabezado | `ANALISIS`, `REGISTRO AMBIENTAL`, `SENAL IDENTIFICADA` o `ARCHIVO HISTORICO` |

### 11.2 Objetos escaneables del prototipo

| ID | Nombre | Nivel/Zona | Función | Texto HUD |
|----|--------|-----------|---------|-----------|
| SC-01 | Esferas de hematita | Niv.1 — Zona 1 | Activa FB-01. Revela ruta oculta | `ANALISIS: Esferas de hematita cristalizada. Diametro: 4,5 mm...` |
| SC-02 | Meteorito de hierro | Niv.1 — Zona 3 | Activa FB-02 | `ANALISIS: Aleacion hierro-niquel. Primer meteorito identificado en otro planeta...` |
| SC-03 | Formación arcilla | Niv.1 — Zona 4 | Activa FB-03. Abre puerta al Leviatán | `ANALISIS: Filosilicatos. Agua de pH neutro...` |
| SC-04 | Registro de tormenta | Niv.2 — Inicio | Activa FB-05. Desbloquea filtro minimapa | `REGISTRO AMBIENTAL: Tormenta de polvo global. Tau maximo: 10,8...` |
| SC-05 | Glifo del Relicto | Niv.2 — Entrada | Activa ascensor al Niv.2 | `ANALISIS: Patron electromagnetico coherente. Frecuencia: 437,5 MHz...` |
| SC-06 | Archivo Spirit | Niv.2 — Ala C | Activa escena de Spirit | `SENAL IDENTIFICADA: ROVER CLASE MER-A. SOL 2.208...` |

### 11.3 Tabla de flashbacks

| ID | Sol | Contenido | Trigger | Nivel/Zona | SI mín. |
|----|-----|-----------|---------|-----------|---------|
| FB-01 | 339 | Blueberries de hematita. `EVIDENCIA DE AGUA LIQUIDA ANTIGUA CONFIRMADA` | Escanear primera formación cristal | Niv.1 Z1 | 61% |
| FB-02 | 951 | Panorámica cráter Endurance. Solo imagen + sonido ruedas | Escanear meteorito de hierro Z3 | Niv.1 Z3 | 61% |
| FB-03 | 2.681 | Arcillas cráter Endeavour. `FILOSILICATOS. POTENCIALMENTE HABITABLE` | Escanear formación central sala Leviatán | Niv.1 Z4 | 47% |
| FB-04 | 3.846 | Odómetro alcanza 42.195 km. `MARATON COMPLETA` | Completar 42 km en odómetro (pasivo) | Cualquiera | — |
| FB-05 | 5.111 | La tormenta. Imagen→estática. `SENAL TX: PERDIDA. BATERIA: 9%` | Escanear panel checkpoints Niv.2 | Niv.2 Inicio | 29% |
| FB-06 | 2.208 | Transmisión de Spirit. B/N. Telemetría bajando a cero | Escanear archivo central Ala C | Niv.2 Ala C | 9% |

---

## 12. Narrativa y cinemáticas

### 12.1 Arco narrativo — Cuatro actos

| Acto | Momento | SI | Emoción objetivo |
|------|---------|-----|-----------------|
| **I — Despertar** | Niv.1 inicio | 74% | Esperanza silenciosa |
| **II — Descubrimiento** | Niv.1 completo | 61% | Maravilla + tensión creciente |
| **III — La carrera** | Niv.2 | 47→9% | Urgencia + tristeza anticipada |
| **IV — El silencio** | Cinemática final | 1% | Pérdida + redención |

### 12.2 Pantalla de carga — Datos del sistema

| Parámetro | Valor | Color |
|-----------|-------|-------|
| SOL ACTUAL | 5.847 | Blanco |
| ÚLTIMA TX RECIBIDA (TIERRA) | Sol 5.111 — 10 junio 2018 | Rojo |
| SEÑAL | PERDIDA — tormenta global de polvo | Rojo |
| INTENTOS DE RECONTACTO | 1.034 | Rojo |
| ESTADO MISIÓN (TIERRA) | CONCLUIDA — 13 febrero 2019 | Rojo |
| ESTADO MISIÓN (ROVER) | ACTIVA — protocolo de exploración en curso | Verde |
| INTEGRIDAD ESTRUCTURAL | 74% | Ámbar |
| MODO | EXPLORACIÓN AUTÓNOMA | Blanco |

### 12.3 Secuencia del final — Especificación escena por escena

| # | Escena | Descripción técnica |
|---|--------|-------------------|
| 1 | El último intento | Brazo falla, logra en segundo intento (delay 1,2 s). Solo audio mecánico del brazo |
| 2 | Transmisión interrumpida | Barra: 1%→11%→23%→38%. Energía = 0%. Imagen congela. `ENERGIA INSUFICIENTE. Fragmentos: 341/891`. Sin música |
| 3 | Apagón de sistemas | Cada sistema cierra con línea de log. Última línea verde: `PROTOCOLO DE MISION: COMPLETADO`. Pantalla negra |
| 4 | **El silencio** | **Negro absoluto. Sin música. Sin texto. Sin efectos. 10 segundos exactos. No saltable** |
| 5 | El informe | Texto blanco sobre negro, fade 0,8 s/línea: JPL recibió 341 fragmentos → evidencia agua + vida → misión humana 2037 |
| 6 | Marte, 2037 | Sin diálogo. Astronauta aterriza, camina, se arrodilla. Sin mostrar rostro. Toca objeto en polvo |
| 7 | Plano final | Astronauta sostiene rover contra cielo naranja. Fade a negro 5 s. Texto de cierre |

**Texto final:**
```
La misión de Opportunity duró 5.111 soles.
Recorrió 45,16 km — una maratón completa en otro planeta.
Sus datos cambiaron para siempre la comprensión humana de Marte.

Su última señal interpretada fue:
«My battery is low and it's getting dark.»

Nunca dejó de transmitir.
```

> ⚠️ **Regla de oro:** El astronauta nunca habla. Cualquier diálogo destruye la escena. Sin excepciones.

### 12.4 Transmisión de Spirit — Escena del Ala C

| Parámetro | Valor |
|-----------|-------|
| Formato | Pantalla dividida: izq = escaneo Opportunity, der = transmisión Spirit |
| Contenido derecha | Telemetría Spirit en monoespacio. Batería: 12%→0% en 40 s. Temperatura cae |
| Audio | Zumbido electrónico. Desaparece cuando batería = 0% |
| Último dato | `TEMPERATURA INTERNA: -40C. BATERIA: 0%. MODO: —` |
| HUD de Opportunity | `SENAL IDENTIFICADA: ROVER CLASE MER-A` (3 s) |
| Duración | 45–55 s. **No saltable** |

---

## 13. Arte y animación

### 13.1 Paleta de colores

| Paleta | Uso | Colores principales | Colores de acento |
|--------|-----|--------------------|--------------------|
| **Rover + UI** | Rover, HUD, efectos del rover | `#E07040` naranja polvoriento, `#8A9BA8` gris metálico, `#F0EBE3` blanco envejecido | `#3A8FC1` azul indicador, `#4CAF50` verde sistema, `#F44336` rojo alerta |
| **Nivel 1** | Fondos, tiles, iluminación | `#0A0A12` negro cueva, `#1A3A6E` azul cristal, `#2ECC71` verde bioluminiscente | `#00BCD4` turquesa detalle, `#E8F5E9` blanco destellos |
| **Nivel 2** | Fondos, tiles, iluminación | `#060610` negro profundo, `#4A1A7A` violeta alienígena, `#C0581A` naranja oxidado | `#B8860B` dorado circuitos, `#00E5FF` cian tecnológico |

### 13.2 Proporciones de sprites (1×)

| Personaje | Sprite 1× (px) | Escala Unity | Notas |
|-----------|----------------|-------------|-------|
| Rover Opportunity | 52 × 32 | ×1,0 (referencia) | Horizontal bajo. Ruedas diferenciadas. Brazo robótico lado derecho |
| Ser Bioluminiscente | 24 × 24 | ×0,75 | Esférico con tentáculos. Translúcido — shader transparencia + emisión |
| Leviatán | 96 × 64 | ×3,0 | Solo cuerpo. Tentáculos = GameObjects separados con sprite y hitbox propio |
| Drone Patrullero | 32 × 20 | ×1,0 | Diamante horizontal. Sin ruedas. Propulsores visibles |
| Drone Detector | 36 × 36 | ×1,1 | Cúbico con ojo central. Cono de visión = VFX, no sprite |
| Centinela Secundario | 44 × 52 | ×1,3 | Vertical, más alto que rover. Cañón en hombro derecho |
| Centinela Principal | 120 × 80 | ×3,75 | Simétrico. 3 núcleos de ataque visibles |

### 13.3 Animator Controller del Rover — Estados

| Estado | Frames | FPS | Loop | Condición de entrada | Prioridad |
|--------|--------|-----|------|---------------------|-----------|
| `Idle` | 8 | 8 | Sí | Sin input de movimiento | MVP |
| `Run` | 10 | 12 | Sí | `velocidadX > 0,1` | MVP |
| `Jump_Rise` | 6 | 12 | No | `!isGrounded && velocidadY > 0` | MVP |
| `Jump_Fall` | 4 | 8 | Sí | `!isGrounded && velocidadY ≤ 0` | MVP |
| `Land` | 4 | 12 | No | `isGrounded` (desde Jump_Fall) | MVP |
| `Dash` | 6 | 24 | No | `isDashing` | MVP |
| `Damage` | 5 | 12 | No | `onDamageReceived()` | MVP |
| `Death` | 12 | 8 | No | `SI = 0` | MVP |
| `Scan_Loop` | 8 | 8 | Sí | `isScanning` | MVP |
| `Idle_Degrade` | 8 | 8 | Sí | Sin input && SI ≤47% | V2 |
| `Run_Limp` | 12 | 12 | Sí | `velocidadX > 0,1` && SI ≤29% | V2 |
| `Dash_End` | 4 | 12 | No | `!isDashing` (desde Dash) | V2 |
| `Wall_Slide` | 6 | 8 | Sí | `isOnWall && velocidadY < 0` | V2 |
| `Wall_Jump` | 5 | 12 | No | wallJump input | V2 |
| `Climb_Start` | 4 | 12 | No | `isClimbing` (inicio) | V2 |
| `Climb_Loop` | 6 | 8 | Sí | `isClimbing` (sostenido) | V2 |
| `Climb_End` | 4 | 12 | No | `!isClimbing` (desde Climb) | V2 |
| `Scan_Start` | 5 | 12 | No | `onScanActivate()` (primer frame) | V2 |

### 13.4 Animación de enemigos

| Enemigo | Estado | Frames | FPS | Condición |
|---------|--------|--------|-----|-----------|
| Ser Bioluminiscente | `Float_Idle` | 8 | 8 | Sin rover detectado |
| Ser Bioluminiscente | `Float_Alert` | 6 | 12 | Rover en rango |
| Ser Bioluminiscente | `Float_Chase` | 6 | 12 | Chase activo |
| Ser Bioluminiscente | `Death` | 8 | 8 | HP = 0 |
| Drone Patrullero | `Patrol` | 6 | 8 | Estado base |
| Drone Patrullero | `Chase` | 6 | 12 | Rover detectado |
| Drone Patrullero | `Attack` | 4 | 12 | attackRange |
| Drone Patrullero | `Death` | 8 | 10 | HP = 0 |
| Drone Detector | `Patrol` | 6 | 8 | Estado base |
| Drone Detector | `Alert` | 4 | 12 | Rover en cono |
| Drone Detector | `Chase` | 6 | 12 | Chase activo |
| Drone Detector | `Attack` | 5 | 12 | attackRange |
| Drone Detector | `Search` | 6 | 8 | Rover fuera de rango 4 s |
| Drone Detector | `Death` | 8 | 10 | HP = 0 |
| Centinela Principal | `Idle` | 8 | 8 | Fase 1 sin ataque |
| Centinela Principal | `Attack_F1` | 10 | 12 | Abanico activo |
| Centinela Principal | `Attack_F2` | 12 | 12 | Fase 2 |
| Centinela Principal | `Enrage` | 8 | 16 | Transición entre fases |
| Centinela Principal | `Death` | 32 | 8 | HP = 0 — **no interrumpible** |

---

## 14. Audio

**Decisión:** Unity Audio Mixer nativo. FMOD fuera del scope.

### 14.1 Estados de música

| Estado | Condición | Contenido | Crossfade |
|--------|-----------|-----------|-----------|
| `EXPLORATION` | SI ≥41% y sin enemigos detectados | Ambient baja intensidad. Percusión espaciada. Viento marciano | 1,5 s |
| `TENSION` | Enemigo en Alert/Chase, o SI 20–40% | Drones de tensión. Percusión irregular. Sin melodía | 0,8 s |
| `COMBAT` | Boss activo o múltiples enemigos en Chase | Orquestación alta energía. Percusión rítmica | 0,5 s |
| `CINEMATIC` | Cinemáticas, flashbacks, final | Música original por escena | Gestionado por `CinematicManager` |

### 14.2 Efectos de sonido del rover

| Efecto | Trigger | Prioridad | Notas |
|--------|---------|-----------|-------|
| Ruedas sobre roca | `OnCollisionEnter2D` + vel. >1 u/s | MVP | Pitch ±10% según velocidad. 2 variantes: liso/rugoso |
| Dash | `onDash()` | MVP | Mecánico corto (0,15 s) |
| Daño recibido | `onDamageReceived()` | MVP | Metálico. Pitch baja 5% por fase de degradación |
| Activación escaneo | `onScanActivate()` | MVP | Tono electrónico ascendente. Diferente para lore vs vacío |
| Antena vibrando | Idle con SI ≤73% | Fase 2 | Loop vibración sutil. Volumen sube al bajar SI |
| Alarma batería | SI ≤40% | Fase 4 | Pip c/30 s. Distorsión digital |

---

## 15. Arquitectura de software

### 15.1 Módulos principales

| Módulo | Responsabilidad |
|--------|----------------|
| `GameManager` | Estado global de la partida: `MainMenu`, `Playing`, `Paused`, `GameOver`, `Cinematic`. Carga/descarga de escenas |
| `PlayerController` | Movimiento del rover (carrera, salto, dash, escalada). Recibe input de `InputSystem`. Eventos: `onDamageReceived`, `onDash`, `onLand`, `onDeath` |
| `DegradationSystem` | Gestiona SI del rover. Aplica modificadores de fase al `PlayerController` vía `ScriptableObject`. Expone SI al `HUDManager` |
| `AIManager` | Controlador central de NPCs. DDA, comunicación entre drones, spawn de enemigos. Cada NPC = agente registrado |
| `ScanSystem` | Radio de detección, triggers de objetos escaneables, comunicación con `HUDManager`. Dispara flashbacks vía `CinematicManager` |
| `LevelManager` | Estado del nivel: checkpoints, posición respawn, enemigos derrotados, objetos recogidos. Persiste entre muertes de la sesión |
| `HUDManager` | Actualiza Canvas en tiempo real: SI bar, slots celda, Sol, transmisión, minimapa, Alert Strip. Suscrito a `DegradationSystem` |
| `AudioManager` | 4 estados de música vía `Unity Audio Mixer`. Pool de SFX. Triggers de `GameManager`, `PlayerController`, `AIManager` |
| `CinematicManager` | Flashbacks, escena Spirit, secuencia final. Congela `GameManager` durante cinemáticas. Gestiona timings del final |

### 15.2 Stack tecnológico

| Herramienta | Versión | Uso |
|-------------|---------|-----|
| Unity | 6.4 (6000.4.8f1) | Motor de juego |
| URP | Incluido en Unity 6.4 | Rendering |
| Unity Input System | 1.7+ | Gestión de input |
| Unity AI Navigation | 1.1+ | NavMesh 2D |
| Unity Audio Mixer | Nativo | Audio adaptativo |
| TextMeshPro | Nativo | UI y HUD |
| C# | .NET 6 / IL2CPP | Scripting |
| Blender | 3.x LTS | Modelado 3D (elementos ambientales Relicto) |
| Aseprite / Libresprite | Cualquiera | Sprites 2D |
| Git + GitHub | Git 2.40+ | Control de versiones |

---

## 16. Configuración de Unity

### 16.1 Layer Collision Matrix

> Configurar en **Edit → Project Settings → Physics 2D → Layer Collision Matrix**

| | Player | Enemy | PlayerProjectile | EnemyProjectile | Ground | Platform | Interactable | Trigger |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **Player** | — | ● | — | ● | ● | ● | ● | ● |
| **Enemy** | ● | — | ● | — | ● | ● | — | — |
| **PlayerProjectile** | — | ● | — | — | ● | — | — | — |
| **EnemyProjectile** | ● | — | — | — | ● | — | — | — |
| **Ground** | ● | ● | ● | ● | — | — | — | — |
| **Platform** | ● | ● | — | — | — | — | — | — |
| **Interactable** | ● | — | — | — | — | — | — | — |
| **Trigger** | ● | — | — | — | — | — | — | — |

**Nota sobre Platform:** Usa `PlatformEffector2D` con `rotationalOffset = 0` y `useOneWay = true`. El rover puede caer a través desde arriba pero no atravesarlas desde abajo. Los enemigos permanecen sobre ellas sin lógica one-way.

---

## 17. Requisitos de hardware

| Componente | Mínimo | Recomendado |
|-----------|--------|-------------|
| SO | Windows 10 64-bit (1903+) | Windows 10/11 64-bit |
| CPU | Intel i5-4460 / AMD Ryzen 3 1200 | Intel i7-7700 / AMD Ryzen 5 3600 |
| RAM | 4 GB | 8 GB |
| GPU | GTX 950 / RX 460 (2 GB VRAM, DX11) | GTX 1060 / RX 580 (4 GB VRAM) |
| Almacenamiento | 500 MB (HDD) | 500 MB (SSD) |
| Resolución | 1280×720 | 1920×1080 |
| API gráfica | DirectX 11 | DirectX 11 |
| Conectividad | No requerida (offline) | No requerida |
| Input | Teclado + ratón | Teclado + ratón o gamepad XInput |

| Rendimiento objetivo | Valor |
|---------------------|-------|
| Framerate objetivo | 60 FPS estables (config recomendada) |
| Framerate mínimo | 30 FPS estables (config mínima) |
| Resolución HUD | 1920×1080 (Canvas Scaler referencia 1080p, `Scale With Screen Size`) |
| VRAM estimada | ~350 MB @ 1080p |
| Tiempo carga nivel | <8 s (SSD), <20 s (HDD) |

---

*─── Fin del documento ───*
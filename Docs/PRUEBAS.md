# Plan y Ejecución de Pruebas Beta

**UNIVERSIDAD CENTRAL DEL ECUADOR**
**Facultad de Ingeniería y Ciencias Aplicadas — Carrera de Computación**
**Proyecto de Videojuegos — Trabajo Grupal 2026**

---

## 1. Información general

| Campo                           | Detalle                                                                                          |
| ------------------------------- | ------------------------------------------------------------------------------------------------ |
| **Nombre del videojuego**       | Opportunity: Red Dust Legacy                                                                     |
| **Género**                      | Metroidvania 2.5D / Plataformas de ciencia ficción                                               |
| **Motor**                       | Unity 6.4 (6000.4.8f1) — Universal Render Pipeline 17.4                                          |
| **Plataforma**                  | PC — Windows 10/11                                                                               |
| **Versión evaluada**            | Beta 1.0 (rama `develop`, prototipo: menú principal + 2 niveles jugables con pantallas de carga) |
| **Fecha de la prueba**          | 10 de julio de 2026 — primera tanda (4 jugadores)                                                |
| **Duración estimada de sesión** | 60–85 min (primera partida completa, según GDD)                                                  |

### Integrantes del equipo — Red Dust Team

| Integrante      | Rol                         |
| --------------- | --------------------------- |
| Edison Enríquez | Líder / Gameplay Programmer |
| Angelo Silva    |                             |
| Kelly Ledesma   |                             |
| Doris           |                             |
| Kevin           |                             |
| Stalin          |                             |

### Objetivo de la prueba

Validar la experiencia de juego del prototipo funcional de _Opportunity: Red Dust Legacy_ con jugadores externos al desarrollo, evaluando:

1. La **precisión y respuesta del control del rover** (carrera, salto variable, coyote time, dash con freeze-frame, wall jump).
2. La **legibilidad de la IA enemiga** (Ser Bioluminiscente, drones patrulleros/detectores, Leviatán y Centinelas): el jugador que muere debe entender por qué murió.
3. El **combate mediante dash ofensivo** y la mecánica de aturdir al Ser Bioluminiscente con el escaneo: ¿el jugador las descubre y comprende sin explicación?
4. El **Sistema de Integridad Estructural (SI)** con sus 6 fases de degradación: ¿el jugador percibe y entiende la pérdida progresiva de movilidad?
5. La **curva de dificultad** de los niveles 1 y 2, incluyendo el jefe del Nivel 1 (Leviatán) y su ventana de vulnerabilidad.
6. La **claridad del HUD y los menús** (barra de SI por colores, celdas de energía, avisos, pausa, Game Over).
7. La **estabilidad y rendimiento** del build en hardware variado, registrando todos los errores encontrados.

---

## 2. Perfil de los jugadores beta

La prueba contempla un mínimo de **10 a 20 jugadores** que no hayan participado en el desarrollo. **Primera tanda ejecutada: 4 jugadores** (10-jul-2026) — los datos de las secciones 4.6, 5, 6 y 7 corresponden a esta tanda (fuente: `Docs/Metricas Pruebas.xlsx`).

| ID  | Edad | Experiencia (baja / media / alta) | Géneros que suele jugar |
| --- | ---- | --------------------------------- | ----------------------- |
| P01 | 20   | Alta                              | Variado                 |
| P02 | 22   | Alta                              | Shooters                |
| P03 | 15   | Media                             | Shooters                |
| P04 | 21   | Alta                              | RPG                     |
| P05 |      |                                   |                         |
| P06 |      |                                   |                         |
| P07 |      |                                   |                         |
| P08 |      |                                   |                         |
| P09 |      |                                   |                         |
| P10 |      |                                   |                         |
| P11 |      |                                   |                         |
| P12 |      |                                   |                         |

---

## 3. Escenarios de prueba

Tareas que cada jugador deberá realizar durante la sesión, sin ayuda del equipo:

| #   | Escenario                               | Descripción                                                                                                                                                                                          | Criterio de éxito                            |
| --- | --------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------- |
| E1  | Navegar el menú principal               | Iniciar partida con **Nueva Misión** y esperar la pantalla de carga                                                                                                                                  | Llega al Nivel 1 sin ayuda                   |
| E2  | Dominar el movimiento básico            | Correr, saltar (salto completo y corto), hacer dash y wall jump (saltar mientras se desliza por una pared)                                                                                           | Ejecuta las 4 mecánicas al menos una vez     |
| E3  | Superar los peligros del Nivel 1        | Atravesar plataformas móviles, cristales que caen y zonas de daño, evitando caídas altas (dañan al rover)                                                                                            | Cruza la zona de peligros                    |
| E4  | Combatir al Ser Bioluminiscente         | Aturdirlo con el escaneo (`E`) y/o destruirlo embistiéndolo con el dash                                                                                                                              | Supera el encuentro sin morir más de 3 veces |
| E5  | Recoger y usar una celda de energía     | Recolectar una celda (reserva máx. 2) y activarla con `Q` para recuperar 20 de SI                                                                                                                    | Usa la celda correctamente                   |
| E6  | Gestionar la degradación (SI)           | Recibir daño, observar el cambio de color/fase en la barra del HUD y notar la pérdida de movilidad en fases avanzadas                                                                                | Explica qué le pasó al rover                 |
| E7  | Usar el sistema de checkpoints          | Cruzar un checkpoint (repara la SI), morir y reintentar desde él sin recargar el nivel; tras 3 muertes seguidas en el mismo checkpoint, el nivel reinicia desde el principio con el rover restaurado | Comprende el respawn sin explicación         |
| E8  | Derrotar al Leviatán (jefe del Nivel 1) | Esquivar los tentáculos y golpear el núcleo con el dash durante la ventana de vulnerabilidad; su muerte abre la salida del nivel                                                                     | Derrota al jefe                              |
| E9  | Superar el Nivel 2                      | Evadir drones patrulleros y detectores con sus proyectiles, y combatir a los Centinelas con el dash (el Principal invoca a un Secundario en Fase 2)                                                  | Avanza por el nivel                          |
| E10 | Completar el recorrido del prototipo    | Llegar a la salida del Nivel 2 (regresa al menú principal)                                                                                                                                           | Termina el prototipo                         |
| E11 | Pausar y reanudar                       | Abrir la pausa con `Esc`, reanudar, reintentar o volver al menú                                                                                                                                      | Usa el menú de pausa sin ayuda               |

### Controles entregados al jugador

| Acción                | Teclado                               | Gamepad         |
| --------------------- | ------------------------------------- | --------------- |
| Moverse               | `A` / `D` · `←` / `→`                 | Stick izquierdo |
| Saltar (variable)     | `W` o `↑` (mantener)                  | —               |
| Wall jump             | `W` mientras se desliza por una pared | —               |
| Dash (también ataca)  | `Shift izq.`                          | —               |
| Escanear / aturdir    | `E` (mantener; detiene al rover)      | —               |
| Usar celda de energía | `Q`                                   | —               |
| Pausa                 | `Esc`                                 | `Start`         |

### Limitaciones conocidas de la versión beta

Se informan a los evaluadores para distinguir bugs nuevos de carencias ya registradas:

- Guardado de un solo slot (S06 T5): autosave al entrar a cada nivel y al registrar checkpoint. **Continuar** retoma el nivel guardado **desde su inicio** (no guarda posición) con SI/celdas/lore; **Nueva Misión** pide confirmación si pisaría un guardado. **Opciones** y **Archivo de Misión** siguen deshabilitados.
- El gamepad solo controla el movimiento; saltar, dash y escanear requieren teclado. Las teclas no son reasignables.
- El escaneo aún no muestra datos de lore; su uso actual es aturdir al Ser Bioluminiscente.
- Derrotar al **Centinela Principal** (Nivel 2) todavía no dispara la secuencia de victoria/cinemática final (en desarrollo); el combate sí es completo (se le daña con el dash).
- No existe el sistema de upgrades previsto en el GDD; el wall jump está disponible desde el inicio.

---

## 4. Aspectos a evaluar

Escala de **1 a 5** (1 = muy malo, 5 = excelente). Registrar el promedio de todos los jugadores.

### 4.1 Jugabilidad

| Criterio                                              | Promedio (1–5) | Observaciones |
| ----------------------------------------------------- | :------------: | ------------- |
| Facilidad para aprender a jugar                       |                |               |
| Respuesta de los controles                            |                |               |
| Precisión de los movimientos (salto, dash, wall jump) |                |               |
| Dificultad adecuada                                   |                |               |
| Diversión                                             |                |               |

### 4.2 Interfaz de usuario

| Criterio                                                                            | Promedio (1–5) | Observaciones |
| ----------------------------------------------------------------------------------- | :------------: | ------------- |
| Claridad de los menús (principal y pausa)                                           |                |               |
| Tamaño de textos                                                                    |                |               |
| Comprensión de los iconos (barra de SI por colores, celdas, avisos, barra del jefe) |                |               |
| Retroalimentación del sistema (pulso de daño, alertas, Game Over)                   |                |               |

### 4.3 Diseño

| Criterio                                              | Promedio (1–5) | Observaciones |
| ----------------------------------------------------- | :------------: | ------------- |
| Calidad gráfica (URP, iluminación 2D, parallax)       |                |               |
| Animaciones (rover y enemigos)                        |                |               |
| Efectos visuales                                      |                |               |
| Diseño de niveles (cavernas / instalación alienígena) |                |               |

### 4.4 Audio

| Criterio                                | Promedio (1–5) | Observaciones |
| --------------------------------------- | :------------: | ------------- |
| Música (BGM adaptativa por estados)     |                |               |
| Efectos de sonido (rover, enemigos, UI) |                |               |
| Balance del volumen                     |                |               |

### 4.5 Rendimiento

| Criterio                       | Promedio (1–5) | Observaciones |
| ------------------------------ | :------------: | ------------- |
| Fluidez (FPS)                  |                |               |
| Tiempos de carga entre escenas |                |               |
| Errores o bloqueos             |                |               |
| Consumo de recursos            |                |               |

### 4.6 Experiencia del usuario

| Pregunta                              | Respuestas registradas                                                                                                                                                                                                    |
| ------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| ¿Comprendió el objetivo del juego?    | Sí; todos entendieron que debían superar las plataformas.                                                                                                                                                                 |
| ¿Se sintió motivado a seguir jugando? | No; la cantidad de errores/fallas en el nivel y el agotamiento del rover redujeron la motivación.                                                                                                                         |
| ¿Qué fue lo más divertido?            | Intentar pasar las plataformas; el parkour fue lo más destacado.                                                                                                                                                          |
| ¿Qué fue lo más frustrante?           | Cooldown del dash muy alto, enemigos difíciles de derrotar cuerpo a cuerpo, no poder saltar por falta de energía, checkpoint lejano en el Nivel 2 y enemigos que atacan desde lejos sin ataque a distancia para el rover. |

---

## 5. Registro de errores (Bug Report)

Severidad: **Crítica** (impide continuar) · **Alta** (afecta gravemente la jugabilidad) · **Media** (molesta pero con workaround) · **Baja** (cosmética).

| ID      | Descripción                                       | Pasos para reproducir                                                                                   | Resultado esperado                                                                                                        | Resultado obtenido                                                                                                    | Severidad | Evidencia |
| ------- | ------------------------------------------------- | ------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------- | --------- | --------- |
| BUG-001 | Fallo en pantalla de Game Over al morir           | Morir en medio de las plataformas móviles o espinas mecánicas.                                          | Al llegar a 0% de vida, debe desplegarse inmediatamente la pantalla de Game Over.                                         | El Rover muere, pero la pantalla tarda en aparecer o no aparece en absoluto, dejando el juego congelado.              | Crítica   | —         |
| BUG-002 | Incapacidad de reiniciar el juego tras el bloqueo | Sufrir el bug donde no carga la pantalla de Game Over e intentar forzar el reinicio.                    | El botón de reinicio o el comando correspondiente debe limpiar la escena y recargar desde el menú principal o checkpoint. | El bucle de muerte bloquea el flujo del código y no se puede iniciar ni interactuar al presionar reiniciar.           | Crítica   | —         |
| BUG-003 | Activación errónea del botón Continuar            | Iniciar el juego por primera vez (sin datos de guardado previos) en el menú principal.                  | El botón de "Continuar" debe aparecer desactivado o bloqueado si no hay una partida previa.                               | El botón está completamente funcional y permite interactuar con él, lo cual corrompe la inicialización.               | Alta      | Corregido S06 T5 (2026-07-11): `Btn_Continuar` se deshabilita sin guardado. |
| BUG-004 | Bloqueo físico bajo plataformas con espinas       | Avanzar por el Nivel 1 y caer o quedar atrapado justo debajo de la plataforma que contiene dos espinas. | El Rover debe recibir daño por colisión y ser empujado o destruido (activar muerte limpia).                               | El sprite del Rover se queda permanentemente atascado o bugeado bajo la geometría de la plataforma sin salir.         | Alta      | —         |
| BUG-005 | Fallo crítico del Checkpoint en el Nivel 2        | Morir en el Nivel 2 y reaparecer automáticamente en el último checkpoint alcanzado.                     | El Rover debe revivir en la posición del checkpoint con la barra de vida al 100% y sus estados limpios.                   | El Rover muere instantáneamente al aparecer en el checkpoint porque la barra de vida no se restaura y se queda en 0%. | Crítica   | —         |

---

## 6. Métricas

### 6.1 Por jugador

| ID  | Tiempo Nivel 1 | Tiempo Nivel 2 | N.º de muertes | N.º de intentos vs. Leviatán | ¿Completó el prototipo? |  ¿Abandonó?   |
| --- | :------------: | :------------: | :------------: | :--------------------------: | :---------------------: | :-----------: |
| P01 |     9 min      |     12 min     |       20       |              14              |      No registrado      | No registrado |
| P02 |     11 min     |     13 min     |       25       |              12              |      No registrado      | No registrado |
| P03 |     30 min     |     10 min     |       30       |              17              |      No registrado      | No registrado |
| P04 |     10 min     |     12 min     |       29       |              19              |      No registrado      | No registrado |
| P05 |                |                |                |                              |                         |               |
| P06 |                |                |                |                              |                         |               |
| P07 |                |                |                |                              |                         |               |
| P08 |                |                |                |                              |                         |               |
| P09 |                |                |                |                              |                         |               |
| P10 |                |                |                |                              |                         |               |
| P11 |                |                |                |                              |                         |               |
| P12 |                |                |                |                              |                         |               |

### 6.2 Resumen global

| Métrica                                        | Valor de referencia (GDD) | Valor obtenido |
| ---------------------------------------------- | :-----------------------: | :------------: |
| Tiempo promedio de partida completa            |         60–85 min         |   26,75 min    |
| Promedio de muertes por jugador                |             —             |      26,0      |
| Total de errores encontrados                   |             —             |       38       |
| Jugadores que derrotaron al Leviatán (Nivel 1) |             —             |     0 / 4      |
| Jugadores que completaron el Nivel 2 (salida)  |             —             |     0 / 4      |
| Jugadores que abandonaron antes de terminar    |             —             |     2 / 4      |

---

## 7. Encuesta de satisfacción

### Califique del 1 al 5

| Afirmación                          | Promedio (1–5) |
| ----------------------------------- | :------------: |
| El juego fue entretenido            |      3,75      |
| Los controles fueron intuitivos     |      4,25      |
| El nivel de dificultad fue adecuado |      3,25      |
| La interfaz fue clara               |      4,50      |
| Me gustaría volver a jugar          |      5,00      |

### Preguntas abiertas

| Pregunta                          | Respuestas más frecuentes                                                                                                     |
| --------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| ¿Qué fue lo que más le gustó?     | El diseño del rover, sus sprites y animaciones; el parkour y el diseño de niveles.                                            |
| ¿Qué mejoraría?                   | La dificultad del ascensor, pinchos y robots; el balance del dash; los checkpoints; y el combate a distancia.                 |
| ¿Qué errores encontró?            | Bug del Game Over, sprite de muerte persistente, bug al quedar sobre el drone patrullero y fallos del checkpoint del Nivel 2. |
| ¿Qué nuevas funciones propondría? | Un arma o ataque a distancia y más habilidades de apoyo para derrotar enemigos.                                               |

---

## 8. Análisis de resultados

### 8.1 Resumen de los principales problemas detectados

### 8.2 Errores más frecuentes

### 8.3 Problemas de usabilidad

### 8.4 Aspectos mejor valorados

### 8.5 Priorización de mejoras

---

## 9. Plan de mejoras

| Problema | Prioridad | Solución propuesta | Responsable |
| -------- | :-------: | ------------------ | ----------- |
|          |           |                    |             |
|          |           |                    |             |
|          |           |                    |             |
|          |           |                    |             |
|          |           |                    |             |

---

## 10. Conclusiones

**¿La versión está lista para una versión final?**

**¿Qué aspectos deben corregirse obligatoriamente?**

**¿Qué aprendió el equipo durante la prueba beta?**

---

## Rúbrica de evaluación

| Criterio                                                         | Puntaje |
| ---------------------------------------------------------------- | :-----: |
| Planificación de la prueba                                       |    2    |
| Selección de jugadores beta                                      |    2    |
| Ejecución de las pruebas                                         |    3    |
| Registro de bugs                                                 |    4    |
| Análisis de resultados                                           |    4    |
| Propuesta de mejoras                                             |    3    |
| Calidad del informe y evidencias (capturas, videos, formularios) |    2    |
| **Total**                                                        | **20**  |

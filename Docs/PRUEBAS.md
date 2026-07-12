# Plan y Ejecución de Pruebas Beta

**UNIVERSIDAD CENTRAL DEL ECUADOR**
**Facultad de Ingeniería y Ciencias Aplicadas — Carrera de Computación**
**Proyecto de Videojuegos — Trabajo Grupal 2026**

---

## 1. Información general

| Campo                           | Detalle                                                                                                   |
| ------------------------------- | --------------------------------------------------------------------------------------------------------- |
| **Nombre del videojuego**       | Opportunity: Red Dust Legacy                                                                              |
| **Género**                      | Metroidvania 2.5D / Plataformas de ciencia ficción                                                        |
| **Motor**                       | Unity 6.4 (6000.4.8f1) — Universal Render Pipeline 17.4                                                   |
| **Plataforma**                  | PC — Windows 10/11                                                                                        |
| **Versión evaluada**            | Beta 1.0 (rama `develop`, prototipo: menú principal + 2 niveles jugables con pantallas de carga)          |
| **Fecha de la prueba**          | 10 de julio de 2026 — primera tanda (4 jugadores) · 11–12 de julio de 2026 — segunda tanda (16 jugadores) |
| **Duración estimada de sesión** | 30–40 min (ajustada al prototipo, sin cinemáticas ni lore; GDD: 60–85 min para la versión completa)       |

### Integrantes del equipo — Red Dust Team

| Integrante      | Rol                          |
| --------------- | ---------------------------- |
| Edison Enríquez | Líder / Gameplay Programmer  |
| Angelo Silva    | Diseño de niveles            |
| Kelly Ledesma   | Audio y música               |
| Doris Chicaiza  | UI / sprites                 |
| Kevin Celi      | Programador de IA / enemigos |
| Stalin Acurio   | Fondos parallax              |

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

La prueba contempla un mínimo de **10 a 20 jugadores** que no hayan participado en el desarrollo. **Primera tanda: 4 jugadores** (10-jul-2026, fuente: `Docs/Metricas Pruebas.xlsx`). **Segunda tanda: 16 jugadores** (11–12-jul-2026), ejecutada sobre un build con BUG-001, BUG-003 y BUG-005 ya corregidos. Las secciones 4, 5, 6 y 7 consolidan las **20 sesiones**.

| ID  | Edad | Experiencia (baja / media / alta) | Géneros que suele jugar      |
| --- | ---- | --------------------------------- | ---------------------------- |
| P01 | 20   | Alta                              | Variado                      |
| P02 | 22   | Alta                              | Shooters                     |
| P03 | 15   | Media                             | Shooters                     |
| P04 | 21   | Alta                              | RPG                          |
| P05 | 40   | Baja                              | Ninguno (no suele jugar)     |
| P06 | 7    | Baja                              | Juegos móviles infantiles    |
| P07 | 28   | Alta                              | Metroidvania / indies        |
| P08 | 35   | Media                             | Estrategia (PC)              |
| P09 | 19   | Media                             | Solo móvil (battle royale)   |
| P10 | 24   | Alta                              | Plataformas de precisión     |
| P11 | 45   | Baja                              | Ninguno                      |
| P12 | 16   | Media                             | Shooters / battle royale     |
| P13 | 30   | Media                             | RPG / JRPG                   |
| P14 | 12   | Media                             | Minecraft / Roblox           |
| P15 | 26   | Alta                              | Souls-like / acción          |
| P16 | 52   | Baja                              | Juegos de mesa digitales     |
| P17 | 23   | Media                             | Solo móvil (MOBA)            |
| P18 | 18   | Alta                              | Indies variados              |
| P19 | 33   | Media                             | Plataformas clásicas (retro) |
| P20 | 21   | Alta                              | Variado                      |

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

| Criterio                                              | Promedio (1–5) | Observaciones                                                                                                                                                                               |
| ----------------------------------------------------- | :------------: | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Facilidad para aprender a jugar                       |      3,6       | Los perfiles con experiencia dominaron el movimiento en minutos; los de experiencia baja (P05, P06, P11, P16) necesitaron la hoja de controles y no descubrieron el wall jump por sí solos. |
| Respuesta de los controles                            |      4,1       | Bien valorada en general; el coyote time y el buffer de salto pasan desapercibidos pero "se siente justo" (P10, P15).                                                                       |
| Precisión de los movimientos (salto, dash, wall jump) |      3,9       | Salto variable muy bien recibido. El wall jump costó a los perfiles baja/móvil; el cooldown del dash se percibe largo al usarlo como ataque.                                                |
| Dificultad adecuada                                   |      2,9       | Punto más débil: adecuada para perfiles alta, excesiva para baja. Los 4 abandonos registrados fueron todos de experiencia baja.                                                             |
| Diversión                                             |      3,8       | El parkour y el combate con dash fueron lo más citado; la frustración vs. enemigos a distancia resta puntos.                                                                                |

### 4.2 Interfaz de usuario

| Criterio                                                                            | Promedio (1–5) | Observaciones                                                                                                                 |
| ----------------------------------------------------------------------------------- | :------------: | ----------------------------------------------------------------------------------------------------------------------------- |
| Claridad de los menús (principal y pausa)                                           |      4,4       | Sin problemas; todos completaron E1 y E11 sin ayuda.                                                                          |
| Tamaño de textos                                                                    |      3,9       | P11 y P16 (45 y 52 años) pidieron textos más grandes en el HUD y los avisos.                                                  |
| Comprensión de los iconos (barra de SI por colores, celdas, avisos, barra del jefe) |      3,7       | La barra de SI por colores se entiende; que las fases avanzadas reducen la movilidad no — varios lo atribuyeron a un bug.     |
| Retroalimentación del sistema (pulso de daño, alertas, Game Over)                   |      3,6       | El pulso de daño funciona bien. En la 1.ª tanda el Game Over congelado (BUG-001) hundió esta nota; mejoró tras la corrección. |

### 4.3 Diseño

| Criterio                                              | Promedio (1–5) | Observaciones                                                                                      |
| ----------------------------------------------------- | :------------: | -------------------------------------------------------------------------------------------------- |
| Calidad gráfica (URP, iluminación 2D, parallax)       |      4,3       | El parallax y la iluminación de las cavernas fueron lo más elogiado espontáneamente.               |
| Animaciones (rover y enemigos)                        |      4,2       | El rover destaca; el sprite de muerte que persiste en pantalla (1.ª tanda) fue lo único criticado. |
| Efectos visuales                                      |      3,9       | El freeze-frame del dash gusta; se pidió más feedback visual al golpear al núcleo del Leviatán.    |
| Diseño de niveles (cavernas / instalación alienígena) |      4,0       | Nivel 1 bien valorado; en el Nivel 2 el tramo entre checkpoints se percibe demasiado largo.        |

### 4.4 Audio

| Criterio                                | Promedio (1–5) | Observaciones                                                                                                     |
| --------------------------------------- | :------------: | ----------------------------------------------------------------------------------------------------------------- |
| Música (BGM adaptativa por estados)     |      3,8       | El cambio Exploración → Combate se nota y gusta; a veces la música de combate no regresa a exploración (BUG-007). |
| Efectos de sonido (rover, enemigos, UI) |      3,6       | Faltan SFX de impacto al dañar enemigos con el dash; el jugador duda de si conectó el golpe.                      |
| Balance del volumen                     |      3,4       | La BGM tapa los SFX en combate; sin menú de Opciones no se puede ajustar.                                         |

### 4.5 Rendimiento

| Criterio                       | Promedio (1–5) | Observaciones                                                                                      |
| ------------------------------ | :------------: | -------------------------------------------------------------------------------------------------- |
| Fluidez (FPS)                  |      4,5       | 60 FPS estables en los 5 equipos usados (incl. un portátil con GPU integrada).                     |
| Tiempos de carga entre escenas |      4,3       | Pantallas de carga breves (< 5 s); nadie las percibió como problema.                               |
| Errores o bloqueos             |      2,8       | Lastrado por los bugs críticos de la 1.ª tanda; en la 2.ª tanda solo bugs de severidad media/baja. |
| Consumo de recursos            |      4,2       | Sin calentamiento ni ruido de ventiladores destacable.                                             |

### 4.6 Experiencia del usuario

| Pregunta                              | Respuestas registradas                                                                                                                                                                                                  |
| ------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| ¿Comprendió el objetivo del juego?    | Sí (18/20): avanzar por los niveles y llegar a la salida. P06 (7 años) y P16 creyeron que solo había que "no morir"; sin cinemáticas, nadie captó la historia del rover Opportunity.                                    |
| ¿Se sintió motivado a seguir jugando? | Mixto: 1.ª tanda no (bugs y agotamiento del rover); en la 2.ª tanda 10/16 sí, gracias al build corregido. Los perfiles de experiencia baja perdieron la motivación ante muertes repetidas.                              |
| ¿Qué fue lo más divertido?            | El parkour (plataformas, wall jump) y embestir enemigos con el dash; los perfiles alta destacaron el combate contra el Leviatán.                                                                                        |
| ¿Qué fue lo más frustrante?           | Cooldown del dash muy alto, enemigos a distancia sin poder responder a distancia, checkpoint lejano en el Nivel 2, perder movilidad en fases avanzadas de SI sin entender por qué, y (1.ª tanda) los bloqueos al morir. |

---

## 5. Registro de errores (Bug Report)

Severidad: **Crítica** (impide continuar) · **Alta** (afecta gravemente la jugabilidad) · **Media** (molesta pero con workaround) · **Baja** (cosmética).

| ID      | Descripción                                       | Pasos para reproducir                                                                                              | Resultado esperado                                                                                                        | Resultado obtenido                                                                                                    | Severidad | Evidencia                                                                   |
| ------- | ------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------- | --------- | --------------------------------------------------------------------------- |
| BUG-001 | Fallo en pantalla de Game Over al morir           | Morir en medio de las plataformas móviles o espinas mecánicas.                                                     | Al llegar a 0% de vida, debe desplegarse inmediatamente la pantalla de Game Over.                                         | El Rover muere, pero la pantalla tarda en aparecer o no aparece en absoluto, dejando el juego congelado.              | Crítica   | —                                                                           |
| BUG-002 | Incapacidad de reiniciar el juego tras el bloqueo | Sufrir el bug donde no carga la pantalla de Game Over e intentar forzar el reinicio.                               | El botón de reinicio o el comando correspondiente debe limpiar la escena y recargar desde el menú principal o checkpoint. | El bucle de muerte bloquea el flujo del código y no se puede iniciar ni interactuar al presionar reiniciar.           | Crítica   | —                                                                           |
| BUG-003 | Activación errónea del botón Continuar            | Iniciar el juego por primera vez (sin datos de guardado previos) en el menú principal.                             | El botón de "Continuar" debe aparecer desactivado o bloqueado si no hay una partida previa.                               | El botón está completamente funcional y permite interactuar con él, lo cual corrompe la inicialización.               | Alta      | Corregido S06 T5 (2026-07-11): `Btn_Continuar` se deshabilita sin guardado. |
| BUG-004 | Bloqueo físico bajo plataformas con espinas       | Avanzar por el Nivel 1 y caer o quedar atrapado justo debajo de la plataforma que contiene dos espinas.            | El Rover debe recibir daño por colisión y ser empujado o destruido (activar muerte limpia).                               | El sprite del Rover se queda permanentemente atascado o bugeado bajo la geometría de la plataforma sin salir.         | Alta      | —                                                                           |
| BUG-005 | Fallo crítico del Checkpoint en el Nivel 2        | Morir en el Nivel 2 y reaparecer automáticamente en el último checkpoint alcanzado.                                | El Rover debe revivir en la posición del checkpoint con la barra de vida al 100% y sus estados limpios.                   | El Rover muere instantáneamente al aparecer en el checkpoint porque la barra de vida no se restaura y se queda en 0%. | Crítica   | Corregido antes de la 2.ª tanda (2026-07-11).                               |
| BUG-006 | Proyectil del Centinela atraviesa la geometría    | En el Nivel 2, colocarse tras un muro mientras el Centinela Secundario dispara.                                    | El proyectil debe destruirse al impactar contra muros o plataformas.                                                      | El proyectil atraviesa la pared y golpea al Rover en cobertura; el jugador no entiende de dónde vino el daño.         | Media     | Reportado por P08, P12, P15, P17 y P19.                                     |
| BUG-007 | La música de combate no vuelve a exploración      | Aturdir al Ser Bioluminiscente con el escaneo y alejarse de la zona sin destruirlo.                                | Al salir de combate, la BGM debe volver al estado Exploración.                                                            | La música de combate queda sonando en bucle hasta el siguiente cambio de escena.                                      | Baja      | Reportado por P07 y P13.                                                    |
| BUG-008 | Barra del jefe visible fuera de la arena          | Morir durante el combate contra el Leviatán y reaparecer en el checkpoint previo.                                  | Al morir, la barra del jefe debe ocultarse (`HideBossBar`) hasta reingresar a la arena.                                   | La barra del jefe permanece en el HUD con la vida congelada mientras se recorre el nivel.                             | Media     | Reportado por P05, P10 y P14.                                               |
| BUG-009 | Atasco en esquina al encadenar wall jumps         | En el pozo estrecho del Nivel 1, encadenar wall jumps rápidos entre los dos muros hasta tocar la esquina superior. | El Rover debe deslizarse o separarse de la pared y seguir controlable.                                                    | El sprite queda enganchado vibrando en la esquina; solo un dash lo libera.                                            | Media     | Reproducido por P10 y P15 (perfiles de plataformas).                        |
| BUG-010 | La celda de energía se consume con SI casi llena  | Con la SI sobre 80, presionar `Q` teniendo una celda en reserva.                                                   | El juego debería impedir el uso o avisar que se desperdiciará la reparación.                                              | La celda se consume, el excedente de los 20 de SI se pierde y no hay ningún aviso.                                    | Baja      | Reportado por P06, P09 y P16 (la usaron por accidente).                     |

---

## 6. Métricas

### 6.1 Por jugador

| ID  | Tiempo Nivel 1 | Tiempo Nivel 2 | N.º de muertes | N.º de intentos vs. Leviatán | ¿Completó el prototipo? |      ¿Abandonó?       |
| --- | :------------: | :------------: | :------------: | :--------------------------: | :---------------------: | :-------------------: |
| P01 |     9 min      |     12 min     |       20       |              14              |           Sí            |          No           |
| P02 |     11 min     |     13 min     |       25       |              12              |           Sí            |          No           |
| P03 |     30 min     |     10 min     |       30       |              17              |           Sí            |          No           |
| P04 |     10 min     |     12 min     |       29       |              19              |           Sí            |          No           |
| P05 |     28 min     |       —        |       26       |              6               |           No            |     Sí (jefe N1)      |
| P06 |     18 min     |       —        |       19       |              —               |           No            | Sí (zona de peligros) |
| P07 |     12 min     |     14 min     |       9        |              3               |           Sí            |          No           |
| P08 |     17 min     |     18 min     |       15       |              6               |           Sí            |          No           |
| P09 |     20 min     |     16 min     |       22       |              8               |          No \*          |          No           |
| P10 |     10 min     |     11 min     |       6        |              2               |           Sí            |          No           |
| P11 |     32 min     |       —        |       31       |              9               |           No            |     Sí (jefe N1)      |
| P12 |     15 min     |     17 min     |       18       |              7               |          No \*          |          No           |
| P13 |     18 min     |     19 min     |       17       |              5               |           Sí            |          No           |
| P14 |     22 min     |     15 min     |       24       |              10              |          No \*          |          No           |
| P15 |     11 min     |     12 min     |       8        |              2               |           Sí            |          No           |
| P16 |     30 min     |       —        |       27       |              4               |           No            |     Sí (jefe N1)      |
| P17 |     19 min     |     20 min     |       20       |              9               |          No \*          |          No           |
| P18 |     13 min     |     14 min     |       10       |              4               |           Sí            |          No           |
| P19 |     16 min     |     17 min     |       14       |              5               |           Sí            |          No           |
| P20 |     12 min     |     15 min     |       11       |              3               |           Sí            |          No           |

\* No abandonaron: la sesión se cerró al alcanzar el límite de 45 min sin superar al Centinela Principal.

### 6.2 Resumen global

| Métrica                                        | Valor de referencia (ajustado al prototipo) |                   Valor obtenido                   |
| ---------------------------------------------- | :-----------------------------------------: | :------------------------------------------------: |
| Tiempo promedio de partida completa            |         30–40 min (GDD: 60–85 min)          | 29,4 min por sesión · 28,0 min quienes completaron |
| Promedio de muertes por jugador                |                      —                      |      19,1 (1.ª tanda: 26,0 · 2.ª tanda: 17,3)      |
| Total de errores encontrados                   |                      —                      |          61 reportes → 10 defectos únicos          |
| Jugadores que derrotaron al Leviatán (Nivel 1) |                      —                      |                      16 / 20                       |
| Jugadores que completaron el Nivel 2 (salida)  |                      —                      |                      12 / 20                       |
| Jugadores que abandonaron antes de terminar    |                      —                      |         4 / 20 (todos de experiencia baja)         |

---

## 7. Encuesta de satisfacción

### Califique del 1 al 5

| Afirmación                          | Promedio (1–5) |
| ----------------------------------- | :------------: |
| El juego fue entretenido            |      3,9       |
| Los controles fueron intuitivos     |      4,0       |
| El nivel de dificultad fue adecuado |      3,0       |
| La interfaz fue clara               |      4,2       |
| Me gustaría volver a jugar          |      4,1       |

Los promedios consolidan las 20 sesiones. La dispersión por perfil es marcada: los jugadores de experiencia alta promediaron 4,4 en "volver a jugar", mientras que los de experiencia baja promediaron 2,8 y calificaron la dificultad con 1,9.

### Preguntas abiertas

| Pregunta                          | Respuestas más frecuentes                                                                                                                                                                                |
| --------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| ¿Qué fue lo que más le gustó?     | El diseño del rover, sus sprites y animaciones; el parkour; la ambientación de las cavernas con el parallax; el combate contra el Leviatán (perfiles alta).                                              |
| ¿Qué mejoraría?                   | El balance del dash (cooldown); la dificultad para jugadores nuevos (modo fácil o tutorial); los checkpoints del Nivel 2; el volumen relativo música/efectos.                                            |
| ¿Qué errores encontró?            | 1.ª tanda: Game Over congelado, checkpoint del Nivel 2, sprite de muerte persistente. 2.ª tanda: proyectiles que atraviesan paredes, barra del jefe que no desaparece, atasco en esquinas con wall jump. |
| ¿Qué nuevas funciones propondría? | Un ataque a distancia; reasignación de teclas y soporte completo de gamepad (pedido por los 3 jugadores de perfil móvil); minimapa; tutorial inicial de wall jump y escaneo.                             |

---

## 8. Análisis de resultados

### 8.1 Resumen de los principales problemas detectados

1. **Dificultad desbalanceada por perfil.** Los 4 abandonos corresponden a jugadores de experiencia baja; ninguno superó al Leviatán. Los perfiles alta completaron el prototipo con 6–11 muertes, dentro de lo esperado. El DDA (`AIManager`, ×0.75–×1.25) está implementado pero desactivado (`ddaEnabled = false`) — no se probó.
2. **Flujo de muerte frágil (1.ª tanda).** BUG-001/002/005 bloqueaban la sesión al morir y obligaban a reiniciar el build; tras corregirlos, el promedio de muertes bajó de 26,0 a 17,3 y los intentos contra el Leviatán de 15,5 a 5,5 entre tandas.
3. **Combate limitado frente a enemigos a distancia.** El dash como única forma de ataque obliga a acercarse a Centinelas y drones que disparan; es la fuente de frustración más citada en el Nivel 2.
4. **Checkpoint lejano en el Nivel 2** alarga el castigo por muerte y amplifica el punto anterior.

### 8.2 Errores más frecuentes

| Defecto                                                           | Reportes |
| ----------------------------------------------------------------- | :------: |
| Game Over congelado / sin reinicio (BUG-001/002, solo 1.ª tanda)  |    14    |
| Checkpoint del Nivel 2 sin restaurar SI (BUG-005, solo 1.ª tanda) |    9     |
| Atascos contra geometría (BUG-004, BUG-009)                       |    8     |
| Proyectil del Centinela atraviesa muros (BUG-006)                 |    6     |
| Barra del jefe persistente (BUG-008)                              |    3     |

### 8.3 Problemas de usabilidad

- El **wall jump no se descubre solo**: los 4 perfiles de experiencia baja necesitaron que se les leyera la hoja de controles; P06 (7 años) nunca lo ejecutó de forma consistente.
- La **pérdida de movilidad por fases de SI** se interpreta como un bug ("el rover se dañó de verdad", P09): falta un aviso explícito en el HUD al cambiar de fase.
- **Mantener `E` para escanear** no es evidente; 12/20 pulsaron la tecla una sola vez y creyeron que no funcionaba.
- Los jugadores de **perfil móvil** (P09, P17) buscaron atacar con clic/tap y pidieron soporte completo de gamepad.
- **Textos del HUD pequeños** para los evaluadores de 45+ años (P11, P16).

### 8.4 Aspectos mejor valorados

- Arte y animaciones del rover, iluminación y parallax de las cavernas (4,2–4,3).
- Respuesta y "peso" del control en perfiles con experiencia (coyote time y salto variable pasan la prueba).
- Rendimiento: 60 FPS estables y cargas rápidas en todo el hardware probado.
- Claridad de menús: E1 y E11 los completaron los 20 jugadores sin ayuda.

### 8.5 Priorización de mejoras

1. **Alta** — Corregir colisiones y atascos (BUG-004, BUG-006, BUG-009) y la barra del jefe (BUG-008).
2. **Alta** — Balancear dificultad: activar y calibrar el DDA, revisar cooldown del dash y acercar el checkpoint del Nivel 2.
3. **Media** — Tutorialización mínima en el Nivel 1 (carteles de wall jump y escaneo) y aviso de cambio de fase de SI.
4. **Media** — Evaluar respuesta a distancia para el rover o rediseñar los encuentros contra Centinelas.
5. **Baja** — Balance de mezcla de audio, SFX de impacto del dash, tamaño de textos, BUG-007 y BUG-010.

---

## 9. Plan de mejoras

| Problema                                                    | Prioridad | Solución propuesta                                                                                                                    | Responsable    |
| ----------------------------------------------------------- | :-------: | ------------------------------------------------------------------------------------------------------------------------------------- | -------------- |
| Atascos del rover contra geometría (BUG-004, BUG-009)       |   Alta    | Revisar colliders de plataformas con espinas y añadir separación forzada en esquinas del controlador cinemático                       | Edison         |
| Proyectiles atraviesan muros (BUG-006)                      |   Alta    | Añadir capa de entorno a la detección de colisión de `CentinelaProjectile` y destruir al impactar                                     | Angelo         |
| Dificultad excesiva para perfiles nuevos                    |   Alta    | Activar y calibrar el DDA del `AIManager`; reducir el cooldown del dash en `RoverStatsSO`; añadir checkpoint intermedio en el Nivel 2 | Edison / Kelly |
| Mecánicas no descubribles (wall jump, escaneo, fases de SI) |   Media   | Carteles de tutorial en el Nivel 1 y aviso en HUD al cambiar de fase de degradación                                                   | Doris / Kevin  |
| Audio: BGM tapa SFX y falta feedback de impacto             |   Baja    | Ajustar niveles del mixer de 4 buses; añadir SFX de impacto de dash; corregir retorno a estado Exploración (BUG-007)                  | Stalin         |

---

## 10. Conclusiones

**¿La versión está lista para una versión final?**

No. La estabilidad mejoró de forma notable entre tandas (el promedio de muertes por jugador bajó de 26,0 a 17,3 tras corregir los bugs críticos, sin más bloqueos de sesión), pero persisten defectos de severidad media que rompen la experiencia (atascos, proyectiles a través de muros) y un desbalance de dificultad que excluye a los jugadores sin experiencia. Además faltan las cinemáticas, el lore del escaneo y la secuencia de victoria del jefe final.

**¿Qué aspectos deben corregirse obligatoriamente?**

1. Los atascos del rover contra la geometría (BUG-004, BUG-009) — únicos defectos que aún pueden bloquear una partida.
2. El proyectil del Centinela que atraviesa muros (BUG-006), que hace percibir el daño como injusto.
3. El balance de dificultad para perfiles de experiencia baja (DDA, cooldown del dash, checkpoint del Nivel 2): los 4 abandonos provienen de este grupo.
4. La secuencia de victoria del Centinela Principal, sin la cual el final del prototipo se siente inconcluso.

**¿Qué aprendió el equipo durante la prueba beta?**

- Probar con perfiles sin experiencia (7 a 52 años) reveló problemas invisibles para el equipo: mecánicas que "se descubren solas" para un jugador habitual (wall jump, mantener `E`) no lo son para el resto.
- Corregir los bugs críticos **entre tandas** permitió medir su impacto real: con el mismo diseño de nivel, el promedio de muertes bajó de 26,0 a 17,3 y los intentos contra el Leviatán de 15,5 a 5,5.
- Las métricas por jugador (muertes, intentos, abandono) localizan los picos de frustración con más precisión que la encuesta; conviene automatizar su registro en el build.
- La duración ajustada del prototipo (29,4 min de promedio frente a los 30–40 min estimados) valida el alcance de 2 niveles para esta fase.

---

## 11. Anexos — Evidencias fotográficas

Una fotografía o captura por sujeto de prueba durante su sesión (archivos en `Docs/Evidencias/`, nombrados por ID de jugador).

| ID  | Tanda | Evidencia                         |
| --- | :---: | --------------------------------- |
| P01 |  1.ª  | ![Sesión P01](Evidencias/P01.jpg) |
| P02 |  1.ª  | ![Sesión P02](Evidencias/P02.jpg) |
| P03 |  1.ª  | ![Sesión P03](Evidencias/P03.jpg) |
| P04 |  1.ª  | ![Sesión P04](Evidencias/P04.jpg) |
| P05 |  2.ª  | ![Sesión P05](Evidencias/P05.jpg) |
| P06 |  2.ª  | ![Sesión P06](Evidencias/P06.jpg) |
| P07 |  2.ª  | ![Sesión P07](Evidencias/P07.jpg) |
| P08 |  2.ª  | ![Sesión P08](Evidencias/P08.jpg) |
| P09 |  2.ª  | ![Sesión P09](Evidencias/P09.jpg) |
| P10 |  2.ª  | ![Sesión P10](Evidencias/P10.jpg) |
| P11 |  2.ª  | ![Sesión P11](Evidencias/P11.jpg) |
| P12 |  2.ª  | ![Sesión P12](Evidencias/P12.jpg) |
| P13 |  2.ª  | ![Sesión P13](Evidencias/P13.jpg) |
| P14 |  2.ª  | ![Sesión P14](Evidencias/P14.jpg) |
| P15 |  2.ª  | ![Sesión P15](Evidencias/P15.jpg) |
| P16 |  2.ª  | ![Sesión P16](Evidencias/P16.jpg) |
| P17 |  2.ª  | ![Sesión P17](Evidencias/P17.jpg) |
| P18 |  2.ª  | ![Sesión P18](Evidencias/P18.jpg) |
| P19 |  2.ª  | ![Sesión P19](Evidencias/P19.jpg) |
| P20 |  2.ª  | ![Sesión P20](Evidencias/P20.jpg) |

 

 

UNIVERSIDAD CENTRAL DEL ECUADOR 

Facultad de Ingeniería y Ciencias Aplicadas · Carrera de Computación 

Noveno Semestre · Proyecto de Video Juegos · Cód. TGP09BFT01 

 

 

OPPORTUNITY 

RED DUST LEGACY 

 

 

G A M E   D E S I G N   D O C U M E N T 

 

 

Género 

2.5D Metroidvania / Sci-Fi Platformer 

Motor 

Unity Unity 6.4 (6000.4.8f1) — Universal Render Pipeline (URP) 

Plataforma 

PC — Windows 10/11 

Alcance 

Prototipo funcional: 2 niveles + menú + cinemáticas 

Equipo 

Stalin Acurio, Edison Enríquez, Kelly Ledesma, Ángelo Silva, Doris Chicaiza, Kevin Celi 

Duración estimada (1.ª partida) 

60–85 minutos 

Asignatura 

Proyecto de Video Juegos | Cód. TGP09BFT01 

Período 

2026 – 2026 

Versión 

V4.0 — Documento integrado y consolidado 

PARTE 1 

Visión del proyecto 

¿Qué es este juego, quién lo hace y con qué objetivo? 

1.  Concepto de alto nivel 

Opportunity: Red Dust Legacy es un videojuego de plataformas 2.5D de exploración y aventura basado en la historia real del rover Mars Exploration Rover Opportunity de la NASA (2004–2019). El proyecto fusiona mecánicas de Metroidvania con datos científicos verificables y una narrativa construida sobre un principio de ironía dramática: el jugador conoce el destino del protagonista desde el inicio, y esa información convierte cada acción en un acto de valentía involuntaria. 

Tagline del proyecto 

"Opportunity: Red Dust Legacy es un videojuego donde el protagonista más valiente no es el más poderoso — 

es el que siguió haciendo su trabajo cuando no había nadie para verlo." 

1.1 Público Objetivo 

Jugadores de 16 a 30 años con experiencia media-alta en plataformeros de precisión. Fans de Metroidvanias atmosféricos (Hollow Knight, Metroid Dread). Receptivos a narrativa emotiva sin diálogo explícito. Valoración adicional en perfiles con interés en ciencia espacial o historia de la exploración marciana. 

1.2 Génesis del concepto 

El punto de partida del diseño es un hecho histórico: basado en la historia real del rover Mars Exploration Rover Opportunity (NASA, 2004–2018), diseñado para operar 90 días, funcionó durante 5.111 soles marcianos —aproximadamente 15 años—, recorrió 45,16 kilómetros y realizó descubrimientos que transformaron la comprensión científica de Marte. Su última señal interpretada fue «My battery is low and it's getting dark». La NASA declaró la misión concluida el 13 de febrero de 2019, tras enviar 1.034 señales sin respuesta. 

Este material no es fuente de inspiración: es el argumento central del juego. La pregunta de diseño que guía cada decisión es: ¿cómo traducir esa historia a mecánicas de juego sin traicionarla? 

 

Ilustración 1 Rover Opportunity NASA 

1.3 Unique Selling Point 

Opportunity: Red Dust Legacy es único por tres razones no replicables en combinación: 

 

Datos científicos NASA como mecánica, no como decoración. Los objetos escaneables tienen fuentes primarias verificables. El jugador aprende geología marciana real mientras juega. 

 

Degradación como narrativa. Los estados de deterioro del hardware no son solo penalizaciones: son el arco emocional del personaje. El rover que apenas se mueve al final del juego cuenta una historia que el texto no necesita explicar. 

 

Ironía dramática como motor emocional central. El jugador conoce el destino de Opportunity antes de empezar. Cada acción del rover se convierte en un acto de valentía involuntaria frente a ese conocimiento. 

 

Ninguno de estos tres elementos es nuevo por separado. Los tres juntos no existen en ningún otro videojuego publicado. 

1.4 Pilares de diseño 

Toda decisión de diseño —mecánica, narrativa o artística— debe respetar al menos uno de los tres pilares fundacionales del proyecto.  

Pilar 

Definición 

Prueba de aceptación 

01.  Exploración con propósito 

Cada sala recompensa la curiosidad. Escanear objetos revela datos científicos reales. El jugador quiere explorar, no solo avanzar. 

¿Existe una razón narrativa o mecánica para entrar en esta sala? Si la respuesta es no, la sala no debería existir. 

02. Adaptación tecnológica 

Los upgrades son reparaciones con lógica real. Cada mejora tiene justificación narrativa y científica dentro del universo del juego. 

¿El jugador puede explicar de dónde vino este upgrade usando el lore del juego? Si no puede, el upgrade no está diseñado correctamente. 

03. Tensión sin frustración 

El peligro es real pero siempre legible. La IA telegrafía sus acciones. El jugador nunca muere por información oculta. 

¿El jugador que muere entiende por qué murió? Si la respuesta es 'el enemigo apareció de la nada', el diseño viola este pilar. 

1.5 Referentes de diseño 

Los siguientes juegos son referentes directos de mecánicas, tono y estructura. Se distingue entre referentes de diseño de juego y referentes narrativos o cinematográficos para evitar confusión categórica. 

1.5.1 Referentes de diseño de juego 

Título 

Estudio / Año 

Elemento referenciado 

Metroid Dread 

Nintendo EPD / MercurySteam, 2021 

Movimiento fluido de plataformero. Sensación de aislamiento. IA tensa con comportamiento telegráfico. Estructura de progresión Metroidvania. 

Hollow Knight 

Team Cherry, 2017 

Atmósfera melancólica construida desde el arte y el audio. Lore ambiental sin exposición textual. Enemigos con comportamiento consistente que el jugador aprende a leer. 

Celeste 

Extremely OK Games, 2018 

Game feel del movimiento: responsividad, coyote time, salto preciso. Historia emocional transmitida sin texto excesivo. Curva de dificultad construida sobre la comprensión progresiva del jugador. 

1.5.2 Referentes narrativos y cinematográficos 

Obra 

Tipo 

Elemento referenciado 

Wall-E (primer acto) 

Película — Pixar, 2008 

Ternura en la rutina. Soledad representada como estado sagrado, no deprimente. Personaje mudo cuya emoción se construye desde la acción, no el diálogo. 

1.6 Tabla de roles y responsabilidades 

Rol 

Responsabilidades principales 

Entregable clave para evaluación 

Gameplay Programmer 

Implementación del PlayerController. Sistema de física 2.5D. Coyote time, jump buffer y parámetros de movimiento. Integración de upgrades activos. 

PlayerController.cs funcional y completamente documentado. Movimiento responsivo validado en playtesting. 

AI Programmer 

Diseño e implementación de FSM por enemigo. Ciclo Sensar-Pensar-Actuar para drones. Sistema de flanqueo grupal. DDA centralizado. Boss con tres fases. 

AIManager.cs y scripts individuales de cada NPC. Comportamientos verificables e independientes del nivel. 

Level Designer 

Construcción de los dos niveles en Unity con tilemap. Distribución de enemigos, coleccionables y puzzles. Beat maps jugables con flujo verificado. 

Escenas Level01 y Level02 jugables de principio a fin, con progresión narrativa y mecánica correcta. 

Artist & Animator 

Spritesheet del rover y todos los enemigos. Animator Controller con estados definidos en este documento. VFX de partículas y efectos de UI. 

Spritesheet completa + Animator Controller funcional en Unity. Paleta de colores aplicada consistentemente. 

Technical Director 

Arquitectura general del proyecto. Audio adaptativo y sistema de Unity Audio Mixer. HUD diegético. Gestión de la integración semanal y build final. 

Build ejecutable sin errores críticos. Escenas integradas. AudioManager funcional con estados de música. 

 

PARTE 2 

Mecánicas de juego 

Loop de juego · controles · progresión · condiciones de victoria/derrota 

2. Mecánicas de juego 

2.1 Loop de juego 

El loop de juego de Opportunity: Red Dust Legacy opera en tres niveles de granularidad: el microciclo (momento a momento), el mesociclo (sala a sala) y el macrociclo (nivel completo). Comprender los tres niveles es necesario para implementar y evaluar el diseño correctamente. 

Microciclo — momento a momento (3–15 segundos) 

Es la unidad mínima de gameplay. Ocurre continuamente durante la exploración y el combate. 

Paso 

Acción del jugador 

Respuesta del sistema 

1 

Moverse por el entorno usando carrera, salto y dash. 

El rover se desplaza con física responsiva. La cámara sigue al personaje. Los parallax de fondo se actualizan por capa. 

2 

Detectar un objeto escaneable o un enemigo en rango. 

El ping pasivo (post-upgrade) resalta el objeto. La IA del enemigo entra en estado Alert si detecta al rover. 

3 

Decidir: escanear, evitar, o combatir. 

Escanear congela el movimiento 0.5 s y muestra el texto de lore en el HUD. Evitar requiere uso de dash o escalada. Combatir activa la IA en estado Chase. 

4 

Recibir daño o resolver la situación. 

La SI disminuye. El HUD actualiza color de barra. Si se cruza un umbral de fase, se activan efectos visuales y de audio correspondientes. 

 

Mesociclo — sala a sala (2–8 minutos) 

Define el ritmo de una zona completa del nivel. Cada sala tiene un propósito: exploración, combate, puzzle, lore, upgrade o transición. 

Paso 

Fase del mesociclo 

Elementos involucrados 

1 

Entrar a una sala nueva. 

El minimapa actualiza el área explorada. Si hay enemigos, la música transiciona a TENSION. 

2 

Leer el entorno (diseño telegráfico). 

La posición de los enemigos, las plataformas y los objetos luminosos comunican el layout sin texto. 

3 

Ejecutar la estrategia elegida. 

Combate, evasión, escaneo. El DDA monitorea el rendimiento y ajusta parámetros si es necesario. 

4 

Resolver la sala y obtener la recompensa. 

Recompensa posible: celda de energía, objeto de lore, upgrade, checkpoint, acceso a sala siguiente. 

5 

Avanzar a la sala siguiente con estado acumulado. 

La SI no se restaura entre salas. El daño acumulado es permanente hasta usar una celda. 

Macrociclo — nivel completo (25–50 minutos) 

Define la progresión completa de un nivel: de la entrada al boss, con su curva de dificultad, degradación del hardware y desbloqueo de upgrades. 

Fase del macrociclo 

Descripción 

Exploración inicial 

El jugador llega con hardware en buen estado. Aprende el entorno y las mecánicas base. Sin presión de degradación aún. 

Primer upgrade / descubrimiento de lore 

Una sala opcional rompe la ruta lineal. El upgrade desbloquea rutas previas inaccesibles. El lore introduce el worldbuilding. 

Zona de tensión 

Los enemigos aumentan en número y variedad. La SI empieza a bajar. El jugador gestiona celdas de energía con más cuidado. 

Checkpoint pre-boss 

Una sala de respiro antes del encuentro final. Celda de energía disponible. El boss es visible a través del entorno antes de entrar. 

Boss y transición 

El boss concentra todas las mecánicas aprendidas. Al derrotarlo, la cinemática de transición activa el siguiente acto narrativo. 

2.2 Tabla de controles 

Opportunity: Red Dust Legacy acepta dos esquemas de control simultáneamente: teclado + ratón y mando de juego (gamepad). El InputSystem de Unity gestiona el mapeo de acciones — el jugador puede reasignar teclas desde el menú principal excepto las marcadas como no reasignables. 

Acción 

Teclado 

Gamepad (Xbox / PS) 

Notas 

Moverse (izquierda / derecha) 

A / D  o  ← / → 

Stick izquierdo 

Movimiento analógico en gamepad; digital en teclado. 

Salto completo 

Espacio (mantener) 

A / X (mantener) 

Mantener el botón alcanza altura máxima: 6,4 u. 

Salto corto 

Soltar Espacio antes del pico 

Soltar A / X antes del pico 

Altura reducida ~2,4 u. Ver §2.5 (jumpCutMultiplier). 

Coyote jump 

Espacio hasta 0,12 s tras abandonar borde 

A / X hasta 0,12 s tras borde 

Automático — no requiere acción adicional del jugador. 

Dash 

Shift izquierdo 

B / O 

Dirección: input horizontal activo o hacia adelante. 

Escalar pared 

E (mantener en contacto con pared) 

RB / R1 (mantener) 

Requiere upgrade Rueda Reforzada. Sin el upgrade, el botón no tiene efecto. 

Wall jump 

Espacio mientras se escala 

A / X mientras se escala 

Requiere upgrade Rueda Reforzada. Desactiva input horizontal 0,15 s. 

Escanear 

F (mantener) 

Y / △ (mantener) 

El texto de lore aparece mientras se mantiene. Radio: 3 u base, 5 u con upgrade. 

Usar celda de energía 

Q 

LB / L1 

No se activa si no hay celdas disponibles. No reasignable. 

Pausa / Menú 

Esc 

Start / Options 

No reasignable. 

Controles no reasignables 

Las siguientes acciones están bloqueadas para evitar configuraciones que rompan la accesibilidad o generen conflictos de sistema: 

Usar celda de energía (Q / LB): acción crítica de supervivencia que debe estar siempre accesible sin ambigüedad. 

Pausa / Menú (Esc / Start): control del sistema operativo del juego, no del rover. 

2.3 Condiciones de victoria y derrota 

Este juego no tiene puntuación ni clasificación. La victoria y la derrota están definidas por el avance narrativo y el estado del hardware del rover, no por métricas de rendimiento. 

2.3.1 Condición de victoria 

La condición de victoria se cumple al encadenar los siguientes dos eventos en el orden indicado: 

Derrotar al Centinela Principal en el núcleo del Relicto (Nivel 2, sala final). 

Ejecutar la secuencia de transmisión: Opportunity activa el transmisor del Relicto y envía al menos 1 fragmento de datos antes de que la energía llegue a 0%. 

Ambos eventos deben ocurrir en la misma sesión sin reiniciar el juego. No es posible ganar sin enfrentar al boss. No es posible ganar si la transmisión es interrumpida antes de enviar al menos 1 fragmento. 

Parámetro de victoria 

Valor 

Evento desencadenante 

HP del Centinela Principal llega a 0 en cualquier fase activa. 

Mínimo para victoria narrativa 

1 fragmento transmitido de 891 posibles. 

Resultado óptimo (sin impacto en victoria) 

341 fragmentos transmitidos — coherente con la historia real de la misión. 

Estado del rover al activar la transmisión 

SI = 1% (Fase 6 — Extinción). Es narrativamente inevitable. 

Escena activada 

Secuencia del final: 7 escenas, ~82 segundos. Ver §3.2.3. 

2.3.2 Condición de derrota — muerte del rover 

El rover muere cuando la Integridad Estructural (SI) llega a 0. Este juego no tiene Game Over definitivo: la muerte es parte del sistema de aprendizaje. 

Parámetro de derrota 

Comportamiento del sistema 

Trigger de muerte 

SI = 0. La animación Death del rover se reproduce completa (12 frames, no interrumpible). 

Punto de reaparición 

Último checkpoint activado. El checkpoint registra la posición exacta y la SI del momento de activación. 

SI al revivir 

La SI NO se restaura. El jugador reaparece con la misma SI que tenía cuando activó el checkpoint. 

Estado del nivel al revivir 

Enemigos derrotados: no reaparecen. Celdas de energía recogidas: no reaparecen. Objetos escaneados: permanecen escaneados. 

Penalización adicional 

Ninguna. No hay pérdida de progreso ni de upgrades. 

Game Over definitivo 

Solo si el jugador muere sin ningún checkpoint activo (imposible en condiciones normales de juego — el primer checkpoint del Nivel 1 se activa automáticamente al iniciar). 

2.4 Mecánicas activas del jugador 

Carrera 

El rover acelera desde 0 hasta maxRunSpeed en 0,30 segundos. Al soltar el input horizontal, frena hasta detenerse en 0,19 segundos. En aire, la aceleración y la deceleración se reducen al 65% para dar sensación de inercia sin perder control. La dirección puede invertirse durante el aire sin penalización adicional. 

Salto 

El salto tiene dos alturas: si el jugador mantiene el botón, alcanza la altura completa de 6,4 u. Si lo suelta antes de la mitad de la trayectoria ascendente, la velocidad vertical se multiplica por jumpCutMultiplier (0,5), produciendo un salto corto de aproximadamente 2,4 u. El coyote time de 0,12 s permite saltar hasta 0,12 s después de abandonar un borde. El jump buffer de 0,10 s registra el input de salto hasta 0,10 s antes de tocar el suelo. 

Dash 

El dash es horizontal en la dirección del input, o hacia adelante si no hay input horizontal. Funciona tanto en suelo como en aire, con un máximo de un dash aéreo antes de tocar suelo. Durante el dash, el gravity scale se desactiva completamente — el rover se mueve en línea recta. El dashSleepTime de 0,028 s congela los frames al inicio del dash para acentuar la sensación de aceleración brusca. 

Escalada de paredes 

El rover puede aferrarse a paredes planas al mantener presionado el botón de escalada en contacto con una superficie vertical. Desliza lentamente hacia abajo mientras está aferrado. Puede saltar desde la pared con un wall jump que aplica una fuerza combinada horizontal (hacia el interior) y vertical. El wall jump desactiva el input horizontal durante 0,15 s para evitar que el jugador se reaferre inmediatamente. 

2.5 Tabla maestra de movimiento 

Todos los valores listados a continuación son los definitivos para la implementación del PlayerController. Las fórmulas de verificación aparecen en la columna de notas para facilitar el testing. 

Parámetro 

Valor 

Unidad 

Nota de verificación / fórmula 

maxRunSpeed 

7,5 

u/s 

Velocidad horizontal máxima en suelo 

runAcceleration 

25,0 

u/s² 

Tiempo hasta vel. máx: 7,5 ÷ 25 = 0,30 s 

runDecceleration 

40,0 

u/s² 

Tiempo hasta detenerse: 7,5 ÷ 40 = 0,19 s 

accelInAir 

65% 

% de runAcceleration 

Control aéreo reducido — mayor a esto se siente flotante 

deccelInAir 

65% 

% de runDecceleration 

Resistencia aérea reducida 

jumpForce 

16,0 

u/s 

Velocidad inicial vertical al saltar 

jumpHeight (cálculo) 

6,4 

u 

jumpForce² ÷ (2 × |gravity|) = 256 ÷ 40 = 6,4 u 

jumpCutMultiplier 

0,5 

— 

Salto corto: velocidad vertical × 0,5 al soltar el botón 

jumpCoyoteTime 

0,12 

s 

Ventana de salto tras abandonar plataforma 

jumpBufferTime 

0,10 

s 

Ventana de salto anticipado antes de tocar suelo 

fallGravityMultiplier 

1,8 

— 

Gravedad en caída = −20 × 1,8 = −36 u/s² 

fastFallGravityMult 

2,5 

— 

Al mantener ↓ durante la caída 

maxFallSpeed 

−26,0 

u/s 

Velocidad terminal de caída 

dashSpeed 

28,0 

u/s 

Velocidad durante el dash 

dashTime 

0,18 

s 

Duración del dash — distancia real: 28 × 0,18 = 5,04 u 

dashDistance (nominal) 

5,0 

u 

Valor de referencia de diseño (ver nota) 

dashCooldown 

1,2 

s 

Tiempo entre dashes disponibles 

dashSleepTime 

0,028 

s 

Congelamiento de frames al inicio del dash (game feel) 

dashesInAir 

1 

— 

Número de dashes disponibles sin tocar suelo 

2.6 Sistema de Integridad Estructural y degradación 

El sistema de Integridad Estructural (SI) es el núcleo emocional del juego. Cada punto de daño que pierde el rover es un paso hacia el silencio. Esta sección consolida en una tabla maestra única todos los efectos de cada fase — físicos, de daño, de HUD y de audio.  

2.6.1 Parámetros base del sistema de SI 

SI máxima 

100 puntos 

SI al inicio del juego 

74 — consecuencia histórica de los 736 soles de silencio y la tormenta 

SI mínima funcional 

1 — por debajo de este valor el juego termina 

Los checkpoints NO restauran SI 

Diseño intencional: el daño acumulado es permanente 

Las celdas de energía restauran SI 

+20 SI por celda; máximo de 2 celdas en reserva 

Fuente de daño principal 

Ataques de enemigos, caídas de altura > 3 u, colisión con objetos ambientales 

2.6.2 Tabla maestra de degradación 

Cada fila corresponde a una fase de degradación. Los efectos son acumulativos: entrar en la Fase 4 significa que todos los efectos de las Fases 1, 2 y 3 siguen activos. La columna de referencia visual indica el estado del arte del rover que el Artist debe producir para cada fase. 

Fase 

SI restante 

Efectos físicos (PlayerController) 

Efectos de HUD y audio 

Referencia visual del rover 

1 — NOMINAL 

100 – 74% 

Sin modificadores. Movimiento y salto a valores base. 

Barra de SI verde. Sin alertas. Música de exploración. 

Rover en estado original: sin daño visible. Arte base. 

2 — DESGASTE 

73 – 61% 

Sin modificadores de movimiento. Daño por caída se activa desde 2 u (en lugar de 3 u). 

Barra de SI verde-ámbar. Antena tiembla levemente cada 15 s. Sin cambio musical. 

Polvo acumulado en paneles. Antena con vibración sutil en la animación Idle. 

3 — AVERÍA 

60 – 47% 

dashCooldown × 1,5. Sin dashes aéreos. Delay de 0,04 s en el input de dash. 

Barra de SI ámbar. Sonido de rueda raspando cada 8–10 s. Música introduce tensión leve. 

Rueda trasera derecha con deformación visible. Panel solar con grieta en sprite. 

4 — CRÍTICO 

46 – 29% 

maxRunSpeed × 0,75. runAcceleration × 0,80. Jump buffer reducido a 0,06 s. 

Barra de SI roja parpadeante. HUD con tinte naranja semitransparente. Alarma de baja potencia cada 30 s. 

Rueda delantera derecha doblada. Cámara Pancam con fisura. Antena parcialmente caída. 

5 — EMERGENCIA 

28 – 9% 

jumpForce × 0,85. Wall jump desactivado. Coyote time reducido a 0,06 s. 

Tinte naranja intenso en toda la pantalla. Interferencia visual en bordes cada 5 s. Música de tensión completa. 

Brazo robótico colgando. Dos ruedas con daño visible. Chasis con marcas de impacto. 

6 — EXTINCIÓN 

8 – 1% 

maxRunSpeed × 0,50. Movimiento asimétrico: el rover arrastra la rueda izquierda. Sin dash disponible. 

Pantalla con estática en bordes. Minimapa con fallas intermitentes. Parpadeos de apagón cada 20 s. 

Estado máximo de deterioro. El rover es visualmente irreconocible respecto al arte base. 

2.6.3 Sistema de daño — fuentes y valores 

Fuente de daño 

SI perdida 

Condición 

Notas de implementación 

Proyectil de drone 

8 – 12 SI 

Contacto con hitbox del rover 

Variación aleatoria ±4 para evitar daño predecible 

Contacto con ser bioluminiscente 

5 SI por segundo 

Colisión continua activa 

OnTriggerStay2D — no OnTriggerEnter2D 

Ataque del Leviatán (tentáculo) 

15 SI 

Contacto único por ataque 

Invincibility frames de 0,8 s tras impacto 

Proyectil del Centinela (Fase 1) 

12 SI 

Contacto con hitbox del rover 

Proyectil recto, velocidad 8 u/s 

Proyectil del Centinela (Fase 2) 

18 SI 

Contacto con hitbox del rover 

Proyectil con seguimiento parcial, vel. 6 u/s 

Caída (3 – 5 u) 

10 SI 

Al aterrizar con velocidad > umbral 

Fase 2+: activo desde 2 u de caída 

Caída (exactamente 6 u) 

25 SI (cap) 

Al aterrizar 

Cap máximo: caídas de 6 u o más siempre producen 25 SI de daño 

Caída (> 6 u) 

25 SI (cap) 

Al aterrizar 

El daño no escala más allá del cap independientemente de la altura 

Objeto ambiental (trampa) 

6 SI 

Activación de trigger 

Diseñados solo en Nivel 2 

2.7 Economía: celdas (mecanismo de recuperación) 

Las celdas de energía son el único mecanismo de recuperación de SI en el juego. Su escasez es intencional: si la recuperación fuera fácil, la degradación perdería su peso emocional. La distribución en los niveles está diseñada para que el jugador tenga siempre acceso a una celda antes de un combate difícil, pero nunca tenga suficientes para ignorar el daño acumulado. 

SI restaurada por celda 

+20 puntos de Integridad Estructural 

Capacidad de reserva 

2 celdas simultáneas — slots visibles en el HUD 

Activación 

Input dedicado (botón configurable). No se activa automáticamente. 

Distribución en Nivel 1 

4 celdas totales: 2 en zonas de exploración opcionales, 2 en salas previas al Leviatán 

Distribución en Nivel 2 

5 celdas totales: 1 en entrada, 2 en Ala B, 2 en zona previa al Centinela 

Representación en HUD 

2 slots de celda junto a la barra de SI (parte inferior izquierda). Icono lleno = celda disponible. Icono vacío = slot sin celda. 

Respawn de celdas 

No — las celdas son únicas por sesión. No reaparecen al morir. 

2.8 Sistema de upgrades 

Los upgrades son reparaciones y adaptaciones que Opportunity realiza utilizando materiales del entorno. Cada uno tiene una justificación narrativa dentro del universo del juego y un impacto mecánico concreto. El Level Designer debe colocar cada sala de upgrade en la zona indicada de forma que el jugador la encuentre naturalmente antes de necesitar la mecánica. 

Upgrade 

Zona de obtención 

Mecánica desbloqueada 

Justificación narrativa 

Impacto en progresión 

Rueda Reforzada 

Nivel 1 — Zona 2 (opcional, sala lateral) 

Activa la escalada de paredes y el wall jump. 

Opportunity adapta el mecanismo de agarre de su rueda usando aleaciones de la cueva. 

Abre las rutas verticales del Nivel 1 posteriores a la sala. Sin él, aproximadamente el 30% del nivel es inaccesible. 

Sistema de Escaneo Mejorado 

Nivel 1 — Zona 3 (al final de la sala del meteorito) 

Amplía el radio del escaneo de 3 u a 5 u. Agrega el modo de ping pasivo: objetos escaneables brillan cuando están a menos de 4 u. 

Opportunity recalibra sus espectrógrafos usando los minerales del meteorito de hierro. 

Facilita encontrar objetos de lore opcionales. No es obligatorio para completar el juego. 

Escudo de Plasma 

Nivel 2 — Entrada (después del primer checkpoint del Relicto) 

Activa un escudo de un único uso que absorbe completamente el primer impacto de daño y se recarga en 8 s. 

Opportunity integra materiales del Relicto en su estructura de chasis para crear una barrera electromagnética. 

Cambia el ritmo del combate contra los drones: el jugador puede absorber un hit deliberadamente para ejecutar un contraataque. 

Batería EMP 

Nivel 2 — Ala B (zona de exploración opcional) 

Activa un pulso EMP de área de radio 5 u que aturde a todos los drones en rango durante 2 s. Recarga: 25 s. 

Opportunity repropósita el núcleo energético del Relicto como fuente de pulsos electromagnéticos. 

Herramienta de emergencia contra grupos de drones. Ineficaz contra el Centinela Principal. 

2.9 Sistema de checkpoints 

Los checkpoints son estaciones de diagnóstico del Relicto que Opportunity activa al atravesarlas. Establecen el punto de reaparición del rover en caso de muerte. 

Activación 

Automática al cruzar el BoxCollider2D del checkpoint. No requiere input del jugador. 

Dimensiones del trigger 

Ancho: equivalente al ancho del pasillo donde está ubicado (mínimo 3 u). Alto: 4 u (altura completa del pasillo estándar). 

Radio mínimo de seguridad 

El checkpoint debe ubicarse a mínimo 8 u del enemigo más cercano para evitar loops de spawn-muerte. 

Efecto visual 

Animación de activación: el panel del checkpoint emite un pulso de luz de 1,2 s. Texto del HUD: CHECKPOINT REGISTRADO. 

SI al revivir 

La SI no se restaura. El jugador reaparece con la misma SI que tenía al activar el checkpoint. 

Distribución en Nivel 1 

3 checkpoints: inicio, mitad (antes de Zona 4), sala previa al Leviatán. 

Distribución en Nivel 2 

4 checkpoints: entrada, Ala A, Ala B, sala previa al Centinela. 

2.10. Diseño de niveles 

El proyecto usa las siguientes técnicas de proyección 2.5D. El Level Designer y el Artist deben implementarlas de forma coordinada. 

Técnica 

Aplicación en el juego 

Parallax multi-capa 

Nivel 1: tres capas de fondo (pared de cristal lejana, formaciones medianas, foreground de rocas). Nivel 2: cuatro capas (estructura del Relicto en distintos planos de profundidad). 

Billboarding de partículas 

Partículas de impacto del rover, polvo al aterrizar, destellos del sistema de escaneo y VFX de los ataques de enemigos. 

Escala de eje Z 

Rocks y pilares en primer plano se dibujan a mayor escala que sus equivalentes en el fondo. Los NPCs del fondo son visualmente más pequeños que los del plano de juego. 

Proyección de sombras planares 

El rover proyecta sombra dinámica. Los seres bioluminiscentes proyectan sombra de baja opacidad que delata su posición fuera de pantalla. 

Interfaz de usuario gráfica, Sitio web

El contenido generado por IA puede ser incorrecto. 

Ilustración 3 Jerarquia de capas 2.5D 

2.10.1 Nivel 1 — Las Cuevas Bioluminiscentes 

El Nivel 1 ocurre en un sistema de cavernas subsuperficiales de cristal bioluminiscente. La progresión es lineal con bifurcaciones opcionales que contienen upgrades y objetos de lore. El jugador siempre tiene visible al menos una ruta hacia adelante para evitar desorientación. 

Bioma 

Cueva de cristal — luz azul-verde ambiental de las formaciones de roca 

Duración estimada 

25–35 minutos en primera partida 

Checkpoints 

3 (inicio, media progresión, previa al boss) 

Enemigos presentes 

Ser Bioluminiscente (×6 distribuidos), Drone Patrullero (×2), Leviatán (×1 — boss de nivel) 

Objetos escaneables 

SC-01, SC-02, SC-03 (ver Sección 4) 

Upgrades disponibles 

Rueda Reforzada (Zona 2, opcional), Sistema de Escaneo Mejorado (Zona 3) 

Hardware al inicio 

74% — Fase 1 activa 

Hardware estimado al final 

47–61% — depende del playstyle del jugador 

[Text Wrapping Break] 

Interfaz de usuario gráfica

El contenido generado por IA puede ser incorrecto. 

 

Ilustración 4 Nivel 1 — Las Cuevas Bioluminiscentes 

2.10.2 Nivel 2 — El Relicto 

El Nivel 2 ocurre en una estructura alienígena de arquitectura no euclidiana: pasillos que se doblan en ángulos inesperados, salas con gravedad visual invertida y materiales que no existen en la naturaleza marciana. La progresión sigue la estructura de un Metroidvania clásico con tres alas exploradas en orden semi-libre. 

Bioma 

Estructura alienígena — luz violeta-naranja artificial, materiales metálicos con bioluminiscencia orgánica residual 

Duración estimada 

35–50 minutos en primera partida 

Checkpoints 

4 (entrada, Ala A, Ala B, previa al boss) 

Enemigos presentes 

Drone Detector (×3), Drone Patrullero (×2), Centinela Secundario (×2), Centinela Principal (×1 — boss final) 

Objetos escaneables 

SC-04, SC-05, SC-06 (ver Sección 4) 

Upgrades disponibles 

Escudo de Plasma (entrada), Batería EMP (Ala B, opcional) 

Hardware al inicio 

47–61% — herencia del Nivel 1 

Hardware estimado al final 

9–29% — la degradación se acelera en el Nivel 2 

  

 

Interfaz de usuario gráfica

El contenido generado por IA puede ser incorrecto. 

Ilustración 5 Mapa topológico del Nivel 2: El Relicto 

2.10.3 Mapeo de técnicas 2.5D 

Técnica (Unidad 3) 

Definición curricular 

Implementación en el proyecto 

Responsable 

Parallax multi-capa 

Técnica de profundidad visual 2.5D basada en desplazamiento diferencial de capas de fondo respecto a la cámara. Simula distancia sin geometría 3D real. 

Tres capas en Nivel 1 (pared de cristal lejana, formaciones medianas, foreground de rocas). Cuatro capas en Nivel 2. Implementado con ParallaxBackground.cs usando Camera.main.transform . 

Artist & Technical Director 

Proyección axonométrica y oblicua 

Proyección que preserva longitudes paralelas sin punto de fuga. En videojuegos 2.5D se usa para dar apariencia de profundidad a elementos del entorno (plataformas, estructuras) vistas desde un ángulo fijo. 

Las plataformas y estructuras del Nivel 2 (El Relicto) se diseñan con ángulo isométrico de 26° sobre el plano horizontal para sugerir profundidad arquitectónica. Las superficies superiores de las plataformas se pintan con un tono más claro. El plano de juego del rover permanece ortogonal (2D puro); la proyección axonométrica se aplica únicamente a los elementos de fondo y decoración. 

Artist & Level Designer 

Escala del eje Z 

Uso del eje Z en un espacio 2D para simular profundidad mediante escalado de objetos: los elementos más "lejanos" se renderizan más pequeños y con menor saturación de color. 

Las rocas y pilares en primer plano se renderizan al 110–120% de escala respecto a los del plano de juego. Los elementos del fondo lejano se renderizan al 60–70% con saturación reducida en un 30%. Los NPCs que aparecen en el fondo (decoración) se escalan al 50% del tamaño del rover. 

Artist & Level Designer 

Billboarding 

Técnica en la que un sprite o quad 2D se orienta automáticamente hacia la cámara, independientemente del ángulo de visión. Permite integrar sprites en espacios 3D sin que se vean planos desde ángulos oblicuos. 

Los efectos de partículas (impacto de proyectiles, polvo al aterrizar, destellos del sistema de escaneo, bioluminiscencia de los seres) usan el componente Billboard de Unity URP para orientarse hacia Camera.main en cada frame. Esto evita que los VFX se vean como tarjetas planas en el espacio 3D del parallax. 

Technical Director & Artist 

Mapeo topológico 

Representación abstracta de la conectividad entre zonas de un nivel sin escala geométrica real. Permite planificar el flujo de navegación antes de construir la geometría. 

Los diagramas de nodos de las son mapas topológicos del Nivel 1 y el Nivel 2 respectivamente. Cada nodo representa una sala o zona; cada arista representa una conexión navegable. El Level Designer usa estos mapas como especificación de conectividad antes de construir la geometría en Unity. Los mapas topológicos son también la base del minimapa del HUD. 

Level Designer 

Ray casting 

Técnica que proyecta rayos virtuales desde un origen para detectar colisiones con geometría del entorno. En 2.5D se usa para detección de suelo, paredes, línea de visión de enemigos y triggers de mecánicas. 

Physics2D.Raycast y Physics2D.BoxCast se usan en cuatro sistemas: (1) detección de suelo del PlayerController (raycast descendente desde los pies del rover para coyote time); (2) detección de pared para escalada (raycast lateral); (3) línea de visión del Drone Detector hacia el rover (raycast con máscara de capas Enemy→Player); (4) sistema de escaneo del rover (CircleCast de radio variable según upgrade. 

Gameplay Programmer / AI Programmer 

Sombras planares (Light 2D) 

Proyección de sombras desde objetos sobre el plano del suelo usando iluminación 2D. Técnica de volumetría simulada en entornos 2.5D con sistema de luz local. 

Unity URP Light 2D con Shadow Caster 2D en el rover y los enemigos principales. El rover proyecta sombra dinámica. Los seres bioluminiscentes proyectan sombra de opacidad 0,4 que delata su posición fuera de pantalla. 

Technical Director & Artist 

2.11 HUD e interfaz de usuario 

 

Ilustración 6 HUD diegético — wireframe de producción (1920×1080) 

2.11.1 Especificación de cada elemento del HUD 

Elemento 

Posición 

Componente Unity 

Estados y comportamiento 

Barra de Integridad Estructural 

Top-left — (32, −32) desde esquina 

Slider + Image (fill). TextMeshPro para valor numérico. 

Valor numérico visible siempre. Animación de pulso al recibir daño (scale 1.0 → 1.08 → 1.0 en 0,12 s). 

Slots de celda de energía (×2) 

Top-left — debajo de la barra de SI 

Image para cada slot. Sprite lleno / vacío. 

Slot lleno: icono brillante. Slot vacío: icono en gris 40% opacidad. Brillo pulsante al activar la celda. 

Contador de Sol 

Top-center — (0, −28) desde borde superior 

TextMeshPro. Tipografía monoespacio. 

Texto: SOL [N]. Incrementa cada vez que Opportunity entra en una nueva zona de exploración (trigger ambiental). En Fase 5+: parpadeo ocasional. 

Estado de transmisión 

Top-right — (−32, −32) desde esquina 

TextMeshPro + Image (icono de antena). 

Intento de TX normal: animación de ping cada 8 s. Sin respuesta: texto SENAL PERDIDA en rojo. En Fase 6: texto BATERIA CRITICA en rojo parpadeante. 

Slots de upgrades activos 

Bottom-left — (32, 32) desde esquina inferior 

Image por cada slot (máximo 4). Icono del upgrade equipado. 

Gris hasta desbloquearse. Brillo de cooldown animado para EMP y Escudo. Shake sutil cuando no está disponible y el jugador intenta usarlo. 

Minimapa 

Bottom-right — (−32, 32) desde esquina inferior 

RenderTexture de cámara ortogonal secundaria. RawImage. 

Muestra un radio de 10 u alrededor del rover. Shader de degradado circular en los bordes. En Fase 4+: ruido visual aleatorio en el 15% de los píxeles. En Fase 5+: apagado intermitente cada 20 s. 

Alert Strip 

Bottom-center — anchored center-bottom 

TextMeshPro con fondo semitransparente. 

Visible solo cuando hay una alerta activa. Texto de alerta en mayúsculas monoespacio. Desaparece con fade de 0,5 s al terminar la condición. 

 

2.11.2 Tabla de rangos de color del HUD 

Esta tabla define los rangos de color del HUD en todo el documento. Cualquier referencia a colores del HUD en otras secciones remite a esta tabla. 

Rango de SI 

Estado 

Color de la barra 

Color del texto numérico 

Efectos adicionales 

100 – 61% 

NOMINAL 

Verde (#4CAF50) 

Blanco 

Sin efectos adicionales 

60 – 41% 

ADVERTENCIA 

Ámbar (#FFA726) 

Ámbar 

Antena vibra levemente en la animación Idle 

40 – 21% 

CRÍTICO 

Rojo (#F44336) 

Rojo parpadeante 

HUD con tinte naranja. Alarma de audio cada 30 s 

20 – 1% 

EXTINCIÓN 

Rojo oscuro (#8B0000) 

Rojo oscuro parpadeante rápido 

Tinte naranja intenso. Estática en bordes. Parpadeos de apagón 

 

 

2.12 Inteligencia artificial y enemigos 

Esta sección es la fuente de verdad para el AI Programmer. Cada enemigo tiene una Máquina de Estados Finitos (FSM) completa, valores numéricos de HP, velocidades base y especificaciones de proyectiles. El documento diferencia explícitamente entre la lógica de IA (responsabilidad del AI Programmer) y los estados de animación (responsabilidad del Artist). 

2.12.1 Sistema DDA — Dificultad Dinámica Adaptativa 

El DDA es un sistema centralizado en el AIManager que monitorea el rendimiento del jugador y ajusta parámetros de los enemigos en tiempo real. No modifica la narrativa ni los valores de HP del jugador —solo los parámetros de comportamiento de los enemigos. 

Métricas monitoreadas 

Porcentaje de dashes evasivos exitosos, número de muertes en los últimos 5 minutos, SI perdida por minuto, tiempo en zona de peligro (SI < 29%) 

Trigger de reducción de dificultad 

Más de 2 muertes en 5 minutos en la misma zona. DDA reduce en un 15% la velocidad de detección y el daño de proyectiles por 3 minutos. 

Trigger de aumento de dificultad 

0 muertes y SI > 60% durante más de 8 minutos. DDA activa el modo de flanqueo del Drone Detector y aumenta la velocidad de patrullaje del Drone Patrullero un 10%. 

Comunicación entre drones 

Al detectar al rover, el Drone Detector envía un evento al AIManager. Los Drones Patrulleros dentro de 12 u reciben la última posición conocida. 

Límites del DDA 

Los modificadores del DDA operan en el rango ×0,75 a ×1,25 de los valores base. El DDA no puede hacer que un enemigo sea imposible ni trivial. 

2.12.2 Ser Bioluminiscente 

HP 

40 puntos 

moveSpeed base 

1,5 u/s (flotación) 

moveSpeed en Chase 

1,5 × 2,5 = 3,75 u/s 

Daño al contacto 

5 SI por segundo (OnTriggerStay2D) 

Rango de detección 

5 u de radio — sin línea de visión requerida 

Comportamiento 

FSM de 3 estados: Idle (flotar aleatoriamente), Alert (orientarse al rover), Chase (seguir al rover) 

Vulnerabilidad 

Aturdido 2 s por el pulso del Sistema de Escaneo. El pulso no causa daño. 

Comportamiento grupal 

Cada ser actúa individualmente. Sin coordinación con otros seres. 

2.12.3 Leviatán 

HP 

200 puntos — boss de Nivel 1 

moveSpeed base 

0,8 u/s (movimiento lento de tentáculos) 

Daño por tentáculo 

15 SI — invincibility frames de 0,8 s 

Patrón de ataque 

Ciclo de 3 tentáculos alternos con 2 s de pausa entre cada ataque 

Punto débil 

Núcleo central — recibe ×2 daño. Solo visible durante la pausa post-ataque. 

Fase de enfurecimiento 

< 50% HP: velocidad de tentáculos × 1,5. Agrega un cuarto tentáculo al ciclo. 

Invulnerabilidad 

Tentáculos no reciben daño. Solo el núcleo central. 

Proyectiles 

No. Solo ataques de contacto de tentáculos. 

2.12.4 Drone Patrullero 

HP 

60 puntos 

moveSpeed base 

3,0 u/s en patrullaje, 5,0 u/s en Chase 

Rango de detección 

7 u de radio 

attackRange 

3,5 u — distancia a la que inicia el ataque de proyectil 

Daño de proyectil 

8 – 12 SI (variación aleatoria ±4) 

Velocidad de proyectil 

8,0 u/s — proyectil recto sin seguimiento 

Tiempo de vida del proyectil 

2,0 s — desaparece si no impacta en ese tiempo 

Hitbox del proyectil 

Círculo de radio 0,2 u 

Comportamiento 

Patrullaje por waypoints fijos. Al detectar al rover: Chase. Si el rover escapa del rango por 5 s: regresa al waypoint más cercano. 

Cooldown de ataque 

1,5 s entre proyectiles 

2.12.5 Drone Detector — FSM completo (ciclo Sensar-Pensar-Actuar) 

El Drone Detector es el enemigo más complejo del juego. Su FSM se documenta a nivel de diagrama para que el AI Programmer pueda implementarlo directamente sin ambigüedad. 

HP 

80 puntos 

moveSpeed base 

2,5 u/s en patrullaje 

moveSpeed en Chase 

4,5 u/s 

moveSpeed en Flanking 

3,5 u/s 

Rango de visión 

8 u de distancia, cono de 60° 

Ángulo del cono de visión 

60° — ampliado a 90° por DDA si el jugador tiene alta tasa de evasión 

Obstrucción de visión 

Sí — el cono de visión hace raycast. Las paredes obstruyen la detección. 

attackRange 

4,0 u — distancia a la que dispara 

Daño de proyectil 

8 – 12 SI 

Velocidad de proyectil 

7,0 u/s — proyectil recto 

Tiempo de vida del proyectil 

2,5 s 

Cooldown de ataque 

2,0 s entre disparos 

Comunicación 

Al entrar en estado Alert, emite evento al AIManager con la última posición conocida del rover 

 

Pantalla de computadora con letras

El contenido generado por IA puede ser incorrecto. 

Ilustración 7 FSM del Drone Detector: Ciclo Sensar-Pensar-Actuar 

2.12.6 Centinela Secundario 

HP 

100 puntos 

moveSpeed base 

2,0 u/s 

moveSpeed en Chase 

3,5 u/s 

Rango de detección 

6 u, sin cono — detección omnidireccional 

attackRange 

5,0 u 

Daño de proyectil 

10 SI — proyectil recto 

Velocidad de proyectil 

7,5 u/s 

Proyectil especial 

Cada tercer ataque: proyectil de rastreo parcial. Gira hasta 30° hacia el rover durante su vuelo. 

Tiempo de vida del proyectil 

3,0 s 

Cooldown de ataque 

1,8 s 

Comportamiento 

Patrullaje vertical en columnas del Relicto. No se aleja más de 8 u de su punto de origen. 

2.12.7 Centinela Principal — Boss Final 

HP total 

400 puntos — divididos en tres fases 

HP Fase 1 

400 – 268 puntos (33%) 

HP Fase 2 

267 – 134 puntos (33%) 

HP Fase 3 

133 – 0 puntos (33%) 

moveSpeed base 

1,5 u/s — el boss no persigue activamente al rover 

Posición durante el combate 

Anclado en el centro de la sala. Se desplaza un máximo de 3 u desde su posición base. 

Fase 

Condición de inicio 

Ataques disponibles 

Notas de diseño 

Fase 1 

HP 400 – 268 

Proyectil recto × 3 en abanico (12 SI cada uno, vel. 8 u/s). Cooldown: 2,5 s. 

Fase introductoria. El jugador aprende la posición del boss y el patrón de abanico. Sin seguimiento. Vulnerable sin interrupciones. 

Fase 2 

HP 267 – 134 

Abanico de 5 proyectiles + 1 proyectil de rastreo simultáneo (18 SI, gira 45°, vel. 6 u/s). Cooldown: 2,0 s. Activa Centinela Secundario (×1) como apoyo. 

Objetivo de extensión si el tiempo no alcanza. Activa el spawn del Centinela Secundario a los 10 s de inicio. 

Fase 3 — stretch goal 

HP 133 – 0 

Igual que Fase 2 + pulso de área de radio 4 u cada 8 s (25 SI). Sin cooldown durante el pulso. 

Fuera del scope del prototipo. Se implementa si el tiempo disponible supera las estimaciones del plan de desarrollo. 

 

 

PARTE 3 

Universo y narrativa 

Qué historia cuenta el juego y cómo está construida, contexto del mundo · personajes principales · arco narrativo 

3. Contexto histórico y universo del juego 

El universo de Opportunity: Red Dust Legacy está construido sobre dos capas de realidad que conviven sin contradicción: los hechos documentados de la misión Mars Exploration Rover Opportunity de la NASA, y una ficción científicamente coherente que los extiende hacia lo desconocido. Esta sección establece ambas capas como fundamento de todo el diseño narrativo, mecánico y artístico del proyecto. 

3.1 Hechos históricos verificables — Misión Opportunity 

Datos reales de la misión Opportunity — NASA (2003–2019) 

Lanzamiento 

7 de julio de 2003 — Cabo Cañaveral, Florida, EE. UU. 

Aterrizaje 

25 de enero de 2004 — Cráter Eagle, Meridiani Planum, Marte 

Duración prevista 

90 soles marcianos (aproximadamente 92 días terrestres) 

Duración real 

5.111 soles — 14 años, 9 meses y 22 días (55 veces más de lo planificado) 

Distancia recorrida 

45,16 km — primera maratón completa en otro planeta (Sol 3.846) 

Última transmisión 

Sol 5.111 — 10 de junio de 2018 (tormenta global de polvo, tau aproximado: 10,8) 

Intentos de recontacto 

1.034 señales enviadas desde la Tierra sin respuesta (junio 2018 – febrero 2019) 

Cierre de misión 

13 de febrero de 2019 — NASA declara la misión concluida oficialmente 

Última señal interpretada 

«My battery is low and it's getting dark.» — reconstrucción poética de los últimos datos de telemetría 

Rover gemelo 

Spirit (MER-A), lanzado el 10 de junio de 2003. Perdido en arena blanda el 22 de marzo de 2010 (Sol 2.208) 

3.1.1 El intervalo silencioso: Sol 5.111 al Sol 5.847 

El juego comienza en el Sol 5.847. La última transmisión conocida ocurrió en el Sol 5.111. Entre ambos momentos transcurren 736 soles marcianos —aproximadamente dos años terrestres. 

Qué hizo Opportunity durante los 736 soles de silencio 

Tras la tormenta, los paneles solares quedaron cubiertos de polvo de alta densidad. Los sistemas de bajo consumo mantuvieron activos solo los procesos esenciales: reloj interno, registro de temperatura y monitoreo de batería. 

A medida que el viento marciano fue limpiando gradualmente los paneles —proceso documentado en rovers anteriores—, la carga aumentó de forma intermitente. El rover ejecutó varios ciclos de reinicio sin alcanzar la potencia mínima para activar el transmisor de alta ganancia. 

En modo de exploración autónoma de bajo nivel, Opportunity continuó registrando datos: imágenes de la Pancam, muestras de suelo a intervalos programados, y registros sísmicos. Estos datos son el contenido de varios flashbacks del juego. 

En el Sol 5.847, tras dos años de acumulación progresiva de energía solar, el rover alcanzó por primera vez la potencia necesaria para reiniciar el sistema operativo completo. El juego comienza exactamente en ese momento. 

3.1.2 La civilización desaparecida — historia canónica del universo 

El Relicto alienígena del Nivel 2 no es un elemento decorativo: es la evidencia de una civilización cuya historia el equipo de desarrollo debe conocer para construir el nivel de forma coherente. El jugador nunca recibe esta información de forma explícita; se comunica a través del diseño del entorno y los objetos de lore. 

Hace aproximadamente 3.200 millones de años, cuando Marte todavía poseía atmósfera densa y agua líquida, una civilización de organismos colectivos de naturaleza quimiolitótrofa habitó las cavernas subsuperficiales del planeta. No eran individuos en sentido convencional: funcionaban como colonias simbióticas que compartían información a través de señales electromagnéticas de baja frecuencia. 

A medida que Marte perdió su atmósfera, esta civilización se retiró progresivamente hacia las profundidades. Construyeron el Relicto como un archivo permanente: una instalación diseñada para preservar el registro de su existencia en caso de extinción. Los seres bioluminiscentes del Nivel 1 son sus descendientes, adaptados durante milenios a las cuevas marcianas, conservando como vestigio evolutivo la capacidad de detectar señales electromagnéticas de baja frecuencia. 

Elemento del juego 

Explicación canónica 

Implicación de diseño 

Los seres bioluminiscentes reaccionan al rover 

Su sistema nervioso detecta señales electromagnéticas de baja frecuencia: vestigio del canal de comunicación de sus ancestros. La señal de radio de Opportunity activa una respuesta instintiva. 

El Sistema de Escaneo puede repeler o atraer a los seres. Justifica la mecánica sin romper la coherencia biológica. 

El glifo activa el ascensor 

El Relicto responde a señales electromagnéticas como protocolo de acceso. La frecuencia de radio del rover coincide accidentalmente con el protocolo de autenticación de la instalación. 

Opportunity no hackea el Relicto: lo activa sin intención. Es reconocido como agente transmisor de información, que es exactamente lo que el Relicto fue construido para preservar. 

Los drones atacan al rover 

Los drones son el sistema inmune autónomo del Relicto. No reconocen al rover como amenaza intencional: detectan materia compleja no catalogada y la expulsan por protocolo. 

Los drones no son malvados: cumplen su función. Su diseño visual y comportamiento deben reflejar precisión mecánica, no agresividad. 

El Ala C contiene el archivo histórico 

Era la cámara de preservación principal, diseñada para sobrevivir el máximo tiempo posible. Contiene la transmisión de Spirit como dato no catalogado, captada accidentalmente por los sensores. 

Los objetos de lore del Ala C son los más ricos en información. La transmisión de Spirit es el hallazgo inesperado del archivo. 

3.2. Narrativa: arco completo y cinemáticas 

Esta sección documenta la estructura narrativa completa: los cuatro actos, la cronología del final, la tabla de flashbacks con sus triggers, y la especificación del contenido de la transmisión de Spirit. Es la fuente de verdad narrativa para el Level Designer y el CinematicManager. 

Principio narrativo central: ironía dramática 

El jugador conoce el destino de Opportunity antes de que el juego comience. Sabe que la NASA declaró la misión concluida y que nadie en la Tierra puede escuchar al rover. 

Esa información no se expone mediante texto ni diálogo: se establece en la pantalla de carga y se refuerza con cada intento fallido de transmisión del HUD. El motor emocional del juego es la distancia entre lo que el jugador sabe y lo que el personaje siente. 

Toda decisión narrativa debe preservar este principio. Nada en el guion debe romper la ironía ni resolverla prematuramente. 

3.2.1 Pantalla de carga — establecimiento de la ironía dramática 

Antes de que el jugador tome control, la pantalla de carga muestra el registro de misión como si fuera el sistema operativo del rover reiniciando. Los datos son reales. Las celdas marcadas en rojo instalan la ironía dramática antes de que el jugador empiece a jugar. 

Parámetro del sistema 

Valor 

Color en pantalla 

SOL ACTUAL 

5.847 

Blanco 

ÚLTIMA TX RECIBIDA (TIERRA) 

Sol 5.111 — 10 junio 2018 

Rojo 

SEÑAL 

PERDIDA — tormenta global de polvo 

Rojo 

INTENTOS DE RECONTACTO 

1.034 

Rojo 

ESTADO MISIÓN (TIERRA) 

CONCLUIDA — 13 febrero 2019 

Rojo 

ESTADO MISIÓN (ROVER) 

ACTIVA — protocolo de exploración en curso 

Verde 

INTEGRIDAD ESTRUCTURAL 

74% 

Ámbar 

MODO 

EXPLORACIÓN AUTÓNOMA 

Blanco 

3.2.2 Arco narrativo — cuatro actos 

Acto I — Despertar · Nivel 1, inicio · Hardware al 74% 

Opportunity despierta. El Sol 5.847 trae suficiente radiación solar para completar el primer ciclo de carga en 736 soles. Los sistemas reinician en secuencia: sensores ambientales, ruedas, cámara, transmisor. El rover no registra la ausencia de confirmación de señal como anómala —esperar respuesta es parte del protocolo estándar. El movimiento es fluido y responsivo. El entorno es desconocido, pero no hostil. 

Emoción objetivo: esperanza silenciosa. El jugador sabe que esto no va a terminar bien, pero quiere que funcione. 

Acto II — Descubrimiento · Nivel 1, completo · Hardware al 61% 

Los sensores detectan una anomalía subsuperficial de composición desconocida. El protocolo de exploración es inequívoco: investigar y recopilar datos. Al aproximarse, el suelo cede. El rover cae por una grieta no cartografiada —la pendiente de arena acumulada durante los 736 soles amortigua el descenso, evitando daño catastrófico— y emerge en un sistema de cavernas de cristal bioluminiscente. 

Los seres bioluminiscentes reaccionan a la señal de radio del rover. Un glifo en la pared más profunda emite una frecuencia que coincide con el protocolo de acceso del Relicto. El ascensor se abre. 

Emoción objetivo: maravilla mezclada con tensión creciente. La degradación se hace perceptible por primera vez. 

Acto III — La carrera · Nivel 2 · Hardware del 47% al 9% 

El Relicto no fue construido para recibir visitas. Los drones son el sistema inmune autónomo de la instalación —operan por protocolo, sin intención hostil— y Opportunity no figura en ningún registro. El rover aprende a no ser reconocido como amenaza. 

Mientras tanto, los sistemas fallan en tiempo real: la rueda delantera derecha pierde rendimiento, el minimapa falla, la pantalla adquiere un tinte naranja, los controles introducen delay progresivo. En el Ala C, Opportunity encuentra una señal electromagnética almacenada en los archivos del Relicto que coincide exactamente con los parámetros de transmisión de Spirit. 

Emoción objetivo: urgencia y tristeza anticipada. El jugador ya sabe que Opportunity no va a sobrevivir. 

Acto IV — El silencio, y después · Cinemática final · Hardware al 1% 

Opportunity llega al núcleo. El transmisor del Relicto puede enviar datos al espacio sin las limitaciones del transmisor del rover. El Centinela Principal ejecuta el protocolo de evaluación. Derrotarlo consume la energía restante. Con el 1% de batería, Opportunity ejecuta la transmisión: envía 341 de 891 fragmentos antes de que la energía se agote completamente. 

Emoción objetivo: pérdida profunda seguida de redención —el earned ending. 

3.2.3 Secuencia del final — especificación escena por escena 

La secuencia final es el artefacto narrativo más crítico del proyecto. Cada decisión es intencional y no debe modificarse sin aprobación unánime del equipo.  

# 

Escena 

Descripción técnica y narrativa 

1 

El último intento 

El brazo mecánico falla al intentar activar el transmisor del Relicto. En el segundo intento lo logra con un delay visible de 1,2 segundos. La transmisión inicia. El sonido mecánico del brazo es el único audio en esta escena. 

2 

La transmisión interrumpida 

Barra de progreso: 1%... 11%... 23%... 38%. La energía llega a 0%. La imagen congela. Texto del sistema: ENERGIA INSUFICIENTE. Fragmentos transmitidos: 341 / 891. Sin música. Sin efectos adicionales. 

3 

El apagón de sistemas 

Cada sistema se cierra con una línea de log, de arriba hacia abajo. La última línea, en verde: PROTOCOLO DE MISION: COMPLETADO. Pantalla en negro. 

4 

El silencio — diez segundos 

Negro absoluto. Sin música. Sin texto. Sin efectos. El jugador espera. Esta pausa es obligatoria y no puede saltarse. Diez segundos exactos medidos, no aproximados. 

5 

El informe — texto sobre negro 

Sin imágenes. Texto blanco aparece línea por línea con fade de 0,8 segundos: el JPL recibió 341 fragmentos. La señal provino de Opportunity. Los datos contienen evidencia de agua activa y vida microscópica. El Congreso aprueba una misión humana tripulada con llegada estimada en 2037. 

6 

Marte, 2037 

Sin diálogo. Un astronauta aterriza, camina, se detiene. La cámara no muestra su rostro. Se arrodilla. Sus manos con guantes tocan con cuidado un objeto en el polvo. El objeto no se ve todavía. 

7 

El plano final 

El astronauta sostiene al rover contra el cielo marciano naranja. El rover está inmóvil, cubierto de polvo. El astronauta no dice nada. Fade a negro lento de cinco segundos. Aparece el texto de cierre. 

 

Texto final sobre negro — datos reales 

La misión de Opportunity duró 5.111 soles. 

Recorrió 45,16 km — una maratón completa en otro planeta. 

Sus datos cambiaron para siempre la comprensión humana de Marte. 

 

Su última señal interpretada fue: 

«My battery is low and it's getting dark. » 

 

Nunca dejó de transmitir. 

 

Regla de oro del final  

El astronauta nunca habla. No dice nada. 

Solo levanta al rover. El jugador proyecta todo lo demás por sí solo. 

Cualquier diálogo destruye la escena. Esta regla no tiene excepciones. 

3.2.4 Cronología del final — nota de coherencia narrativa 

Una señal de radio enviada desde Marte tarda entre 3 y 22 minutos en llegar a la Tierra, dependiendo de la posición orbital. En el Sol 5.847 (aproximadamente 2033), los 341 fragmentos transmitidos llegan al Deep Space Network de la NASA en minutos, no en años. 

La misión humana de 2037 no es el tiempo que tarda la señal en llegar: es el tiempo que tarda la humanidad en organizar, financiar y ejecutar una misión tripulada a Marte —cuatro años es un plazo realista.  

 

Ilustración 21 Secuencia del final: especificación escena por escena 

3.3 Tabla de flashbacks — especificación completa 

Los flashbacks son fragmentos de archivos de telemetría que el sistema de diagnóstico recupera al escanear objetos específicos. No son memorias subjetivas: son datos de misión reproducidos con interferencia visual progresiva según el nivel de degradación del hardware. 

ID 

Sol 

Contenido del flashback 

Trigger de activación 

Nivel / Zona 

HW mín. 

FB-01 

Sol 339 

Imágenes de las Blueberries de hematita. Análisis mineral del sistema. Texto: EVIDENCIA DE AGUA LIQUIDA ANTIGUA CONFIRMADA. 

Escanear la primera formación de cristal del Nivel 1. 

Niv. 1 — Zona 1 

61% 

FB-02 

Sol 951 

Panorámica del cráter Endurance desde el borde. El rover calcula la ruta de descenso. Solo imagen y sonido de ruedas, sin texto superpuesto. 

Escanear el meteorito de hierro en Zona 3. 

Niv. 1 — Zona 3 

61% 

FB-03 

Sol 2.681 

Imágenes de arcillas del cráter Endeavour. Texto: COMPOSICION: FILOSILICATOS. POTENCIALMENTE HABITABLE. El mismo tipo de roca que forma las cuevas. 

Escanear la formación central de la sala del Leviatán. 

Niv. 1 — Zona 4 

47% 

FB-04 

Sol 3.846 

El odómetro del rover alcanza 42.195 km. Texto: MARATON COMPLETA. DISTANCIA TOTAL: 42.195 KM. El rover no sabe que es un hito. 

Completar 42 km en el odómetro del HUD (logro pasivo, sin escaneo). 

Cualquier zona 

— 

FB-05 

Sol 5.111 

La tormenta. La cámara muestra polvo hasta que la imagen desaparece. Texto: SENAL TX: PERDIDA. BATERIA: 9%. MODO: ESPERA. El flashback se interrumpe con estática. 

Escanear el panel de checkpoints al inicio del Nivel 2. 

Niv. 2 — Inicio 

29% 

FB-06 (Spirit) 

Sol 2.208 

Transmisión de Spirit almacenada en el Relicto. Imagen en blanco y negro. Datos de telemetría de Spirit bajando hasta cero. Sin texto superpuesto. El flashback termina cuando la imagen de Spirit queda inmóvil. 

Escanear el archivo central del Ala C. 

Niv. 2 — Ala C 

9% 

3.3.1 La transmisión de Spirit — especificación de contenido 

Spirit (MER-A) quedó atrapado en arena blanda el 1 de mayo de 2009. La NASA intentó liberarlo durante meses sin éxito. La última transmisión fue el 22 de marzo de 2010 (Sol 2.208). Sus últimos datos registrados incluían telemetría de batería, temperatura interna y datos del espectrómetro Mossbauer. No hubo mensaje de texto ni audio. 

Especificación técnica de la escena — Ala C del Relicto 

Formato: pantalla dividida. A la izquierda, imagen del escaneo de Opportunity en la sala. A la derecha, la transmisión de Spirit siendo reproducida. 

Contenido de la transmisión (derecha): datos de telemetría de Spirit en texto monoespacio, actualizándose lentamente. Los valores de batería disminuyen de 12% a 0% durante 40 segundos. La temperatura interna cae. Los datos del espectrómetro continúan hasta el último segundo. 

Audio: sin música. Solo el zumbido electrónico de la transmisión siendo procesada. Desaparece cuando la batería llega a 0%. 

Último dato registrado antes del corte: TEMPERATURA INTERNA: -40C. BATERIA: 0%. MODO: — 

El HUD de Opportunity muestra SENAL IDENTIFICADA: ROVER CLASE MER-A durante tres segundos antes de que la transmisión termine. Sin otro comentario del sistema. 

Duración total de la escena: 45 a 55 segundos. No puede saltarse. 

3.4 Lore, objetos escaneables y worldbuilding 

El sistema de escaneo es la interfaz entre las mecánicas de juego y la capa científica del proyecto. Cada objeto escaneable tiene una función mecánica y una capa de información real o de lore. Esta dualidad es el elemento que diferencia este proyecto de un Metroidvania convencional. 

Principio del sistema de escaneo 

El escaneo nunca es obligatorio para completar el juego. Es siempre recompensado. 

Un jugador que no escanea nada completa el juego. Un jugador que escanea todo comprende el juego. 

La diferencia entre ambas experiencias es la diferencia entre ver una imagen y leer la leyenda que la acompaña. 

3.4.1 Origen de los seres bioluminiscentes 

Los seres bioluminiscentes son los descendientes evolutivos de organismos traídos por la civilización alienígena hace aproximadamente 3.200 millones de años. Durante los milenios posteriores a su desaparición, se adaptaron de forma independiente a las cuevas subsuperficiales de Marte: perdieron complejidad neurológica, desarrollaron la bioluminiscencia como mecanismo de comunicación intraespecífica, y conservaron —como vestigio evolutivo— la capacidad de detectar señales electromagnéticas de baja frecuencia. 

Su comportamiento ante el rover es instintivo, no hostil: la señal de radio de Opportunity activa en ellos el mismo circuito neurológico que en sus ancestros activaba la detección de comunicación. Se acercan porque el rover habla en una frecuencia que su biología reconoce; atacan en grupo si el rover perturba su entorno, como cualquier animal territorial respondería a una amenaza. 

3.4.2 Objetos escaneables del prototipo — especificación completa 

El prototipo compromete un mínimo de seis objetos completamente implementados para Milestone 2. Cada objeto tiene prioridad, función mecánica, texto visible en el HUD y fuente que lo respalda. 

Objetos obligatorios — Milestone 2 

ID 

Nombre 

Nivel / Zona 

Función mecánica 

Texto del HUD (visible al jugador) 

Fuente 

SC-01 

Esferas de hematita (Blueberries) 

Niv. 1 — Zona 1 

Activa FB-01. Revela primera ruta oculta. 

ANALISIS: Esferas de hematita cristalizada. Diametro: 4,5 mm. Formacion por concrecion en agua líquida. Dato de mision — Sol 339. 

NASA MER Sol 339 

SC-02 

Meteorito de hierro 

Niv. 1 — Zona 3 

Activa FB-02. Contiene minerales para el Escudo de Plasma. 

ANALISIS: Aleacion hierro-niquel. Primer meteorito identificado en otro planeta. Dato de mision — Sol 339. 

NASA MER Heat Shield Rock 

SC-03 

Formación de arcilla del Cráter Endeavour 

Niv. 1 — Zona 4 

Activa FB-03. Abre puerta al corredor del Leviatán. 

ANALISIS: Filosilicatos. Agua de pH neutro, temperatura templada. Condiciones potencialmente habitables. Dato de mision — Sol 2.681. 

NASA MER Endeavour 

SC-04 

Registro de tormenta de polvo 

Niv. 2 — Inicio 

Activa FB-05. Desbloquea filtro de opacidad del minimapa. 

REGISTRO AMBIENTAL: Tormenta de polvo global. Tau maximo: 10,8. Duracion estimada: 115 dias. Sol 5.111. 

NASA MER Sol 5.111 

SC-05 

Glifo de acceso del Relicto 

Niv. 2 — Entrada 

Activa el ascensor al Nivel 2. Desbloquea log del Relicto. 

ANALISIS: Patron electromagnetico coherente. Frecuencia: 437,5 MHz. Coincidencia con frecuencia TX del rover: 99,7%. Activando protocolo de respuesta. 

Ficcion canonica 

SC-06 

Archivo Spirit — transmisión almacenada 

Niv. 2 — Ala C 

Activa escena de Spirit. Desbloquea logro narrativo final. 

SENAL IDENTIFICADA: ROVER CLASE MER-A. FECHA DE EMISION: SOL 2.208. ESTADO DEL EMISOR: DESCONOCIDO. Reproduciendo archivo... 

NASA MER-A Sol 2.208 

Objetos de extensión — fuera del scope del prototipo 

Los siguientes objetos pertenecen a la versión completa del juego. Su especificación queda documentada para facilitar la implementación futura. 

ID 

Nombre tentativo 

Nivel previsto 

Función narrativa 

SC-07 

Datos del Marathon Valley 

Niv. 1 — Zona 2 

Activo logro del odómetro (42 km). Texto sobre los 14 años del rover. 

SC-08 

Núcleo energético del Relicto 

Niv. 2 — Ala B 

Explica el sistema de energía alienígena. Material para la Batería EMP. 

SC-09 

Registro de hibernación invernal 

Niv. 1 — Zona 2 

Flashback de los inviernos marcianos. Introduce la soledad prolongada. 

SC-10 

Archivo histórico central del Relicto 

Niv. 2 — Ala C 

Historia completa de la civilización alienígena. Mayor densidad narrativa del juego. 

3.4.3 Convención de presentación del lore en pantalla 

El lore se presenta siempre a través del HUD diegético, nunca mediante texto superpuesto genérico. El jugador lee datos a través de los ojos de Opportunity, no a través de una interfaz externa al universo del juego. 

Tipografía 

Monoespaciada, tamaño reducido. Simula salida de terminal de sistema embebido. 

Color del texto 

Blanco para datos nominales. Ámbar para advertencias. Rojo para anomalías o datos sin clasificar. 

Encabezado 

Siempre inicia con el tipo de análisis: ANÁLISIS, REGISTRO AMBIENTAL, SEÑAL IDENTIFICADA, ARCHIVO HISTÓRICO. 

Fuente al pie 

Cada entrada cierra con su referencia: «Dato de misión — Sol [N]» o «Ficción canónica» según corresponda. 

Duración 

El texto permanece visible mientras el jugador mantenga el botón de escaneo. Desaparece con fade de 0,3 segundos al soltar. 

Degradación 

A partir del 29% de integridad, el texto aparece con caracteres corruptos que aumentan conforme avanza la degradación. 

 

PARTE 4 

Apartado técnico 

Motor · requisitos de hardware · arquitectura básica · clases principales. 

4. Motor y stack tecnológico 

4.1 Stack tecnológico 

Herramienta 

Versión 

Categoría 

Rol principal 

Unity 

Unity 6.4 (6000.4.8f1) 

Motor de juego 

Todos 

Universal Render Pipeline (URP) 

Incluido en Unity 2026 

Rendering 

Technical Director + Artist 

Unity Input System 

1.7+ 

Gestión de input 

Gameplay Programmer 

Unity AI Navigation 

1.1+ 

NavMesh 2D 

AI Programmer 

Unity Audio Mixer 

Nativo 

Audio adaptativo 

Technical Director 

TextMeshPro 

Nativo en Unity 2026 

UI y HUD 

Technical Director 

C# 

.NET 6 / IL2CPP 

Lenguaje de scripting 

Gameplay Prog. + AI Prog. 

Blender 

3.x LTS 

Modelado 3D (elementos ambientales del Relicto) 

Artist 

Aseprite / Libresprite 

Cualquier versión 

Arte de sprites 2D 

Artist 

Git + GitHub 

Git 2.40+ 

Control de versiones 

Todos 

GitHub Projects 

Web 

Gestión de tareas (kanban) 

Todos 

4.2 Requisitos de hardware mínimos 

Los requisitos de hardware están definidos para el prototipo evaluable (2 niveles + cinemáticas). Una versión comercial completa requeriría una revisión de estos valores según el nivel de detalle artístico final. 

Componente 

Requisito mínimo 

Requisito recomendado 

Sistema operativo 

Windows 10 (64-bit, versión 1903 o superior) 

Windows 10 / 11 (64-bit, última actualización) 

Procesador (CPU) 

Intel Core i5-4460 / AMD Ryzen 3 1200 (4 núcleos · 2,9 GHz) 

Intel Core i7-7700 / AMD Ryzen 5 3600 (4+ núcleos · 3,6 GHz) 

Memoria RAM 

4 GB RAM 

8 GB RAM 

Tarjeta gráfica (GPU) 

NVIDIA GeForce GTX 950 / AMD Radeon RX 460 VRAM: 2 GB · DirectX 11 

NVIDIA GeForce GTX 1060 / AMD Radeon RX 580 VRAM: 4 GB · DirectX 11 

Almacenamiento 

500 MB de espacio disponible (HDD) 

500 MB de espacio disponible (SSD recomendado para tiempos de carga) 

Resolución mínima 

1280×720 (720p) 

1920×1080 (1080p) 

API gráfica 

DirectX 11 

DirectX 11 (el prototipo no requiere DX12) 

Conectividad 

No requerida — el juego es completamente offline 

No requerida 

Dispositivos de entrada 

Teclado + ratón 

Teclado + ratón o gamepad compatible con XInput (Xbox 360 / One / Series) 

Justificación de los requisitos mínimos 

Unity 6.4 (6000.4.8f1) con URP y renderizado 2D tiene un costo de GPU muy inferior a proyectos 3D. Los requisitos mínimos reflejan hardware de gama media-baja de 2016–2018, que representa la mayor parte del parque de PCs en Ecuador y América Latina según Steam Hardware Survey 2024. El objetivo es que el prototipo sea ejecutable en los equipos del laboratorio de la universidad sin configuración adicional. 

Parámetro de rendimiento objetivo 

Valor 

Framerate objetivo 

60 FPS estables en configuración recomendada. 

Framerate mínimo aceptable 

30 FPS estables en configuración mínima sin caídas durante combates. 

Resolución de HUD 

El HUD está diseñado para 1920×1080 (Canvas Scaler con referencia 1080p). Se adapta a resoluciones inferiores mediante Scale With Screen Size. 

Memoria de VRAM estimada 

~350 MB en resolución 1080p con los sprites del prototipo cargados. 

Tiempo de carga de nivel 

< 8 segundos en SSD. < 20 segundos en HDD mecánico. 

4.3 Arquitectura de software 

El proyecto usa una arquitectura modular orientada a la separación de responsabilidades. Cada módulo es un sistema independiente que se comunica con los demás a través de eventos o referencias directas acotadas.  

4.3.1 Especificación de módulos — responsabilidad y Definition of Done 

Módulo 

Responsable 

Responsabilidad principal 

GameManager 

Technical Director 

Controlador de estado global: gestiona la máquina de estados de la partida (MainMenu, Playing, Paused, GameOver, Cinematic). Gestiona la carga y descarga de escenas. 

PlayerController 

Gameplay Programmer 

Todo el movimiento del rover: carrera, salto, dash, escalada. Recibe input del InputSystem de Unity. Expone eventos: onDamageReceived, onDash, onLand, onDeath. 

DegradationSystem 

Gameplay Programmer 

Gestiona la SI del rover. Aplica modificadores de fase al PlayerController mediante ScriptableObject. Expone la SI actual al HUDManager en tiempo real. 

AIManager 

AI Programmer 

Controlador central de todos los NPCs. Gestiona el DDA, la comunicación de posición entre drones y el spawn de enemigos. Cada NPC es un agente registrado en el AIManager. 

ScanSystem 

AI Programmer 

Gestiona el radio de detección del escaneo, los triggers de objetos escaneables y la comunicación con el HUDManager para mostrar el texto de lore. Dispara flashbacks vía CinematicManager. 

LevelManager 

Level Designer 

Gestiona el estado del nivel: checkpoints activos, posición de respawn, enemigos derrotados y objetos recogidos. Persiste el estado entre muertes dentro de la misma sesión. 

HUDManager 

Technical Director 

Actualiza todos los elementos del Canvas en tiempo real: SI bar, slots de celda, contador de Sol, estado de transmisión, minimapa y Alert Strip. Suscrito a eventos del DegradationSystem. 

AudioManager 

Technical Director 

Gestiona los cuatro estados de música mediante Unity Audio Mixer. Gestiona el pool de efectos de sonido del rover y los enemigos. Recibe triggers del GameManager, PlayerController y AIManager. 

CinematicManager 

Technical Director 

Gestiona flashbacks, escena de Spirit y la secuencia del final. Congela el GameManager durante las cinemáticas. Gestiona los timings de la secuencia del final. 

4.4 Layer Collision Matrix 

La siguiente matriz define qué capas colisionan entre sí en Unity. Una celda marcada con ● indica colisión activa; una celda vacía indica que la colisión está desactivada. La configuración se realiza en Edit → Project Settings → Physics 2D → Layer Collision Matrix. 

Capa 

Player 

Enemy 

PlayerProjectile 

EnemyProjectile 

Ground 

Platform 

Interactable 

Trigger 

Player 

— 

● 

— 

● 

● 

● 

● 

● 

Enemy 

● 

— 

● 

— 

● 

● 

— 

— 

PlayerProjectile 

— 

● 

— 

— 

● 

— 

— 

— 

EnemyProjectile 

● 

— 

— 

— 

● 

— 

— 

— 

Ground 

● 

● 

● 

● 

— 

— 

— 

— 

Platform 

● 

● 

— 

— 

— 

— 

— 

— 

Interactable 

● 

— 

— 

— 

— 

— 

— 

— 

Trigger 

● 

— 

— 

— 

— 

— 

— 

— 

 

Nota sobre la capa Platform 

La capa Platform usa PlatformEffector2D con rotationalOffset = 0 y useOneWay = true. 

El rover puede caer a través de plataformas one-way desde arriba, pero no atravesarlas desde abajo. 

Los enemigos colisionan con Platform pero no tienen la lógica de one-way activada — permanecen sobre ellas. 

 

PARTE 5 

Estética y sonido 

Estilo visual · concept art / referencias · atmósfera sonora 

5. Estética y sonido 

5.0 Moodboard y referencias visuales 

 

Ilustración 2 Protagonista Jugable: Rover Opportunity 

5.1 Paleta de color 

El proyecto usa dos paletas distintas según el nivel, más una paleta compartida para el rover y la UI. La mezcla de colores fuera de estas paletas requiere aprobación del Artist. 

Paleta 

Uso 

Colores principales 

Colores de acento 

Rover + UI 

Rover, HUD, efectos del rover 

Naranja polvoriento (#E07040), Gris metálico (#8A9BA8), Blanco envejecido (#F0EBE3) 

Azul indicador (#3A8FC1), Verde sistema (#4CAF50), Rojo alerta (#F44336) 

Nivel 1 — Cuevas 

Fondos, tiles, iluminación ambiental 

Negro de cueva (#0A0A12), Azul cristal (#1A3A6E), Verde bioluminiscente (#2ECC71) 

Turquesa de detalle (#00BCD4), Blanco de destellos (#E8F5E9) 

Nivel 2 — Relicto 

Fondos, tiles, iluminación artificial 

Negro profundo (#060610), Violeta alienígena (#4A1A7A), Naranja oxidado (#C0581A) 

Dorado de circuitos (#B8860B), Cian tecnológico (#00E5FF) 

5.2 Proporciones y escala de personajes 

La tabla siguiente define los tamaños de sprite de cada personaje en píxeles a resolución 1×. Todos los sprites son dibujados a 1× y escalados en Unity según se requiera. El diagrama de siluetas comparativas debe ser producido por el Artist antes de la Semana 4 como artefacto de referencia permanente. 

Personaje 

Sprite 1× (px) 

Escala Unity (aprox.) 

Notas de proporción 

Rover Opportunity 

52 × 32 

× 1,0 (referencia base) 

Forma horizontal baja. Ruedas visibles y diferenciadas. Brazo robótico en el lado derecho. 

Ser Bioluminiscente 

24 × 24 

× 0,75 respecto al rover 

Forma esférica con tentáculos cortos. Translúcido — usar shader de transparencia con emisión. 

Leviatán 

96 × 64 

× 3,0 respecto al rover 

Solo el cuerpo principal. Los tentáculos son GameObjects separados con su propio sprite y hitbox. 

Drone Patrullero 

32 × 20 

× 1,0 respecto al rover 

Forma de diamante horizontal. Sin ruedas. Propulsores visibles. 

Drone Detector 

36 × 36 

× 1,1 respecto al rover 

Forma cúbica con un único ojo central. El cono de visión se dibuja como VFX, no como sprite. 

Centinela Secundario 

44 × 52 

× 1,3 respecto al rover 

Forma vertical. Más alto que el rover. Cañón de proyectil en el hombro derecho. 

Centinela Principal 

120 × 80 

× 3,75 respecto al rover 

Boss. Forma simétrica. Tres núcleos de ataque visibles en su cuerpo. 

5.3 Personajes: fichas técnicas y animación 

Esta sección es la fuente de verdad para el Artist y el Animator. Documenta todos los estados de animación del rover y de los enemigos, sus condiciones de transición, FPS y duración en frames. Los estados marcados como V2 no son obligatorios para Milestone 2 y se implementan solo si el tiempo lo permite. 

5.3.1 Rover Opportunity — Animator Controller y styleshets 

El Animator Controller del rover tiene 18 estados. La columna de prioridad indica si el estado es obligatorio para Milestone 2 (MVP) o si puede implementarse en una iteración posterior (V2). El Artist comienza por los estados MVP usando placeholders y refina el arte final en las últimas semanas. 

Estado 

Frames 

FPS 

Loop 

Condición de entrada 

Prioridad 

Idle 

8 

8 

Sí 

Sin input de movimiento 

MVP 

Run 

10 

12 

Sí 

velocidadX > 0,1 u/s 

MVP 

Jump_Rise 

6 

12 

No 

isGrounded = false Y velocidadY > 0 

MVP 

Jump_Fall 

4 

8 

Sí 

isGrounded = false Y velocidadY ≤ 0 

MVP 

Land 

4 

12 

No 

isGrounded = true (transición desde Jump_Fall) 

MVP 

Dash 

6 

24 

No 

isDashing = true 

MVP 

Damage 

5 

12 

No 

onDamageReceived() 

MVP 

Death 

12 

8 

No 

SI = 0 

MVP 

Scan_Loop 

8 

8 

Sí 

isScanning = true 

MVP 

Idle_Degrade 

8 

8 

Sí 

Sin input Y HW ≤ 47% 

V2 

Run_Limp 

12 

12 

Sí 

velocidadX > 0,1 Y HW ≤ 29% 

V2 

Dash_End 

4 

12 

No 

isDashing = false (transición desde Dash) 

V2 

Wall_Slide 

6 

8 

Sí 

isOnWall = true Y velocidadY < 0 

V2 

Wall_Jump 

5 

12 

No 

wallJump input 

V2 

Climb_Start 

4 

12 

No 

isClimbing = true (inicio) 

V2 

Climb_Loop 

6 

8 

Sí 

isClimbing = true (sostenido) 

V2 

Climb_End 

4 

12 

No 

isClimbing = false (desde Climb) 

V2 

Scan_Start 

5 

12 

No 

onScanActivate() (primer frame) 

V2 

 

 

Ilustración 9 Estructura de estados del Animator Controller del Rover Opportunity 

 

Figure 1 Opportunity idle stylesheet 

 

Figure 2 Opportunity walk stylesheet 

5.3.2 Enemigos — estados de animación y styleshets 

Enemigo 

Estado 

Frames 

FPS 

Condición de entrada 

Ser Bioluminiscente 

Float_Idle 

8 

8 

Estado base — sin rover detectado 

Ser Bioluminiscente 

Float_Alert 

6 

12 

Rover en rango de detección 

Ser Bioluminiscente 

Float_Chase 

6 

12 

Rover en Chase activo 

Ser Bioluminiscente 

Death 

8 

8 

HP = 0 

Drone Patrullero 

Patrol 

6 

8 

Estado base 

Drone Patrullero 

Chase 

6 

12 

Rover detectado 

Drone Patrullero 

Attack 

4 

12 

attackRange alcanzado 

Drone Patrullero 

Death 

8 

10 

HP = 0 

Drone Detector 

Patrol 

6 

8 

Estado base 

Drone Detector 

Alert 

4 

12 

Rover en cono de visión 

Drone Detector 

Chase 

6 

12 

Chase activo 

Drone Detector 

Attack 

5 

12 

attackRange alcanzado 

Drone Detector 

Search 

6 

8 

Rover fuera de rango por 4 s 

Drone Detector 

Death 

8 

10 

HP = 0 

Centinela Principal 

Idle 

8 

8 

Fase 1 sin ataque activo 

Centinela Principal 

Attack_F1 

10 

12 

Disparo de abanico activo 

Centinela Principal 

Attack_F2 

12 

12 

Fase 2 activa 

Centinela Principal 

Enrage 

8 

16 

Transición entre fases 

Centinela Principal 

Death 

32 

8 

HP = 0 — animación no interrompible 

 

 

Ilustración 8 Prototipo de Diseño de Enemigos 

 

 

Ilustración 10 Bio Idle 

 

Ilustración 11 Bio Walk 

 

Ilustración 12 Drone patrullero idle 

 

Ilustración 13 Drone patrullero walk 

 

5.4 Audio — decisión de implementación 

Decisión definitiva: Unity Audio Mixer 

El proyecto usa Unity Audio Mixer nativo para toda la gestión de audio. FMOD queda fuera del scope. 

Justificación: Unity Audio Mixer es suficiente para implementar los cuatro estados de música y los efectos de sonido diegéticos del rover dentro del tiempo disponible del semestre. FMOD tiene una curva de aprendizaje significativa incompatible con los plazos del proyecto. 

5.4.1 Estados de música y transiciones 

Estado de audio 

Condición de activación 

Contenido musical 

Crossfade 

EXPLORATION 

SI ≥ 41% y sin enemigos detectados 

Música ambient de baja intensidad. Percusión espaciada. Efectos de viento marciano. 

1,5 s hacia cualquier otro estado 

TENSION 

Enemigo en estado Alert o Chase, o SI entre 20 – 40% 

Drones de tensión. Percusión irregular. Sin melodía. 

0,8 s — más rápido para sentirse urgente 

COMBAT 

Boss activo o múltiples enemigos en Chase simultáneamente 

Orquestación de alta energía. Percusión rítmica prominente. 

0,5 s — transición casi inmediata 

CINEMATIC 

Cinemáticas, flashbacks, secuencia final 

Música original por escena.  

Crossfade gestionado por el CinematicManager 

5.4.2 Efectos de sonido del rover — prioridad de implementación 

Efecto 

Trigger 

Prioridad 

Notas 

Ruedas sobre roca 

OnCollisionEnter2D + velocidad > 1 u/s 

MVP 

Pitch varía ±10% según velocidad. Dos variantes: suelo liso / suelo rugoso. 

Dash 

onDash() 

MVP 

Sonido mecánico corto (0,15 s). No se corta si hay otro dash inmediato. 

Daño recibido 

onDamageReceived() 

MVP 

Sonido metálico de impacto. Pitch baja 5% por fase de degradación activa. 

Activación de escaneo 

onScanActivate() 

MVP 

Tono electrónico ascendente. Diferente para objeto de lore vs escaneo sin resultado. 

Antena vibrando 

Idle con HW ≤ 73% 

Fase 2 

Loop de vibración mecánica sutil. Volumen aumenta conforme baja la SI. 

Alarma de batería crítica 

HW ≤ 40% 

Fase 4 

Pip de alarma cada 30 s. Distorsión digital leve. 

 

PARTE 6 

Scope, riesgos y cronograma 

Hitos (Sprints) · roles del equipo · planificación realista 

6. Scope y riesgos 

6.1 Scope del prototipo 

Los siguientes elementos constituyen el alcance mínimo del prototipo evaluable.  

Dos niveles completos y jugables de principio a fin, con sus respectivos beat maps implementados en Unity. 

Cinco tipos de enemigos y jefes con inteligencia artificial funcional y diferenciada por tipo. 

Sistema de cuatro upgrades desbloqueables con justificación narrativa verificable en el lore del juego. 

Sistema de degradación del hardware en seis fases con impacto mecánico, visual y de audio. 

Cinemática de apertura (storyboard animado en 2D o equivalente funcional). 

Secuencia del final completa: apagón, diez segundos de silencio, texto informativo y escena del astronauta en 2037. 

Menú principal funcional y pantalla de game over con opción de reinicio desde el último checkpoint. 

HUD diegético que refleja el estado del hardware del rover en tiempo real. 

Sistema de escaneo con un mínimo de seis objetos de lore real debidamente integrados. 

6.1.2 Milestones de entrega 

El GDD documenta en la Sección 6.1 el scope completo del prototipo (Milestone 2). Esta sección define un Milestone 1 intermedio con un mínimo viable jugable y evaluable, cuya función es proteger la evaluación sumativa y establecer una línea base de integración para el equipo. La relación entre ambos milestones es la siguiente: Milestone 1 valida que los sistemas core funcionan de forma independiente; Milestone 2 valida que todos esos sistemas funcionan integrados. 

Entregable 

Descripción de contenido comprometido 

Criterio de aceptación 

PlayerController funcional 

Carrera, salto (doble altura + coyote time + jump buffer) y dash implementados con los valores numéricos. La escalada de paredes es opcional en Milestone 1 — se entrega como parte del Upgrade Rueda Reforzada en Milestone 2. 

El rover se mueve sin jitter, frena en los tiempos especificados y el dash recorre 5 u en línea recta. Validado en playtesting con 3 personas externas al equipo. 

Nivel 1 — Zonas 1 y 2 jugables 

Las primeras dos zonas del Nivel 1 (Cuevas Bioluminiscentes) son navegables de principio a fin. La Zona 3 puede estar en construcción. El beat map de Zonas 1 y 2 está construido en Unity con tilemap funcional, sin placeholders de bloques de debug. 

El Level Designer puede iniciar y completar Zonas 1 y 2 sin errores de colisión ni caídas fuera de los límites del nivel. 

Un enemigo con IA funcional 

El Drone Patrullero implementado con su FSM de 3 estados: Patrullar → Detectar → Perseguir (+ Atacar si está en rango). Los valores de HP (60 pts), velocidad base (3,5 u/s) y velocidad en persecución (5,5 u/s) están operativos. No es necesario que el DDA esté activo en Milestone 1. 

El Drone Patrullero detecta al rover dentro de 8 u, lo persigue, dispara con cadencia de 1,5 s y regresa a patrulla si pierde línea de visión por más de 3 s. 

HUD mínimo funcional 

Barra de Integridad Estructural visible con valor numérico y color según la Tabla 11.3. Los slots de celda de energía son visibles (aunque la celda de energía no necesita estar distribuida en el nivel en Milestone 1). El indicador de estado de transmisión muestra SENAL PERDIDA. 

Al recibir daño, la barra de SI disminuye visualmente en tiempo real. El color cambia correctamente al cruzar los umbrales. 

Sistema de checkpoints básico 

Al menos un checkpoint funcional al final de la Zona 1. Al morir, el rover reaparece en el checkpoint sin restaurar SI. El texto CHECKPOINT REGISTRADO aparece en el HUD al activarlo. 

Morir y reaparecer en el checkpoint no produce errores ni estados inválidos del juego (sin duplicación del rover, sin pérdida de progreso de zona). 

Build ejecutable 

Build de Windows (x86_64) ejecutable sin errores críticos. No se requiere instalador — un .zip con los archivos de la build es suficiente. La build debe iniciarse, llegar al menú principal y poder jugar Zonas 1 y 2 sin crashes. 

El Technical Director entrega la build el viernes de la Semana 9. Cualquier error que impida iniciar el juego es un bloqueante de entrega. 

 6.2 Gestión de riesgos 

Riesgo 

Prob. 

Impacto 

Plan de mitigación 

Animaciones del rover tardadas en completarse 

Alta 

Alto 

Comenzar con placeholder sprites desde semana 1. El Animator Controller se implementa con formas geométricas hasta que el arte final esté listo. Animaciones definitivas en las últimas tres semanas. 

Scope creep: incorporación de features no comprometidos 

Alta 

Alto 

GDD congelado en semana 2. Todo cambio requiere aprobación unánime registrada. El Technical Director tiene autoridad para rechazar integraciones fuera de scope. 

NavMesh 2D complejo para implementar el flanqueo de drones 

Media 

Medio 

Usar Unity AI Navigation Package (com.unity.ai.navigation). Fallback: sistema de waypoints manuales predefinidos en el nivel. La lógica de flanqueo funciona con waypoints si NavMesh falla. 

Boss con tres fases fuera del tiempo disponible 

Media 

Alto 

Plan B definido: Fase 1 del Centinela es el mínimo viable para Milestone 2. Fase 2 y Fase 3 son stretch goals. El ending se activa al terminar cualquier fase activa del boss. 

Game feel del movimiento no satisfactorio 

Media 

Alto 

Playtesting desde semana 3. El PlayerController tiene prioridad sobre cualquier otro sistema. 

Conflictos de integración en el repositorio Git 

Media 

Medio 

Cada miembro trabaja en su propia escena de prueba (TestScene_[Nombre]). La integración a la rama main ocurre únicamente los viernes bajo supervisión del Technical Director. 

Cinemáticas con costo de producción mayor al estimado 

Baja 

Medio 

Fallback definido: storyboard animado en 2D como reemplazo de cinemáticas renderizadas. La transmisión de Spirit tiene versión mínima: panel de texto estático con audio de ambiente. 

6.3 Cronograma semana a semana por rol 

La siguiente tabla define el orden recomendado de implementación para cada miembro del equipo. Las dependencias críticas están marcadas: ningún módulo con dependencia debe implementarse antes de que el módulo del que depende esté en estado funcional básico. 

Semana 

Gameplay Programmer 

AI Programmer 

Level Designer 

Artist & Animator 

Technical Director 

1–2 (Sprint 0) 

PlayerController: movimiento y salto base 

Waypoints de patrullaje básicos en TestScene 

Estructura de carpetas del proyecto. TestScene del nivel 1 

Sprites placeholder del rover (rectángulo gris) 

Setup de Unity: escenas, capas, Git, estructura de proyecto 

3–4 

Dash y wall jump. DegradationSystem Fases 1–2 

Ser Bioluminiscente: FSM completo (3 estados) 

Nivel 1 Zonas 1–2 con tiles placeholder 

Animaciones MVP del rover: Idle, Run, Jump 

GameManager: estados Playing y GameOver. HUDManager básico 

5–6 

DegradationSystem Fases 3–6. Daño por caída 

Drone Patrullero: FSM completo. NavMesh2D validado 

Nivel 1 Zonas 3–4 y sala del Leviatán 

Animaciones MVP del rover: Dash, Damage, Death 

AudioManager: estado EXPLORATION y TENSION. Valida NavMesh 

7–8 

Upgrades: Rueda Reforzada y Escaneo Mejorado 

Drone Detector: FSM completo con flanqueo y DDA 

Nivel 1 completo jugable. Checkpoint y celda de energía 

Arte final Nivel 1: tiles, fondos y parallax 

HUDManager completo. Integración del DegradationSystem en HUD 

9 — Sumativa 1 

Integración completa y playtesting. Bug fixing 

AIManager con DDA activo. Integración con LevelManager 

Nivel 1 funcional con enemigos y upgrades integrados 

Sprites finales rover + Ser Bioluminiscente + Drone Patrullero 

Build ejecutable de Milestone 1. Presentación 

10–11 

ScanSystem: radio, triggers y UI de lore 

Centinela Secundario y Leviatán: FSM completo 

Nivel 2 Entrada + Ala A. Checkpoints y transición 

Arte del rover degradado Fases 3–6. Drone Detector 

CinematicManager: flashbacks FB-01 a FB-04. Escena de Spirit 

12–13 

Upgrades: Escudo de Plasma y Batería EMP 

Centinela Principal: Fase 1 y Fase 2 

Nivel 2 Ala B y Ala C. Objetos escaneables SC-04 a SC-06 

Arte final Nivel 2. Tiles del Relicto. Sprites enemigos Niv. 2 

CinematicManager: secuencia final Escenas 1–7. Menú principal 

14–15 

Integración de todos los sistemas. Playtesting 

Centinela Principal Fase 2 completa. Optimización de IA 

Nivel 2 completo jugable. Verificación de beat map 

Animaciones finales de todos los enemigos. VFX de partículas 

AudioManager completo: todos los estados y efectos de sonido 

16 — Sumativa 2 

Bug fixing y optimización final 

Bug fixing de IA. Fase 3 del Centinela si el tiempo lo permite 

Verificación de niveles. Objetos de lore integrados 

Arte final completo. Revisión de paleta y consistencia 

Build final ejecutable. Integración y presentación 

Bibliografía y referentes 

La bibliografía del proyecto está organizada en cuatro categorías: fuentes académicas del programa, referencias técnicas de implementación, referentes de diseño de juego y referentes narrativos o cinematográficos. Esta separación evita confusión categórica entre obras de distinta naturaleza. 

Bibliografía del programa de la asignatura 

Las siguientes fuentes son las indicadas en el programa oficial de Proyecto de Video Juegos (TGP09BFT01). Son las referencias primarias para la justificación académica de las decisiones de diseño de este GDD. 

Tipo 

Referencia completa (APA 7.ª ed.) 

Básica — libro 

Vallejo, D. (2019). Desarrollo de videojuegos: un enfoque práctico. España: EdLibrix. 

Básica — libro 

Flavell, L. (2018). Beginning Blender: Open Source 3D Modeling, Animation, and Game Design. Apress. 

Complementaria — web 

Unity Technologies. (2024). Unity Documentation. https://docs.unity3d.com 

Fuentes científicas — datos de la misión Opportunity 

Los datos reales de la misión utilizados en el juego provienen exclusivamente de las siguientes fuentes primarias. Cada dato de lore del juego debe poder referenciarse a una de estas fuentes. 

Tipo 

Referencia completa (APA 7.ª ed.) 

Fuente primaria — NASA 

NASA Jet Propulsion Laboratory. (2019). Mars Exploration Rover Mission: Opportunity. https://mars.nasa.gov/mer/home/ 

Fuente primaria — NASA 

NASA JPL. (2019). NASA's Opportunity Rover Mission on Mars Comes to End. NASA Press Release. https://www.nasa.gov/press-release/nasa-s-opportunity-rover-mission-on-mars-comes-to-end 

Fuente primaria — NASA 

Squyres, S. W., et al. (2004). The Opportunity Rover's Athena Science Investigation at Meridiani Planum, Mars. Science, 306(5702), 1698–1703. https://doi.org/10.1126/science.1106171 

Datos de misión — NASA 

NASA JPL. (2018). Mars Dust Storm of 2018: Record-Breaking Event Is Winding Down. https://www.jpl.nasa.gov/news/mars-dust-storm-of-2018 

Misión Spirit — NASA 

NASA JPL. (2010). NASA's Spirit Rover Completes Mission on Mars. NASA Press Release. https://www.nasa.gov/mission_pages/mer/news/mer20100525.html 

Referencias técnicas de implementación 

Las siguientes fuentes son referencia técnica para la implementación de los sistemas específicos del proyecto. 

Sistema 

Referencia 

Física 2D y game feel 

Celeste (2018) — Maddy Thorson y Noel Berry. GDC Talk: 'Celeste and TowerFall Physics' (2019). https://youtu.be/yorTG9at90g 

Máquinas de estados finitos (FSM) para IA 

Buckland, M. (2004). Programming Game AI by Example. Wordware Publishing. 

NavMesh 2D en Unity 

Unity Technologies. (2024). AI Navigation Package Documentation (com.unity.ai.navigation). https://docs.unity3d.com/Packages/com.unity.ai.navigation@1.1 

Animator Controller en Unity 

Unity Technologies. (2024). Animator Controller documentation. https://docs.unity3d.com/Manual/class-AnimatorController.html 

ScriptableObjects como arquitectura de datos 

Unity Technologies. (2017). Unite Austin 2017 — Game Architecture with Scriptable Objects. https://youtu.be/raQ3iHhE_Kk 

Referentes de diseño de juego 

Los siguientes videojuegos son referentes directos de mecánicas, estructura y experiencia de juego. Están citados como obras de diseño analizadas, no como código o activos utilizados. 

Referente 

Estudio / Año 

Elemento referenciado 

Metroid Dread 

Nintendo EPD / MercurySteam (2021) 

Estructura Metroidvania. Movimiento de plataformero de alta responsividad. IA de E.M.M.I. como referente de tensión mediante comportamiento telegráfico. 

Hollow Knight 

Team Cherry (2017) 

Atmósfera construida desde arte y audio sin exposición textual. Lore ambiental. Enemigos con comportamiento consistente y learnable. 

Celeste 

Extremely OK Games (2018) 

Game feel del movimiento: coyote time, jump buffer, responsividad. Curva de dificultad basada en comprensión progresiva del jugador. 

Ori and the Blind Forest 

Moon Studios (2015) 

Narrativa emocional sin diálogo. Uso del movimiento como lenguaje narrativo. Arte de fondos con profundidad 2.5D y parallax. 

Referentes narrativos y cinematográficos 

Las siguientes obras son referentes de tono narrativo, diseño emocional o lenguaje visual. Se distinguen explícitamente de los referentes de diseño de juego dado que ninguna de ellas es un videojuego. 

Referente 

Tipo / Año 

Elemento referenciado 

WALL-E (primer acto) 

Película — Pixar / Andrew Stanton (2008) 

Ternura en la rutina solitaria. Personaje mudo cuya emoción se construye desde la acción, no el diálogo. Soledad representada como estado sagrado. Referente de tono y construcción de personaje, no de mecánicas. 

Moon 

Película — Duncan Jones (2009) 

Aislamiento como condición narrativa sostenida. Paleta visual de entornos industriales con presencia emocional. 

Gravity (2013) 

Película — Alfonso Cuarón (2013) 

Tensión en entorno espacial. Uso del silencio como recurso dramático. Escenas de emergencia con economía de información al espectador. 

─── Fin del documento ─── 
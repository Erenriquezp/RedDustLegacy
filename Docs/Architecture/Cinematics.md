# Cinemáticas y victoria — Guía de implementación y diseño (S05 T4)

> Ref. GDD: §1.3 (victoria) · §12.1 (arco) · §12.3 (final) · §12.4 (Spirit) · §11.3 (flashbacks) · Estado: por implementar

Regla general de tono: **el rover no es consciente del drama**. Ninguna cinemática subraya la emoción con música épica ni diálogos — solo telemetría, imágenes y silencio. La tristeza la pone el jugador (GDD §12.1: los cuatro actos van de "esperanza silenciosa" a "pérdida + redención").

## 1. Arquitectura

```
CinematicManager.cs (nuevo — Scripts/Cinematics/; singleton de escena, no DontDestroyOnLoad)
 ├─ API: PlayIntro() · PlayFlashback(id) · PlaySpiritScene() · PlayFinalSequence()
 ├─ congela el juego vía GameManager.SetCinematic(true/false)  ← ya existe
 ├─ música: AudioManager.SetMusicState(Cinematic) al entrar; restaurar al salir (T6)
 ├─ canvas overlay propio (sorting por encima del HUD), fades con CanvasGroup
 └─ timings con corrutinas en TIEMPO REAL (WaitForSecondsRealtime — SetCinematic no toca
    el timeScale hoy, pero el overlay no debe depender de él)
```

**Data-driven** (patrón del proyecto): `FlashbackSO` — id, imagen, líneas (texto + segundos), clip de audio, `siMaximo` (GDD §11.3: FB-05 exige SI ≤29%, FB-06 ≤9%), `saltable`. Las piezas grandes (intro, Spirit, final) son secuencias codificadas con sus assets serializados — no vale la pena generalizarlas.

**Skip:** piezas saltables muestran `[ESPACIO] OMITIR` discreto (esquina, alfa 0,5) tras 2 s. **No saltables:** Spirit y "El silencio" (GDD lo exige) — sin prompt, para no invitar a intentarlo.

## 2. Cinemática de introducción (NUEVA — no está en el GDD)

Objetivo: dar el contexto de la misión en **<60 s**, con costo de producción mínimo (texto + 1–2 imágenes estáticas + audio). Siempre saltable — el que reinicia partida no la vuelve a sufrir. Se dispara desde `NUEVA MISION`, antes de la pantalla de carga del N1.

| # | Beat | Contenido | Duración |
|---|------|-----------|----------|
| 1 | Telemetría | Negro. Aparece línea a línea la telemetría de la pantalla de carga (GDD §12.2): `SOL ACTUAL: 5.847` · `ULTIMA TX RECIBIDA: SOL 5.111` (rojo) · `SEÑAL: PERDIDA` (rojo) · `INTENTOS DE RECONTACTO: 1.034` (rojo) | ~10 s |
| 2 | La tormenta | Imagen estática (Marte cubierto por la tormenta) + texto mono: `10 DE JUNIO DE 2018. UNA TORMENTA GLOBAL DE POLVO CUBRE MARTE.` — viento como único audio | ~8 s |
| 3 | El beat | Texto sobre negro, dos líneas con pausa entre ellas: `LA NASA DECLARO CONCLUIDA LA MISION EL 13 DE FEBRERO DE 2019.` … `OPPORTUNITY NUNCA RECIBIO EL MENSAJE.` | ~10 s |
| 4 | Despertar | Última línea en **verde** (única del bloque): `MODO: EXPLORACION AUTONOMA — PROTOCOLO: CONTINUAR` → fade al gameplay del N1, HUD encendiéndose con SI 74% | ~6 s |

El contraste rojo (Tierra: misión muerta) / verde (rover: misión activa) ES la premisa del juego — es el mismo contraste de la tabla §12.2. No añadir nada más: ni logo animado, ni narrador.

## 3. Flashbacks FB-01…06 (overlay ligero, 8–12 s)

- **Formato:** viñeta única — imagen en B/N o sepia con marco de telemetría (`SOL 339 — REGISTRO DE MISION`), 1–2 líneas monoespaciadas debajo, audio característico (ruedas sobre regolito, viento, estática). Contenidos y triggers: tabla GDD §11.3.
- Pausa el juego (`SetCinematic`), auto-cierra al terminar o con `ESPACIO` (saltables).
- Entrada: fade blanco corto (0,15 s — un "fogonazo de memoria"), salida: fade negro 0,3 s.
- `PlayFlashback(id)` valida el SI máximo de la tabla; si el SI actual es mayor, no se reproduce (queda pendiente hasta que el arco alcance ese punto — GDD ata memoria a degradación).
- FB-04 (maratón) es pasivo: lo dispara un odómetro acumulado en `PlayerController`/save — **stretch**, no bloquea T4.

## 4. Escena de Spirit (SC-06 — Ala C) — GDD §12.4, copiar literal

Pantalla dividida (izq: escaneo de Opportunity · der: telemetría de Spirit), 45–55 s, **no saltable**. Batería de Spirit 12%→0% en 40 s con temperatura cayendo; el zumbido electrónico muere exactamente cuando la batería llega a 0; último dato: `TEMPERATURA INTERNA: -40C. BATERIA: 0%. MODO: —`. Es el único momento del juego en que Opportunity "conoce" a otro rover: el silencio posterior debe durar 2–3 s antes de devolver el control.

## 5. Victoria — secuencia de transmisión (GDD §1.3)

Flujo tras matar al boss final:

```
CentinelaPrincipalAI.OnDefeated  ← ya existe
 → BossArenaTrigger del Relicto (T3) abre la sala/antena de transmisión
 → el jugador LLEGA y ACTIVA la antena (interacción = mantener E — reusar el gesto de escanear)
 → transmisión jugable-cinemática → PlayFinalSequence()
```

- **La transmisión es un beat interactivo, no un cutscene:** barra `FRAGMENTOS TRANSMITIDOS: 0/891` subiendo mientras `ENERGIA` cae hacia 0% — el jugador mantiene la interacción y VE que no le va a alcanzar. Al llegar energía a 0% se congela la imagen: `ENERGIA INSUFICIENTE. FRAGMENTOS: 341/891` (GDD §12.3 esc. 2).
- **No es fallable en el prototipo:** llegar a la antena garantiza ≥1 fragmento (la "condición" del GDD es dramática, no un fail-state). Sin música (GDD).
- El `LevelExit` actual de Level02 (que encadena al menú) se reemplaza por este flujo.

## 6. Secuencia final — 7 escenas (GDD §12.3, fuente de verdad)

Implementar la tabla del GDD tal cual. Recordatorios duros e innegociables:

1. **El último intento** — solo audio mecánico del brazo (falla al 1.º, logra al 2.º con delay 1,2 s).
2. **Transmisión interrumpida** — ver §5 (es la misma escena, continúa desde ahí).
3. **Apagón de sistemas** — log línea a línea; la última, en verde: `PROTOCOLO DE MISION: COMPLETADO`.
4. **El silencio** — **negro absoluto, 10 segundos EXACTOS, sin música, sin texto, sin SFX, no saltable.** T6: silenciar todos los buses del mixer, no solo la música.
5. **El informe** — texto blanco sobre negro, fade 0,8 s/línea (JPL recibió 341 fragmentos → evidencia de agua y vida → misión humana 2037).
6. **Marte, 2037** — astronauta aterriza, camina, se arrodilla, toca el rover en el polvo. Sin rostro. **Nunca habla** (regla de oro del GDD — sin excepciones).
7. **Plano final** — astronauta sostiene el rover contra el cielo naranja; fade a negro 5 s; texto de cierre **literal del GDD §12.3** (incluye *«My battery is low and it's getting dark.»*).

Tras el texto: fade → menú principal (S06: borrar guardado aquí). Toda la secuencia no es saltable — es el pago del juego completo; si en la beta resulta un problema, se discute con datos.

## 7. Assets mínimos requeridos (encargar a arte YA)

| Pieza | Assets |
|-------|--------|
| Intro | 1 imagen (tormenta global) |
| Flashbacks | 6 imágenes (blueberries, Endurance, arcillas, odómetro, estática de tormenta, Spirit B/N) |
| Spirit | layout de pantalla dividida (UI, sin arte nuevo) |
| Final | 3–4 imágenes (astronauta: llegada, arrodillado, plano final) + SFX brazo mecánico/estática/viento |

Todo lo demás es texto monoespaciado sobre negro — deliberadamente barato y coherente con el tono terminal.

## 8. Orden de implementación y aceptación

1. `CinematicManager` + overlay + `SetCinematic` + skip → probar con un flashback dummy.
2. Flashbacks data-driven (`FlashbackSO` ×6) + handoff desde `ScanSystem` ([ScanSystem.md](ScanSystem.md) §2.3).
3. Intro (beats 1–4) enganchada a `NUEVA MISION`.
4. Transmisión + secuencia final enganchadas a `OnDefeated`.
5. Spirit (SC-06).

- [ ] Durante cualquier cinemática el rover no responde al input y los enemigos no actúan; al salir, todo se restaura.
- [ ] `ESPACIO` salta intro y flashbacks; Spirit y "El silencio" no se pueden saltar.
- [ ] "El silencio" dura 10,0 s medidos y no suena NADA (verificar buses).
- [ ] Matar al Centinela Principal → antena → transmisión → final completo → menú, sin pasar por `LevelExit`.
- [ ] Morir no es posible durante la transmisión (el combate ya terminó; desactivar spawns residuales).

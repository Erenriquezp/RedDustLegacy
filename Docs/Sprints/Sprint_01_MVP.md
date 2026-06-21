# Sprint 01 — MVP Base

> **Estado:** ✅ Completado · **Ref. GDD:** §2, §3, §14

Movimiento completo del rover, menú funcional, estructura de escenas y audio base.

## Entregado

- **PlayerController** — controlador cinemático 2D (gravedad manual, `gravityScale = 0`): carrera con accel/decel suelo-aire, salto variable, coyote time, jump buffer, dash con freeze-frames + dash aéreo, wall slide / wall jump, escaneo. Expone eventos `OnGroundedChanged`, `OnJumped`, `OnDashed`, `OnWallJumped`, `OnWallSliding`.
- **RoverStatsSO** — tuning del rover en `RoverStats_Default.asset` (el `.asset` manda sobre los defaults del `.cs`).
- **PlayerAnimatorController** — bridge desacoplado (params `Speed`, `IsGrounded`, `IsJumping`, `IsScanning`) + flip de sprite.
- **PlayerAudioController** — crossfade idle↔walk con dos AudioSources, one-shots, fade-out en aire.
- **AudioManager** y **SceneLoader** — singletons `DontDestroyOnLoad`.
- **MainMenu** funcional (Play/Quit) + escenas `Dev`, `Level01`, `Level02`.
- Physics 2D: `Fixed Update`, material `NoFriction`, layer `Ground`.

## Pendientes que arrastra (deuda técnica)

| Tema | Detalle | Sigue en |
|------|---------|----------|
| Bug audio | `PlayerAudioController.cs:39` — `OnDashed += PlayDamageSound` debería ser un SFX de dash propio o removerse | Sprint 02 |
| Bindings vs GDD §2 | Jump = `W`/`↑` (GDD: `Espacio`), Scan = `E` (GDD: `F`) | Sprint 02 |

Detalle de arquitectura: [PlayerController.md](../Architecture/PlayerController.md) · [AnimatorSetup.md](../Architecture/AnimatorSetup.md)

So then going back to our trigger, `MapEvent` is one method we actually have to write. Turn the notification into that output shape.

Every `MapEvent` returns a `TriggerEvent` with the output shape, and it should include an idempotency key so that Automate can tell if this is a duplicate event or not. That way a retry, or the same notification arriving twice, can't double-fire the automation and book the same trip twice.

The other fields here, `TriggerAlias`, `InitiatorType` and `Output`, control who this looks like it came from and what it carries. `TriggerAlias` is the pub/sub key — there's no target automation set here, so this event fans out to every published automation subscribed to it, not just one. `InitiatorType` is one of a handful of well-known values — system, user, webhook, scheduled, replay — and it's what lets Automate tell a human clicking "run now" apart from an automatic fire, which matters for things like the circuit breaker deciding whether to allow a run through. `Output` is just the payload we already built.

That's the entire job of this method. No orchestration, no branching logic, none of that lives here — that's coming, and it lives somewhere much more visual.

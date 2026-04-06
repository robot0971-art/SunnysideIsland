## Context

The current project already contains the main pieces needed for consumable gameplay: `InventorySystem`, `ItemData`, `ItemConsumptionService`, and the survival systems for hunger, health, and stamina. The problem is that the behavior contract is spread across item data, recipe/fish fallback lookups, UI entry points, and scene-level DI registration, which makes future expansion risky.

This change touches multiple modules at once: inventory, survival, UI, game data, and save/load verification. The design therefore needs to make the gameplay flow explicit before implementation expands the feature set.

## Goals / Non-Goals

**Goals:**
- Make consumable item effects come from a single authoritative gameplay data source.
- Define a consistent use-validation rule that depends on current player survival state.
- Keep the current service-oriented flow, but expose explicit consumption results through events instead of hidden side effects.
- Ensure inventory deduction, stat restoration, HUD refresh, and save/load verification can be reasoned about as one flow.

**Non-Goals:**
- Adding buff/debuff duration systems, status ailments, or animation state machines.
- Redesigning the full inventory UI or quick-slot UX.
- Reworking crafting and cooking data models beyond their relationship to consumable outcome data.
- Introducing new third-party dependencies or replacing the existing DI container/event system.

## Decisions

### 1. Consumable effect data will be standardized on `ItemData`

`ItemData` will be the authoritative source for hunger, health, and stamina restoration values. Recipe and fish data may continue to describe acquisition or production, but not the final runtime effect contract.

Rationale:
- The codebase already has explicit consumable effect fields in `ItemData`.
- A single data source avoids divergent balancing values across item, recipe, and fish tables.
- It reduces future branching when adding more effect types.

Alternative considered:
- Keep recipe/fish fallback lookups in `ItemConsumptionService`.
  Rejected because it preserves hidden coupling and makes balance validation harder.

### 2. The existing `ItemConsumptionService` will remain the orchestration point

The service will continue to coordinate the flow: validate use, remove inventory quantity, apply survival changes, and publish an explicit result event.

Rationale:
- The project already has a dedicated service and DI registration for consumable use.
- Central orchestration avoids duplicating use logic in UI panels or survival systems.
- This keeps implementation small while still improving the architecture.

Alternative considered:
- Split immediately into separate resolver/applier services.
  Deferred because the current scope only needs one stable orchestration point, not a full effect framework.

### 3. Consumable use will be gated by actual recoverable value

A consumable will only be usable if it can restore at least one relevant stat at the time of use. If hunger, health, and stamina are all already full relative to the item's effect set, use must be rejected.

Rationale:
- Prevents wasting consumables without player intent.
- Keeps the UI rule easy to explain and test.
- Matches the actual player need rather than only checking item type.

Alternative considered:
- Allow all consumables to be used regardless of current stats.
  Rejected because it creates accidental waste and weakens feedback.

### 4. Consumption outcomes will be surfaced through explicit events

After successful use, the system will publish a dedicated consumption event carrying item identity, quantity, and resolved recovery values. Downstream systems such as HUD, quest tracking, achievements, and audio can subscribe to this without depending on inventory or survival internals.

Rationale:
- Existing stat change events tell observers that a stat changed, but not which item caused it.
- A dedicated event improves extensibility without changing core systems again later.

Alternative considered:
- Let consumers infer item usage by observing inventory and stat changes separately.
  Rejected because the inference is fragile and race-prone.

### 5. Verification will include save/load and scene registration paths

The implementation plan will explicitly verify that inventory and survival state remain consistent after save/load. If save registration is incomplete, it must be corrected as part of this change or treated as a blocking follow-up.

Rationale:
- Consumable gameplay is player-state mutation; failing to persist that state breaks trust immediately.
- Current save registration appears incomplete across systems, so it cannot be ignored during rollout.

## Risks / Trade-offs

- [Recipe/fish fallback removal exposes missing item data] -> Audit consumables and fill missing `ItemData` effect values before relying solely on item definitions.
- [Use-validation rules may conflict with current player expectation] -> Surface clear disabled-state or failure feedback in the inventory/detail UI.
- [Adding a new event increases event-subscriber surface area] -> Keep payload small and document the event as a post-success notification only.
- [Save registration fixes may expand scope] -> Treat persistence verification as mandatory, but keep schema/data migration minimal.
- [Current UI triggers could consume items from multiple entry points] -> Reuse one service path for button and double-click actions to avoid duplicated logic.

## Migration Plan

1. Add and document the new consumable-survival requirements.
2. Consolidate runtime consumable effect lookup around `ItemData`.
3. Adjust `CanConsume`/use validation to reflect real recoverable value.
4. Publish a dedicated consumption event after successful application.
5. Update UI feedback to use the new contract while retaining existing stat-bar updates.
6. Verify save/load registration and persistence behavior for inventory and survival systems.

Rollback is low risk because the feature is internal to the gameplay loop. If necessary, the project can temporarily revert to the previous `ItemConsumptionService` behavior while keeping the new specs for follow-up.

## Open Questions

- Should a consumable with mixed effects be usable if only one of its affected stats is below max? The recommended answer is yes.
- Should item-use failures display a toast immediately, or is button disable state sufficient for the first iteration?
- Should the dedicated consumption event include both requested recovery values and actual applied deltas after clamping?

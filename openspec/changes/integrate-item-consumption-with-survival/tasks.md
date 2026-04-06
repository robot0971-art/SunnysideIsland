## 1. Data Contract Cleanup

- [ ] 1.1 Audit current consumable items and identify any hunger, health, or stamina values that are still inferred from recipe or fish data instead of `ItemData`
- [ ] 1.2 Update the runtime consumable lookup path so `ItemData` is treated as the authoritative source for consumable restoration values
- [ ] 1.3 Remove or isolate fallback behavior that hides missing consumable effect data in recipe or fish tables

## 2. Consumption Flow

- [ ] 2.1 Update consumable validation so an item can only be used when at least one affected survival stat can actually be restored
- [ ] 2.2 Ensure successful consumable use deducts inventory quantity and applies hunger, health, and stamina restoration through one service path
- [ ] 2.3 Add a dedicated post-success consumable outcome event containing item identifier, quantity, and resolved restoration values

## 3. UI and Feedback Integration

- [ ] 3.1 Update inventory/detail-panel use logic to rely on the revised consumable validation contract
- [ ] 3.2 Add or refine player-facing feedback for successful and rejected consumable use
- [ ] 3.3 Verify HUD stat bars continue to refresh through survival change events after consumable use

## 4. Persistence and Verification

- [ ] 4.1 Verify `InventorySystem`, `HealthSystem`, `HungerSystem`, and `StaminaSystem` are registered with save/load flow where required
- [ ] 4.2 Test consume-save-load scenarios to confirm consumed quantity and restored survival values persist correctly
- [ ] 4.3 Run a final gameplay verification pass covering inventory use button, double-click use, full-stat rejection, and mixed-stat restoration cases

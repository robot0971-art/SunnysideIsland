## Why

The project already has separate inventory, consumable item data, and survival systems, but the gameplay contract between them is only partially defined and is not robust enough for expansion. A formal change is needed now so item consumption can become a stable gameplay loop instead of a collection of ad hoc links between UI, inventory, and survival code.

## What Changes

- Define a formal consumable-to-survival flow covering item use, validation, inventory deduction, and stat restoration.
- Standardize consumable effect data so hunger, health, and stamina recovery are sourced consistently from item definitions.
- Define player-facing rules for when a consumable can or cannot be used based on current survival state.
- Add explicit consumption result signaling so UI, quests, achievements, and feedback systems can react without coupling to implementation details.
- Define verification requirements for save/load behavior and HUD updates after item consumption.

## Capabilities

### New Capabilities
- `consumable-survival-integration`: Defines how consumable items restore hunger, health, and stamina, how use validity is determined, and how consumption outcomes are exposed to the rest of the game.

### Modified Capabilities
- None.

## Impact

- Affected code: `Assets/Scripts/Inventory`, `Assets/Scripts/Survival`, `Assets/Scripts/UI/Inventory`, `Assets/Scripts/UI/HUD`, `Assets/Scripts/GameData`, and save registration flow in core scene/game bootstrap logic.
- Affected data: `ItemData` consumable fields and related recipe/fish content usage rules.
- Affected systems: inventory interaction, player survival state, HUD refresh, save/load verification, and downstream event consumers such as quests or achievements.

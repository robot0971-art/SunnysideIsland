## ADDED Requirements

### Requirement: Consumable effects are resolved from item definitions
The system SHALL resolve hunger, health, and stamina restoration for a consumable item from the item's authoritative gameplay definition before the item can be used.

#### Scenario: Consumable has explicit effect values
- **WHEN** a player attempts to use an item whose item definition includes hunger, health, or stamina restoration values
- **THEN** the system SHALL use those item definition values as the runtime consumable effect

#### Scenario: Non-consumable item is checked for use
- **WHEN** a player attempts to use an item that is not defined as a consumable item
- **THEN** the system SHALL reject the consumable-use request

### Requirement: Consumable use is validated against current survival state
The system SHALL only allow a consumable item to be used when it can restore at least one relevant survival stat for the player at the moment of use.

#### Scenario: At least one affected stat is below maximum
- **WHEN** a consumable can restore hunger, health, or stamina and at least one of those affected stats is below its maximum value
- **THEN** the system SHALL allow the consumable to be used

#### Scenario: All affected stats are already full
- **WHEN** a consumable's affected hunger, health, and stamina values are all already at maximum for the player
- **THEN** the system SHALL reject the consumable-use request

### Requirement: Successful consumable use mutates inventory and survival state together
The system SHALL remove the consumed quantity from inventory and apply the resolved restoration values to the player's survival state as one successful use flow.

#### Scenario: Successful use from inventory
- **WHEN** a player successfully uses a consumable item that exists in inventory
- **THEN** the system SHALL deduct the consumed quantity from inventory
- **AND** the system SHALL apply the resolved hunger, health, and stamina restoration to the corresponding survival systems

#### Scenario: Inventory quantity is insufficient
- **WHEN** a player attempts to use a consumable item without sufficient inventory quantity
- **THEN** the system SHALL reject the consumable-use request
- **AND** the system SHALL leave survival values unchanged

### Requirement: Successful consumable use emits a dedicated outcome event
The system SHALL publish a dedicated consumable outcome event after a successful use so that UI and dependent systems can react without inspecting internal service state.

#### Scenario: Successful consumable use publishes result
- **WHEN** a consumable item is successfully used
- **THEN** the system SHALL publish an event containing the consumed item identifier, quantity, and resolved restoration outcome

### Requirement: Survival and HUD state remain consistent after consumable use
The system SHALL update observable survival state after successful consumable use so HUD and related listeners reflect the new values without requiring manual refresh logic beyond existing event subscriptions.

#### Scenario: HUD reflects restored values
- **WHEN** a consumable restores hunger, health, or stamina
- **THEN** the corresponding survival change events SHALL be emitted
- **AND** HUD stat displays SHALL reflect the updated values

### Requirement: Consumable state changes are verifiable across save and load
The system SHALL preserve inventory quantity and resulting survival values after save and load for consumable use flows covered by the survival systems.

#### Scenario: Save after consuming an item
- **WHEN** a player consumes an item, saves the game, and then loads that save
- **THEN** the consumed inventory quantity SHALL remain deducted
- **AND** the resulting hunger, health, and stamina values SHALL be restored from save data

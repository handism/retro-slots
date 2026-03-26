# TODO: Implementation vs. Documentation Discrepancies

This document tracks the differences between the requirements/design documents and the current implementation.

## 1. Missing Features

- [ ] **Game Explanation (Help) Modal**: The Settings menu should include a "Game Explanation" (ゲーム説明) button and modal as per `requirements.md` section 2.7 and 2.8.
- [ ] **Auto-Spin Count Selector**: The HUD should allow selecting from 10, 25, 50, or 100 auto-spins as specified in `requirements.md` section 2.7. Currently, it seems to have a fixed or limited selection.
- [ ] **Spin Button "Stop" State**: The spin button should change to a "STOP" button during a spin to allow for early stopping (early stop logic is implemented in `SpinManager`, but the UI does not reflect the state change).

## 2. Specification Deviations

- [ ] **Spin Button Interactability**: `UIManager.SetSpinButtonInteractable(false)` is called during a spin, which prevents the user from clicking it to "Stop" early. It should remain interactable and change its label/visuals.
- [ ] **Volume Reset Logic**: Verify if the volume should be reset when the "Reset Coins" button is pressed. Currently, only coins are reset.

## 3. Verification Required

- [ ] **Letterbox Support**: Verify that the UI correctly handles 16:9 aspect ratio with letterboxing on various screen sizes as required in `requirements.md` section 3.2.
- [ ] **Skip Mode Sound Effects**: Ensure that `SEType.ReelStop` plays correctly and doesn't sound distorted/overlapping when all reels stop simultaneously in skip mode.
- [ ] **Max Coins Display**: Verify that the coin display handles the maximum value (9,999,999) gracefully without UI overlap or clipping.

## 4. Potential Improvements

- [ ] **Sequential Line Highlight Timing**: The 500ms delay between lines in `UIManager` might feel slow if there are many winning lines. Consider making this adjustable.
- [ ] **Bonus Round Reward Weights**: Verify that the weights in `PayoutTableData` for bonus rewards result in the intended RTP distribution.
- [ ] **Save Data Tampering**: While checksums are implemented, consider adding more obfuscation if security becomes a higher priority (though currently out of scope for a local game).

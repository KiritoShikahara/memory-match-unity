# Tasks: 神経衰弱（2〜4人対戦・CPU戦）

**Input**: `specs/001-memory-match/` の spec.md / plan.md
**Tests**: 必須（FR-010）。各 Story はテストを先に書き、`unity-cli test` で Red を確認してから実装する。

## Phase 1: Setup

- [x] T001 Unity 6000.6.4f1 プロジェクト作成、`Packages/manifest.json` に test-framework 追加
- [x] T002 unity-cli Bridge 導入（`unity-cli setup`）、`system ping` で版一致を確認
- [x] T003 asmdef 作成: `Assets/Scripts/Core/MemoryMatch.Core.asmdef`、`Assets/Tests/EditMode/MemoryMatch.Tests.EditMode.asmdef`

## Phase 2: User Story 1 - ペアでカードを取得する (P1)

- [x] T010 [US1] テスト: `Assets/Tests/EditMode/DeckTests.cs`（16枚・各2枚・シード再現）
- [x] T011 [US1] テスト: `Assets/Tests/EditMode/MemoryGameTests.cs`（初期状態・一致で取得・不一致で戻る・二重選択/3枚目不可）
- [x] T012 [US1] Red 確認（コンパイル失敗/失敗）
- [x] T013 [US1] 実装: `Assets/Scripts/Core/Card.cs`、`Deck.cs`、`MemoryGame.cs`
- [x] T014 [US1] Green 確認

## Phase 3: User Story 2 - 手番を回して対戦する (P1)

- [x] T020 [US2] テスト: `MemoryGameTests.cs` に手番移動・一致で継続・終了と勝者・引き分け・終了後不可・参加1席は例外
- [x] T021 [US2] Red 確認 → 実装 `MemoryGame.cs`、`PlayerSlot.cs` → Green 確認

## Phase 4: User Story 3 - CPU と対戦する (P1)

- [x] T030 [US3] テスト: `Assets/Tests/EditMode/CpuPlayerTests.cs`（既知ペアを取る・相方を選ぶ・選択可能なカードのみ・未見は知らない・記憶率）
- [x] T031 [US3] Red 確認 → 実装 `Assets/Scripts/Core/CpuPlayer.cs` → Green 確認

## Phase 5: User Story 4 - 席設定・やり直し・画面 (P2)

- [x] T040 [US4] `Assets/Scripts/Game/MemoryMatchApp.cs`（設定画面・盤面・HUD・入力・CPUタイマー・R）
- [x] T041 [US4] `Assets/Scenes/MemoryMatch.unity` 作成、Build Settings に登録

## Phase 6: 検証

- [x] T050 全テスト再実行（件数・合否を照合）
- [x] T051 Play モードで 人1+CPU1 / CPUのみ4人 を実行し `specs/001-memory-match/verification.md` に記録（`Assets/Tests/PlayMode/MemoryMatchAppTests.cs`）

# Implementation Plan: 神経衰弱（2〜4人対戦・CPU戦）

**Branch**: `001-memory-match` | **Date**: 2026-11-24 | **Spec**: [spec.md](./spec.md)

## Summary

ルールとCPU思考を UnityEngine 非依存の `MemoryMatch.Core` に置き、EditMode テストで TDD する。表示・入力は IMGUI の `MemoryMatchApp`（MonoBehaviour 1つ）が Core を呼ぶだけにする。テストと Editor 操作は unity-cli で行う。

## Technical Context

**Language/Version**: C# 9 / Unity 6000.6.4f1
**Primary Dependencies**: Unity Test Framework 1.8.0、unity-cli 0.19.1 Bridge
**Testing**: NUnit EditMode（`unity-cli test --mode editmode`）
**Target Platform**: Unity Editor（Play モード）
**Project Type**: 2D ゲーム（単一 Unity プロジェクト）
**Constraints**: 外部アセットなし、追加パッケージなし（上記2つを除く）

## Constitution Check

| 原則 | 判定 | 根拠 |
|---|---|---|
| I Spec First | PASS | spec.md の FR-001〜010 のみ実装 |
| II Test-First | PASS | tasks.md で各ルールのテストを実装より前に配置 |
| III 分離 | PASS | Core asmdef は `noEngineReferences: true` |
| IV 公平性 | PASS | CPU は `CardRevealed` イベントと `IsSelectable` だけを使う。裏の絵柄を返す公開APIなし |
| V Simplicity | PASS | IMGUI＋文字記号で描画 |

## Project Structure

```text
Assets/
├── Scenes/MemoryMatch.unity          # Camera + MemoryMatchApp
├── Scripts/Core/                     # MemoryMatch.Core.asmdef（Engine非依存）
│   ├── Card.cs        CardState
│   ├── Deck.cs        シード付きシャッフル（Fisher–Yates / System.Random）
│   ├── PlayerSlot.cs  SeatKind / CpuLevel
│   ├── MemoryGame.cs  フェーズ・手番・判定・勝者
│   └── CpuPlayer.cs   記憶と選択
├── Scripts/Game/                     # MemoryMatch.Game.asmdef
│   └── MemoryMatchApp.cs  設定画面・盤面・HUD・入力・CPUタイマー
└── Tests/EditMode/                   # MemoryMatch.Tests.EditMode.asmdef
    ├── DeckTests.cs
    ├── MemoryGameTests.cs
    └── CpuPlayerTests.cs
```

## Data Model

- `Card { int Index; int PairId; CardState State; int Owner }` — State: FaceDown / FaceUp / Matched
- `MemoryGame(PlayerSlot[] players, int pairs, int seed)`
  - `Phase`: WaitingFirst / WaitingSecond / ShowingMismatch / GameOver
  - `bool Flip(int index)`、`void ResolveMismatch()`、`int CurrentPlayer`、`int[] Winners()`
  - `int VisiblePairId(int index)` — 表か取得済みのときだけ絵柄、裏なら -1
  - `event Action<int,int> CardRevealed`
- `CpuPlayer(CpuLevel level, int seed)` — `Observe(index, pairId)`、`int ChooseCard(MemoryGame game)`
  - 記憶率: Weak 0.3 / Normal 0.7 / Strong 1.0
  - 1枚目: 記憶内に裏のまま残る完全ペアがあればその片方 → なければ未見のカード → なければ任意
  - 2枚目: 1枚目の相方を記憶していればそれ → なければ未見 → なければ任意

## 時間パラメータ（Inspector で調整可・K-01 パラメーター外出し）

- 不一致表示 1.0 秒、CPU の1枚ごとの待ち 0.6 秒

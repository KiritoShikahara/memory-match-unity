# Plan: 002-smb3-card-match

Unity 6000.6.4f1 / C# / IMGUI / unity-cli (akiojin) v0.19.1。001 の asmdef 構成を踏襲。

## Structure

```text
Assets/Scripts/Core/
  IMemoryBoard.cs      CpuPlayer が見る公開情報だけの接点（MemoryGame / CardMatchGame が実装）
  Prize.cs             Prize 列挙とポイント
  CardMatchBoard.cs    8パターン、取得済みの持ち越し、次の盤面
  CardMatchGame.cs     手番・2ミス脱落・終了・勝者
Assets/Scripts/Game/
  PixelArt.cs          文字列グリッド→Texture2D（オリジナル絵）
  CardMatchApp.cs      席設定・盤面・HUD・入力・CPUタイマー
Assets/Scenes/CardMatch.unity
Assets/Tests/EditMode/CardMatchTests.cs
Assets/Tests/PlayMode/CardMatchAppTests.cs
```

## Design

- 配置: 基本15枚（M3 F3 S3 U2 C10×2 C20×2）をシード k(0..7) の Fisher–Yates で並べ、末尾に M,F,S。異なることはテストで保証
- `CardMatchBoard { LayoutIndex; Prize At(i); bool IsTaken(i); Take(i); IsCleared; Next(seed) }` — 対戦をまたいで App が保持
- `CardMatchGame(PlayerSlot[], CardMatchBoard)` — 001 と同じ Phase / Flip / ResolveMismatch / CardRevealed。`MissesOf`, `IsOut`, `PointsOf`, `PrizesOf`
- CpuPlayer の引数を `IMemoryBoard` に広げ、両モードで共有
- 時間: 不一致表示 1.0 秒、CPU 0.6 秒（Inspector で調整）

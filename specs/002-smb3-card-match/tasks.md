# Tasks: 002-smb3-card-match

- [x] T001 テスト: `Assets/Tests/EditMode/CardMatchTests.cs`（配置8種・一致/不一致・2ミス脱落・終了・勝者・持ち越し・CPU）
- [x] T002 Red 確認（`unity-cli test --mode editmode`）
- [x] T003 実装: `IMemoryBoard.cs` `Prize.cs` `CardMatchBoard.cs` `CardMatchGame.cs`、CpuPlayer 引数変更
- [x] T004 Green 確認
- [x] T010 `PixelArt.cs` `CardMatchApp.cs`、`CardMatch.unity` 作成・Build Settings 登録、001 画面から切替
- [x] T011 PlayMode テスト `CardMatchAppTests.cs`（CPU4人 / 人1+CPU1）
- [x] T030 テスト更新（24枚・脱落廃止・連鎖 Combo・PairMatched イベント）→ Red
- [x] T031 実装: Prize / CardMatchBoard / CardMatchGame → Green
- [x] T032 演出: `Assets/Scripts/Game/Celebration.cs`（紙吹雪・ポップアップ・揺れ・フラッシュ・花火）、`SoundFx.cs`（生成音）
- [x] T033 PlayMode 更新・全テスト・スクショ確認
- [x] T020 全テスト再実行、`verification.md` 記録

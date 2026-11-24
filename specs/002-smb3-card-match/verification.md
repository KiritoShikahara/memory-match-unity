# Verification: 002-smb3-card-match（2026-11-24）

ツール: akiojin/unity-cli v0.19.1 / Unity 6000.6.4f1

| 種別 | コマンド | 結果 |
|---|---|---|
| Red | `unity-cli test --mode editmode`（実装前） | コンパイル失敗（CardMatchGame / CardMatchBoard 未定義） |
| EditMode | `unity-cli test --mode editmode` | 93件 0失敗（CardMatchTests 12件含む） |
| PlayMode | `unity-cli test --mode playmode --disable-domain-reload` | 4件 0失敗 |

- `CardMatchAppTests.FourCpus_PlayToGameOver_ThenNextRoundKeepsTakenCards`: CPU4人で終了、次の回で取得済みカードが残る（全取得時は新配置）
- `CardMatchAppTests.HumanVsCpu_PlayToGameOver`: 人1+CPU1で終了・勝者あり
- 絵柄は PixelArt.cs のオリジナル 12×12 ドット絵（任天堂素材は不使用）
- 未確認: 実機の目視（シーン CardMatch.unity は Build Settings 登録済み、手動プレイ未実施）

## 2026-11-24 追加（C5〜C7）

| 種別 | 結果 |
|---|---|
| Red | Clover / Gem / Combo / PairMatched 未定義でコンパイル失敗を確認 |
| EditMode（`--filter MemoryMatch`） | 38件 0失敗 |
| PlayMode（`--disable-domain-reload --filter MemoryMatch`） | 4件 0失敗（CPU4人で24枚すべて取得・合計460pt・花火発生・次の回は新配置） |
| 画面 | 6×4 の24枚表示をスクリーンショットで確認 |

- 全件実行時の1失敗は Bridge 自身のテスト `SimulateGamepadInput_ButtonHoldSeconds_Releases`（本プロジェクト外）
- 効果音は SoundFx.cs で矩形波を生成（外部素材なし）。音の聞こえ方は未確認

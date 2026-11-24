# Verification: 001-memory-match（2026-11-24）

ツール: akiojin/unity-cli v0.19.1（Bridge `com.akiojin.unity-cli-bridge` 0.19.1、版一致）/ Unity 6000.6.4f1

| 種別 | コマンド | 結果 |
|---|---|---|
| EditMode | `unity-cli test --mode editmode` | 81件 0失敗（MemoryMatch 27件 + Bridge 同梱テスト） |
| PlayMode | `unity-cli test --mode playmode --disable-domain-reload --filter MemoryMatch` | 2件 0失敗 |

## PlayMode 検証内容（`Assets/Tests/PlayMode/MemoryMatchAppTests.cs`）

- `FourCpus_PlayToGameOver`: CPU 4人（弱い/普通/強い/普通）が自動対戦し GameOver、取得ペア合計 8、勝者あり
- `HumanVsCpu_PlayToGameOver`: 人1 + CPU(強い)1。人の手番でカードをめくり、CPU と交代しながら GameOver、取得ペア合計 8

## 必須ルールとの対応

- ペアでカードを取得できる: `MemoryGameTests.Flip_Match_TakesPair_SamePlayerContinues` ほか
- 二人以上で対戦（CPU戦含む）: `Turn_WrapsAroundThreePlayers`、`CpuPlayerTests`、上記 PlayMode 2件

## 注意

- PlayMode はドメインリロード有効だと Bridge の認証トークンが再生成され `UNAUTHORIZED` になるため `--disable-domain-reload` を付ける
- `com.unity.modules.vr` は 6000.6 に存在しないため manifest から削除

# Verification: 004-game-select-menu（2026-11-24）

| 種別 | 結果 |
|---|---|
| Red | ImageDecks / TitleApp / ImageCardApp.Deck 未定義でコンパイル失敗を確認 |
| EditMode（`--filter MemoryMatch`） | 48件 0失敗 |
| PlayMode（`--disable-domain-reload --filter MemoryMatch`） | 11件 0失敗（デッキ選択で画像モードがそのフォルダを使う・戻ると選択画面・Title が Build 先頭） |
| 画面 | タイトル画面をスクリーンショットで確認。選択画面は未目視 |

- `hevensgate/` は `.gitignore` 済み。画像はリポジトリに含めない

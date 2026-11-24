# Verification: 003-image-card-mode（2026-11-24）

| 種別 | 結果 |
|---|---|
| Red | ImageCatalog / GridLayout 未定義でコンパイル失敗を確認 |
| EditMode（`--filter MemoryMatch`） | 46件 0失敗（ImageCardTests 8件含む） |
| PlayMode（`--disable-domain-reload --filter MemoryMatch`） | 6件 0失敗（生成画像3種で CPU 同士が終了まで・画像0枚は開始不可） |
| 実画像 | 設定画面に「36 種類 × 2枚 = 72 枚」と表示（`clock/` 読み込み確認） |

- テストはユーザー画像に依存しない（一時フォルダに生成した単色 png を使用）
- `clock/` は `.gitignore` 済み。画像はリポジトリに含めない

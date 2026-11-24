# 引き継ぎ (2026-11-24 16:44 / main)

次の一手: Unity プロジェクト一式（`MemoryMatch.unity` を含む）をリポジトリ直下に置き、commit・push する

## 完了
- 講義資料 pptx を md 化: `docs/201124 SPECから初めて、Unityで実証する.md`（docs/ は git 対象外）
- private リポジトリ作成: https://github.com/KiritoShikahara/memory-match-unity（commit 96da6f1）
- `.gitignore` で `docs/`・`.orchestrator/`・`roc-solo-*.cmd` を除外

## 残り（優先順・最大5件）
- Unity プロジェクト一式を追加して push
- Interview で未決定事項（枚数・ミス上限・スコア・シード固定）を決める
- `/speckit-specify` で `specs/` 配下に spec.md を作成（受け入れシナリオ M1〜M6）
- plan.md / tasks.md 作成 → TDD でペア判定ロジック実装
- unity-cli で AI プレイ検証し verification.md を書く

## 再開に必要なもの
- git に入っていないファイル: `docs/201124 SPECから初めて、Unityで実証する.pptx` と同名 `.md`（元PCから手動コピー）
- Unity Editor、Hermes、unity-cli v0.19.1（Bridge と版一致を `unity-cli --output json system ping` で確認）
- `gh` ログイン済みであること

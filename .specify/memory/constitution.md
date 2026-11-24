# Memory Match Constitution

## Core Principles

### I. Spec First（SPECが先）
SPECにないルールは計画・実装で追加しない。必要になったら先に `spec.md` を改訂し、承認後に実装する。

### II. Test-First（NON-NEGOTIABLE）
ゲームロジックは EditMode テストを先に書き、unity-cli で実行して失敗（Red）を確認してから実装する（Green → Refactor）。テストは `unity-cli test` で実行し、件数・合否を照合する。0件・全スキップは未確認扱い。

### III. ロジックと表示の分離
ルール（配置・手番・判定・CPU思考）は UnityEngine に依存しない純粋な C#（`MemoryMatch.Core`）に置く。表示・入力は薄い層（`MemoryMatch.Game`）に限定する。

### IV. 公平性（CPUは見ない）
CPU は人間と同じ公開情報（表になったカード）だけを使う。裏の絵柄を参照するAPIを CPU に渡さない。検証時も裏のデータを覗いて「当てた」ことにしない。

### V. Simplicity
外部アセット・追加パッケージは使わない（unity-cli Bridge とテストフレームワークを除く）。絵柄は色と記号で表現する。

## Governance
この憲章は plan.md のゲート評価に使う。改訂は spec と同様に先に文書を変更する。

**Version**: 1.0.0 | **Ratified**: 2026-11-24 | **Last Amended**: 2026-11-24

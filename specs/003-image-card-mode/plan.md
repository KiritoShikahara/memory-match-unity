# Plan: 003-image-card-mode

```text
Assets/Scripts/Core/ImageCatalog.cs   ファイル名→採用する画像一覧（純粋関数）
Assets/Scripts/Core/GridLayout.cs     枚数・領域・縦横比→列数とカードサイズ（純粋関数）
Assets/Scripts/Game/ImageCardApp.cs   読み込み・席設定・盤面・HUD・演出（MemoryGame + CpuPlayer + Celebration + SoundFx）
Assets/Scenes/ImageCard.unity
Assets/Tests/EditMode/ImageCardTests.cs
Assets/Tests/PlayMode/ImageCardAppTests.cs
```

- 画像は `Texture2D.LoadImage(File.ReadAllBytes)` で実行時に読む。Assets には取り込まない
- 列数: cols=1..N を全探索し、幅制約と高さ制約の小さい方で決まるカード幅が最大のもの

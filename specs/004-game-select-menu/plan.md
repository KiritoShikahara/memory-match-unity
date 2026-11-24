# Plan: 004-game-select-menu

```text
Assets/Scripts/Core/ImageDeck.cs     ImageDeck(Title, FolderName) と ImageDecks.All / Find
Assets/Scripts/Game/ImageDeckPaths.cs  フォルダ名→実パス（Editor: プロジェクト直下 / ビルド: StreamingAssets）、画像数
Assets/Scripts/Game/TitleApp.cs      タイトル→選択。選んだデッキを保持してシーン遷移。BackToMenu()
Assets/Scripts/Game/ImageCardApp.cs  デッキを TitleApp.SelectedDeck から取る
Assets/Scenes/Title.unity            Build index 0
Assets/Tests/EditMode/ImageDeckTests.cs
Assets/Tests/PlayMode/TitleAppTests.cs
```

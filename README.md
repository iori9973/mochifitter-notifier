# MochiFitter Notifier

もちふぃった～（OutfitRetargetingSystem）の処理が完了したタイミングで通知音を再生する Unity Editor 拡張です。

> **注意**: 本ツールの動作にはもちふぃった～（OutfitRetargetingSystem）が必要です。もちふぃった～は別途ご購入ください。

## VCC からインストール（推奨）

VCC の **Settings → Packages → Add Repository** に以下の URL を追加してください。

```
https://iori9973.github.io/mochifitter-notifier/index.json
```

追加後は **My Projects → Manage Project → MochiFitter Notifier → Install** でインストールできます。
インストール後は**設定不要で自動的に動作**します。

## 手動インストール

Unity Package Manager → `Add package from disk...` → `package.json` を選択してください。

## 設定

`Edit → Preferences → MochiFitter Notifier` から設定を変更できます。

| 項目 | 説明 |
|------|------|
| 通知を有効にする | ON/OFF の切り替え |
| 通知音 | カスタム音声ファイルの指定（空欄でデフォルト音） |
| テスト再生 | 設定した音を即時確認 |

### 対応フォーマット

| プラットフォーム | 対応フォーマット |
|---|---|
| Windows | WAV / MP3 / M4A |
| macOS | WAV / MP3 / M4A |
| Linux | WAV のみ |

### 設定の共有について

設定は PC 全体で共有されるため、**すべての Unity プロジェクトに反映**されます。

カスタム音声ファイルを指定する場合は、デスクトップや Documents など **プロジェクトフォルダの外** に置いてください。プロジェクト内の Assets フォルダに置いた場合、他のプロジェクトでは再生されません。


## リリース方法（開発者向け）

```bash
git tag v1.0.0
git push origin v1.0.0
```

GitHub Actions が自動的にパッケージ zip を作成し、VPM リポジトリを更新します。

## 動作環境

- Unity 2022.3 以降
- Windows / macOS / Linux

## ライセンス

MIT License

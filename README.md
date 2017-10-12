# What?

* シンプルなモバイルプラットフォーム向けのビルドを実行します。
* Jenkins などの CI ツールから叩かれることを想定して、プラットフォーム別の外部公開メソッドを提供しています。
* アプリケーションそのものと AssetBundle のビルドを提供します。

# Why?

* Jenkins から叩ける汎用的なビルド処理の受け口が欲しかったので実装しました。

# Install

```shell
$ npm install -D github:umm-projects/simple_build.git
```

# Usage

```shell
/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath /path/to/unity -executeMethod SimpleBuild.BuildPlayer.Run_iOS
/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath /path/to/unity -executeMethod SimpleBuild.BuildPlayer.Run_Android
/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath /path/to/unity -executeMethod SimpleBuild.BuildAssetBundle.Run_iOS
/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath /path/to/unity -executeMethod SimpleBuild.BuildAssetBundle.Run_Android
```

* Jenkins などから、 `SimpleBuild.BuildPlayer.Run_XXX` でプラットフォーム別にビルドを呼び出します。
* プロジェクトのメニューの `Project` &gt; `Build Player` を実行すると、現在エディタが向いているプラットフォーム向けのプレイヤービルドを実行します。
* プロジェクトのメニューの `Project` &gt; `Build AssetBundle` を実行すると、現在エディタが向いているプラットフォーム向けの AssetBundle ビルドを実行します。

## AssetBundle ビルド前後処理

* Unity 2017.1 時点では AssetBundle のビルド前後に処理を挟むことができません。
* そのため、擬似的に処理を実行できる仕組みを組み込んでいます。
* interface として `SimpleBuild.IPreprocessBuildAssetBundle` と `SimpleBuild.IPostprocessBuildAssetBundle` を提供しているので、必要に応じてそれぞれ `OnPreprocessBuildAssetBundle` と `OnPostprocessBuildAssetBundle` を実装してください。
  * どちらのメソッドも引数に AssetBundle 出力先のディレクトリパスを渡します。


# License

Copyright (c) 2017 Tetsuya Mori

Released under the MIT license, see [LICENSE.txt](LICENSE.txt)


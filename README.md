# What?

* シンプルなモバイルプラットフォーム向けのビルドを実行します。
* Jenkins などの CI ツールから叩かれることを想定して、プラットフォーム別の外部公開メソッドを提供しています。

# Why?

* Jenkins から叩ける汎用的なビルド処理の受け口が欲しかったので実装しました。

# Install

```shell
$ npm install @umm/simple_build
```

# Usage

```shell
/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath /path/to/unity -executeMethod SimpleBuild.BuildPlayer.Run_iOS
/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath /path/to/unity -executeMethod SimpleBuild.BuildPlayer.Run_Android
```

* Jenkins などから、 `SimpleBuild.BuildPlayer.Run_XXX` でプラットフォーム別にビルドを呼び出します。
* プロジェクトのメニューの `Project` &gt; `Build Player` を実行すると、現在エディタが向いているプラットフォームでのビルドを実行します。

# License

Copyright (c) 2017 Tetsuya Mori

Released under the MIT license, see [LICENSE.txt](LICENSE.txt)


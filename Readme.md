# 文書管理システム

小規模の会社で文書管理を行う。

## Description

10 〜 20 人規模の会社で紙ベースの文書ファイルを電子化して管理する。
スキャナーで取り込んだファイルを(スキャナーの機能で)一度共有サーバーに
アップロードし、ユーザーにタグをつけてもらい、アップロードする。
アップロードした文書はタグで検索し、参照することができる。

PC操作にはあまり馴染みのないユーザーを想定し、アップロードの際には専用
クライアントを使用してログイン・タグ付け・複数ファイルアップロードの補助をする。

***DEMO:***

![Demo](https://image-url.gif) (TODO)

## Features

- 複数ファイルのタグ付け・アップロード
- (TODO)表示方法

## Requirement

(TBD)

- Microsoft Azure で実行する (既存アカウントがあるから)
- SSLの使用
- アカウント管理の簡略化

## Installation

```
$ git clone https://github.com/team-itp/docms
$ cd src
$ dotnet run
```

## 作業の方法

作業はまずブランチを (ユーザー名)/(任意の作業ブランチの名前) で作成し、作業が
完了した時点でプルリクエストを発行する。このリポジトリをフォークする必要はない。
プルリクエストは作業内容共有のため全オーナーの認証を受けるものとする。
作業手順の変更が必要な場合は当READMEを更新し、プルリクエストを発行すること。

## Author

[@chameleonhead](https://twitter.com/chameleonhead)
[@OmosirokiKotoMo](https://twitter.com/OmosirokiKotoMo)

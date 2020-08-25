# MusicPlayer

HackU中部大学2020用の作品です

## 作る手順
* Webカメラを起動する																			
* 現在画面に映っている画像をpngファイルとして保存										
* GoogleCloudAPIに画像を与える										
* JSONデータをダウンロードする										
* JSONデータから"Angry"や"Happy"などの感情のデータを取り出す					
* 1番割合が多い感情を現在の感情とみなす
* みなした感情によってフォルダを選択する										
* 指定した音源フォルダの音楽をかける										

## 仕様
**事前に環境変数GOOGLE_APPLICATION_CREDENTIALSを設定しておく必要があります** <br>
参考ページ : " https://www.atmarkit.co.jp/ait/articles/1712/22/news033.html " <br>
秘密鍵/公開鍵のペアファイルは"C:\HackU2020"フォルダに入れておかないと、例外が発生するようです。 <br>

**また、音楽ファイルは"C:\HackU2020\Music\Joy", "C:\HackU2020\Music\Anger", "C:\HackU2020\Music\Sorrow", "C:\HackU2020\Music\Surprise"フォルダに入れておくようにしてください** <br>
色々手間がかかり、申し訳ないです。

### プログラムの動作の流れ
* プログラムを実行するとWebカメラが起動し、キャプチャ画像を表示します。
Webカメラが見つからない場合は"camera was not found!"というメッセージボックスを出力して、プログラムは終了します。
* Captureボタンを押すと、押した時点のキャプチャ画像を保存します。この画像は"C:\HackU2020"に保存されます。
また、画像ファイル"cap.png"はCaptureボタンが押される度に上書き保存されます。
* 画像ファイル"cap.png"をGoogleCloudVisionAPIを用いて検出された、喜び、怒り、悲しみ、驚きの度合い(6段階)を取得します。
複数の顔が検出された場合、それぞれの顔の感情の度合いを取得します。
* 取得した感情の度合いを0~4の整数に変換、感情ごとに合計し、最も大きい値の感情を今の感情とみなします。
* 今の感情に合わせた音楽のフォルダを選択し、ランダムに再生します。
* 音量は右の1番上のトラックバーを動かすことで変更することが出来ます(0~100)。初期値は50です。
音量は音楽が変わっても変更した大きさのままです。
* 音楽のスピードは右の上から2番目のトラックバーを動かすことで変更することが出来ます(0.1~5.0)。初期値は1.0です。
音楽のスピードは曲が変わると1.0倍に戻ります。
 

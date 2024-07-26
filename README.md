# qua2ma

将Quaver的`.qp`文件转换成Malody的`.mcz`。支持变拍和SV变速谱。  
Converts quaver files to malody. Supports timing points and SV (slider velocity) conversion.

这是**命令行工具**，需要在终端执行才可运行，请勿双击文件。  
This is a **command line program**. It needs to be run in a terminal, not by double clicking.

## 使用方法 Usage

```sh
qua2ma 1.0.0+a57ebe2cf83273792e41a2cc7e216684d19befee
Copyright (C) 2024 qua2ma
USAGE:
转换单个qp文件:
  qua2ma path_to_qp.qp
将音频文件转换成ogg（需要ffmpeg）:
  qua2ma --ogg path_to_qp.qp
转换qp文件，并将输出放置在给定文件夹内:
  qua2ma --output out/dir/ path_to_qp.qp
转换一个文件夹内的所有qp文件:
  qua2ma path/to/qps/
变更语言:
  qua2ma --language zh-hans-cn path/to/qps/

  -h, --help        Display help

  -o, --output      Output Directory

  -l, --language    Language/语言 (Supported: zh-hans-cn)

  --ogg             (Default: false) Convert audio files to ogg

  --help            Display this help screen.

  --version         Display version information.

  qp (pos. 0)       Path to .qp file

qua2ma 1.0.0+a57ebe2cf83273792e41a2cc7e216684d19befee
Copyright (C) 2024 qua2ma
USAGE:
Convert single qp:
  qua2ma path_to_qp.qp
Convert audio to ogg (requires ffmpeg):
  qua2ma --ogg path_to_qp.qp
Convert file to directory:
  qua2ma --output out/dir/ path_to_qp.qp
Convert a directory of qps:
  qua2ma path/to/qps/
Language:
  qua2ma --language zh-hans-cn path/to/qps/

  -h, --help        Display help

  -o, --output      Output Directory

  -l, --language    Language/语言 (Supported: zh-hans-cn)

  --ogg             (Default: false) Convert audio files to ogg

  --help            Display this help screen.

  --version         Display version information.

  qp (pos. 0)       Path to .qp file

```

## 注意事项 Note

### Malody不支持MP3文件 MP3 is not supported in Malody

Malody不支持某些mp3文件，如果您发现转换成果导入失败，请按以下步骤操作：

* 如果您安装了[ffmpeg](https://ffmpeg.org)，请在运行本程序时添加`--ogg`选项，所有mp3会自动转换成ogg。
* 否则，您需要使用在线工具或其他本地工具手动音频转换。请记得在对应`.qua`文件里更改指向的音频文件名称。

Malody does not support MP3 files. If the converted `.mcz` files fail to import, please follow one of the following
procedures:

* If you have installed [ffmpeg](https://ffmpeg.org), you can add `--ogg` to the command line options, which
  automatically converts all mp3's to ogg.
* Otherwise, you will have to use online or other tools to convert the audio files manually. Please remember to change
  the audio file name in the `.qua` files as well. 

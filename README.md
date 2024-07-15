# qua2ma

将Quaver的`.qp`文件转换成Malody的`.mcz`。支持变拍和SV变速谱。
Converts quaver files to malody. Supports timing points and SV (slider velocity) conversion.

```sh
Copyright (C) 2024 qua2ma

USAGE:
转换单个qp文件:
Convert single qp:
  qua2ma path_to_qp.qp
转换qp文件，并将输出放置在给定文件夹内:
Convert file to directory:
  qua2ma --output out/dir/ path_to_qp.qp
转换一个文件夹内的所有qp文件:
Convert a directory of qps:
  qua2ma path/to/qps/
Language:
  qua2ma --language zh-hans-cn path/to/qps/

  -o, --output      Output Directory

  -l, --language    Language/语言 (Supported: zh-hans-cn)

  --help            Display this help screen.

  --version         Display version information.

  qp (pos. 0)       Required. Path to .qp file

```

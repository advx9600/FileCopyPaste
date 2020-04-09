# FileCopyPaste
方便进行文件的复制和粘贴<br/>
![image](https://github.com/advx9600/FileCopyPaste/raw/master/FileCopyPaste/shots/main.png)

# 功能： 方便view0,view1,view2中文件进行相互复制和粘贴等操作
#  view0到view2 只显示文件夹，view3只显示文件，Btn0和Btn设置对应View的目录，view2显示桌面
# 操作示例
1 选中view0中的一个节点<br/>
2 再选中view1中的一个节点，对应目录如果有文件，就会显示在View3中<br/>
3 选中View3中的任意一个文件，按 ctry + 1，文件就复制粘贴到了View0的目录<br/>

# 修改目录下面的 data.xml 即可修改各种操作

# 快捷键
| 参数                | 按键                   | 描述                           |
| ------------------ | ----------------------- | -------------------------------------------------------------------- |
| key_paste          | `ctry+1,ctry+2,ctry+3`  | 粘贴操作，ctry+1复制到view0，ctry+2 复制到view1，ctry+3复制到view2     |
| key_delete         | `Delete`                | 删除操作，焦点在哪里就删除哪个view中选中的文件，最多删除2层 |
| key_enter          | `Return`                | 打开View3中的文件，最好不要更改，对应 open_file_apps 进行操作            |
| key_new_folder     | `ctry+n`                | 新建文件夹操作，焦点需要对应到显示文件夹的view                            |
| key_copy_path      | `ctry+c`                | 复制路径，把view3中选中的文件的路径及文件复制到剪贴板上，单一选择        |
| key_change_theme   | `ctry+b`                | 改变显示主题，颜色切换为白天或黑夜模式                                  |
| key_close_window   | `ctry+w`                | 关闭窗口，也可以 alt+f4                                 |
| key_default_open_app| `ctry+Return`          | 用设定的程序打开，default_open_app_exe 参数进行设置                    |

# 其它参数
| 参数                | 描述                           |
| ------------------- | ------------------------------------------- | 
|open_file_apps       |设置后缀名打开方式,其中一个app就对应一种打开方式 |
|open_file_apps->app| |                                               |
| app->file_ext       | 文件后缀名，以逗号分开，不需要加 .                    |
|app->open_exe        | 用来打开的命令或者程序路径，如果包含空格需要加双引号  |
|ignore_files         |忽略的文件名，不显示                                  |
|ignore_dirs          |忽略的目录名，不显示                               |
|path0                |对应view0路径                                      |
|path1                 |对应view1路径                                    |
|path2                 |对应view2路径                                 |
|default_open_app_exes  |用此程序打开文件夹和其它文件，以逗号分开                     |
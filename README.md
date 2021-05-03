# Console File Manager
### ***Description***
This is a simple Console File Manager that allows you simply manage your files.
This project is still under development, so description may be updated from time to time.
For now, the features that are already realized:
* You can configurate how the application looks in the console: background color, foreground color, symbol to draw window borders with, amount of files shown per page.
* Application saves its state just before the exit. So you can continue your work when you start the application again.
* A log file is created for any occured error that can not be handled by user. So you always can try to figure out what went wrong.
* You can browse your files, copy them, delete them or get information about them.


### ***Commands (general information)***
Here are all commands that are currently available in the application:

Command   | Short Description                                               | 
----------|-----------------------------------------------------------------|
_gotd_    | Show file structure for the specified directory                 |
_cpy_     | Copy specified file or directory to another specified directory |
_del_     | Delete specified file or directory                              |
_info_    | Show information about specified file or directory              |
_exit_    | Exit from the application (state saves automatically)           |

Each command, except 'exit' has one or more required arguments (most commonly these arguments are paths) and zero or more extra arguments.

### ***Commands (in details)***
#### gotd
1. Using: `gotd "<path_to_directory>" [-p <integer>]`
2. Arguments:<br/>
&nbsp;&nbsp; `"<path_to_directory>"` - the path to the directory to show file structure of. <br/>
&nbsp;&nbsp; `-p <integer>` - the number of page to show; if not specified then first page will be shown.
3. Examples: <br/>
&nbsp;&nbsp; `gotd "C:\Users"`<br/>
&nbsp;&nbsp; `gotd "C:\Users" -p 2` <br/>
&nbsp;&nbsp; `gotd "Program Files\Microsoft"` - you can also use relative path. <br/>
&nbsp;&nbsp; `gotd ""` - you can use empty path to specify the current directory. <br/>
&nbsp;&nbsp; `gotd "" -p 5`

#### cpy
1. Using: `gotd "<path_to_object_to_copy>" "<path_to_destination_folder>" [-rf <boolean>]`
2. Arguments:<br/>
&nbsp;&nbsp; `"<path_to_object_to_copy>"` - the path to the directory to the object (folder or file) you want to copy. <br/>
&nbsp;&nbsp; `"<path_to_destination_folder>"` - the path to the directory where you want to copy the object to. <br/>
&nbsp;&nbsp; `-rf <boolean>` - if true then files will be replaced automatically, otherwise you will be warned that file with the same name already exists.
3. Examples: <br/>
&nbsp;&nbsp; `cpy "C:\Test" "D:\"` - you will be asked what to do each time the file with the same name is already exist.<br/>
&nbsp;&nbsp; `cpy "C:\Test" "D:\" -rf false` - the same as previous.<br/>
&nbsp;&nbsp; `cpy "C:\Test" "D:\" -rf true` - you will be asked what to do only if replaced file is occupied by another process. <br/>
&nbsp;&nbsp; `cpy "C:\Test" ""` - you can also specify destination folder as current folder using empty path. <br/>
&nbsp;&nbsp; `cpy "C:\Test\test.pdf" "D:\Documents"` - if file with the same name already exists you will be asked to specify `-rf` argument. <br/>
&nbsp;&nbsp; `cpy "C:\Test\test.pdf" "D:\Documents" -rf false` - if file with the same name already exists a copy of this file will be created. <br/>
&nbsp;&nbsp; `cpy "Test\test.pdf" "Desktop\New Folder" -rf false` - relative path is also allowed. <br/>


#### del
1. Using: `del "<path_to_object_to_delete>" [-r true]`
2. Arguments:<br/>
&nbsp;&nbsp; `"<path_to_object_to_delete>"` - the path to the object (file or folder) you want to delete. <br/>
&nbsp;&nbsp; `-r true` - must be specified to allow recursive deletion if you want to delete folder that is not empty.
3. Examples: <br/>
&nbsp;&nbsp; `del "C:\Test"` - if folder is empty it will be deleted; if not - you will be asked to specifiy the `-r true` argument. <br/>
&nbsp;&nbsp; `del "C:\Test" -r true` - will delete the specified directory with all its files and directories inside.<br/>
&nbsp;&nbsp; `del "C:\Test\test.pdf"`<br/>
&nbsp;&nbsp; `del ""` - you can also use an empty path to delete the current directory. <br/>
&nbsp;&nbsp; `del "Users\Admin\Desktop\test.pdf"` - relative path is also allowed. <br/>
&nbsp;&nbsp; `del "" -r true`


#### info
1. Using: `info "<path_to_object>"`
2. Arguments:<br/>
&nbsp;&nbsp; `"<path_to_object>"` - the path to the object (file or folder) you want to show information about. <br/>
3. Examples: <br/>
&nbsp;&nbsp; `info "C:\Test"`<br/>
&nbsp;&nbsp; `info "C:\Test\test.pdf"`<br/>
&nbsp;&nbsp; `info ""` - show information about current directory. <br/>
&nbsp;&nbsp; `info "Users\Admin"` - relative path is also allowed. <br/>

#### exit
1. Using: `exit`

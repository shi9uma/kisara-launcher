## server

以整合包 kisara-dream 为例，假设使用 jdk-21、forge-47.2.20、minecraft-1.20.1、mods 列表

服主搭配 kisara-launcher-server，开放在：`https://domain.com/minecraft/api/kisara-dream/{config}`，`{config}` 可以取

1.   `main_config`，获取到

     ```json
     {
         'main': {
             'version': '1.20.1',
             'directory': 'games/kisara-dream'
         },
         'jre': {
             'version': '17.0.1',
             'directory': 'jre/jre-17.0.1',
             'url': 'https://xxx.xxx/jre-17.0.1.zip',
             'hash': '123456'
         },
         'mod_loader': {
             'type': 'forge',	// forge, fabric, vanilla
             'version': '47.2.20',
             'path': 'games/kisara-dream/libraries/net/minecraftforge/forge/1.20.1-47.2.20',
             'url': 'https://xxx.xxx/forge-v47.2.20-installer.jar',
             'hash': '123456'
         },
         'server': {
             'server_host': 'domain.com',	// domain or ip
             'port': '9999'
         }
     }
     ```

2.   `mods_config`，可得

     ```json
     {
         {
         	'name': '[创世神]-worldedit-v7.3.0.jar',
         	'path': 'games/kisara-dream/mods/[创世神]-worldedit-v7.3.0.jar'
         	'url': 'https://cdn.modrinth.com/.../worldedit-mod-7.3.0.jar'	// 使用各路下载源, launcher 本身将不提供任何列表, 都是由服务器一键配置后获取 url 本地再下载, 当然, 如果服务器带宽很大, 很自信, 也可以自建下载源
         	'hash': '54321',
         	'type': 'required'
     	},
         {
         	'name': '[jei]-jei-v1.2.3.jar',
         	'path': 'games/kisara-dream/mods/[jei]-jei-v1.2.3.jar'
         	'url': 'https://domain.com/minecraft/api/kisara-dream/mods/[jei]-jei-v1.2.3.jar'
         	'hash': '54322',
             'type': 'required'
     	},
         {
         	'name': '[投影]-Forgematica-0.1.3-mc1.20.1.jar',
         	'path': 'games/kisara-dream/mods/[jei]-jei-v1.2.3.jar'
         	'url': 'https://domain.com/minecraft/api/kisara-dream/mods/[jei]-jei-v1.2.3.jar'
         	'hash': '54320',
             'type': 'optional'
     	},
     }
     ```

3.   其中，server 的文件拓扑结构如下：

     ```bash
     .
     `-- api
         |-- kisara-another
         |   |-- main_config
         |   |-- mods
         |   `-- mods_config
         `-- kisara-dream
             |-- main_config
             |-- mods
             `-- mods_config
     ```

     root 目录其实就是 api，使用 npm 快速创建一个 webserver 开放在指定端口，可以自行配置 nginx 来进行端口反代和 tls

以上配置需要服主手动配置（除了初期配置比较麻烦，但这也是开服不得不品的一环），**个人觉得最快的方式还是直接发送整合包，毕竟这个项目设计出来是方便 mods 列表更新的**

## scripts

1.   将 mods 改好名，放到 `api/kisara-dream/mods` 目录下
2.   配置：`work_path = api/kisara-dream/mods`，`client_path_root = games/kisara-dream/mods`，`url_root = https://domain.com/minecraft/api/kisara-dream/mods`
3.   脚本自动对 jar 测 hash，并输出配置文件 `api/kisara-dream/mods_config`
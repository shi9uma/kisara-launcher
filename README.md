# kisara-launcher

老是让我手动改 mod 然后打包，其中还要写一个 "读我.txt" 来教人怎么安装和删除 mod 真的很烦；

this repo forks from [Shanwer/NsisoLauncher](https://github.com/Shanwer/NsisoLauncher.git)，get root src here：[Coloryr/NsisoLauncher-1](https://github.com/Coloryr/NsisoLauncher-1.git)，another version：[Coloryr/ColorMC](https://github.com/Coloryr/ColorMC.git)。是的，在经过各种依赖的折磨，仍然不能启动这个项目的经历后，我决定在参考工程设计的情况下重写。尝试了以下几种技术构建：

| 前端                    | 后端                                                | 交互                                                         | 备注                                                         |
| ----------------------- | --------------------------------------------------- | ------------------------------------------------------------ | ------------------------------------------------------------ |
| electron + vue          | cpp 纯编译成可执行文件，使用 httplib 开放在特定端口 | 二者通过 axios 交互                                          | 不方便打包成单个应用                                         |
| electron + vue          | cpp                                                 | 使用 ffi-napi 和 node-gyp 将 cpp 源码编译成 *.node 文件，供前端调用 | ffi-napi 和 node-gyp 在不同平台还要做差异化调整，开发编写也比较麻烦 |
| electron + vue          | cpp                                                 | 使用 Webassembly 将 cpp 编译成 *.wasm，供前端调用            | 想在 windows 将 Emscripten portable 化很麻烦                 |
| **electron + vue + js** |                                                     |                                                              | **一把梭，如无必要、勿增实体**                               |

## mine

虽然 fork 的 source 都是用 csharp + xaml 写的，但是我想尝试一下现在比较新的一些东西：~~electron + cpp~~ electron + vue 来写：

-   前端用 electron 开一个 app
-   **首页**：单开一个页面用来提示信息（公告等），旁边直接列出对话框输入账号密码、登录；右下角有启动器设置；首页应该可以选择版本隔离对应的 mod 包
-   **账号认证、鉴权**：游戏账号的第三方/官方 oath2 认证，在选择第三方时可以配置 authlib-injector 地址、账号注册一键直达、获取账号的皮肤头像等信息
-   **游戏启动设置**：全局 default 设置、单个包的特定 java 设置（强制每个包都必须手动指定相对路径下的 java 版本文件夹）、游戏窗口名、游戏版本名
-   **下载设置**：资源的获取，下载源选择、代理选择
    -   minecraft java edition 的统一资源获取与版本更新
    -   帮助补全 java 内容，设计为统一采用相对位置（版本隔离）的自带开源 jdk（jre） 路径
    -   mods 资源列表检查和更新
-   后端的主要逻辑就是调用系统 cmd 实现各种启动命令，例如 `java -x1 -x2 -x3 -jar xxx.jar`，还要给出详细的 log 记录，追踪错误路径
-   （可选）client 加一个用于显示目前加了什么 mod 的页面，给出 mc 百科介绍、百科链接、下载链接

## craft

为服主提供一个快速方便的自定义启动器真的很有必要，还要设计得易上手，有简明易懂的文档教如何制作自己的整合包；服主只需要配置好 server 端和 client，client 中配置好一个访问 server 提供的 api 获取 mod、java、minecraft 版本（自带下载链接，发给 client 后依次下载），发送给玩家 client（也可以发送完整包，减少 client 再去下载资源的 io）：

1.   以整合包 kisara-dream 为例，假设使用 jdk-21、forge-47.2.20、minecraft-1.20.1、mods 列表

2.   服主配置并启动 kisara-launcher-server，开放在：`https://domain.com/minecraft/api/kisara-dream/{config}`，[具体查看](./server/readme.md)

3.   发送 client 阶段

     1.   **如果是以整合包的形式分发**；服主在 client 中，选择包 kisara-dream（如果没有这个文件夹，则自动创建 `games/kisara-dream`），在其 meta url 里填上：`https://domain.com/minecraft/api/kisara-dream/`（这里还要检查一下是否有效），并且配置一些全局信息，将 client 及其同目录下的 config、games、jre 文件夹打包（也可以没有东西在其中，这些文件夹都是通过 client 在初次运行时、或改变设置时自动检测并创建的，主要内容是一些第三方登录、自定义 client 外观等），打包完成后发给用户；
     2.   **只发 launcher 单文件**；此时一般都是默认，没有 meta url

     每个用户的账户密码等信息不会被打包到其中，因为这些敏感信息被设定为被存放在 `user/.kisara-launcher/config` 下，并且不会有口令明文，而是 access token

4.   用户解压，进入目录，打开 client，登录账号密码，启动游戏，此时
     1.   **如果是以整合包的形式分发**；只根据 config 获得的信息去检查对应文件路径的 hash，正确则跳过，错误则先记录，操作完后统一交由用户选择下载；**这里有一个启用文件完整性检查的选项，如果用户很自信，可以关闭，提高游戏启动速度（快几秒？）**
     2.   **如果只发 launcher 单文件（没有 forge、fabric 等）**；此时往往没有 meta url 填写，需要再约定发一个 url，client 根据  url 获取到的 main_config、mods_config，下载相应内容并放置，对于 installer 则执行一系列操作自动安装

5.   用户后续启动时，检查 url 的 config（如果 server 挂了，也可以跳过检查），主要是 mods_config，逐一匹配 mod 的 hash，如果出现错误则记录，结束后弹对话框建议用户更新 mod 和 server 一致，更新结束后重新执行所有步骤

附上 client 端的目录：

```bash
.
|-- config
|-- games
|   |-- kisara-anoter
|   |   |-- extra_mods
|   |   |-- libraries
|   |   |-- logs
|   |   |-- mods
|   |   |-- saves
|   |   `-- versions
|   `-- kisara-dream
|       |-- extra_mods
|       |-- libraries
|       |-- logs
|       |-- mods
|       |   `-- worldedit.jar
|       |-- saves
|       `-- versions
|-- jre
|   |-- jre-11.0.1
|   |   |-- bin
|   |   `-- lib
|   `-- jre-17.0.1
|       |-- bin
|       `-- lib
`-- kisara-launcher.exe
```

## src-tree

-   gui：前端，应用程序代码、配置文件、资源文件、视图模型、视图和其他辅助类；
    -   Config（配置相关）
    -   Resource（资源文件）
    -   Themes（主题相关）
    -   Utils（工具类）
-   core：后端，工具类、核心组件、配置类、异常类等；
    -   Authenticator（认证相关）
    -   Component（组件相关）
    -   Config（配置相关）
    -   LaunchException（启动异常相关）
    -   Modules（模块相关）
    -   Nbt（NBT 格式相关）
    -   Net（网络相关）
    -   User（用户相关）
    -   Util（工具类）
-   coretest：后端测试功能
    -   LauncherMetaApiTest
    -   ServerInfoTest

## licenses & thanks

-   [BMCLAPI](https://bmclapidoc.bangbang93.com/)
-   [MahApps/MahApps.Metro](https://github.com/MahApps/MahApps.Metro)
-   [MahApps/MahApps.Metro.IconPacks](https://github.com/MahApps/MahApps.Metro.IconPacks)
-   [JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
-   [Fody/Fody](https://github.com/Fody/Fody)
-   [Fody/Costura](https://github.com/Fody/Costura)
-   [Live-Charts/Live-Charts](https://github.com/Live-Charts/Live-Charts)
-   [icsharpcode/SharpZipLib](https://github.com/icsharpcode/SharpZipLib)
-   [hawezo/MojangSharp](https://github.com/hawezo/MojangSharp)
-   [ghuntley/Heijden.Dns](https://github.com/ghuntley/Heijden.Dns)
-   [Minecraft-Console-Client](https://github.com/ORelio/Minecraft-Console-Client)
-   [cyotek/Cyotek.Data.Nbt](https://github.com/cyotek/Cyotek.Data.Nbt)

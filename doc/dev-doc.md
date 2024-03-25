# dev doc

## init for cpp（deprecate）

以下是项目前后端调用逻辑的参考，以一个 sum 函数为例

1.   `cnpm install -g @vue/cli`

2.   `ngx`（windows），建议 linux like 去配置这些内容

3.   `cd client`，`vue create launcher`

4.   `cd launcher`

5.   `vue add electron-builder`

6.   `cnpm install --save-dev node-gyp ffi-napi`

7.   `touch src/components/sum_form.vue`

     ```vue
     <!-- launcher/src/components/sum_form.vue -->
     <template>
         <div>
             <input v-model="num1" type="number" placeholder="Number 1" />
             <input v-model="num2" type="number" placeholder="Number 2" />
             <button @click="calculateSum">Calculate Sum</button>
             <div v-if="result !== null">Result: {{ result }}</div>
         </div>
     </template>
     
     <script>
     export default {
         data() {
             return {
                 num1: 0,
                 num2: 0,
                 result: null,
             };
         },
         methods: {
             async calculateSum() {
                 const ffi = require('ffi-napi');
                 const libSum = ffi.Library('path_to_compiled_sum_binary', {
                     'sum': ['int', ['int', 'int']]
                 });
                 this.result = libSum.sum(parseInt(this.num1), parseInt(this.num2));
             },
         },
     };
     </script>
     ```

8.   `touch ../core/sum.cpp ../core/sum.h`

     ```cpp
     // client/core/sum.cpp
     #include "sum.h"
     
     int sum(int a, int b) {
         return a + b;
     }
     ```

     ```cpp
     // client/core/sum.h
     #ifndef SUM_H
     #define SUM_H
     
     int sum(int a, int b);
     
     #endif // SUM_H
     ```

9.   `touch binding.gyp`

     ```python
     {
       "targets": [
         {
           "target_name": "sum",
           "sources": [ "../core/sum.cpp" ]
         }
       ]
     }
     ```

10.   `node-gyp configure build`，生成 `build\Release\sum.node` 这个文件

## init for only js

1.   `cnpm install -g @vue/cli`，`cnpm install -g yarn`

2.   `vue create client`，`cd client`

3.   `yarn install electron electron-builder --save-dev`

4.   `touch main.js`：

     ```js
     const { app, BrowserWindow } = require('electron')
     
     function createWindow () {
       // 创建浏览器窗口
       let win = new BrowserWindow({
         width: 800,
         height: 600,
         webPreferences: {
           nodeIntegration: true
         }
       })
     
       // 并且为你的应用加载index.html
       win.loadFile('dist/index.html')
     }
     
     app.on('ready', createWindow)
     ```

5.   touch `src/components/SumCalculator.vue`：

     ```vue
     <template>
         <div>
             <input v-model.number="number1" type="number" placeholder="Enter number 1" />
             <input v-model.number="number2" type="number" placeholder="Enter number 2" />
             <button @click="calculateSum">Calculate Sum</button>
             <p>Sum: {{ sumResult }}</p>
         </div>
     </template>
     
     <script>
     export default {
         data() {
             return {
                 number1: 0,
                 number2: 0,
                 sumResult: null,
             };
         },
         methods: {
             calculateSum() {
                 window.electron.ipcRenderer.send('sum-request', { number1: this.number1, number2: this.number2 });
             },
         },
         created() {
             window.electron.ipcRenderer.on('sum-response', (event, result) => {
                 this.sumResult = result;
             });
         },
         beforeDestroy() {
             window.electron.ipcRenderer.removeAllListeners('sum-response');
         },
     };
     </script>
     ```

6.   修改 `public/index.html`：

     ```html
     <!DOCTYPE html>
     <html lang="en">
     
     <head>
       <meta charset="utf-8">
       <meta http-equiv="X-UA-Compatible" content="IE=edge">
       <meta name="viewport" content="width=device-width,initial-scale=1.0">
       <link rel="icon" href="<%= BASE_URL %>favicon.ico">
       <title>your-project-name</title>
       <script>if (typeof require !== 'undefined') window.electron = require('electron')</script>
     </head>
     
     <body>
       <noscript>
         <strong>We're sorry but your-project-name doesn't work properly without JavaScript enabled. Please enable it to
           continue.</strong>
       </noscript>
       <div id="app"></div>
       <!-- built files will be auto injected -->
     </body>
     
     </html>
     ```

7.   修改 `src/App.vue`：

     ```vue
     <script>
     import SumCalculator from './components/SumCalculator.vue'
     
     export default {
       name: 'App',
       components: {
         SumCalculator
       }
     }
     </script>
     ```

8.   修改 `package.json` 部分：

     ```json
       "main": "main.js",
       "scripts": {
         "serve": "vue-cli-service serve",
         "build": "vue-cli-service build",
         "lint": "vue-cli-service lint",
         "electron:serve": "vue-cli-service build && electron ."
       },
     ```

9.   修改 `vue.config.js` 部分：

     ```js
     const { defineConfig } = require('@vue/cli-service')
     module.exports = defineConfig({
       transpileDependencies: true,
       publicPath: './',
     })
     ```

10.   进行前端开发和调试：`yarn serve`，生成 vue release：`yarn build`

11.   启动 electron 应用：`yarn electron:serve`

12.   项目发布：

      1.   修改 `package.json`：

           ```json
             "scripts": {
               "serve": "vue-cli-service serve",
               "build": "vue-cli-service build",
               "lint": "vue-cli-service lint",
               "electron:serve": "vue-cli-service build && electron .",
               "electron:build": "vue-cli-service build && electron-builder build --windows --linux --mac"
             },
             "build": {
               "appId": "github.shi9uma.kisara-launcher",
               "productName": "Kisara-Launcher",
               "copyright": "Shiguma",
               "directories": {
                 "output": "dist_electron"
               },
               "mac": {
                 "category": "github.shi9uma.kisara-launcher"
               },
               "win": {
                 "target": "portable"
               },
               "publish": [
                 {
                   "provider": "github",
                   "url": "https://github.com/shi9uma/kisara-launcher"
                 }
               ]
             },
           ```

      2.   build 成 electron 应用发布：`yarn electron:build`（windows 下需要管理员）

13.   完整 `packages.json` 如下：

      ```json
      {
        "name": "kisara-launcher",
        "version": "0.1.0",
        "private": true,
        "author": "shi9uma <https://github.com/shi9uma>",
        "description": "A Minecraft Java Edition Launcher with Vue",
        "main": "index.js",
        "scripts": {
          "serve": "vue-cli-service serve",
          "build": "vue-cli-service build",
          "lint": "vue-cli-service lint",
          "electron:serve": "vue-cli-service build && electron .",
          "electron:build": "vue-cli-service build && electron-builder build --windows"
        },
        "build": {
          "appId": "github.shi9uma.kisara-launcher",
          "productName": "Kisara-Launcher",
          "copyright": "shiguma",
          "directories": {
            "output": "dist_electron"
          },
          "electronDownload": {
            "cache": "./electron-cache"
          },
          "mac": {
            "category": "github.shi9uma.kisara-launcher"
          },
          "win": {
            "target": "portable"
          }
        },
        "dependencies": {
          "core-js": "^3.8.3",
          "vue": "^3.2.13"
        },
        "devDependencies": {
          "@babel/core": "^7.12.16",
          "@babel/eslint-parser": "^7.12.16",
          "@vue/cli-plugin-babel": "~5.0.0",
          "@vue/cli-plugin-eslint": "~5.0.0",
          "@vue/cli-service": "~5.0.0",
          "electron": "^29.1.5",
          "electron-builder": "^24.13.3",
          "electron-builder-squirrel-windows": "^24.13.3",
          "eslint": "^7.32.0",
          "eslint-plugin-vue": "^8.0.3"
        },
        "eslintConfig": {
          "root": true,
          "env": {
            "node": true
          },
          "extends": [
            "plugin:vue/vue3-essential",
            "eslint:recommended"
          ],
          "parserOptions": {
            "parser": "@babel/eslint-parser"
          },
          "rules": {}
        },
        "browserslist": [
          "> 1%",
          "last 2 versions",
          "not dead",
          "not ie 11"
        ]
      }
      ```

## init for vite

1.   `cnpm install -g yarn vite`

2.   `yarn create vite kisara-launcher --template vue`，`cd kisara-launcher`

3.   `yarn add electron electron-builder --dev`

4.   electron 入口文件 `touch index.js`：

     ```js
     import { app, BrowserWindow } from 'electron';
     import path from 'path';
     import { fileURLToPath } from 'url';
     
     const __dirname = path.dirname(fileURLToPath(import.meta.url));
     
     function createWindow() {
         const win = new BrowserWindow({
             width: 800,
             height: 600,
             webPreferences: {
                 nodeIntegration: true,
                 contextIsolation: false,
             },
         });
     
         const startUrl = process.env.NODE_ENV === 'development'
             ? 'http://localhost:3000'
             : new URL('dist/index.html', import.meta.url).toString();
     
         win.loadURL(startUrl);
     }
     
     app.whenReady().then(createWindow);
     ```

5.   vue 模块 `touch /src/components/sum_test.vue`：

     ```vue
     <template>
         <div id="app">
             <input v-model="number1" type="number" placeholder="Number 1" />
             <input v-model="number2" type="number" placeholder="Number 2" />
             <button @click="sumNumbers">Sum</button>
             <div>Result: {{ result }}</div>
         </div>
     </template>
     
     <script>
     export default {
         data() {
             return {
                 number1: 0,
                 number2: 0,
                 result: 0,
             };
         },
         methods: {
             sumNumbers() {
                 this.result = parseInt(this.number1) + parseInt(this.number2);
             },
         },
     };
     </script>
     ```

6.   vue 入口文件 `src/App.vue`：

     ```vue
     <script setup>
     import HelloWorld from './components/HelloWorld.vue'
     import sum_test from './components/sum_test.vue';
     </script>
     
     <template>
       <div>
         <a href="https://vitejs.dev" target="_blank">
           <img src="/vite.svg" class="logo" alt="Vite logo" />
         </a>
         <a href="https://vuejs.org/" target="_blank">
           <img src="./assets/vue.svg" class="logo vue" alt="Vue logo" />
         </a>
       </div>
       <HelloWorld msg="Vite + Vue" />
       <sum_test />
     </template>
     
     <style scoped>
     .logo {
       height: 6em;
       padding: 1.5em;
       will-change: filter;
       transition: filter 300ms;
     }
     
     .logo:hover {
       filter: drop-shadow(0 0 2em #646cffaa);
     }
     
     .logo.vue:hover {
       filter: drop-shadow(0 0 2em #42b883aa);
     }
     </style>
     ```

7.   vite 执行配置 `vite.config.js`：

     ```js
     import { defineConfig } from 'vite'
     import vue from '@vitejs/plugin-vue'
     
     // https://vitejs.dev/config/
     export default defineConfig({
       plugins: [vue()],
       base: './',
     })
     ```

8.   完整 `package.json` 如下（包含后续添加）：

     ```json
     {
       "name": "kisara-launcher",
       "version": "0.1.0",
       "private": true,
       "author": "shi9uma <https://github.com/shi9uma>",
       "description": "A Minecraft Java Edition Launcher with Vue",
       "type": "module",
       "main": "index.js",
       "scripts": {
         "dev": "vite",
         "electron:dev": "vite build && electron .",
         "build": "vite build && electron-builder"
       },
       "build": {
         "appId": "github.shi9uma.kisara-launcher",
         "productName": "Kisara-Launcher",
         "copyright": "shi9uma",
         "directories": {
           "output": "dist_electron"
         },
         "electronDownload": {
           "cache": "./electron-cache"
         },
         "mac": {
           "category": "github.shi9uma.kisara-launcher"
         },
         "win": {
           "target": "portable"
         }
       },
       "dependencies": {
         "vue": "^3.4.21"
       },
       "devDependencies": {
         "@vitejs/plugin-vue": "^5.0.4",
         "electron": "^29.1.5",
         "electron-builder": "^24.13.3",
         "vite": "^5.2.0"
       }
     }
     ```

9.   开发，测试

     1.   前端：`vite`；
     2.   electron 整合：`yarn electron:dev`

10.   打包：`yarn build`（windows 可能需要管理员）

## references

1.   [electron-builder 打包详解](https://github.com/QDMarkMan/CodeBlog/blob/master/Electron/electron-builder%E6%89%93%E5%8C%85%E8%AF%A6%E8%A7%A3.md)
2.   [Vite 官方中文文档](https://cn.vitejs.dev/guide/)
3.   
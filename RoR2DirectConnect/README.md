# RoR2DirectConnect

BepInEx 插件，为 Risk of Rain 2 提供 IP 直连多人游戏功能，完全绕过 Steam 网络。

## 功能

- **IP 直连** — 通过 IP 地址创建/加入游戏（支持 IPv4 和 IPv6）
- **完整 Steam 绕过** — 无需 Steam 认证、大厅或 P2P
- **原生 UI** — 集成到多人游戏菜单（另有 F6 IMGUI 备用界面）
- **自定义昵称** — 无需 Steam 即可设置游戏内显示名称
- **设置保存** — IP、端口、昵称等在会话之间保持
- **无侵入** — 不使用直连时，正常游戏模式完全不受影响

## 安装

1. 通过 r2modman 安装 [BepInEx 5](https://thunderstore.io/package/bbepis/BepInExPack/)和[RoR2DirectConnect](https://thunderstore.io/package/Cindy_Master/RoR2DirectConnect/)
2. 启动游戏

## 使用方法

### 创建房间
1. 进入 **多人游戏** 菜单
2. 点击 **IP Direct Connect**
3. 设置昵称和最大玩家数
4. 点击 **Host Game**
5. 将你的 IP 分享给好友（默认端口：7777）

### 加入房间
1. 进入 **多人游戏** 菜单
2. 点击 **IP Direct Connect**
3. 输入主机的 IP 地址和端口
4. 点击 **Connect**

### 控制台命令
- `dc_host` — 快速创建直连服务器
- `dc_connect ip:port` — 通过命令行连接
- `dc_status` — 查看当前连接状态

## 从源码构建

```bash
dotnet build -c Release
# 输出: bin/Release/netstandard2.1/RoR2DirectConnect.dll
```

**依赖**（在 .csproj 中引用，未包含在仓库中）：
- RoR2 游戏程序集（`Risk of Rain 2_Data/Managed/`）
- BepInEx 程序集

## 文件结构

```
RoR2DirectConnect.dll        # 编译好的插件
Plugin.cs                    # 入口点，Harmony 补丁注册
DirectConnectUI.cs           # F6 IMGUI 备用界面
Localization.cs              # 多语言 UI 字符串（11 种语言）
RoR2DirectConnect.csproj     # 构建配置
Patches/
  SteamBypassPatches.cs      # 核心 Steam 绕过（大厅、P2P、认证、服务器）
  PlatformAuthPatch.cs       # 直连认证自定义 PlatformID
  PlatformHostPatch.cs       # 主机命令绕过
  DisplayNamePatch.cs        # 显示名称注入
  AddressPortPairPatch.cs    # IPv6 地址解析支持
  DirectConnectCommands.cs   # 控制台命令
UI/
  MenuInjector.cs            # 原生菜单按钮和面板注入
  DirectConnectPanel.cs      # 直连面板 UI
```

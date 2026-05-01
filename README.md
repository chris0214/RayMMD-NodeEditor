# Ray-MMD Node Editor

Ray-MMD Node Editor 是一个面向 Ray-MMD 的专用节点材质编辑器。它用于生成 Ray-MMD 可以使用的 `material_2.0.fx`、`material_common_2.0.fxsub`、`ray.conf`、`ray_advanced.conf` 和可选的 Shader patch 副本。

这个项目的目标不是替代 Ray-MMD，也不是做一个通用 MME 编辑器。它只针对 Ray-MMD 的材质和部分最终着色管线，把常用参数、贴图、数学节点、PBR/BRDF 辅助节点、Ray 通道节点和高级 patch 工作流做成可视化编辑。

当前版本：`0.1.0-preview.1`。版权作者为 `克里斯提亚娜`。

## 重要说明

- 本仓库只包含编辑器源码，不包含 Ray-MMD 本体。
- 本工具不会修改原始 Ray-MMD 目录。导出时默认写入副本目录。
- 兼容模式只生成原版 Ray-MMD 可识别的材质参数。
- 高级节点模式会 patch 导出副本里的 Ray 文件，例如 `Materials/material_common_2.0.fxsub` 和部分 `Shader/*.fxsub`。
- 如果使用了 `Ray Shading Output`、SceneColor、SSAO、IBL、Fog、Shadow、最终光照混合等节点，必须按整套导出的 Ray 包使用，不能只拿一个 `material_2.0.fx` 去套模型。

## 功能概览

### 兼容模式

兼容模式是默认模式。它不 patch Ray 核心文件，只生成 Ray 原版材质系统认识的 `#define` 和 `const` 参数。

适合：

- 调整 Albedo、Alpha、Smoothness、Metalness、Specular、Occlusion、Normal、Parallax、Emissive 等材质槽。
- 使用常量数学，例如 `0.8 * 0.5` 导出为固定数值。
- 使用单张文件贴图乘常量，例如 Smoothness 贴图乘一个强度。
- 把 Roughness 输入映射到 Ray 的 Smoothness 系统。
- 做稳定、可分享、兼容性优先的材质预设。

不适合：

- 两张贴图逐像素相乘。
- 使用 SceneColor、SSAO、Fog、IBL、Shadow 等 Ray 最终通道。
- 对 Ray 最终光照结果做混合。

### 高级节点模式

高级节点模式会在导出副本中插入 HLSL helper 函数和覆盖逻辑。它用于支持更多逐像素表达式。

第一类高级功能写入：

```text
ExportedRayPreset/Materials/material_common_2.0.fxsub
```

例如：

- `Texture A * Texture B -> Smoothness`
- `Noise -> Albedo`
- `ColorRamp -> Emissive`
- `LayerBlend -> Albedo`
- `CustomA / CustomB` 输出到 Ray custom 材质数据

第二类最终着色功能写入：

```text
ExportedRayPreset/Shader/ShadingMaterials.fxsub
```

例如：

- Ray Shading Output
- SceneColor
- SceneDepth
- SceneNormal
- SSAO
- Shadow
- IBL
- SSR
- Fog
- Diagnostic / Channel Split
- 最终光照 Add / Multiply / Color 混合

## 目录结构

源码目录大致如下：

```text
RayMmdNodeEditor/
  Controls/                  节点画布、搜索、右键菜单、颜色控件、UI 控件
  Graph/                     节点类型、节点定义、图结构、示例图
  Services/                  Ray 编译器、导出器、patcher、兼容性检查、配置写入
  MainForm.cs                WinForms 主窗口和面板逻辑
  Program.cs                 程序入口和 --self-test
  RayMmdNodeEditor.csproj    .NET 项目文件
  RayMmdNodeEditor.sln       解决方案
  LICENSE                    MIT License
  README.md                  使用说明
```

生成物和本地缓存不会进入仓库：

```text
bin/
obj/
publish-single/
ExportedRayPreset/
.dotnet-home/
.appdata/
.localappdata/
```

## 环境要求

### 运行编辑器

- Windows 10 或 Windows 11
- 如果使用自包含单文件发布版，不需要单独安装 .NET Runtime
- 如果从源码运行，需要 .NET 8 SDK 或更新版本

### 使用导出结果

- MikuMikuDance
- MikuMikuEffect
- Ray-MMD 原版目录

Ray-MMD 目录至少应包含：

```text
ray.fx
ray.conf
ray_advanced.conf
Materials/
  material_2.0.fx
  material_common_2.0.fxsub
Shader/
  ShadingMaterials.fxsub
```

## 从源码构建

在仓库目录打开 PowerShell：

```powershell
dotnet build RayMmdNodeEditor.sln -c Release
```

运行自检：

```powershell
dotnet run --project RayMmdNodeEditor.csproj -c Release -- --self-test
```

期望输出包含：

```text
OK
COMPAT_MATH_OK
ADVANCED_OK
```

发布单文件 exe：

```powershell
dotnet publish RayMmdNodeEditor.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true -o publish-single
```

发布结果：

```text
publish-single/RayMmdNodeEditor.exe
```

如果发布时报 `Access to the path ... RayMmdNodeEditor.exe is denied`，说明旧的编辑器 exe 还在运行。关闭它后重新发布。

## 第一次使用

1. 启动 `RayMmdNodeEditor.exe`。
2. 打开右侧或上方的 `Ray 参数` 面板。
3. 设置 `Ray 根目录`。
4. 设置 `导出目录`。
5. 选择材质导出模式：
   - `兼容模式`
   - `高级节点模式`
6. 在节点画布里编辑材质图。
7. 查看 `FX 预览` 和 `问题` 页。
8. 导出。

### Ray 根目录

Ray 根目录是原始 Ray-MMD 文件夹，不是 MMD 文件夹，也不是模型文件夹。

正确示例：

```text
D:\MMD\Effects\ray-mmd\
```

这个目录下应该能看到：

```text
D:\MMD\Effects\ray-mmd\ray.fx
D:\MMD\Effects\ray-mmd\ray.conf
D:\MMD\Effects\ray-mmd\ray_advanced.conf
D:\MMD\Effects\ray-mmd\Materials\material_common_2.0.fxsub
```

### 导出目录

导出目录是本工具生成副本的位置。建议不要选原始 Ray-MMD 目录。

推荐示例：

```text
D:\MMD\Effects\RayMmdNodeEditor_Export\
```

导出后可能得到：

```text
RayMmdNodeEditor_Export/
  ray.fx
  ray.conf
  ray_advanced.conf
  Materials/
    material_2.0.fx
    material_common_2.0.fxsub
    textures/
  Shader/
    ShadingMaterials.fxsub
    ...
  RayMmdNodeEditor_Report.txt
```

## 基础工作流

### 只做普通材质

如果你只是调整材质参数，使用兼容模式即可。

典型节点：

```text
Ray Texture Slot -> Ray Material Output.Albedo
Scalar           -> Ray Material Output.Smoothness
Scalar           -> Ray Material Output.Metalness
Color            -> Ray Material Output.Emissive
```

导出后，在 MMD/MME 里给模型材质加载：

```text
ExportedRayPreset/Materials/material_2.0.fx
```

如果你没有使用最终着色节点，通常只需要材质 FX 和同目录的 `material_common_2.0.fxsub`。

### 使用高级材质表达式

如果要让两张贴图逐像素混合，或使用噪声、ColorRamp、复杂数学输出材质槽，使用高级节点模式。

示例：

```text
Ray Texture Slot A
Ray Texture Slot B
Multiply
Ray Material Output.Smoothness
```

这会生成 patched：

```text
Materials/material_common_2.0.fxsub
```

在 MMD/MME 中应使用导出目录里的材质 FX，并保持 `material_common_2.0.fxsub` 在同一目录。

### 使用最终着色节点

如果使用以下节点或类似节点：

- Ray Scene Color
- Ray Scene Depth
- Ray Scene Normal
- Ray SSAO
- Ray Shadow
- Ray IBL
- Ray SSR
- Ray Fog
- Ray Channel Split
- Ray Diagnostic
- Ray Shading Output

必须使用整套导出的 Ray 包。

正确连接方式：

```text
Ray Scene Color / Ray SSAO / Ray Fog / Ray IBL / Shadow ...
    -> Ray Shading Output.Color/Add/Multiply
```

不要接到：

```text
Ray Material Output.Albedo
```

因为 `Albedo` 属于材质阶段，SceneColor/SSAO/Fog/Shadow 属于 Ray 已经计算后的最终着色阶段。

## 在 MMD/MME 中使用导出结果

### 情况 A：只用材质 FX

适用条件：

- 只使用兼容模式，或只 patch 了 `Materials/material_common_2.0.fxsub`
- 没有使用 `Ray Shading Output`
- 没有使用 SceneColor、SSAO、IBL、Fog、Shadow 等最终着色通道

使用方法：

1. 打开 MMD。
2. 打开 MME 面板。
3. 给模型材质加载：

```text
ExportedRayPreset/Materials/material_2.0.fx
```

4. 确认同目录存在：

```text
ExportedRayPreset/Materials/material_common_2.0.fxsub
```

5. 确认贴图目录存在：

```text
ExportedRayPreset/Materials/textures/
```

不要只复制 `material_2.0.fx` 一个文件。Ray 的 include 是相对路径，缺少 `material_common_2.0.fxsub` 会报：

```text
failed to open source file: material_common_2.0.fxsub
```

### 情况 B：使用整套导出的 Ray 包

适用条件：

- 使用了 `Ray Shading Output`
- 使用了 SceneColor、SceneDepth、SceneNormal
- 使用了 SSAO、Shadow、IBL、SSR、Fog
- 使用了最终光照混合、诊断通道、Ray 通道节点

使用方法：

1. 导出时使用完整 Ray 预设导出。
2. 在 MMD/MME 中加载导出目录里的：

```text
ExportedRayPreset/ray.fx
```

3. 给模型材质加载：

```text
ExportedRayPreset/Materials/material_2.0.fx
```

4. 保持以下目录结构不变：

```text
ExportedRayPreset/
  ray.fx
  ray.conf
  ray_advanced.conf
  Materials/
    material_2.0.fx
    material_common_2.0.fxsub
    textures/
  Shader/
    ShadingMaterials.fxsub
```

不要把 `material_2.0.fx` 单独拖到模型贴图目录里。只要用了最终着色节点，就必须使用导出的 `ray.fx` 和 `Shader` 目录。

## 节点连接规则

### Ray Material Output

用于材质参数阶段。

常见输入：

- `Albedo`
- `SubAlbedo`
- `Alpha`
- `Normal`
- `SubNormal`
- `Smoothness`
- `Metalness`
- `Specular`
- `Occlusion`
- `Parallax`
- `Emissive`
- `CustomA`
- `CustomB`

适合连接：

- Scalar
- Color
- Float2 / Float3 / Float4
- Ray Texture Slot
- Add / Subtract / Multiply / Divide
- Power
- Clamp / Saturate
- Lerp
- OneMinus
- Split / Compose
- UV Transform
- Noise / ColorRamp / LayerBlend
- BRDF/PBR 辅助节点中能输出材质参数的节点

### Ray Shading Output

用于最终着色阶段。

输入：

- `Color`
- `Add`
- `Multiply`

适合连接：

- Ray Scene Color
- Ray Scene Depth
- Ray Scene Normal
- Ray SSAO
- Ray Shadow
- Ray IBL
- Ray SSR
- Ray Fog
- Ray Channel Split
- Ray Diagnostic
- 最终光照混合类节点

如果把最终着色节点接到 `Ray Material Output.Albedo`，导出时会提示：

```text
uses a Ray final-shading channel node. Connect it to Ray Shading Output instead.
```

这不是崩溃，是阶段保护。

## 贴图路径管理

`Ray Texture Slot` 支持文件贴图。

推荐做法：

1. 把图片拖进画布。
2. 工具会生成一个 `Ray Texture Slot`。
3. 用户手动选择它要接到哪个材质槽。
4. 导出时，贴图会复制到：

```text
Materials/textures/
```

5. FX 里的路径会改写为相对路径：

```hlsl
"textures/your_texture.png"
```

本工具不会根据文件名自动猜测 Albedo、Normal、Roughness 等目标槽。这样可以避免错误猜测影响工作流。

## 材质文件命名

可以在导出参数里修改材质 FX 文件名。

例如：

```text
skin_soft.fx
metal_wet.fx
cloth_blue.fx
```

完整导出时文件会写入：

```text
ExportedRayPreset/Materials/skin_soft.fx
```

单独导出 FX 时，也会按你选择的文件名写出。

## 导出方式

### 导出 Ray 预设

推荐主流程。

会导出：

```text
ray.fx
ray.conf
ray_advanced.conf
Materials/material_2.0.fx
Materials/material_common_2.0.fxsub
Materials/textures/
Shader/
RayMmdNodeEditor_Report.txt
```

如果开启完整 Ray 包复制，会先复制 Ray 根目录，再覆盖工具生成的文件。

### 自动导出

设置页的 `自动导出` 用于在 MMD/MME 中伪实时查看改动。开启后，节点图或 Ray 参数变化会在约 750ms 后自动覆盖当前导出目录里的生成文件：

```text
ray.conf
ray_advanced.conf
Materials/material_2.0.fx
Materials/material_common_2.0.fxsub
Shader/ShadingMaterials.fxsub
RayMmdNodeEditor_Report.txt
```

自动导出不会每次复制完整 Ray 包。第一次使用某个导出目录时，建议先手动执行一次 `导出 Ray 预设`，再开启自动导出做细调。

### 单独导出材质 FX

只适合普通材质或轻量高级材质。

会写出：

```text
selected_name.fx
material_common_2.0.fxsub
textures/
```

不适合：

- Ray Shading Output
- SceneColor
- SSAO
- IBL
- SSR
- Fog
- Shadow
- 任何需要 patch `Shader/` 的节点

如果使用这些节点，请导出整套 Ray 预设。

## Ray 参数面板

Ray 参数面板用于编辑常见 `ray.conf`、`ray_advanced.conf` 和 `material_common_2.0.fxsub` 参数。

设计原则：

- 已暴露的参数可以在 UI 中修改。
- 未暴露的参数尽量保留 Ray 原文件内容。
- 导出时只修改目标项。
- 下拉菜单用于 Ray 枚举值。
- 数值项保持原 Ray 配置格式。

常见项：

- 质量预设
- ray.conf 开关
- ray_advanced.conf 光照、Bloom、SSR、SSDO、SSSS、PSSM、FXAA 等参数
- material_common 贴图过滤模式
- 材质导出模式
- 完整 Ray 包复制
- 贴图复制
- 材质文件名

## 模式说明

### 兼容模式

输出目标：

```text
Materials/material_2.0.fx
```

特点：

- 不 patch Ray common
- 不 patch Shader
- 最稳定
- 适合基础材质参数

### 高级节点模式

输出目标可能包括：

```text
Materials/material_2.0.fx
Materials/material_common_2.0.fxsub
Shader/ShadingMaterials.fxsub
Shader/directional_lighting.fxsub
```

特点：

- 支持逐像素表达式
- 支持部分最终着色通道
- 支持 Ray 自身通道混合
- 必须保持导出包结构

## 常见错误和处理

### failed to open source file: material_common_2.0.fxsub

原因：

`material_2.0.fx` 找不到同目录下的 `material_common_2.0.fxsub`。

处理：

- 不要只复制 `material_2.0.fx`
- 确保 `material_common_2.0.fxsub` 和材质 FX 在同一目录
- 推荐直接使用完整导出的 `Materials/` 目录

### unexpected token 'point'

原因：

旧版本高级 patch 里使用了 `point` 作为局部变量名，D3DX FX 编译器会冲突。

处理：

- 使用新版重新导出
- 覆盖旧的 `material_common_2.0.fxsub`

### unrecognized identifier 'MaterialParam'

原因：

旧版本高级 patch 插入到了 `MaterialParam` 结构体声明之前。

处理：

- 使用新版重新导出
- 覆盖旧的 `material_common_2.0.fxsub`

### Ray final-shading channel node

提示示例：

```text
Albedo uses a Ray final-shading channel node. Connect it to Ray Shading Output instead.
```

原因：

SceneColor、SSAO、Fog、IBL、Shadow 等最终着色节点被接到了 `Ray Material Output`。

处理：

- 添加 `Ray Shading Output`
- 把节点接到 `Color`、`Add` 或 `Multiply`
- 使用完整 Ray 预设导出

### 导出后效果缺失

检查：

- 是否使用了导出目录里的 `ray.fx`
- 是否保持 `Shader/` 目录
- 是否保持 `Materials/` 目录
- 是否加载了导出的材质 FX
- 是否误用了原始 Ray-MMD 的 `ray.fx`

如果用了 `Ray Shading Output`，但 MMD 里仍然加载原始 Ray-MMD 的 `ray.fx`，Shader patch 不会生效。

## 自检和调试

运行：

```powershell
dotnet run --project RayMmdNodeEditor.csproj -c Release -- --self-test
```

自检覆盖：

- 兼容模式基础编译
- 常量数学折算
- 高级模式 patch block 生成
- 高级 common patch 插入位置
- 高级 shader patch 基础结构

导出后会生成报告：

```text
RayMmdNodeEditor_Report.txt
```

报告内容包括：

- 导出模式
- Ray 根目录
- 导出目录
- 材质文件名
- patched 文件
- 复制的贴图
- 兼容性问题
- 使用的高级节点槽
- 自动开启的 Ray 能力

## 开发说明

核心类：

- `RayMaterialCompiler`
  - 兼容模式材质编译器
  - 输出 `material_2.0.fx`

- `RayAdvancedMaterialCompiler`
  - 高级节点 HLSL 表达式编译器
  - 输出 common patch 和 shading patch block

- `RayAdvancedCommonPatcher`
  - patch `Materials/material_common_2.0.fxsub`

- `RayAdvancedShadingPatcher`
  - patch `Shader/ShadingMaterials.fxsub`

- `RayAdvancedLightingPatcher`
  - patch Ray 光照相关 shader 文件

- `RayCompatibilityChecker`
  - 导出前检查节点、贴图、路径和 Ray 阶段兼容性

- `RayTextureExportManager`
  - 复制贴图并改写 FX 相对路径

- `RayPackageExportManager`
  - 复制完整 Ray 包到导出目录

- `RayExportReportWriter`
  - 生成导出报告

- `NodeRegistry.RayDefinitions`
  - Ray 专用节点定义

- `NodeCanvas`
  - 节点画布、拖拽、连接、搜索和菜单

## 添加新节点的大致流程

1. 在 `Graph/NodeKind.cs` 增加节点枚举。
2. 在 `Graph/NodeRegistry.RayDefinitions.cs` 或对应 registry 文件增加节点定义。
3. 如果是兼容模式可折算节点，在 `RayMaterialCompiler` 增加求值逻辑。
4. 如果是高级模式逐像素节点，在 `RayAdvancedMaterialCompiler` 增加 HLSL 表达式生成逻辑。
5. 如果节点依赖 Ray 的最终通道，标记为 Shading 阶段，并接入 `Ray Shading Output`。
6. 在 `RayNodeSupport` 中补充兼容模式/高级模式可用性。
7. 在 `RayCompatibilityChecker` 中补充错误提示。
8. 在 `Program.cs --self-test` 中加入最小验证。

## 兼容性边界

本工具优先保证：

- 不修改原始 Ray-MMD
- 导出副本可追踪
- 兼容模式稳定
- 高级模式 patch 尽量局部
- 报错时阻止明显错误的导出

不保证：

- 所有手写 Ray 材质都能和高级 patch 混用
- 所有 Ray-MMD fork 都有相同的 shader 文件结构
- 所有 MME / D3DX 编译器差异都完全兼容
- 高级节点在所有 Ray 版本上都能无修改工作

## 许可证

本项目使用 MIT License。详见 [LICENSE](LICENSE)。

Ray-MMD、MikuMikuDance、MikuMikuEffect 及相关资源属于各自作者。本项目不包含这些第三方项目的源码或资源。

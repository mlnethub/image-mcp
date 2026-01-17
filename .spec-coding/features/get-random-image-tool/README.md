# get-random-image-tool 功能定义

## 目标
完善现有的 MCP 工具 `get_random_image`，使其能够更稳定地从 Unsplash API 获取随机图片，从而支持如“给我一些灵感”、“换个背景”或“随便来张图”等用户请求。

## 关键规格
1.  **工具名称**: `get_random_image`
2.  **API 端点**: Unsplash `/photos/random`
3.  **代码位置**: `Tools/ImageSearchTools.cs`
4.  **参数**:
    *   `query` (string?): 可选的主题关键词（例如 "nature", "cats"）。
    *   `count` (int): 返回图片的数量（默认为 1）。
    *   `orientation` (string): 图片方向，可选值为 'landscape'（横向，默认）、'portrait'（纵向）或 'squarish'（方形）。
5.  **返回类型**: `IEnumerable<ImageResult>`（复用现有的数据模型）。
6.  **配置**: 复用 `ImageApiOptions` 类（使用 `ClientId` 和 `BaseUrl`）。

## 功能描述
在既有 MCP 工具 `get_random_image` 的基础上，增强对 Unsplash `/photos/random` 接口的调用，支持可选的 `query` 主题、返回数量 `count` 与图片方向 `orientation` 参数，确保响应内容稳定映射为 `ImageResult` 集合，并继续复用 `ImageSearchTools.cs` 与 `ImageApiOptions` 的既有实现。

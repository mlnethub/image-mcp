# get-random-image-tool 功能定义

## 目标
实现一个新的 MCP 工具 `get_random_image`，用于从 Unsplash API 获取随机图片。这将允许大模型响应如“给我一些灵感”、“换个背景”或“随便来张图”之类的用户请求。

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

# 设计文档 - get-random-image-tool

## 1. 简介
本文档详细描述了 `get_random_image` MCP 工具的架构和实现细节。该工具将集成到现有的 `Tools/ImageSearchTools.cs` 类中，利用 Unsplash API 的 `/photos/random` 端点。

## 2. 现有代码分析
*   **文件**: `Tools/ImageSearchTools.cs`
*   **当前功能**: 包含 `SearchImages` 静态方法，用于搜索图片。
*   **依赖**:
    *   `HttpClient`: 用于发送网络请求。
    *   `IOptions<ImageApiOptions>`: 用于获取 API 密钥 (`ClientId`)。
    *   `ModelContextProtocol.Server` 特性: `[McpServerTool]`, `[McpServerToolType]`.
    *   `System.Text.Json`: 用于 JSON 解析。

## 3. 详细设计

### 3.1 方法签名
将在 `ImageSearchTools` 类中添加一个新的静态方法：

```csharp
[McpServerTool, Description("Get random images for inspiration or background based on optional query.")]
public static async Task<IEnumerable<ImageResult>> GetRandomImages(
    HttpClient client,
    IOptions<ImageApiOptions> options,
    [Description("Optional query to filter images (e.g. 'nature', 'cats').")] string? query = null,
    [Description("Number of images to return. Default is 1.")] int count = 1,
    [Description("Image orientation: 'landscape' (default), 'portrait', or 'squarish'.")] string orientation = "landscape")
```

### 3.2 实现逻辑
1.  **获取配置**: 从 `options` 获取 `ClientId`。
2.  **构建 URL**:
    *   基础路径: `photos/random` (注意：需要确认 `BaseUrl` 是否包含末尾斜杠，代码中拼接时需注意)。
    *   必需参数: `client_id={clientId}`。
    *   可选参数:
        *   `count={count}` (注意：Unsplash 免费计划单次请求可能有数量限制，通常最大 30)。
        *   `orientation={orientation}`。
        *   `query={query}` (如果 `query` 不为空)。
3.  **发送请求**: `await client.ReadJsonDocumentAsync(url)`.
4.  **解析响应**:
    *   Unsplash Random API 的响应结构：
        *   如果 `count=1`，它可能返回单个对象 *或者* 数组（取决于 Unsplash SDK 或具体 API 版本，通常 `/photos/random?count=X` 即使 X=1 也返回数组，但纯 `/photos/random` 返回单个对象）。
        *   **策略**: 始终使用 `count` 参数，即使是 `count=1`，这样 API 应该始终返回数组，简化解析逻辑。或者，检查根元素是 Array 还是 Object。为了稳健性，建议检查 `ValueKind`。
    *   反序列化逻辑与 `SearchImages` 类似，映射到 `ImageResult`。
5.  **异常处理**: 捕获异常并返回空列表（与现有模式保持一致）。

### 3.3 数据模型映射
*   输入 JSON 字段 -> `ImageResult` 属性:
    *   `urls` -> `Urls`
    *   `description` -> `Description`
    *   `alt_description` -> `AltDescription`
    *   `width`, `height`, `user.name` 等 -> 暂时忽略（因为 `ImageResult` 只定义了上述字段）。

## 4. 安全与约束
*   **API 限制**: Unsplash API 有速率限制 (Rate limits)。
*   **参数校验**: 虽然方法签名允许任意 int，但实际 API 可能限制 `count` (1-30)。代码不强制校验，依赖 API 返回错误或截断，或进行简单通过。
*   **输入清洗**: `query` 参数需进行 URL 编码 (`Uri.EscapeDataString`)。

## 5. 验证计划
*   **手动验证**: 
    *   调用工具不带参数 -> 期望 1 张横向图片。
    *   调用 `query="mountain", orientation="portrait"` -> 期望竖屏山脉图片。

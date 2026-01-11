# 需求规格说明书 - get-random-image-tool

## 1. 概述
本功能旨在为 `image-mcp` 服务添加一个新的 MCP 工具 `get_random_image`。该工具允许用户（或 AI 代理）从 Unsplash 获取随机图片。该功能支持通过关键词、数量和方向进行筛选，旨在满足“寻找灵感”或“更换背景”等场景的需求。

## 2. 用户故事
作为一名 MCP 客户端用户（或 AI 助手），我希望能够调用一个工具来获取随机的高质量图片，并且可以指定图片的主题（如“自然”）和方向（如“横向”作为壁纸），以便我可以快速获得视觉灵感或装饰性素材。

## 3. 功能需求

### 3.1 核心功能 (REQ-001)
*   **REQ-001-001**: 当工具 `get_random_image` 被调用时，系统 **必须** 向 Unsplash API 的 `/photos/random` 端点发起 HTTP GET 请求。
*   **REQ-001-002**: 系统 **必须** 使用现有的 `ImageApiOptions` 配置中的 `BaseUrl` 和 `ClientId` 进行 API 认证和请求构建。

### 3.2 参数处理 (REQ-002)
*   **REQ-002-001 (方向筛选)**: 如果调用时提供了 `orientation` 参数（landscape, portrait, squarish），系统 **必须** 将其作为查询参数传递给 API。
*   **REQ-002-002 (方向默认值)**: 如果调用时未提供 `orientation` 参数，系统 **必须** 默认使用 "landscape"（横向）。
*   **REQ-002-003 (关键词筛选)**: 如果调用时提供了 `query` 参数，系统 **必须** 将其作为查询参数传递给 API，以限制随机图片的范围。
*   **REQ-002-004 (数量控制)**: 如果调用时提供了 `count` 参数，系统 **必须** 将其传递给 API；若未提供，**必须** 默认为 1。

### 3.3 响应处理 (REQ-003)
*   **REQ-003-001**: 系统 **必须** 将 Unsplash API 返回的 JSON 数据反序列化为 `IEnumerable<ImageResult>` 格式。
*   **REQ-003-002**: 每个 `ImageResult` 对象 **必须** 包含图片的 URL（Regular, Small, Thumb 等）、描述（Description/AltDescription）。

### 3.4 错误处理 (REQ-004)
*   **REQ-004-001**: 如果 API 请求失败或没有返回数据，系统 **应当** 返回一个空的集合，而不是抛出未处理的异常，以保持与现有 `SearchImages` 工具行为一致。

## 4. 验收标准
1.  调用 `get_random_image` 且不带参数，应返回 1 张横向（landscape）的随机图片。
2.  调用 `get_random_image` 并带参数 `query="cat"`, `orientation="portrait"`, `count=3`，应返回 3 张关于猫的竖屏图片。
3.  工具的描述和参数注解应正确显示在 MCP 元数据中，以便大模型正确理解如何使用。

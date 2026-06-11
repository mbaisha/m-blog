/** 标签响应 */
export interface TagResponse {
  id: string
  name: string
  slug: string
  color: string | null
  bgColor: string | null
  articleCount: number
  createdAt: string
  updatedAt: string
}

/** 标签列表项 */
export interface TagListItem {
  id: string
  name: string
  slug: string
  color: string | null
  bgColor: string | null
  articleCount: number
  createdAt: string
}

/** 创建标签请求 */
export interface CreateTagRequest {
  name: string
  slug: string
  color?: string
  bgColor?: string
}

/** 更新标签请求 */
export interface UpdateTagRequest {
  name: string
  slug: string
  color?: string
  bgColor?: string
}

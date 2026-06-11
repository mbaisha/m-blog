/** 分类响应 */
export interface CategoryResponse {
  id: string
  name: string
  slug: string
  description: string | null
  coverImageId: string | null
  coverImageUrl: string | null
  sortOrder: number
  parentId: string | null
  level: number
  children: CategoryListItem[]
  articleCount: number
  seoTitle: string | null
  seoDescription: string | null
  seoKeywords: string | null
  createdAt: string
  updatedAt: string
}

/** 分类列表项 */
export interface CategoryListItem {
  id: string
  name: string
  slug: string
  description: string | null
  sortOrder: number
  parentId: string | null
  level: number
  articleCount: number
  children?: CategoryListItem[]
  createdAt: string
}

/** 创建分类请求 */
export interface CreateCategoryRequest {
  name: string
  slug: string
  description?: string
  parentId?: string | null
  sortOrder: number
}

/** 更新分类请求 */
export interface UpdateCategoryRequest {
  name: string
  slug: string
  description?: string
  parentId?: string | null
  sortOrder: number
}
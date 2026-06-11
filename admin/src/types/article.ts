/** 文章查询参数 */
export interface ArticleQueryParams {
  page: number
  pageSize: number
  keyword?: string
  status?: string
  categoryId?: string
  tagId?: string
  isTop?: boolean
  isRecommend?: boolean
  showDeleted?: boolean
  sortBy?: string
  sortOrder?: string
}

/** 文章列表项 */
export interface ArticleListItem {
  id: string
  title: string
  slug: string
  summary: string | null
  status: string
  isTop: boolean
  isRecommend: boolean
  viewCount: number
  likeCount: number
  commentCount: number
  categoryName: string | null
  authorName: string
  tags: string[]
  publishedAt: string | null
  createdAt: string
  updatedAt: string
}

/** 创建文章请求 */
export interface CreateArticleRequest {
  title: string
  slug: string
  summary?: string
  content: string
  coverImageId?: string | null
  categoryIds?: string[]
  tagIds?: string[]
  status: string
  isTop: boolean
  isRecommend: boolean
  seoTitle?: string
  seoDescription?: string
  seoKeywords?: string
}

/** 更新文章请求 */
export interface UpdateArticleRequest {
  title: string
  slug: string
  summary?: string
  content: string
  coverImageId?: string | null
  categoryIds?: string[]
  tagIds?: string[]
  status: string
  isTop: boolean
  isRecommend: boolean
  seoTitle?: string
  seoDescription?: string
  seoKeywords?: string
}

/** 文章详情作者信息 */
export interface ArticleAuthorInfo {
  id: string
  username: string
  avatarUrl: string | null
}

/** 文章详情分类信息 */
export interface ArticleCategoryInfo {
  id: string
  name: string
  slug: string
}

/** 文章详情标签信息 */
export interface ArticleTagInfo {
  id: string
  name: string
  color: string | null
}

/** 文章详情 */
export interface ArticleDetail {
  id: string
  title: string
  slug: string
  summary: string | null
  content: string
  coverImageId: string | null
  coverImageUrl: string | null
  author: ArticleAuthorInfo
  category: ArticleCategoryInfo | null
  categories: ArticleCategoryInfo[]
  tags: ArticleTagInfo[]
  status: string
  isTop: boolean
  isRecommend: boolean
  viewCount: number
  likeCount: number
  commentCount: number
  seoTitle: string | null
  seoDescription: string | null
  seoKeywords: string | null
  publishedAt: string | null
  createdAt: string
  updatedAt: string
}
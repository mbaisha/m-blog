// ===== 统一 API 响应结构 =====

export interface ApiResponse<T> {
  success: boolean
  data: T
  message?: string
}

export interface PagedData<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
}

// ===== 前台公开文章 =====

export interface PublicArticleListItem {
  id: string
  title: string
  slug: string
  summary: string | null
  coverImageUrl: string | null
  category: PublicCategoryInfo | null
  categories: PublicCategoryInfo[]
  tags: PublicTagInfo[]
  authorName: string
  viewCount: number
  commentCount: number
  isRecommend: boolean
  isTop: boolean
  publishedAt: string | null
  updatedAt: string
}

export interface PublicArticleDetail {
  id: string
  title: string
  slug: string
  summary: string | null
  content: string
  coverImageUrl: string | null
  author: PublicAuthorInfo
  category: PublicCategoryInfo | null
  categories: PublicCategoryInfo[]
  tags: PublicTagInfo[]
  viewCount: number
  commentCount: number
  seoTitle: string | null
  seoDescription: string | null
  seoKeywords: string | null
  status: string
  publishedAt: string | null
  updatedAt: string
}

export interface PublicArticleQueryParams {
  page?: number
  pageSize?: number
  keyword?: string
  categoryId?: string
  categorySlug?: string
  tagId?: string
  tagSlug?: string
  sortBy?: 'latest' | 'views' | 'comments'
}

// ===== 前台公开分类 =====

export interface PublicCategoryInfo {
  id: string
  name: string
  slug: string
  description: string | null
  coverImageUrl: string | null
  articleCount: number
}

// ===== 前台公开标签 =====

export interface PublicTagInfo {
  id: string
  name: string
  slug: string
  color: string | null
  bgColor: string | null
  articleCount: number
}

// ===== 前台公开作者 =====

export interface PublicAuthorInfo {
  id: string
  username: string
  avatarUrl: string | null
}

// ===== 页面、导航、页脚、主题 =====

export interface PublicPageItem {
  id: string
  title: string
  slug: string
  summary: string | null
  content: string | null
  seoTitle: string | null
  seoDescription: string | null
  seoKeywords: string | null
  enableComments: boolean
  publishedAt: string | null
  updatedAt: string
}

export interface PublicNavigationItem {
  title: string
  url: string
  icon: string | null
  openInNewTab: boolean
  children: PublicNavigationItem[]
}

export interface PublicFooterConfig {
  id?: string
  copyright: string | null
  icpNumber: string | null
  icpUrl: string | null
  policeNumber: string | null
  policeUrl: string | null
  socialLinks: string
  blocks: string
  style: string
  isVisible: boolean
  contactEmail: string | null
  contactWeChat: string | null
  contactPhone: string | null
}

export interface PublicThemeSetting {
  id?: string
  themeName: string
  primaryColor: string
  accentColor: string
  backgroundColor: string
  textColor: string
  linkColor: string
  navbarBackground: string | null
  navbarTextColor: string | null
  borderRadius: string
  customCss: string
}

export interface PublicModuleLayout {
  id: string
  pageKey: string
  moduleKey: string
  title: string | null
  sortOrder: number
  isEnabled: boolean
  config: string
}

export interface ArchiveArticleItem {
  id: string
  title: string
  slug: string
  summary: string | null
  publishedAt: string
}

export interface ArchiveItem {
  year: number
  month: number
  count: number
  articles: ArchiveArticleItem[]
}

// ===== 站点配置（SEO） =====

export interface SiteConfig {
  siteUrl: string
  siteName: string
  apiBaseUrl: string
}

// ===== 站点设置（后台可配置） =====

export interface PublicSiteSettingResponse {
  siteName: string
  siteDescription: string | null
  logoImageUrl: string | null
  faviconImageUrl: string | null
}

export interface PublicSeoSettingResponse {
  id: string
  pageKey: string
  title: string | null
  description: string | null
  keywords: string | null
  ogImageUrl: string | null
  canonicalUrl: string | null
}

// ===== 评论 =====

export interface CaptchaImageResponse {
  sessionId: string
  imageBase64: string
}

export interface CommentItem {
  id: string
  articleId: string
  parentId: string | null
  parentNickname: string | null
  parentContent: string | null
  nickname: string
  email: string | null
  website: string | null
  content: string
  avatar: string | null
  createdAt: string
}

export interface CreateCommentRequest {
  articleId: string
  parentId?: string | null
  nickname: string
  email?: string
  website?: string
  content: string
  captchaSessionId: string
  captchaAnswer: string
}

export interface CreateCommentResult {
  status: string
  message: string
}

// ===== 个人页面（Phase 6） =====

export interface PublicProfileSection {
  id: string
  sectionType: string
  title: string | null
  content: string | null
  sortOrder: number
  metadata: string
  isEnabled: boolean
  createdAt: string
  updatedAt: string
}

// ===== 项目展示（Phase 6） =====

export interface PublicProjectListItem {
  id: string
  title: string
  slug: string
  summary: string | null
  coverImageUrl: string | null
  techStack: string[]
  projectUrl: string | null
  githubUrl: string | null
  demoUrl: string | null
  sortOrder: number
  createdAt: string
}

export interface PublicProjectDetail {
  id: string
  title: string
  slug: string
  summary: string | null
  content: string | null
  coverImageUrl: string | null
  techStack: string[]
  projectUrl: string | null
  githubUrl: string | null
  demoUrl: string | null
  createdAt: string
  updatedAt: string
}

// ===== 友情链接（Phase 6） =====

export interface PublicFriendItem {
  id: string
  name: string
  url: string
  description: string | null
  logoImageUrl: string | null
  sortOrder: number
}
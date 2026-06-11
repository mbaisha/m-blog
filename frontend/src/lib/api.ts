import type { ApiResponse, CaptchaImageResponse, CommentItem, CreateCommentRequest, CreateCommentResult, PagedData, PublicProfileSection, PublicProjectListItem, PublicProjectDetail, PublicFriendItem, PublicSiteSettingResponse, PublicSeoSettingResponse } from "@/types";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5092/api'

/**
 * 统一 API 请求客户端
 */
class ApiClient {
  private baseUrl: string

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl
  }

  private async request<T>(endpoint: string, options?: RequestInit): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      ...options?.headers,
    }

    const response = await fetch(url, {
      ...options,
      headers,
      cache: 'no-store',
    })

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: '请求失败' }))
      throw new Error(error.message || `HTTP ${response.status}`)
    }

    return response.json()
  }

  /** GET 请求 */
  async get<T>(endpoint: string, params?: Record<string, string | number | undefined>): Promise<T> {
    let url = endpoint
    if (params) {
      const searchParams = new URLSearchParams()
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== '') {
          searchParams.set(key, String(value))
        }
      })
      const query = searchParams.toString()
      if (query) url += `?${query}`
    }
    return this.request<T>(url)
  }

  /** POST 请求 */
  async post<T>(endpoint: string, data?: unknown): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined,
    })
  }
}

export const apiClient = new ApiClient(API_BASE_URL)

// ===== 评论 API =====

/** 获取验证码图片 */
export function fetchCaptcha() {
  return apiClient.get<ApiResponse<CaptchaImageResponse>>('/captcha/image')
}

/** 校验验证码 */
export function verifyCaptcha(sessionId: string, answer: string) {
  return apiClient.post<ApiResponse<{ valid: boolean }>>('/captcha/verify', { sessionId, answer })
}

/** 获取文章评论列表 */
export function fetchComments(articleId: string, page = 1, pageSize = 20) {
  return apiClient.get<ApiResponse<PagedData<CommentItem>>>(`/articles/${articleId}/comments`, { page, pageSize })
}

/** 提交评论 */
export function submitComment(data: CreateCommentRequest) {
  return apiClient.post<ApiResponse<CreateCommentResult>>('/comments', data)
}

// ===== Phase 6: 个人页面 =====

/** 获取公开个人页面模块 */
export function fetchProfileSections() {
  return apiClient.get<ApiResponse<PublicProfileSection[]>>('/profile')
}

/** 获取公开项目列表 */
export function fetchProjects() {
  return apiClient.get<ApiResponse<PublicProjectListItem[]>>('/projects')
}

/** 获取公开项目详情 */
export function fetchProjectBySlug(slug: string) {
  return apiClient.get<ApiResponse<PublicProjectDetail>>(`/projects/${slug}`)
}

/** 获取公开友情链接 */
export function fetchFriends() {
  return apiClient.get<ApiResponse<PublicFriendItem[]>>('/friends')
}

// ===== Phase 7: 自定义页面、导航、页脚、主题、布局 =====

import type { PublicPageItem, PublicNavigationItem, PublicFooterConfig, PublicThemeSetting, PublicModuleLayout } from "@/types";

/** 获取公开自定义页面列表 */
export function fetchPublishedPages() {
  return apiClient.get<ApiResponse<PublicPageItem[]>>('/pages')
}

/** 获取公开自定义页面详情 */
export function fetchPageBySlug(slug: string) {
  return apiClient.get<ApiResponse<PublicPageItem>>(`/pages/${slug}`)
}

/** 获取导航菜单 */
export function fetchNavigation(location: string) {
  return apiClient.get<ApiResponse<PublicNavigationItem[]>>(`/navigation/${location}`)
}

/** 获取页脚配置 */
export function fetchFooterConfig() {
  return apiClient.get<ApiResponse<PublicFooterConfig>>('/footer')
}

/** 获取主题配置 */
export function fetchActiveTheme() {
  return apiClient.get<ApiResponse<PublicThemeSetting>>('/theme')
}

/** 获取模块布局 */
export function fetchPageLayout(pageKey: string) {
  return apiClient.get<ApiResponse<PublicModuleLayout[]>>(`/layout/${pageKey}`)
}

// ===== 留言板 API =====
export interface MessageItem {
  id: string
  parentId?: string
  nickname: string
  email?: string
  content: string
  ipCity?: string
  status: string
  adminReply?: string
  adminRepliedAt?: string
  createdAt: string
  children: MessageItem[]
}

export interface CreateMessageRequest {
  nickname: string
  email?: string
  content: string
  parentId?: string
  captchaSessionId: string
  captchaAnswer: string
}

export function fetchMessages(page = 1, pageSize = 20) {
  return apiClient.get<ApiResponse<PagedData<MessageItem>>>(`/messages?page=${page}&pageSize=${pageSize}`)
}

export function submitMessage(data: CreateMessageRequest) {
  return apiClient.post<ApiResponse<MessageItem>>('/messages', data)
}

/** 获取站点配置 */
export function getSiteConfig(): { siteUrl: string; siteName: string; apiBaseUrl: string } {
  return {
    siteUrl: process.env.NEXT_PUBLIC_SITE_URL || 'http://localhost:3000',
    siteName: process.env.NEXT_PUBLIC_SITE_NAME || '个人博客',
    apiBaseUrl: API_BASE_URL,
  }
}

// ===== 站点设置 & SEO（后台可配置） =====

/** 获取公开站点设置（站点名、描述、Logo、Favicon） */
export async function fetchSiteSettings(): Promise<PublicSiteSettingResponse | null> {
  try {
    const res = await apiClient.get<ApiResponse<PublicSiteSettingResponse>>('/settings')
    if (res.success && res.data?.siteName) return res.data
    return null
  } catch {
    return null
  }
}

/** 获取指定页面的 SEO 设置 */
export async function fetchSeoSettings(pageKey: string): Promise<PublicSeoSettingResponse | null> {
  try {
    const res = await apiClient.get<ApiResponse<PublicSeoSettingResponse>>('/seo', { key: pageKey })
    if (res.success && res.data?.pageKey) return res.data
    return null
  } catch {
    return null
  }
}
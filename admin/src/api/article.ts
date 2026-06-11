import request from '@/utils/request'
import type { ApiResponse, PagedResponse } from '@/types/api'
import type {
  ArticleListItem,
  ArticleDetail,
  ArticleQueryParams,
  CreateArticleRequest,
  UpdateArticleRequest
} from '@/types/article'

/** 分页查询文章列表 */
export function getArticleListApi(params: ArticleQueryParams) {
  return request.get<ApiResponse<PagedResponse<ArticleListItem>>>('/admin/articles', { params })
}

/** 获取文章详情 */
export function getArticleApi(id: string) {
  return request.get<ApiResponse<ArticleDetail>>(`/admin/articles/${id}`)
}

/** 创建文章 */
export function createArticleApi(data: CreateArticleRequest) {
  return request.post<ApiResponse<ArticleDetail>>('/admin/articles', data)
}

/** 更新文章 */
export function updateArticleApi(id: string, data: UpdateArticleRequest) {
  return request.put<ApiResponse<ArticleDetail>>(`/admin/articles/${id}`, data)
}

/** 删除文章 */
export function deleteArticleApi(id: string) {
  return request.delete<ApiResponse>(`/admin/articles/${id}`)
}

/** 更新文章状态 */
export function updateArticleStatusApi(id: string, status: string) {
  return request.patch<ApiResponse>(`/admin/articles/${id}/status`, { status })
}

/** 彻底删除文章 */
export function hardDeleteArticleApi(id: string) {
  return request.delete<ApiResponse>(`/admin/articles/${id}/force`)
}

/** 恢复文章（从回收站还原） */
export function restoreArticleApi(id: string) {
  return request.put<ApiResponse>(`/admin/articles/${id}/restore`)
}

/** AI 提取文章信息 */
export function aiExtractArticleApi(data: { content: string; currentTitle?: string }) {
  return request.post<ApiResponse<{
    title: string
    summary: string
    seoTitle: string
    seoDescription: string
    seoKeywords: string
  }>>('/admin/articles/ai/extract', data, {
    timeout: 300000 // 5 分钟，大模型请求需要较长时间
  })
}

/** AI 生成文章封面图 */
export function aiGenerateCoverApi(data: { title: string; content: string }) {
  return request.post<ApiResponse<{
    mediaId: string
    url: string
  }>>('/admin/articles/ai/generate-cover', data, {
    timeout: 300000 // 5 分钟，大模型请求需要较长时间
  })
}

/** AI 生成文章内配图（插入编辑器光标位置） */
export function aiGenerateArticleImageApi(data: { prompt: string; aspectRatio: string }) {
  return request.post<ApiResponse<{
    mediaId: string
    url: string
  }>>('/admin/articles/ai/generate-article-image', data, {
    timeout: 300000 // 5 分钟，图片生成需要较长时间
  })
}

/** AI 润色文章 */
export function aiPolishArticleApi(data: { content: string; generateImages: boolean; aspectRatio?: string }) {
  return request.post<ApiResponse<{ content: string }>>('/admin/articles/ai/polish', data, {
    timeout: 300000 // 5 分钟，润色+配图需要较长时间
  })
}

/** AI 一键写文 */
export function aiWriteArticleApi(data: { inspiration: string; generateCover: boolean; generateArticleImages: boolean; articleImageAspectRatio?: string }) {
  return request.post<ApiResponse<{
    title: string
    summary: string
    content: string
    seoTitle: string
    seoDescription: string
    seoKeywords: string
    coverUrl?: string
    coverMediaId?: string
  }>>('/admin/articles/ai/write', data, {
    timeout: 300000 // 5 分钟，写文+图片生成需要较长时间
  })
}
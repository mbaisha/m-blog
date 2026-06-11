import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'
import type { SeoSetting, UpdateSeoSettingRequest } from '@/types/seo'

/** 获取所有 SEO 配置 */
export function getSeoSettingsApi() {
  return request.get<ApiResponse<SeoSetting[]>>('/admin/seo')
}

/** 创建或更新 SEO 配置 */
export function saveSeoSettingApi(data: UpdateSeoSettingRequest) {
  return request.put<ApiResponse<SeoSetting>>('/admin/seo', data)
}

/** 生成 Sitemap */
export function generateSitemapApi() {
  return request.post<ApiResponse<{ content: string }>>('/admin/seo/sitemap/generate')
}
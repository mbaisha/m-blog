import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'
import type { ThemeSetting, SaveThemeSettingRequest } from '@/types/pages'

/** 获取所有主题 */
export function getThemeListApi() {
  return request.get<ApiResponse<ThemeSetting[]>>('/admin/theme')
}

/** 获取当前激活主题 */
export function getActiveThemeApi() {
  return request.get<ApiResponse<ThemeSetting>>('/admin/theme/active')
}

/** 保存指定主题 */
export function saveThemeApi(id: string, data: SaveThemeSettingRequest) {
  return request.put<ApiResponse<ThemeSetting>>(`/admin/theme/${id}`, data)
}

/** 创建新主题 */
export function createThemeApi(name: string) {
  return request.post<ApiResponse<ThemeSetting>>(`/admin/theme/create?name=${encodeURIComponent(name)}`)
}

/** 删除主题 */
export function deleteThemeApi(id: string) {
  return request.delete<ApiResponse>(`/admin/theme/${id}`)
}

/** 复制主题 */
export function duplicateThemeApi(id: string, name?: string) {
  const qs = name ? `?name=${encodeURIComponent(name)}` : ''
  return request.post<ApiResponse<ThemeSetting>>(`/admin/theme/${id}/duplicate${qs}`)
}

/** 恢复默认（当前激活主题） */
export function resetThemeApi() {
  return request.post<ApiResponse<ThemeSetting>>('/admin/theme/reset')
}

/** 激活主题 */
export function activateThemeApi(id: string) {
  return request.post<ApiResponse>(`/admin/theme/${id}/activate`)
}
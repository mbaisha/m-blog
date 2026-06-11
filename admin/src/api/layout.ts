import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'
import type { ModuleLayout, BatchSaveLayoutRequest } from '@/types/pages'

/** 获取页面布局模块 */
export function getLayoutModulesApi(pageKey: string) {
  return request.get<ApiResponse<ModuleLayout[]>>(`/admin/layout/${pageKey}`)
}

/** 批量保存布局模块 */
export function saveLayoutModulesApi(data: BatchSaveLayoutRequest) {
  return request.put<ApiResponse<ModuleLayout[]>>('/admin/layout', data)
}

/** 重置页面布局为默认（清空所有模块） */
export function resetLayoutModulesApi(pageKey: string) {
  return request.delete<ApiResponse>('/admin/layout/' + pageKey)
}
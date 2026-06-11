import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'
import type { PageListItem, PageDetail, CreatePageRequest } from '@/types/pages'

/** 获取所有页面 */
export function getPageListApi() {
  return request.get<ApiResponse<PageListItem[]>>('/admin/pages')
}

/** 获取页面详情 */
export function getPageApi(id: string) {
  return request.get<ApiResponse<PageDetail>>(`/admin/pages/${id}`)
}

/** 创建页面 */
export function createPageApi(data: CreatePageRequest) {
  return request.post<ApiResponse<PageDetail>>('/admin/pages', data)
}

/** 更新页面 */
export function updatePageApi(id: string, data: CreatePageRequest) {
  return request.put<ApiResponse<PageDetail>>(`/admin/pages/${id}`, data)
}

/** 删除页面 */
export function deletePageApi(id: string) {
  return request.delete<ApiResponse>(`/admin/pages/${id}`)
}
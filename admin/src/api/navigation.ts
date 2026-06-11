import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'
import type { NavigationItemNode, CreateNavigationItemRequest } from '@/types/pages'

/** 获取所有导航菜单 */
export function getNavigationListApi() {
  return request.get<ApiResponse<NavigationItemNode[]>>('/admin/navigation')
}

/** 获取菜单项详情 */
export function getNavigationItemApi(id: string) {
  return request.get<ApiResponse<NavigationItemNode>>(`/admin/navigation/${id}`)
}

/** 创建菜单项 */
export function createNavigationItemApi(data: CreateNavigationItemRequest) {
  return request.post<ApiResponse<NavigationItemNode>>('/admin/navigation', data)
}

/** 更新菜单项 */
export function updateNavigationItemApi(id: string, data: CreateNavigationItemRequest) {
  return request.put<ApiResponse<NavigationItemNode>>(`/admin/navigation/${id}`, data)
}

/** 删除菜单项 */
export function deleteNavigationItemApi(id: string) {
  return request.delete<ApiResponse>(`/admin/navigation/${id}`)
}

export interface NavigationSortItem {
  id: string
  sortOrder: number
  parentId: string | null
}

/** 批量更新排序 */
export function batchUpdateSortApi(items: NavigationSortItem[]) {
  return request.put<ApiResponse>('/admin/navigation/sort', items)
}
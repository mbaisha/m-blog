import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'
import type { TagListItem, TagResponse, CreateTagRequest, UpdateTagRequest } from '@/types/tag'

/** 获取全部标签列表 */
export function getTagListApi() {
  return request.get<ApiResponse<TagListItem[]>>('/admin/tags')
}

/** 获取标签详情 */
export function getTagApi(id: string) {
  return request.get<ApiResponse<TagResponse>>(`/admin/tags/${id}`)
}

/** 创建标签 */
export function createTagApi(data: CreateTagRequest) {
  return request.post<ApiResponse<TagResponse>>('/admin/tags', data)
}

/** 更新标签 */
export function updateTagApi(id: string, data: UpdateTagRequest) {
  return request.put<ApiResponse<TagResponse>>(`/admin/tags/${id}`, data)
}

/** 删除标签 */
export function deleteTagApi(id: string) {
  return request.delete<ApiResponse>(`/admin/tags/${id}`)
}
import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'
import type { FriendListItem, CreateFriendRequest, UpdateFriendRequest } from '@/types/friend'

/** 获取所有友情链接 */
export function getFriendListApi() {
  return request.get<ApiResponse<FriendListItem[]>>('/admin/friends')
}

/** 获取友情链接详情 */
export function getFriendApi(id: string) {
  return request.get<ApiResponse<FriendListItem>>(`/admin/friends/${id}`)
}

/** 创建友情链接 */
export function createFriendApi(data: CreateFriendRequest) {
  return request.post<ApiResponse<FriendListItem>>('/admin/friends', data)
}

/** 更新友情链接 */
export function updateFriendApi(id: string, data: UpdateFriendRequest) {
  return request.put<ApiResponse<FriendListItem>>(`/admin/friends/${id}`, data)
}

/** 删除友情链接 */
export function deleteFriendApi(id: string) {
  return request.delete<ApiResponse>(`/admin/friends/${id}`)
}
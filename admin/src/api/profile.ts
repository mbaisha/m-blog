import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'
import type { ProfileSection, SaveProfileSectionRequest } from '@/types/profile'

/** 获取所有个人页面模块 */
export function getProfileSectionsApi() {
  return request.get<ApiResponse<ProfileSection[]>>('/admin/profile')
}

/** 批量保存个人页面模块 */
export function saveProfileSectionsApi(data: SaveProfileSectionRequest[]) {
  return request.put<ApiResponse<ProfileSection[]>>('/admin/profile', data)
}
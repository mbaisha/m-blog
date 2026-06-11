import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'
import type { SiteSetting, SaveSiteSettingRequest } from '@/types/pages'

/** 获取站点设置 */
export function getSiteSettingApi() {
  return request.get<ApiResponse<SiteSetting>>('/admin/settings')
}

/** 保存站点设置 */
export function saveSiteSettingApi(data: SaveSiteSettingRequest) {
  return request.put<ApiResponse<SiteSetting>>('/admin/settings', data)
}
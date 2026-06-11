import request from '@/utils/request'
import type { ApiResponse, PagedResponse } from '@/types/api'
import type { VisitListItem, VisitQueryParams, VisitStats, CleanupRequest, CleanupResult } from '@/types/visit'

/** 分页查询访客记录 */
export function getVisitListApi(params?: VisitQueryParams) {
  return request.get<ApiResponse<PagedResponse<VisitListItem>>>('/admin/visits', { params })
}

/** 获取访问统计 */
export function getVisitStatsApi() {
  return request.get<ApiResponse<VisitStats>>('/admin/visits/stats')
}

/** 清理访问记录 */
export function cleanupVisitsApi(data: CleanupRequest) {
  return request.delete<ApiResponse<CleanupResult>>('/admin/visits/cleanup', { data })
}
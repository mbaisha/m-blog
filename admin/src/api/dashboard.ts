import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'

export interface DashboardStats {
  articleCount: number
  draftCount: number
  pendingCommentCount: number
  todayViews: number
  totalViews: number
  mediaCount: number
  categoryCount: number
  tagCount: number
}

/** 获取仪表盘统计 */
export function getDashboardStatsApi() {
  return request.get<ApiResponse<DashboardStats>>('/admin/dashboard/stats')
}
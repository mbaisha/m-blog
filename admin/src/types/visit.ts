/** 访问记录 */
export interface VisitListItem {
  id: string
  pagePath: string | null
  articleTitle: string | null
  ipAddress: string | null
  ipHash: string | null
  location: string | null
  userAgent: string | null
  referer: string | null
  visitedAt: string
}

export interface VisitQueryParams {
  page?: number
  pageSize?: number
  startDate?: string
  endDate?: string
  pagePath?: string
  keyword?: string
}

/** 访问统计 */
export interface VisitStats {
  todayViews: number
  todayVisitors: number
  yesterdayViews: number
  totalViews: number
  totalVisitors: number
  popularPages: PopularPageItem[]
  dailyTrend: DailyTrendItem[]
  deviceDistribution: DeviceDistributionItem[]
  commentTrend: CommentTrendItem[]
}

export interface PopularPageItem {
  pagePath: string
  count: number
}

export interface DailyTrendItem {
  date: string
  views: number
  visitors: number
}

export interface DeviceDistributionItem {
  deviceType: string
  count: number
}

export interface CommentTrendItem {
  date: string
  count: number
}

/** 清理结果 */
export interface CleanupResult {
  deletedCount: number
  beforeDate: string
}

export interface CleanupRequest {
  retentionDays: number
}
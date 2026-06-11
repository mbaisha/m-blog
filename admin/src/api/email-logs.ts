import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'
import type { PagedResponse } from '@/types/api'
import type { EmailLogItem } from '@/types/pages'

/** 获取邮件发送日志（分页） */
export function getEmailLogsApi(params: { page?: number; pageSize?: number; email?: string; isSuccess?: boolean; startDate?: string; endDate?: string }) {
  return request.get<ApiResponse<PagedResponse<EmailLogItem>>>('/admin/email-logs', { params })
}

/** 删除单条日志 */
export function deleteEmailLogApi(id: string) {
  return request.delete<ApiResponse<null>>(`/admin/email-logs/${id}`)
}

/** 清空所有日志 */
export function clearEmailLogsApi() {
  return request.delete<ApiResponse<null>>('/admin/email-logs/clear')
}
import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'
import type { SubscriberItem } from '@/types/pages'

/** 获取订阅者列表 */
export function getSubscribersApi() {
  return request.get<ApiResponse<SubscriberItem[]>>('/admin/subscribers')
}

/** 删除订阅者 */
export function deleteSubscriberApi(id: string) {
  return request.delete<ApiResponse<null>>(`/admin/subscribers/${id}`)
}

/** 获取订阅总数 */
export function getSubscriberCountApi() {
  return request.get<ApiResponse<number>>('/admin/subscribers/count')
}

/** 补发本周周报 */
export function resendWeeklyDigestApi() {
  return request.post<ApiResponse<{ message: string }>>('/admin/subscribers/resend-weekly')
}

/** 向指定订阅者补发周报 */
export function resendWeeklyDigestForSubscriberApi(id: string) {
  return request.post<ApiResponse<{ message: string }>>(`/admin/subscribers/${id}/resend-weekly`)
}
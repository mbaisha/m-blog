import request from '@/utils/request'
import type { ApiResponse, PagedResponse } from '@/types/api'

/** 扁平留言项（后台表格用，不含 children 树） */
export interface MessageFlatItem {
  id: string
  parentId?: string
  parentNickname?: string
  nickname: string
  email?: string
  content: string
  ipCity?: string
  status: string
  adminReply?: string
  adminRepliedAt?: string
  createdAt: string
}

/** 树形留言项（含 children，用于详情弹窗） */
export interface MessageTreeItem extends MessageFlatItem {
  children: MessageTreeItem[]
}

export interface AdminMessageQuery {
  page: number
  pageSize: number
  status?: string
  keyword?: string
}

export interface AdminUpdateMessageRequest {
  status?: string
  adminReply?: string
}

/** 获取留言列表（后台扁平列表，含子回复） */
export function getMessageListApi(params: AdminMessageQuery) {
  return request.get<ApiResponse<PagedResponse<MessageFlatItem>>>('/admin/messages', { params })
}

/** 获取留言详情（树形，含所有子回复，用于详情弹窗） */
export function getMessageDetailApi(id: string) {
  return request.get<ApiResponse<MessageTreeItem>>(`/admin/messages/${id}/detail`)
}

/** 审核/回复留言 */
export function updateMessageApi(id: string, data: AdminUpdateMessageRequest) {
  return request.put<ApiResponse<MessageTreeItem>>(`/admin/messages/${id}`, data)
}

/** 删除留言 */
export function deleteMessageApi(id: string) {
  return request.delete<ApiResponse>(`/admin/messages/${id}`)
}

/** 批量审核通过 */
export function batchApproveMessagesApi(ids: string[]) {
  return Promise.all(ids.map(id => updateMessageApi(id, { status: 'approved' })))
}

/** 批量删除 */
export function batchDeleteMessagesApi(ids: string[]) {
  return Promise.all(ids.map(id => deleteMessageApi(id)))
}

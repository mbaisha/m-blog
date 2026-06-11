import request from '@/utils/request'
import type { ApiResponse, PagedResponse } from '@/types/api'
import type { MediaListItem, MediaQueryParams } from '@/types/media'

/** 分页查询媒体列表 */
export function getMediaListApi(params: MediaQueryParams) {
  return request.get<ApiResponse<PagedResponse<MediaListItem>>>('/admin/media', { params })
}

/** 删除媒体文件 */
export function deleteMediaApi(id: string) {
  return request.delete<ApiResponse>(`/admin/media/${id}`)
}

/** 上传图片 */
export function uploadImageApi(file: File) {
  const formData = new FormData()
  formData.append('file', file)
  return request.post<ApiResponse<{ id: string; url: string }>>('/admin/upload/image', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
}

/** 上传视频 */
export function uploadVideoApi(file: File) {
  const formData = new FormData()
  formData.append('file', file)
  return request.post<ApiResponse<{ id: string; url: string }>>('/admin/upload/video', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
}

/** 上传附件 */
export function uploadFileApi(file: File) {
  const formData = new FormData()
  formData.append('file', file)
  return request.post<ApiResponse<{ id: string; url: string }>>('/admin/upload/file', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
}
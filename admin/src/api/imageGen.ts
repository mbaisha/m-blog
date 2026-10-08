import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'

export interface ImageGenConfig {
  baseUrl: string
  apiKey: string
  model: string
  availableModels: string
  defaultSize: string
  availableSizes: string
  defaultQuality: string
  defaultN: number
  timeoutSeconds: number
}

export interface SaveImageGenConfigRequest {
  baseUrl: string
  apiKey: string
  model: string
  availableModels: string
  defaultSize: string
  availableSizes: string
  defaultQuality: string
  defaultN: number
  timeoutSeconds: number
}

export interface ImageGenRequest {
  prompt: string
  model?: string
  size?: string
  quality?: string
  n: number
}

/** 获取文生图配置 */
export function getImageGenConfigApi() {
  return request.get<ApiResponse<ImageGenConfig>>('/admin/image-gen/config')
}

/** 保存文生图配置 */
export function saveImageGenConfigApi(data: SaveImageGenConfigRequest) {
  return request.put<ApiResponse>('/admin/image-gen/config', data)
}

/** 执行文生图 */
export function generateImageApi(data: ImageGenRequest) {
  return request.post<ApiResponse<{ images: string[] }>>('/admin/image-gen/generate', data, {
    timeout: 300000 // 5 分钟，图片生成需要较长时间
  })
}
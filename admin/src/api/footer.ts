import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'
import type { FooterConfig, SaveFooterConfigRequest } from '@/types/pages'

/** 获取页脚配置 */
export function getFooterConfigApi() {
  return request.get<ApiResponse<FooterConfig>>('/admin/footer')
}

/** 保存页脚配置 */
export function saveFooterConfigApi(data: SaveFooterConfigRequest) {
  return request.put<ApiResponse<FooterConfig>>('/admin/footer', data)
}
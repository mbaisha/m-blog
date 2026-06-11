/** 媒体列表项 */
export interface MediaListItem {
  id: string
  type: string
  url: string
  thumbnailUrl: string | null
  filename: string
  originalFilename: string | null
  sizeBytes: number
  mimeType: string
  width: number | null
  height: number | null
  createdAt: string
}

/** 媒体查询参数 */
export interface MediaQueryParams {
  page: number
  pageSize: number
  type?: string
  keyword?: string
}
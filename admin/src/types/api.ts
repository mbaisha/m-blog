/** API 统一响应类型 */
export interface ApiResponse<T = unknown> {
  code: number
  message: string
  data?: T
  errors?: string[]
  success: boolean
}

/** 登录请求 */
export interface LoginRequest {
  username: string
  password: string
}

/** 登录响应 */
export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: UserInfo
}

/** 用户信息 */
export interface UserInfo {
  id: string
  username: string
  email: string | null
  role: string
  avatarUrl: string | null
}

/** 分页响应 */
export interface PagedResponse<T> {
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  items: T[]
  hasPrevious: boolean
  hasNext: boolean
}
import request from '@/utils/request'
import type { ApiResponse, LoginRequest, LoginResponse } from '@/types/api'

/** 登录 */
export function loginApi(data: LoginRequest) {
  return request.post<ApiResponse<LoginResponse>>('/auth/login', data)
}

/** 刷新令牌 */
export function refreshTokenApi(refreshToken: string) {
  return request.post<ApiResponse<{ accessToken: string; refreshToken: string }>>('/auth/refresh', { refreshToken })
}

/** 退出登录 */
export function logoutApi(refreshToken: string) {
  return request.post<ApiResponse>('/auth/logout', { refreshToken })
}

/** 修改密码 */
export function changePasswordApi(data: { currentPassword: string; newPassword: string }) {
  return request.post<ApiResponse>('/auth/change-password', data)
}
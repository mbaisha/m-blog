import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { ElMessage } from 'element-plus'
import type { ApiResponse } from '@/types/api'
import { useAuthStore } from '@/stores/auth'

/** 创建 Axios 实例 */
const request = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 30000,
  headers: { 'Content-Type': 'application/json' }
})

/** 请求拦截器：自动附带 Token */
request.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('accessToken')
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

/** 是否正在刷新 Token */
let isRefreshing = false
/** 等待刷新 Token 的请求队列 */
let pendingRequests: Array<{
  resolve: (token: string) => void
  reject: (err: unknown) => void
}> = []

/** 刷新 Token */
async function refreshAccessToken(): Promise<string> {
  const refreshToken = localStorage.getItem('refreshToken')
  if (!refreshToken) throw new Error('No refresh token')

  const res = await axios.post<ApiResponse<{ accessToken: string; refreshToken: string }>>(
    `${import.meta.env.VITE_API_BASE_URL}/auth/refresh`,
    { refreshToken }
  )

  if (res.data.success && res.data.data) {
    localStorage.setItem('accessToken', res.data.data.accessToken)
    localStorage.setItem('refreshToken', res.data.data.refreshToken)
    return res.data.data.accessToken
  }
  throw new Error('Refresh failed')
}

/** 响应拦截器：统一错误处理 + Token 自动刷新 */
request.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<ApiResponse>) => {
    const { response, config } = error
    if (!response || !config) return Promise.reject(error)

    const status = response.status

    // 401 未授权：尝试刷新 Token
    if (status === 401) {
      if (!isRefreshing) {
        isRefreshing = true
        try {
          const newToken = await refreshAccessToken()
          isRefreshing = false
          // 重放等待队列中的请求
          pendingRequests.forEach((p) => p.resolve(newToken))
          pendingRequests = []
          // 重放当前请求
          config.headers.Authorization = `Bearer ${newToken}`
          return request(config)
        } catch {
          isRefreshing = false
          pendingRequests.forEach((p) => p.reject(error))
          pendingRequests = []
          // 刷新失败，清除登录态并跳转
          localStorage.removeItem('accessToken')
          localStorage.removeItem('refreshToken')
          const authStore = useAuthStore()
          authStore.clearUser()
          window.location.href = import.meta.env.BASE_URL + 'login'
          return Promise.reject(error)
        }
      } else {
        // 正在刷新，将请求加入队列等待
        return new Promise((resolve, reject) => {
          pendingRequests.push({
            resolve: (token: string) => {
              config.headers.Authorization = `Bearer ${token}`
              resolve(request(config))
            },
            reject
          })
        })
      }
    }

    // 403 无权限
    if (status === 403) {
      ElMessage.error('无权限访问')
      return Promise.reject(error)
    }

    // 400/422 参数错误
    if (status === 400 || status === 422) {
      const msg = response.data?.message || '请求参数错误'
      ElMessage.warning(msg)
      return Promise.reject(error)
    }

    // 500 服务器错误
    if (status >= 500) {
      ElMessage.error('服务器错误，请稍后重试')
      return Promise.reject(error)
    }

    return Promise.reject(error)
  }
)

export default request
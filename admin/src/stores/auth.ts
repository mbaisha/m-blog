import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { UserInfo } from '@/types/api'
import { loginApi, logoutApi } from '@/api/auth'
import type { LoginRequest } from '@/types/api'

/** 认证状态管理 */
export const useAuthStore = defineStore('auth', () => {
  // ========== 状态 ==========
  const user = ref<UserInfo | null>(loadUserFromStorage())
  const accessToken = ref<string>(localStorage.getItem('accessToken') || '')
  const refreshToken = ref<string>(localStorage.getItem('refreshToken') || '')

  // ========== 计算属性 ==========
  const isLoggedIn = computed(() => !!accessToken.value && !!user.value)
  const role = computed(() => user.value?.role || '')

  // ========== 内部方法 ==========
  /** 从 localStorage 恢复用户信息 */
  function loadUserFromStorage(): UserInfo | null {
    try {
      const saved = localStorage.getItem('user')
      if (saved) return JSON.parse(saved)
    } catch { /* ignore */ }
    return null
  }

  /** 保存用户信息到 localStorage */
  function saveUserToStorage(u: UserInfo) {
    localStorage.setItem('user', JSON.stringify(u))
  }

  // ========== 方法 ==========
  /** 登录 */
  async function login(data: LoginRequest) {
    const res = await loginApi(data)
    if (res.data.success && res.data.data) {
      user.value = res.data.data.user
      accessToken.value = res.data.data.accessToken
      refreshToken.value = res.data.data.refreshToken
      localStorage.setItem('accessToken', res.data.data.accessToken)
      localStorage.setItem('refreshToken', res.data.data.refreshToken)
      saveUserToStorage(res.data.data.user)
      return true
    }
    return false
  }

  /** 退出 */
  async function logout() {
    try {
      if (refreshToken.value) {
        await logoutApi(refreshToken.value)
      }
    } catch {
      // 即使接口失败也要清理本地状态
    } finally {
      clearUser()
    }
  }

  /** 清除用户信息 */
  function clearUser() {
    user.value = null
    accessToken.value = ''
    refreshToken.value = ''
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')
  }

  return { user, accessToken, refreshToken, isLoggedIn, role, login, logout, clearUser }
})
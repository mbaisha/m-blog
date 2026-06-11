<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import type { LoginRequest } from '@/types/api'
import { ElMessage } from 'element-plus'

const siteName = import.meta.env.VITE_SITE_NAME || '后台管理'
const router = useRouter()
const authStore = useAuthStore()

/** 登录表单 */
const form = ref<LoginRequest>({
  username: '',
  password: ''
})

/** 是否正在登录 */
const loading = ref(false)

/** 提交登录 */
async function handleLogin() {
  if (!form.value.username || !form.value.password) {
    ElMessage.warning('请输入用户名和密码')
    return
  }

  loading.value = true
  try {
    const success = await authStore.login(form.value)
    if (success) {
      ElMessage.success('登录成功')
      router.push('/dashboard')
    } else {
      ElMessage.error('登录失败')
    }
  } catch (err: any) {
    const msg = err?.response?.data?.message || '登录失败，请重试'
    ElMessage.error(msg)
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <div class="login-card">
      <h2 class="login-title">{{ siteName }}</h2>
      <p class="login-desc">请登录以管理您的博客</p>

      <el-form :model="form" @keyup.enter="handleLogin" label-position="top">
        <el-form-item label="用户名">
          <el-input
            v-model="form.username"
            placeholder="请输入用户名"
            :prefix-icon="'User'"
            size="large"
          />
        </el-form-item>
        <el-form-item label="密码">
          <el-input
            v-model="form.password"
            type="password"
            placeholder="请输入密码"
            :prefix-icon="'Lock'"
            size="large"
            show-password
          />
        </el-form-item>
        <el-form-item>
          <el-button
            type="primary"
            size="large"
            :loading="loading"
            style="width: 100%"
            @click="handleLogin"
          >
            {{ loading ? '登录中...' : '登 录' }}
          </el-button>
        </el-form-item>
      </el-form>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}
.login-card {
  width: 400px;
  padding: 40px;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.15);
}
.login-title {
  text-align: center;
  margin: 0 0 8px;
  font-size: 24px;
  color: #303133;
}
.login-desc {
  text-align: center;
  margin: 0 0 32px;
  font-size: 14px;
  color: #909399;
}
</style>
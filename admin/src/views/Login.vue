<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import type { LoginRequest } from '@/types/api'
import { ElMessage } from 'element-plus'
import request from '@/utils/request'

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
/** 是否需要验证码 */
const needCaptcha = ref(false)
/** 滑块验证码状态 */
const captchaVerified = ref(false)
const captchaLoading = ref(false)
/** 系统是否开启了登录验证码功能 */
const loginCaptchaEnabled = ref(true)

/** 检查站点是否开启了登录验证码 */
async function checkCaptchaEnabled() {
  try {
    const res = await request.get('/settings')
    if (res.data.success && res.data.data) {
      loginCaptchaEnabled.value = res.data.data.loginCaptchaEnabled !== false
    }
  } catch {
    // 默认为开启
  }
}

// 页面加载时检查
checkCaptchaEnabled()

// ===== 滑块验证码 =====
const sliderTrackRef = ref<HTMLDivElement>()
const sliderThumbRef = ref<HTMLDivElement>()
const sliderDragging = ref(false)
const sliderStartX = ref(0)
const sliderLeft = ref(0)
const sliderMaxLeft = ref(0)
const sliderVerified = ref(false)
const sliderSessionId = ref('')
const sliderTargetPercent = ref(0)
/** 目标像素位置（用于标记线） */
const sliderTargetPx = ref(0)

/** 获取滑块验证码 */
async function fetchSliderCaptcha() {
  captchaLoading.value = true
  try {
    const res = await request.get('/captcha/slider')
    if (res.data.success && res.data.data) {
      sliderSessionId.value = res.data.data.sessionId
      sliderTargetPercent.value = res.data.data.targetPercent
      sliderLeft.value = 0
      sliderVerified.value = false
      captchaVerified.value = false
      // 等 DOM 更新后计算目标像素位置
      await nextTick()
      const trackWidth = sliderTrackRef.value?.clientWidth ?? 300
      const thumbWidth = sliderThumbRef.value?.clientWidth ?? 48
      sliderMaxLeft.value = trackWidth - thumbWidth
      sliderTargetPx.value = Math.round(sliderTargetPercent.value * sliderMaxLeft.value)
    }
  } catch {
    ElMessage.error('验证码加载失败')
  } finally {
    captchaLoading.value = false
  }
}

/** 滑块开始拖动 */
function onSliderMouseDown(e: MouseEvent) {
  if (sliderVerified.value) return
  e.preventDefault()
  sliderDragging.value = true
  sliderStartX.value = e.clientX
}

function onSliderTouchStart(e: TouchEvent) {
  if (sliderVerified.value) return
  sliderDragging.value = true
  sliderStartX.value = e.touches[0].clientX
}

/** 全局移动 */
function onGlobalMove(e: MouseEvent | TouchEvent) {
  if (!sliderDragging.value) return
  const clientX = 'touches' in e ? e.touches[0].clientX : e.clientX
  const delta = clientX - sliderStartX.value
  const trackWidth = sliderTrackRef.value?.clientWidth ?? 300
  const thumbWidth = sliderThumbRef.value?.clientWidth ?? 48
  const max = trackWidth - thumbWidth
  sliderMaxLeft.value = max
  sliderLeft.value = Math.max(0, Math.min(delta, max))
}

/** 全局释放 */
async function onGlobalUp() {
  if (!sliderDragging.value) return
  sliderDragging.value = false

  const max = sliderMaxLeft.value
  if (max <= 0) return

  const actualPercent = sliderLeft.value / max
  const target = sliderTargetPercent.value

  // 允许 10% 误差
  if (Math.abs(actualPercent - target) <= 0.10) {
    sliderVerified.value = true
    captchaVerified.value = true
    ElMessage.success('验证通过')
  } else {
    sliderLeft.value = 0
    ElMessage.warning('验证未通过，请将滑块拖到标记线位置')
    await fetchSliderCaptcha()
  }
}

onMounted(() => {
  document.addEventListener('mousemove', onGlobalMove)
  document.addEventListener('mouseup', onGlobalUp)
  document.addEventListener('touchmove', onGlobalMove, { passive: false })
  document.addEventListener('touchend', onGlobalUp)
})

onUnmounted(() => {
  document.removeEventListener('mousemove', onGlobalMove)
  document.removeEventListener('mouseup', onGlobalUp)
  document.removeEventListener('touchmove', onGlobalMove)
  document.removeEventListener('touchend', onGlobalUp)
})

/** 提交登录 */
async function handleLogin() {
  if (!form.value.username || !form.value.password) {
    ElMessage.warning('请输入用户名和密码')
    return
  }

  if (needCaptcha.value && loginCaptchaEnabled.value && !captchaVerified.value) {
    ElMessage.warning('请完成滑块验证')
    return
  }

  loading.value = true
  try {
    const loginData: LoginRequest = {
      username: form.value.username,
      password: form.value.password,
    }

    if (needCaptcha.value && sliderSessionId.value) {
      loginData.captchaSessionId = sliderSessionId.value
      loginData.captchaAnswer = String(Math.round(sliderLeft.value / sliderMaxLeft.value * 100))
    }

    const success = await authStore.login(loginData)
    if (success) {
      ElMessage.success('登录成功')
      router.push('/dashboard')
    } else {
      ElMessage.error('用户名或密码错误')
      if (loginCaptchaEnabled.value) {
        needCaptcha.value = true
        captchaVerified.value = false
        sliderVerified.value = false
        await fetchSliderCaptcha()
      }
    }
  } catch (err: any) {
    // 401 表示凭证错误或需要验证码
    if (err?.response?.status === 401) {
      const msg = err?.response?.data?.message || '用户名或密码错误'
      ElMessage.error(msg)
      if (loginCaptchaEnabled.value) {
        needCaptcha.value = true
        captchaVerified.value = false
        sliderVerified.value = false
        await fetchSliderCaptcha()
      }
    } else if (err?.response?.status === 429) {
      ElMessage.error('登录尝试次数过多，请 15 分钟后再试')
    } else {
      const msg = err?.response?.data?.message || '登录失败，请稍后重试'
      ElMessage.error(msg)
    }
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

        <!-- 滑块验证码 -->
        <el-form-item v-if="needCaptcha && loginCaptchaEnabled" label="安全验证">
          <div class="slider-captcha">
            <div class="slider-track" ref="sliderTrackRef">
              <div class="slider-bg">
                <div
                  class="slider-fill"
                  :style="{ width: (sliderLeft / (sliderMaxLeft || 1)) * 100 + '%' }"
                />
                <!-- 目标位置标记线 -->
                <div
                  class="slider-target-line"
                  :style="{ left: sliderTargetPx + 'px' }"
                />
              </div>
              <div
                class="slider-thumb"
                ref="sliderThumbRef"
                :class="{ verified: sliderVerified }"
                :style="{ left: sliderLeft + 'px' }"
                @mousedown="onSliderMouseDown"
                @touchstart.prevent="onSliderTouchStart"
              >
                <span v-if="sliderVerified">&#10003;</span>
                <span v-else>&#8594;</span>
              </div>
              <span v-if="!sliderVerified" class="slider-text">
                请将滑块拖到标记线位置
              </span>
              <span v-else class="slider-text success">验证通过</span>
            </div>
          </div>
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
  width: 420px;
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

/* 滑块验证码 */
.slider-captcha {
  width: 100%;
}
.slider-track {
  position: relative;
  height: 40px;
  background: #e8e8e8;
  border-radius: 4px;
  overflow: hidden;
  user-select: none;
}
.slider-bg {
  position: absolute;
  inset: 0;
}
.slider-fill {
  height: 100%;
  background: linear-gradient(90deg, #a0d911, #52c41a);
  transition: width 0.05s linear;
  border-radius: 4px 0 0 4px;
}
.slider-target-line {
  position: absolute;
  top: 0;
  width: 2px;
  height: 100%;
  background: #ff4d4f;
  z-index: 1;
  box-shadow: 0 0 4px rgba(255, 77, 79, 0.5);
}
.slider-thumb {
  position: absolute;
  top: 0;
  width: 48px;
  height: 40px;
  background: #fff;
  border: 1px solid #d9d9d9;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: grab;
  font-size: 18px;
  color: #999;
  z-index: 2;
  transition: background 0.2s;
}
.slider-thumb:active {
  cursor: grabbing;
}
.slider-thumb.verified {
  background: #52c41a;
  color: #fff;
  border-color: #52c41a;
  cursor: default;
}
.slider-text {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  color: #999;
  pointer-events: none;
  z-index: 1;
}
.slider-text.success {
  color: #52c41a;
}
</style>

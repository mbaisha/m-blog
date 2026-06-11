<script setup lang="ts">
import { ref } from 'vue'
import { ElMessage } from 'element-plus'
import { changePasswordApi } from '@/api/auth'

const currentPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')
const loading = ref(false)

async function handleSubmit() {
  if (!currentPassword.value) {
    ElMessage.warning('请输入当前密码')
    return
  }
  if (!newPassword.value || newPassword.value.length < 6) {
    ElMessage.warning('新密码长度不能少于 6 位')
    return
  }
  if (newPassword.value !== confirmPassword.value) {
    ElMessage.warning('两次输入的新密码不一致')
    return
  }

  loading.value = true
  try {
    await changePasswordApi({
      currentPassword: currentPassword.value,
      newPassword: newPassword.value
    })
    ElMessage.success('密码修改成功')
    currentPassword.value = ''
    newPassword.value = ''
    confirmPassword.value = ''
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '密码修改失败')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="change-password-page">
    <h3>修改密码</h3>
    <el-card shadow="never" style="max-width: 480px">
      <el-form label-width="100px" @submit.prevent="handleSubmit">
        <el-form-item label="当前密码">
          <el-input
            v-model="currentPassword"
            type="password"
            show-password
            placeholder="请输入当前密码"
          />
        </el-form-item>
        <el-form-item label="新密码">
          <el-input
            v-model="newPassword"
            type="password"
            show-password
            placeholder="至少 6 位"
          />
        </el-form-item>
        <el-form-item label="确认新密码">
          <el-input
            v-model="confirmPassword"
            type="password"
            show-password
            placeholder="再次输入新密码"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :loading="loading" @click="handleSubmit">
            修改密码
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<style scoped>
.change-password-page {
  padding: 20px;
}
.change-password-page h3 {
  margin-bottom: 20px;
}
</style>
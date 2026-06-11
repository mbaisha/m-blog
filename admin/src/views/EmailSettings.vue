<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getEmailSettingApi, saveEmailSettingApi, testEmailApi } from '@/api/email'

const loading = ref(false)
const saving = ref(false)
const testing = ref(false)
const showPassword = ref(false)

const form = ref({
  smtpServer: '',
  smtpPort: 587,
  smtpUsername: '',
  smtpPassword: '',
  senderEmail: '',
  senderName: '',
  useSsl: true
})

const testForm = ref({
  testEmail: ''
})

const testDialogVisible = ref(false)

async function loadSetting() {
  loading.value = true
  try {
    const res = await getEmailSettingApi()
    const d = res.data.data
    if (d && d.id) {
      form.value = {
        smtpServer: d.smtpServer,
        smtpPort: d.smtpPort,
        smtpUsername: d.smtpUsername,
        smtpPassword: '',
        senderEmail: d.senderEmail,
        senderName: d.senderName,
        useSsl: d.useSsl
      }
    }
  } finally {
    loading.value = false
  }
}

async function handleSave() {
  saving.value = true
  try {
    await saveEmailSettingApi({
      smtpServer: form.value.smtpServer,
      smtpPort: form.value.smtpPort,
      smtpUsername: form.value.smtpUsername,
      smtpPassword: form.value.smtpPassword || undefined,
      senderEmail: form.value.senderEmail,
      senderName: form.value.senderName,
      useSsl: form.value.useSsl
    } as any)
    ElMessage.success('邮件设置已保存')
    await loadSetting()
  } finally {
    saving.value = false
  }
}

async function handleTest() {
  if (!testForm.value.testEmail) {
    ElMessage.warning('请输入测试收件邮箱')
    return
  }
  testing.value = true
  try {
    await testEmailApi(testForm.value.testEmail)
    ElMessage.success('测试邮件发送成功！请查收')
    testDialogVisible.value = false
  } catch {
    ElMessage.error('测试邮件发送失败，请检查配置')
  } finally {
    testing.value = false
  }
}

onMounted(loadSetting)
</script>

<template>
  <div class="email-settings-page">
    <div class="page-header">
      <div>
        <h3>邮件设置</h3>
        <p class="text-[12px] mt-1" style="color:#909399">配置 SMTP 发件邮箱，用于发送订阅确认、内容推送等邮件</p>
      </div>
      <div class="flex gap-2">
        <el-button @click="testDialogVisible = true">测试发送</el-button>
        <el-button type="primary" :loading="saving" @click="handleSave">保存设置</el-button>
      </div>
    </div>

    <el-card shadow="never" v-loading="loading">
      <el-form label-width="140px">
        <el-divider content-position="left">SMTP 服务器</el-divider>

        <el-form-item label="服务器地址" required>
          <el-input v-model="form.smtpServer" placeholder="smtp.example.com" style="max-width:400px" />
        </el-form-item>

        <el-form-item label="端口" required>
          <el-input-number v-model="form.smtpPort" :min="1" :max="65535" />
          <span class="form-item-hint" style="margin-left:8px;">常用端口：25（非SSL）、465（SSL）、587（TLS）</span>
        </el-form-item>

        <el-form-item label="用户名" required>
          <el-input v-model="form.smtpUsername" placeholder="SMTP 登录用户名" style="max-width:400px" />
        </el-form-item>

        <el-form-item label="密码" required>
          <div class="flex items-center gap-2">
            <el-input
              v-model="form.smtpPassword"
              :type="showPassword ? 'text' : 'password'"
              placeholder="SMTP 登录密码"
              style="max-width:400px"
              show-password
            />
            <el-button link @click="showPassword = !showPassword">
              {{ showPassword ? '隐藏' : '显示' }}
            </el-button>
          </div>
          <span class="form-item-hint">如不修改请留空，系统将保留原密码</span>
        </el-form-item>

        <el-divider content-position="left">发件人信息</el-divider>

        <el-form-item label="发件人邮箱" required>
          <el-input v-model="form.senderEmail" placeholder="noreply@example.com" style="max-width:400px" />
        </el-form-item>

        <el-form-item label="发件人名称" required>
          <el-input v-model="form.senderName" placeholder="个人博客" style="max-width:400px" />
          <span class="form-item-hint">收件人将看到此名称</span>
        </el-form-item>

        <el-divider content-position="left">安全设置</el-divider>

        <el-form-item label="SSL 加密">
          <el-switch v-model="form.useSsl" />
          <span class="form-item-hint" style="margin-left:8px;">
            {{ form.useSsl ? '启用 SSL 加密传输' : '不使用 SSL 加密' }}
          </span>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 测试发送对话框 -->
    <el-dialog v-model="testDialogVisible" title="测试邮件发送" width="400px">
      <el-form>
        <el-form-item label="收件邮箱">
          <el-input v-model="testForm.testEmail" placeholder="输入测试收件邮箱" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="testDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="testing" @click="handleTest">发送测试</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.email-settings-page {
  padding: 0;
}
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}
.page-header h3 {
  margin: 0;
  font-size: 20px;
}
.form-item-hint {
  font-size: 12px;
  color: #909399;
  margin-left: 4px;
}
.flex {
  display: flex;
}
.items-center {
  align-items: center;
}
.gap-2 {
  gap: 8px;
}
</style>
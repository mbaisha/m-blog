<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getEmailTemplatesApi, getEmailTemplateApi, updateEmailTemplateApi, resetEmailTemplateApi } from '@/api/email'
import type { EmailTemplate } from '@/types/pages'

const loading = ref(false)
const saving = ref(false)
const templates = ref<EmailTemplate[]>([])
const currentKey = ref('confirm_subscription')
const currentTemplate = ref<EmailTemplate | null>(null)
const editForm = ref({
  subject: '',
  htmlContent: '',
  description: ''
})

const templateMeta = computed(() => ({
  'confirm_subscription': { title: '确认订阅邮件', desc: '发送给新订阅者，包含确认链接（3天有效）' },
  'subscription_detail': { title: '每周内容精选（周报）', desc: '发送给已确认订阅的用户，包含本周新文章、热门文章、推荐文章和退订链接' },
  'unsubscribed': { title: '退订确认邮件', desc: '用户成功退订后发送的通知邮件' }
}))

const templateTabs = [
  { key: 'confirm_subscription', label: '确认订阅' },
  { key: 'subscription_detail', label: '订阅详情' },
  { key: 'unsubscribed', label: '退订确认' }
]

async function loadTemplates() {
  loading.value = true
  try {
    const res = await getEmailTemplatesApi()
    templates.value = res.data.data || []
    await loadCurrentTemplate()
  } finally {
    loading.value = false
  }
}

async function loadCurrentTemplate() {
  try {
    const res = await getEmailTemplateApi(currentKey.value)
    const t = res.data.data
    if (t) {
      currentTemplate.value = t
      editForm.value = {
        subject: t.subject,
        htmlContent: t.htmlContent,
        description: t.description || ''
      }
    } else {
      currentTemplate.value = null
      editForm.value = {
        subject: '',
        htmlContent: '',
        description: ''
      }
    }
  } catch {
    currentTemplate.value = null
    editForm.value = {
      subject: '',
      htmlContent: '',
      description: ''
    }
  }
}

function switchTab(key: string) {
  currentKey.value = key
  loadCurrentTemplate()
}

async function handleSave() {
  if (!editForm.value.subject.trim()) {
    ElMessage.warning('邮件主题不能为空')
    return
  }
  if (!editForm.value.htmlContent.trim()) {
    ElMessage.warning('邮件内容不能为空')
    return
  }
  saving.value = true
  try {
    await updateEmailTemplateApi(currentKey.value, {
      subject: editForm.value.subject,
      htmlContent: editForm.value.htmlContent,
      description: editForm.value.description
    })
    ElMessage.success('模板已保存')
    await loadTemplates()
  } finally {
    saving.value = false
  }
}

async function handleReset() {
  try {
    await ElMessageBox.confirm(
      '确定重置该模板为默认内容？自定义修改将丢失',
      '确认重置',
      { type: 'warning', confirmButtonText: '重置', cancelButtonText: '取消' }
    )
    await resetEmailTemplateApi(currentKey.value)
    ElMessage.success('模板已重置为默认内容')
    await loadCurrentTemplate()
  } catch {
    // 取消不处理
  }
}

function insertVariable(varName: string) {
  editForm.value.htmlContent += `{${varName}}`
}

const availableVars = computed(() => {
  const map: Record<string, { label: string; vars: string[] }> = {
    'confirm_subscription': {
      label: '确认订阅',
      vars: ['siteName', 'siteUrl', 'confirmationLink']
    },
    'subscription_detail': {
      label: '订阅详情',
      vars: ['siteName', 'siteUrl', 'content', 'unsubscribeLink', 'currentDate']
    },
    'unsubscribed': {
      label: '退订确认',
      vars: ['siteName', 'siteUrl', 'subscribeLink']
    }
  }
  return map[currentKey.value] || { label: '', vars: [] }
})

/** 预览 HTML：替换变量为示例值 */
const previewHtml = computed(() => {
  return editForm.value.htmlContent
    .replace(/{siteName}/g, '个人博客')
    .replace(/{siteUrl}/g, 'https://example.com')
    .replace(/{confirmationLink}/g, 'https://example.com/confirm?token=xxx')
    .replace(/{content}/g, '<p style="font-size:15px;color:#333;">这是预览内容...</p>')
    .replace(/{unsubscribeLink}/g, 'https://example.com/unsubscribe?token=xxx')
    .replace(/{subscribeLink}/g, 'https://example.com')
    .replace(/{currentDate}/g, new Date().toLocaleString("zh-CN", { year: "numeric", month: "2-digit", day: "2-digit" }))
})

onMounted(loadTemplates)
</script>

<template>
  <div class="email-templates-page">
    <div class="page-header">
      <div>
        <h3>邮件模板管理</h3>
        <p class="text-[12px] mt-1" style="color:#909399">自定义各场景下发送的邮件模板内容</p>
      </div>
      <div class="flex gap-2">
        <el-button @click="handleReset" type="danger" plain>重置为默认</el-button>
        <el-button type="primary" :loading="saving" @click="handleSave">保存模板</el-button>
      </div>
    </div>

    <el-tabs :model-value="currentKey" @tab-change="switchTab">
      <el-tab-pane
        v-for="tab in templateTabs"
        :key="tab.key"
        :label="tab.label"
        :name="tab.key"
      >
        <template #label>
          <span class="tab-label">
            {{ tab.label }}
            <el-tag size="small" type="info" style="margin-left:4px;">
              {{ tab.key }}
            </el-tag>
          </span>
        </template>
      </el-tab-pane>
    </el-tabs>

    <el-card shadow="never" v-loading="loading" style="margin-top:16px;">
      <div class="template-info" v-if="currentTemplate">
        <p style="font-size:12px;color:#909399;margin:0 0 16px;">
          最后更新：{{ new Date(currentTemplate.updatedAt).toLocaleString('zh-CN') }}
        </p>
      </div>

      <el-form label-width="100px">
        <el-form-item label="邮件主题">
          <el-input v-model="editForm.subject" placeholder="邮件主题" style="max-width:500px" />
        </el-form-item>

        <el-form-item label="模板描述">
          <el-input v-model="editForm.description" placeholder="模板用途说明" style="max-width:500px" disabled />
        </el-form-item>

        <el-form-item label="可用变量">
          <div class="flex gap-2 flex-wrap">
            <el-tag
              v-for="v in availableVars.vars"
              :key="v"
              style="cursor:pointer"
              @click="insertVariable(v)"
            >
              {{ '{' + v + '}' }}
            </el-tag>
          </div>
          <span class="form-item-hint" style="display:block;margin-top:4px;">点击变量名插入到内容中</span>
        </el-form-item>

        <el-form-item label="邮件内容 (HTML)">
          <div style="width:100%;">
            <el-input
              v-model="editForm.htmlContent"
              type="textarea"
              :rows="20"
              placeholder="邮件 HTML 内容"
              style="width:100%;font-family:monospace;font-size:13px;"
            />
          </div>
        </el-form-item>

        <el-form-item label="预览">
          <div
            class="email-preview"
            v-html="previewHtml"
          />
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<style scoped>
.email-templates-page {
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
}
.flex {
  display: flex;
}
.gap-2 {
  gap: 8px;
}
.flex-wrap {
  flex-wrap: wrap;
}
.email-preview {
  width: 100%;
  max-height: 500px;
  overflow-y: auto;
  border: 1px solid #dcdfe6;
  border-radius: 4px;
  padding: 16px;
  background: #fff;
}
.tab-label {
  display: inline-flex;
  align-items: center;
}
</style>
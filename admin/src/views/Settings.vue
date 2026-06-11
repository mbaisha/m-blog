<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getSiteSettingApi, saveSiteSettingApi } from '@/api/settings'
import { uploadImageApi } from '@/api/media'

const loading = ref(false)
const saving = ref(false)
const uploadingLogo = ref(false)
const uploadingFavicon = ref(false)
const form = ref({
  siteName: 'My Blog',
  siteDescription: '',
  logoImageId: null as string | null,
  logoPreviewUrl: '' as string,
  faviconImageId: null as string | null,
  faviconPreviewUrl: '' as string,
  commentModerationEnabled: true,
  subscriptionEnabled: false,
  visitRetentionDays: 30
})

async function loadSetting() {
  loading.value = true
  try {
    const res = await getSiteSettingApi()
    const d = res.data.data
    if (d && d.id) {
      form.value = {
        siteName: d.siteName,
        siteDescription: d.siteDescription || '',
        logoImageId: d.logoImageId,
        logoPreviewUrl: d.logoImageUrl || '',
        faviconImageId: d.faviconImageId,
        faviconPreviewUrl: d.faviconImageUrl || '',
        commentModerationEnabled: d.commentModerationEnabled,
        subscriptionEnabled: d.subscriptionEnabled,
        visitRetentionDays: d.visitRetentionDays
      }
    }
  } finally {
    loading.value = false
  }
}

async function handleLogoUpload(file: File) {
  uploadingLogo.value = true
  try {
    const res = await uploadImageApi(file)
    const d = res.data.data
    if (!d) { ElMessage.error('上传返回数据为空'); return }
    form.value.logoImageId = d.id
    form.value.logoPreviewUrl = d.url
    ElMessage.success('Logo 上传成功')
  } catch {
    ElMessage.error('Logo 上传失败')
  } finally {
    uploadingLogo.value = false
  }
}

async function handleFaviconUpload(file: File) {
  uploadingFavicon.value = true
  try {
    const res = await uploadImageApi(file)
    const d = res.data.data
    if (!d) { ElMessage.error('上传返回数据为空'); return }
    form.value.faviconImageId = d.id
    form.value.faviconPreviewUrl = d.url
    ElMessage.success('Favicon 上传成功')
  } catch {
    ElMessage.error('Favicon 上传失败')
  } finally {
    uploadingFavicon.value = false
  }
}

function handleLogoRemove() {
  form.value.logoImageId = null
  form.value.logoPreviewUrl = ''
}

function handleFaviconRemove() {
  form.value.faviconImageId = null
  form.value.faviconPreviewUrl = ''
}

async function handleSave() {
  saving.value = true
  try {
    await saveSiteSettingApi({
      siteName: form.value.siteName,
      siteDescription: form.value.siteDescription || null,
      logoImageId: form.value.logoImageId,
      faviconImageId: form.value.faviconImageId,
      commentModerationEnabled: form.value.commentModerationEnabled,
      subscriptionEnabled: form.value.subscriptionEnabled,
      visitRetentionDays: form.value.visitRetentionDays
    })
    ElMessage.success('站点设置已更新')
  } finally {
    saving.value = false
  }
}

onMounted(loadSetting)
</script>

<template>
  <div class="settings-page">
    <div class="page-header">
      <h3>系统设置</h3>
      <el-button type="primary" :loading="saving" @click="handleSave">保存设置</el-button>
    </div>

    <el-card shadow="never" v-loading="loading">
      <el-form label-width="140px">
        <el-divider content-position="left">基本信息</el-divider>

        <el-form-item label="站点名称">
          <el-input v-model="form.siteName" placeholder="博客站点名称" maxlength="128" style="max-width:400px" />
        </el-form-item>

        <el-form-item label="站点描述">
          <el-input
            v-model="form.siteDescription"
            type="textarea"
            :rows="2"
            placeholder="站点描述或 Slogan，显示在首页及 SEO 元信息中"
            maxlength="512"
            show-word-limit
            style="max-width:500px"
          />
        </el-form-item>

        <el-form-item label="站点 Logo">
          <div class="upload-row">
            <el-upload
              :show-file-list="false"
              :before-upload="handleLogoUpload"
              accept="image/png,image/jpeg,image/svg+xml"
              :disabled="uploadingLogo"
            >
              <el-button :loading="uploadingLogo">上传 Logo</el-button>
            </el-upload>
            <div v-if="form.logoPreviewUrl" class="preview-box">
              <img :src="form.logoPreviewUrl" class="preview-img" />
              <el-button link type="danger" @click="handleLogoRemove">删除</el-button>
            </div>
            <span v-else class="form-item-hint">建议尺寸 200x40px，支持 PNG/JPEG/SVG</span>
          </div>
        </el-form-item>

        <el-form-item label="Favicon 图标">
          <div class="upload-row">
            <el-upload
              :show-file-list="false"
              :before-upload="handleFaviconUpload"
              accept="image/x-icon,image/png,image/svg+xml"
              :disabled="uploadingFavicon"
            >
              <el-button :loading="uploadingFavicon">上传 Favicon</el-button>
            </el-upload>
            <div v-if="form.faviconPreviewUrl" class="preview-box">
              <img :src="form.faviconPreviewUrl" class="preview-favicon" />
              <el-button link type="danger" @click="handleFaviconRemove">删除</el-button>
            </div>
            <span v-else class="form-item-hint">建议尺寸 32x32px，支持 ICO/PNG/SVG</span>
          </div>
        </el-form-item>

        <el-divider content-position="left">功能开关</el-divider>

        <el-form-item label="评论审核">
          <div>
            <el-switch v-model="form.commentModerationEnabled" />
            <span class="form-item-hint" v-if="form.commentModerationEnabled">
              新评论需要管理员审核后才能显示
            </span>
            <span class="form-item-hint" v-else>
              新评论直接发布，无需审核
            </span>
          </div>
        </el-form-item>

        <el-form-item label="邮件订阅">
          <div>
            <el-switch v-model="form.subscriptionEnabled" />
            <span class="form-item-hint">开启后前台首页底部显示邮件订阅表单</span>
          </div>
        </el-form-item>

        <el-form-item label="访问记录保留天数">
          <div>
            <el-input-number v-model="form.visitRetentionDays" :min="1" :max="365" />
            <span class="form-item-hint">超过此天数的访问记录将被自动清理</span>
          </div>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<style scoped>
.settings-page {
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
  margin-left: 12px;
  font-size: 12px;
  color: #909399;
}
.upload-row {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}
.preview-box {
  display: flex;
  align-items: center;
  gap: 8px;
}
.preview-img {
  max-height: 40px;
  max-width: 200px;
  border: 1px solid #e5e7eb;
  border-radius: 4px;
  padding: 4px;
}
.preview-favicon {
  width: 32px;
  height: 32px;
  border: 1px solid #e5e7eb;
  border-radius: 4px;
  padding: 2px;
}
</style>
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getSeoSettingsApi, saveSeoSettingApi, generateSitemapApi } from '@/api/seo'
import { PAGE_KEY_OPTIONS } from '@/types/seo'
import type { SeoSetting, UpdateSeoSettingRequest } from '@/types/seo'

const loading = ref(false)
const settings = ref<SeoSetting[]>([])

/** 编辑对话框 */
const dialogVisible = ref(false)
const editingForm = ref<UpdateSeoSettingRequest>({
  pageKey: 'home',
  title: '',
  description: '',
  keywords: '',
  ogImageId: null,
  canonicalUrl: ''
})
const saving = ref(false)
const sitemapDialogVisible = ref(false)
const sitemapContent = ref('')
const generatingSitemap = ref(false)

async function loadSettings() {
  loading.value = true
  try {
    const res = await getSeoSettingsApi()
    settings.value = res.data.data || []
  } catch {
    ElMessage.error('加载 SEO 配置失败')
  } finally {
    loading.value = false
  }
}

function handleEdit(setting: SeoSetting) {
  editingForm.value = {
    pageKey: setting.pageKey,
    title: setting.title || '',
    description: setting.description || '',
    keywords: setting.keywords || '',
    ogImageId: null,
    canonicalUrl: setting.canonicalUrl || ''
  }
  dialogVisible.value = true
}

function handleCreate() {
  editingForm.value = {
    pageKey: '',
    title: '',
    description: '',
    keywords: '',
    ogImageId: null,
    canonicalUrl: ''
  }
  dialogVisible.value = true
}

async function handleSave() {
  if (!editingForm.value.pageKey) {
    ElMessage.warning('请选择页面标识')
    return
  }
  saving.value = true
  try {
    await saveSeoSettingApi(editingForm.value)
    ElMessage.success('SEO 配置已保存')
    dialogVisible.value = false
    await loadSettings()
  } catch {
    ElMessage.error('保存失败')
  } finally {
    saving.value = false
  }
}

async function handleGenerateSitemap() {
  generatingSitemap.value = true
  try {
    const res = await generateSitemapApi()
    sitemapContent.value = res.data.data?.content || ''
    sitemapDialogVisible.value = true
  } catch {
    ElMessage.error('生成 Sitemap 失败')
  } finally {
    generatingSitemap.value = false
  }
}

/** 复制 Sitemap 内容 */
function copySitemap() {
  navigator.clipboard.writeText(sitemapContent.value).then(() => {
    ElMessage.success('已复制到剪贴板')
  })
}

onMounted(loadSettings)
</script>

<template>
  <div class="seo-page">
    <div class="page-header">
      <h3>SEO 配置</h3>
      <div class="header-actions">
        <el-button @click="handleGenerateSitemap" :loading="generatingSitemap">生成 Sitemap</el-button>
        <el-button type="primary" @click="handleCreate">新增配置</el-button>
      </div>
    </div>

    <el-alert
      title="为每个页面独立配置 SEO 元信息（标题、描述、关键词），有助于搜索引擎优化"
      type="info"
      show-icon
      closable
      class="mb-4"
    />

    <el-card shadow="never">
      <el-table :data="settings" v-loading="loading" stripe style="width: 100%">
        <el-table-column label="页面" width="120">
          <template #default="{ row }">
            <el-tag size="small">
              {{ PAGE_KEY_OPTIONS.find(o => o.value === row.pageKey)?.label || row.pageKey }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="页面标识" width="120">
          <template #default="{ row }"><code>{{ row.pageKey }}</code></template>
        </el-table-column>
        <el-table-column label="SEO 标题" min-width="200">
          <template #default="{ row }">{{ row.title || '-' }}</template>
        </el-table-column>
        <el-table-column label="SEO 描述" min-width="280">
          <template #default="{ row }">
            <span class="desc-text">{{ row.description || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="关键词" width="180">
          <template #default="{ row }">{{ row.keywords || '-' }}</template>
        </el-table-column>
        <el-table-column label="操作" width="100" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleEdit(row)">编辑</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 编辑对话框 -->
    <el-dialog v-model="dialogVisible" :title="editingForm.pageKey ? '编辑 SEO 配置' : '新增 SEO 配置'" width="600px">
      <el-form label-width="100px">
        <el-form-item label="页面标识" required>
          <el-select v-model="editingForm.pageKey" style="width: 100%" filterable :disabled="!!editingForm.pageKey">
            <el-option v-for="opt in PAGE_KEY_OPTIONS" :key="opt.value" :label="`${opt.label} (${opt.value})`" :value="opt.value" />
          </el-select>
        </el-form-item>
        <el-form-item label="SEO 标题">
          <el-input v-model="editingForm.title" placeholder="自定义页面标题，留空使用页面默认标题" maxlength="255" show-word-limit />
        </el-form-item>
        <el-form-item label="SEO 描述">
          <el-input v-model="editingForm.description" type="textarea" :rows="3" placeholder="页面描述，建议 50-160 字符" maxlength="512" show-word-limit />
        </el-form-item>
        <el-form-item label="关键词">
          <el-input v-model="editingForm.keywords" placeholder="逗号分隔，如：博客, 技术, 前端" />
        </el-form-item>
        <el-form-item label="权威链接">
          <el-input v-model="editingForm.canonicalUrl" placeholder="Canonical URL，防止重复内容，如：https://example.com/page" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="handleSave">保存</el-button>
      </template>
    </el-dialog>

    <!-- Sitemap 预览对话框 -->
    <el-dialog v-model="sitemapDialogVisible" title="Sitemap XML" width="700px">
      <div class="sitemap-preview">
        <pre><code>{{ sitemapContent }}</code></pre>
      </div>
      <template #footer>
        <el-button @click="copySitemap">复制到剪贴板</el-button>
        <el-button type="primary" @click="sitemapDialogVisible = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.seo-page {
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
.header-actions {
  display: flex;
  gap: 8px;
}
.mb-4 {
  margin-bottom: 16px;
}
.desc-text {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  word-break: break-all;
}
.sitemap-preview {
  max-height: 400px;
  overflow-y: auto;
  background: #f5f7fa;
  border: 1px solid #e4e7ed;
  border-radius: 4px;
  padding: 12px;
}
.sitemap-preview pre {
  margin: 0;
  white-space: pre-wrap;
  word-break: break-all;
  font-size: 12px;
  line-height: 1.6;
}
</style>
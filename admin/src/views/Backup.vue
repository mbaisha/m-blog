<script setup lang="ts">
import { ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import request from '@/utils/request'

const loading = ref(false)
const exporting = ref(false)
const importing = ref(false)
const stats = ref<any>(null)
const importReport = ref<any>(null)
const selectedFile = ref<File | null>(null)

/** 全部 30 张表的统计项配置 */
const statItems = [
  { key: 'articles', label: '文章', suffix: '篇' },
  { key: 'comments', label: '评论' },
  { key: 'categories', label: '分类' },
  { key: 'tags', label: '标签' },
  { key: 'pages', label: '页面' },
  { key: 'projects', label: '项目' },
  { key: 'users', label: '用户' },
  { key: 'visits', label: '访问' },
  { key: 'subscribers', label: '订阅' },
  { key: 'messages', label: '留言' },
  { key: 'media', label: '媒体' },
  { key: 'friends', label: '友链' },
  { key: 'likes', label: '点赞' },
  { key: 'auditLogs', label: '审计日志' },
  { key: 'profileSections', label: '个人模块' },
  { key: 'articleTags', label: '文章标签关联' },
  { key: 'articleCategories', label: '文章分类关联' },
  { key: 'navigationItems', label: '导航菜单' },
  { key: 'footerConfigs', label: '页脚配置' },
  { key: 'themeSettings', label: '主题设置' },
  { key: 'moduleLayouts', label: '模块布局' },
  { key: 'siteSettings', label: '站点设置' },
  { key: 'seoSettings', label: 'SEO设置' },
  { key: 'refreshTokens', label: '刷新令牌' },
  { key: 'captchaSessions', label: '验证码' },
  { key: 'emailSettings', label: '邮件设置' },
  { key: 'emailTemplates', label: '邮件模板' },
  { key: 'emailLogs', label: '邮件日志' },
  { key: 'llmConfigs', label: 'LLM配置' },
  { key: 'imageGenConfigs', label: '图片生成' },
]

/** 获取数据统计 */
async function fetchStats() {
  loading.value = true
  try {
    const res = await request.get('/admin/backup/stats')
    stats.value = res.data.data
  } catch {
    stats.value = null
  } finally {
    loading.value = false
  }
}

/** 导出数据 */
async function handleExport() {
  let doZip = false
  try {
    await ElMessageBox.confirm(
      '将导出站点全量数据为 JSON 文件。是否需要 ZIP 压缩？',
      '导出选项',
      {
        confirmButtonText: '需要压缩 (.json.zip)',
        cancelButtonText: '不压缩 (.json)',
        distinguishCancelAndClose: true,
        type: 'info',
      }
    )
    doZip = true
  } catch (action: any) {
    if (action !== 'cancel') return
    doZip = false
  }

  exporting.value = true
  try {
    const response = await request.get('/admin/backup/export', {
      params: { zip: doZip },
      responseType: 'blob',
    })

    const mimeType = doZip ? 'application/zip' : 'application/json'
    const ext = doZip ? '.json.zip' : '.json'
    const blob = new Blob([response.data], { type: mimeType })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `mblog-backup-${new Date().toISOString().slice(0, 10)}${ext}`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)

    ElMessage.success('数据导出成功')
  } catch {
    ElMessage.error('导出失败，请稍后重试')
  } finally {
    exporting.value = false
  }
}

/** 文件选择 */
function handleFileChange(e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) {
    selectedFile.value = file
  }
}

/** 清空已选文件 */
function handleClearFile() {
  selectedFile.value = null
}

/** 导入数据 */
async function handleImport() {
  if (!selectedFile.value) {
    ElMessage.warning('请先选择要导入的备份文件')
    return
  }

  try {
    await ElMessageBox.confirm(
      '导入将先清空数据库中的所有数据，然后写入备份文件中的数据。此操作不可撤销，是否继续？',
      '确认导入',
      {
        confirmButtonText: '确认导入',
        cancelButtonText: '取消',
        type: 'warning',
      }
    )
  } catch {
    return
  }

  importing.value = true
  importReport.value = null
  try {
    const formData = new FormData()
    formData.append('file', selectedFile.value)

    const res = await request.post('/admin/import/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })

    importReport.value = res.data.data

    if (importReport.value.success) {
      ElMessage.success(`导入成功，共 ${importReport.value.successCount} 条记录`)
    } else {
      ElMessage.warning(`导入部分完成，成功 ${importReport.value.successCount} 条，错误 ${importReport.value.errors?.length || 0} 个`)
    }

    await fetchStats()
  } catch {
    ElMessage.error('导入失败，请稍后重试')
  } finally {
    importing.value = false
  }
}

fetchStats()
</script>

<template>
  <div class="backup-page">
    <!-- 数据概览 - 置顶 -->
    <el-card class="stats-card">
      <template #header>
        <div class="card-header">
          <span>数据概览</span>
          <el-button size="small" @click="fetchStats" :loading="loading">刷新</el-button>
        </div>
      </template>
      <el-row :gutter="12" v-loading="loading">
        <el-col :span="4" :xs="12" v-for="item in statItems" :key="item.key" v-if="stats">
          <el-statistic :title="item.label" :value="stats[item.key] ?? 0">
            <template #suffix v-if="item.suffix"><span class="stat-unit">{{ item.suffix }}</span></template>
          </el-statistic>
        </el-col>
      </el-row>
    </el-card>

    <!-- 导出 + 导入 双栏 -->
    <el-row :gutter="24" class="action-cards">
      <!-- 导出 -->
      <el-col :span="12" :xs="24">
        <el-card class="action-card">
          <div class="action-inner">
            <div class="action-body">
              <el-icon size="40" color="#409eff"><FolderOpened /></el-icon>
              <div>
                <h3>导出数据</h3>
                <p>将站点全量数据导出为文件</p>
              </div>
            </div>
            <el-button type="primary" :loading="exporting" @click="handleExport">
              <el-icon><Download /></el-icon>
              {{ exporting ? '导出中' : '导出' }}
            </el-button>
          </div>
        </el-card>
      </el-col>

      <!-- 导入 -->
      <el-col :span="12" :xs="24">
        <el-card class="action-card">
          <div class="action-inner">
            <div class="action-body">
              <el-icon size="40" color="#e6a23c"><Upload /></el-icon>
              <div>
                <h3>导入数据</h3>
                <p>从备份文件恢复数据</p>
              </div>
            </div>
            <label class="file-picker">
              <input
                type="file"
                accept=".json,.zip"
                @change="handleFileChange"
                :disabled="importing"
              />
              <el-button :type="selectedFile ? 'success' : 'default'" tag="span">
                <el-icon><FolderAdd /></el-icon>
                {{ selectedFile ? selectedFile.name : '选择文件' }}
              </el-button>
            </label>
          </div>
          <div v-if="selectedFile" class="import-bar">
            <span class="file-hint">{{ selectedFile.name }}</span>
            <div class="import-actions">
              <el-button type="warning" :loading="importing" size="small" @click="handleImport">
                <el-icon><Upload /></el-icon>
                {{ importing ? '导入中' : '开始导入' }}
              </el-button>
              <el-button size="small" @click="handleClearFile" :disabled="importing">
                <el-icon><Delete /></el-icon>
              </el-button>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 导入报告 -->
    <el-card v-if="importReport" class="report-card">
      <template #header>
        <span>导入报告</span>
      </template>

      <el-alert
        :title="importReport.success ? '导入成功' : '导入部分完成'"
        :type="importReport.success ? 'success' : 'warning'"
        :closable="false"
        show-icon
      >
        <template #default>
          <p>成功记录数: <strong>{{ importReport.successCount }}</strong>，耗时: <strong>{{ importReport.elapsedMs }} ms</strong></p>
        </template>
      </el-alert>

      <el-table
        v-if="importReport.tableResults?.length"
        :data="importReport.tableResults"
        border
        stripe
        size="small"
        max-height="400"
        style="margin-top: 16px;"
      >
        <el-table-column prop="tableName" label="表名" width="180" />
        <el-table-column prop="successCount" label="成功数" width="100" align="center" />
        <el-table-column prop="errorCount" label="失败数" width="100" align="center">
          <template #default="{ row }">
            <span :style="{ color: row.errorCount > 0 ? '#f56c6c' : '' }">{{ row.errorCount }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="error" label="错误信息" min-width="200" show-overflow-tooltip />
      </el-table>

      <div v-if="importReport.errors?.length" style="margin-top: 12px;">
        <el-alert
          v-for="(err, idx) in importReport.errors"
          :key="idx"
          :title="err"
          type="error"
          :closable="false"
          style="margin-bottom: 8px;"
        />
      </div>
    </el-card>
  </div>
</template>

<style scoped>
.backup-page {
  max-width: 960px;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.stats-card {
  margin-bottom: 24px;
}

.stats-card .el-statistic {
  padding: 8px 0;
}

.stat-unit {
  font-size: 13px;
  color: #999;
  margin-left: 2px;
}

.action-cards {
  margin-bottom: 24px;
}

.action-card {
  height: 100%;
}

.action-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.action-body {
  display: flex;
  align-items: center;
  gap: 14px;
}

.action-body h3 {
  margin: 0 0 2px;
  font-size: 15px;
  font-weight: 600;
}

.action-body p {
  margin: 0;
  color: #999;
  font-size: 12px;
}

.import-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 14px;
  padding-top: 14px;
  border-top: 1px solid var(--el-border-color-lighter);
}

.import-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.file-picker {
  cursor: pointer;
}

.file-picker input[type="file"] {
  display: none;
}

.file-hint {
  font-size: 13px;
  color: #67c23a;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 200px;
}

.report-card {
  margin-bottom: 24px;
}
</style>

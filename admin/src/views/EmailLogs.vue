<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { EmailLogItem } from '@/types/pages'
import { getEmailLogsApi, deleteEmailLogApi, clearEmailLogsApi } from '@/api/email-logs'

const loading = ref(false)
const logs = ref<EmailLogItem[]>([])
const totalCount = ref(0)
const page = ref(1)
const pageSize = ref(20)

const filterEmail = ref('')
const filterSuccess = ref<string>('') // '' = all, 'true' = success, 'false' = failed
const filterStartDate = ref('')
const filterEndDate = ref('')

async function fetchLogs() {
  loading.value = true
  try {
    const params: any = { page: page.value, pageSize: pageSize.value }
    if (filterEmail.value.trim()) params.email = filterEmail.value.trim()
    if (filterSuccess.value !== '') params.isSuccess = filterSuccess.value === 'true'
    if (filterStartDate.value) params.startDate = new Date(filterStartDate.value).toISOString()
    if (filterEndDate.value) params.endDate = new Date(filterEndDate.value + 'T23:59:59').toISOString()

    const res = await getEmailLogsApi(params)
    if (res.data.success && res.data.data) {
      logs.value = res.data.data.items || []
      totalCount.value = res.data.data.totalCount || 0
    }
  } finally {
    loading.value = false
  }
}

function handleSearch() {
  page.value = 1
  fetchLogs()
}

function handleReset() {
  filterEmail.value = ''
  filterSuccess.value = ''
  filterStartDate.value = ''
  filterEndDate.value = ''
  page.value = 1
  fetchLogs()
}

function handleDelete(id: string) {
  ElMessageBox.confirm('确定要删除该条发送记录？', '确认', { type: 'warning' })
    .then(async () => {
      const res = await deleteEmailLogApi(id)
      if (res.data.success) {
        ElMessage.success('已删除')
        fetchLogs()
      }
    })
    .catch(() => {})
}

function handleClear() {
  ElMessageBox.confirm(
    `确定要清空所有发送日志（共 ${totalCount.value} 条）？此操作不可恢复！`,
    '确认',
    { type: 'warning', confirmButtonText: '确定清空', confirmButtonClass: 'el-button--danger' }
  ).then(async () => {
    const res = await clearEmailLogsApi()
    if (res.data.success) {
      ElMessage.success('日志已清空')
      fetchLogs()
    }
  }).catch(() => {})
}

function handlePageChange(p: number) {
  page.value = p
  fetchLogs()
}

onMounted(() => fetchLogs())
</script>

<template>
  <div class="email-logs-page">
    <el-card shadow="never">
      <template #header>
        <div style="display:flex;justify-content:space-between;align-items:center;flex-wrap:wrap;gap:12px;">
          <span style="font-weight:600;font-size:16px;">📬 邮件发送记录</span>
          <el-button type="danger" size="small" :disabled="totalCount === 0" @click="handleClear">
            清空全部日志
          </el-button>
        </div>
      </template>

      <!-- 搜索筛选区 -->
      <div style="margin-bottom:16px;display:flex;flex-wrap:wrap;gap:12px;align-items:center;">
        <el-input
          v-model="filterEmail"
          placeholder="收件人邮箱"
          clearable
          style="width:200px;"
          @keyup.enter="handleSearch"
        />
        <el-select v-model="filterSuccess" placeholder="发送状态" clearable style="width:130px;" @change="handleSearch">
          <el-option label="全部" value="" />
          <el-option label="成功" value="true" />
          <el-option label="失败" value="false" />
        </el-select>
        <el-date-picker
          v-model="filterStartDate"
          type="date"
          placeholder="开始日期"
          value-format="YYYY-MM-DD"
          style="width:150px;"
          @change="handleSearch"
        />
        <el-date-picker
          v-model="filterEndDate"
          type="date"
          placeholder="结束日期"
          value-format="YYYY-MM-DD"
          style="width:150px;"
          @change="handleSearch"
        />
        <el-button type="primary" @click="handleSearch">搜索</el-button>
        <el-button @click="handleReset">重置</el-button>
      </div>

      <!-- 日志表格 -->
      <el-table v-loading="loading" :data="logs" border stripe style="width:100%;" size="small">
        <el-table-column label="收件人" prop="email" min-width="200" show-overflow-tooltip />
        <el-table-column label="主题" prop="subject" min-width="260" show-overflow-tooltip />
        <el-table-column label="模板" prop="templateKey" width="110">
          <template #default="{ row }">
            <span v-if="row.templateKey">{{ row.templateKey }}</span>
            <span v-else style="color:#999;">-</span>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="80" align="center">
          <template #default="{ row }">
            <el-tag v-if="row.isSuccess" type="success" size="small">成功</el-tag>
            <el-tag v-else type="danger" size="small">失败</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="发送时间" width="170">
          <template #default="{ row }">
            {{ new Date(row.sentAt).toLocaleString() }}
          </template>
        </el-table-column>
        <el-table-column label="错误信息" prop="errorMessage" min-width="180" show-overflow-tooltip>
          <template #default="{ row }">
            <span v-if="row.errorMessage" style="color:#e74c3c;">{{ row.errorMessage }}</span>
            <span v-else style="color:#999;">-</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="80" fixed="right" align="center">
          <template #default="{ row }">
            <el-button link type="danger" size="small" @click="handleDelete(row.id)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div style="display:flex;justify-content:space-between;align-items:center;margin-top:16px;">
        <span style="font-size:13px;color:#999;">共 {{ totalCount }} 条记录</span>
        <el-pagination
          v-if="totalCount > pageSize"
          v-model:current-page="page"
          :page-size="pageSize"
          :total="totalCount"
          layout="prev, pager, next"
          @current-change="handlePageChange"
        />
      </div>
    </el-card>
  </div>
</template>

<style scoped>
.email-logs-page {
  max-width: 1400px;
}
</style>
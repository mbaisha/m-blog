<template>
  <div class="visits-page">
    <div class="page-header">
      <h3>访问记录</h3>
      <div class="header-actions">
        <el-button type="danger" plain size="small" :loading="cleaning" @click="handleCleanup">
          清理历史记录
        </el-button>
      </div>
    </div>

    <!-- 筛选 -->
    <el-card shadow="never" class="filter-bar">
      <el-form :inline="true" :model="query" size="small" label-width="auto">
        <el-form-item label="关键词">
          <el-input v-model="query.keyword" placeholder="路径/IP" clearable @clear="fetchData" @keyup.enter="fetchData" />
        </el-form-item>
        <el-form-item label="页面路径">
          <el-input v-model="query.pagePath" placeholder="如 /articles/" clearable @clear="fetchData" @keyup.enter="fetchData" />
        </el-form-item>
        <el-form-item label="时间范围">
          <el-date-picker
            v-model="dateRange"
            type="datetimerange"
            value-format="YYYY-MM-DDTHH:mm:ss.000Z"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            @change="fetchData"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchData">查询</el-button>
          <el-button @click="resetFilter">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 表格 -->
    <el-card shadow="never">
      <el-table :data="list" v-loading="loading" stripe size="small" style="width: 100%">
        <el-table-column prop="pagePath" label="页面路径" min-width="180" show-overflow-tooltip />
        <el-table-column prop="articleTitle" label="关联文章" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">
            <span>{{ row.articleTitle || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="ipAddress" label="IP 地址" width="150">
          <template #default="{ row }">
            <code style="font-size: 12px">{{ row.ipAddress || '-' }}</code>
          </template>
        </el-table-column>
        <el-table-column prop="location" label="地理位置" width="180">
          <template #default="{ row }">
            <span>{{ row.location || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="ipHash" label="访客标识" width="160">
          <template #default="{ row }">
            <code style="font-size: 12px; color: #999">{{ row.ipHash?.slice(0, 16) || '-' }}...</code>
          </template>
        </el-table-column>
        <el-table-column prop="userAgent" label="设备信息" min-width="200" show-overflow-tooltip>
          <template #default="{ row }">
            <span style="font-size: 12px; color: #666">{{ row.userAgent || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="referer" label="来源" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">
            <span>{{ row.referer || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="visitedAt" label="访问时间" width="180">
          <template #default="{ row }">
            {{ formatDate(row.visitedAt) }}
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrapper">
        <el-pagination
          v-model:current-page="query.page"
          v-model:page-size="query.pageSize"
          :total="total"
          layout="total, sizes, prev, pager, next"
          :page-sizes="[10, 20, 50]"
          small
          @change="fetchData"
        />
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getVisitListApi, cleanupVisitsApi } from '@/api/visit'
import type { VisitListItem } from '@/types/visit'

const loading = ref(false)
const list = ref<VisitListItem[]>([])
const total = ref(0)
const cleaning = ref(false)

const query = reactive({
  page: 1,
  pageSize: 20,
  keyword: '',
  pagePath: '',
  startDate: undefined as string | undefined,
  endDate: undefined as string | undefined,
})

const dateRange = ref<[string, string] | null>(null)

function formatDate(d: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

async function fetchData() {
  loading.value = true
  try {
    if (dateRange.value) {
      query.startDate = dateRange.value[0]
      query.endDate = dateRange.value[1]
    } else {
      query.startDate = undefined
      query.endDate = undefined
    }
    const res = await getVisitListApi(query)
    if (res.data.code === 200 && res.data.data) {
      list.value = res.data.data.items
      total.value = res.data.data.totalCount
    }
  } finally {
    loading.value = false
  }
}

function resetFilter() {
  query.keyword = ''
  query.pagePath = ''
  query.page = 1
  dateRange.value = null
  query.startDate = undefined
  query.endDate = undefined
  fetchData()
}

async function handleCleanup() {
  try {
    await ElMessageBox.confirm('将清理 30 天前的访问记录，确定继续？', '确认清理', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    cleaning.value = true
    const res = await cleanupVisitsApi({ retentionDays: 30 })
    if (res.data.code === 200 && res.data.data) {
      ElMessage.success(`已清理 ${res.data.data.deletedCount} 条记录`)
      fetchData()
    }
  } catch {
    // cancelled
  } finally {
    cleaning.value = false
  }
}

onMounted(fetchData)
</script>

<style scoped>
.visits-page {
  padding: 20px;
}
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}
.page-header h3 {
  margin: 0;
  font-size: 18px;
}
.filter-bar {
  margin-bottom: 16px;
}
.pagination-wrapper {
  margin-top: 16px;
  display: flex;
  justify-content: flex-end;
}
</style>
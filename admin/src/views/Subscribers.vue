<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getSubscribersApi, deleteSubscriberApi, getSubscriberCountApi, resendWeeklyDigestApi, resendWeeklyDigestForSubscriberApi } from '@/api/subscription'
import type { SubscriberItem } from '@/types/pages'

const loading = ref(false)
const resending = ref(false)
const resendingId = ref<string | null>(null)
const subscribers = ref<SubscriberItem[]>([])
const activeCount = ref(0)

async function load() {
  loading.value = true
  try {
    const [listRes, countRes] = await Promise.all([
      getSubscribersApi(),
      getSubscriberCountApi()
    ])
    subscribers.value = listRes.data.data || []
    activeCount.value = countRes.data.data || 0
  } finally {
    loading.value = false
  }
}

async function handleDelete(row: SubscriberItem) {
  try {
    await ElMessageBox.confirm(
      `确定删除订阅者 ${row.email} 的记录？`,
      '确认删除',
      { type: 'warning', confirmButtonText: '删除', cancelButtonText: '取消' }
    )
    await deleteSubscriberApi(row.id)
    ElMessage.success('已删除')
    await load()
  } catch {
    // 取消操作不处理
  }
}

async function handleResend() {
  if (activeCount.value === 0) {
    ElMessage.warning('暂无活跃订阅者，无法发送')
    return
  }
  try {
    await ElMessageBox.confirm(
      `将向 ${activeCount.value} 位已确认订阅者发送本周周报，是否继续？`,
      '补发周报',
      { type: 'info', confirmButtonText: '立即发送', cancelButtonText: '取消' }
    )
    resending.value = true
    const res = await resendWeeklyDigestApi()
    ElMessage.success(res.data.message || '周报发送完成')
  } catch (e: any) {
    if (e?.toString()?.includes('cancel') || e?.toString()?.includes('Cancel')) return
    ElMessage.error(e?.response?.data?.message || '发送失败')
  } finally {
    resending.value = false
  }
}

async function handleResendForSubscriber(row: SubscriberItem) {
  if (!row.isActive || !row.confirmedAt) {
    ElMessage.warning('该订阅者未确认或已退订，无法发送')
    return
  }
  try {
    await ElMessageBox.confirm(
      `确定向 ${row.email} 补发本周周报？`,
      '补发周报',
      { type: 'info', confirmButtonText: '确定发送', cancelButtonText: '取消' }
    )
    resendingId.value = row.id
    const res = await resendWeeklyDigestForSubscriberApi(row.id)
    ElMessage.success(res.data.message || '周报已发送')
  } catch (e: any) {
    if (e?.toString()?.includes('cancel') || e?.toString()?.includes('Cancel')) return
    ElMessage.error(e?.response?.data?.message || '发送失败')
  } finally {
    resendingId.value = null
  }
}

function formatDate(dateStr: string | null) {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleString('zh-CN')
}

onMounted(load)
</script>

<template>
  <div class="subscribers-page">
    <div class="page-header">
      <div>
        <h3>邮件订阅管理</h3>
        <p class="text-[12px] mt-1" style="color:#909399">
          当前活跃订阅者：<strong>{{ activeCount }}</strong> 人
        </p>
      </div>
      <el-button type="primary" :loading="resending" :disabled="activeCount === 0" @click="handleResend">
        📬 补发周报
      </el-button>
    </div>

    <el-card shadow="never" v-loading="loading">
      <el-table :data="subscribers" stripe style="width:100%" v-if="subscribers.length > 0">
        <el-table-column prop="email" label="邮箱" min-width="200" />
        <el-table-column label="状态" width="120">
          <template #default="{ row }">
            <div class="flex flex-col gap-1">
              <el-tag :type="row.isActive ? 'success' : 'info'" size="small">
                {{ row.isActive ? '活跃' : '已退订' }}
              </el-tag>
              <el-tag v-if="row.isActive && !row.confirmedAt" type="warning" size="small">
                待确认
              </el-tag>
              <el-tag v-if="row.confirmedAt" type="success" size="small" plain>
                已确认
              </el-tag>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="订阅时间" width="170">
          <template #default="{ row }">
            {{ formatDate(row.subscribedAt) }}
          </template>
        </el-table-column>
        <el-table-column label="确认时间" width="170">
          <template #default="{ row }">
            {{ formatDate(row.confirmedAt) }}
          </template>
        </el-table-column>
        <el-table-column label="退订时间" width="170">
          <template #default="{ row }">
            {{ formatDate(row.unsubscribedAt) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <div class="flex gap-1">
              <el-button
                link
                type="primary"
                size="small"
                :loading="resendingId === row.id"
                :disabled="!row.isActive || !row.confirmedAt"
                @click="handleResendForSubscriber(row)"
              >
                补发
              </el-button>
              <el-button link type="danger" size="small" @click="handleDelete(row)">删除</el-button>
            </div>
          </template>
        </el-table-column>
      </el-table>

      <el-empty v-else description="暂无订阅者" />
    </el-card>
  </div>
</template>

<style scoped>
.subscribers-page {
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
</style>
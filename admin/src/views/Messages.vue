<template>
  <div class="messages-page">
    <div class="page-header">
      <h2>留言管理</h2>
      <div class="header-actions">
        <el-button type="primary" :disabled="!selectedIds.length" @click="handleBatchApprove">批量审核通过</el-button>
        <el-button type="danger" :disabled="!selectedIds.length" @click="handleBatchDelete">批量删除</el-button>
      </div>
    </div>

    <el-card class="filter-card" shadow="never">
      <el-form :model="query" layout="inline" class="filter-form">
        <el-form-item label="状态">
          <el-select v-model="query.status" clearable placeholder="全部状态" style="width: 140px" @change="handleSearch">
            <el-option label="待审核" value="pending" />
            <el-option label="已通过" value="approved" />
            <el-option label="已拒绝" value="rejected" />
          </el-select>
        </el-form-item>
        <el-form-item label="关键词">
          <el-input v-model="query.keyword" placeholder="搜索昵称或内容" clearable style="width: 200px" @keyup.enter="handleSearch" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">搜索</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card class="list-card" shadow="never">
      <el-table :data="messages" v-loading="loading" stripe @selection-change="handleSelectionChange" style="width: 100%">
        <el-table-column type="selection" width="40" />
        <el-table-column label="留言者" width="120">
          <template #default="{ row }">
            <div class="commenter-info">
              <el-avatar :size="28">{{ row.nickname?.[0] || '?' }}</el-avatar>
              <div class="commenter-detail">
                <div class="nickname">{{ row.nickname }}</div>
                <div class="email" v-if="row.email">{{ row.email }}</div>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="内容" min-width="200">
          <template #default="{ row }">
            <div class="content-wrapper">
              <el-tag v-if="row.parentId" size="small" type="info" class="reply-tag">回复</el-tag>
              <el-link type="primary" :underline="false" class="content-link" @click="openDetailDialog(row)">
                <div class="content-text">{{ row.content }}</div>
              </el-link>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="上下文" width="120">
          <template #default="{ row }">
            <span v-if="row.parentNickname" class="parent-nick">@{{ row.parentNickname }}</span>
            <span v-else class="parent-root">顶级留言</span>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="85">
          <template #default="{ row }">
            <el-tag :type="statusType(row.status)" size="small">{{ statusLabel(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="IP/城市" width="130">
          <template #default="{ row }">
            <div class="cell-small">{{ row.ipCity || '-' }}</div>
          </template>
        </el-table-column>
        <el-table-column label="时间" width="130">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="340" fixed="right">
          <template #default="{ row }">
            <el-button v-if="row.status === 'pending'" type="success" size="small" @click="handleApprove(row.id)">通过</el-button>
            <el-button v-if="row.status === 'pending'" type="warning" size="small" @click="handleReject(row.id)">拒绝</el-button>
            <el-button v-if="row.status === 'approved'" type="primary" size="small" @click="openReplyDialog(row)">回复</el-button>
            <el-button v-if="row.status === 'pending'" type="primary" size="small" @click="openReplyDialog(row)">回复+通过</el-button>
            <el-button type="danger" size="small" @click="handleDelete(row.id)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrapper" v-if="total > 0">
        <el-pagination v-model:current-page="query.page" :page-size="query.pageSize" :total="total" layout="total, prev, pager, next, jumper" @current-change="handlePageChange" />
      </div>
      <el-empty v-if="!loading && messages.length === 0" description="暂无留言" />
    </el-card>

    <!-- 留言详情对话框 -->
    <el-dialog v-model="detailDialogVisible" title="留言详情" width="600px" :close-on-click-modal="false" @close="detailMessage = null">
      <div v-loading="detailLoading" class="detail-section" v-if="detailMessage">
        <div class="detail-row">
          <span class="detail-label">留言者：</span>
          <span class="detail-value">{{ detailMessage.nickname }}</span>
          <el-tag v-if="detailMessage.parentNickname" size="small" type="info" style="margin-left:8px">回复 @{{ detailMessage.parentNickname }}</el-tag>
        </div>
        <div class="detail-row" v-if="detailMessage.email">
          <span class="detail-label">邮箱：</span>
          <span class="detail-value">{{ detailMessage.email }}</span>
        </div>
        <div class="detail-row">
          <span class="detail-label">IP/城市：</span>
          <span class="detail-value">{{ detailMessage.ipCity || '未知' }}</span>
        </div>
        <div class="detail-row">
          <span class="detail-label">状态：</span>
          <el-tag :type="statusType(detailMessage.status)" size="small">{{ statusLabel(detailMessage.status) }}</el-tag>
          <el-button v-if="detailMessage.status === 'pending'" type="success" size="small" style="margin-left:8px" @click="handleApprove(detailMessage.id); detailMessage.status='approved'">审核通过</el-button>
          <el-button v-if="detailMessage.status === 'pending'" type="warning" size="small" style="margin-left:4px" @click="handleReject(detailMessage.id); detailMessage.status='rejected'">拒绝</el-button>
        </div>
        <div class="detail-row">
          <span class="detail-label">时间：</span>
          <span class="detail-value">{{ formatDate(detailMessage.createdAt) }}</span>
        </div>
        <div class="detail-divider"></div>
        <div class="detail-content-block">
          <div class="detail-label" style="margin-bottom:4px">留言内容：</div>
          <div class="detail-content-text">{{ detailMessage.content }}</div>
        </div>
        <div class="detail-divider"></div>
        <div class="detail-content-block" v-if="detailMessage.adminReply">
          <div class="detail-label" style="margin-bottom:4px">管理员回复：</div>
          <div class="detail-admin-reply">{{ detailMessage.adminReply }}</div>
        </div>
        <div class="detail-content-block" v-if="detailMessage.children?.length">
          <div class="detail-label" style="margin-bottom:4px">子回复（共 {{ countAllChildren(detailMessage) }} 条）：</div>
          <div class="child-tree">
            <div v-for="child in detailMessage.children" :key="child.id" class="child-tree-node">
              <div class="detail-child-item">
                <span class="detail-child-nick">{{ child.nickname }}</span>：{{ child.content }}
                <span class="child-audit-actions">
                  <el-tag v-if="child.status === 'pending'" size="small" type="warning" style="margin-left:4px">待审核</el-tag>
                  <el-tag v-if="child.status === 'approved'" size="small" type="success" style="margin-left:4px">已通过</el-tag>
                  <el-tag v-if="child.status === 'rejected'" size="small" type="danger" style="margin-left:4px">已拒绝</el-tag>
                  <el-button v-if="child.status === 'pending'" type="success" size="small" style="margin-left:4px" @click="handleApprove(child.id); child.status='approved'">通过</el-button>
                  <el-button v-if="child.status === 'pending'" type="warning" size="small" style="margin-left:4px" @click="handleReject(child.id); child.status='rejected'">拒绝</el-button>
                </span>
              </div>
              <div v-if="child.children?.length" class="child-tree-sub">
                <div v-for="grand in child.children" :key="grand.id" class="child-tree-node" style="padding-left:16px">
                  <div class="detail-child-item">
                    <span class="detail-child-nick">{{ grand.nickname }}</span>：{{ grand.content }}
                    <span class="child-audit-actions">
                      <el-tag v-if="grand.status === 'pending'" size="small" type="warning" style="margin-left:4px">待审核</el-tag>
                      <el-tag v-if="grand.status === 'approved'" size="small" type="success" style="margin-left:4px">已通过</el-tag>
                      <el-tag v-if="grand.status === 'rejected'" size="small" type="danger" style="margin-left:4px">已拒绝</el-tag>
                      <el-button v-if="grand.status === 'pending'" type="success" size="small" style="margin-left:4px" @click="handleApprove(grand.id); grand.status='approved'">通过</el-button>
                      <el-button v-if="grand.status === 'pending'" type="warning" size="small" style="margin-left:4px" @click="handleReject(grand.id); grand.status='rejected'">拒绝</el-button>
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="detail-divider" v-if="detailMessage.status === 'pending' || detailMessage.status === 'approved'"></div>
        <div class="detail-footer-actions" v-if="detailMessage.status === 'pending' || detailMessage.status === 'approved'">
          <el-button type="primary" @click="openDetailReplyDialog(detailMessage)">回复此留言</el-button>
        </div>
      </div>
    </el-dialog>

    <!-- 回复对话框 -->
    <el-dialog v-model="replyDialogVisible" title="管理员回复" width="520px" :close-on-click-modal="false">
      <div class="reply-message-preview" v-if="replyingMessage">
        <div class="reply-label">留言内容：</div>
        <div class="reply-content">{{ replyingMessage.content }}</div>
        <div class="reply-meta">—— {{ replyingMessage.nickname }} {{ formatDate(replyingMessage.createdAt) }}</div>
      </div>
      <el-input v-model="replyText" type="textarea" :rows="4" placeholder="在此输入回复内容..." maxlength="500" show-word-limit />
      <template #footer>
        <el-button @click="replyDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="replying" @click="confirmReply">发送回复</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getMessageListApi, getMessageDetailApi, updateMessageApi, deleteMessageApi, batchApproveMessagesApi, batchDeleteMessagesApi, type MessageFlatItem, type MessageTreeItem } from '@/api/message'

const loading = ref(false)
const messages = ref<MessageFlatItem[]>([])
const total = ref(0)
const selectedIds = ref<string[]>([])

const query = reactive({ page: 1, pageSize: 20, status: '', keyword: '' })

/** 详情对话框 */
const detailDialogVisible = ref(false)
const detailLoading = ref(false)
const detailMessage = ref<MessageTreeItem | null>(null)

/** 回复对话框 */
const replyDialogVisible = ref(false)
const replyingMessage = ref<MessageFlatItem | null>(null)
const replyText = ref('')
const replying = ref(false)

async function openDetailDialog(row: MessageFlatItem) {
  detailDialogVisible.value = true
  detailLoading.value = true
  detailMessage.value = null
  try {
    const res = await getMessageDetailApi(row.id)
    if (res.data.success) {
      detailMessage.value = res.data.data!
    }
  } catch (err: any) {
    ElMessage.error(err?.response?.data?.message || '加载详情失败')
    detailDialogVisible.value = false
  } finally {
    detailLoading.value = false
  }
}

function openDetailReplyDialog(msg: MessageTreeItem) {
  replyingMessage.value = msg
  replyText.value = ''
  replyDialogVisible.value = true
}

async function fetchMessages() {
  loading.value = true
  try {
    const res = await getMessageListApi({ page: query.page, pageSize: query.pageSize, status: query.status || undefined, keyword: query.keyword || undefined })
    if (res.data.success && res.data.data) {
      messages.value = res.data.data.items
      total.value = res.data.data.totalCount
    }
  } catch (err: any) {
    ElMessage.error(err?.response?.data?.message || '获取留言列表失败')
  } finally { loading.value = false }
}

function handleSearch() { query.page = 1; fetchMessages() }
function handleReset() { query.status = ''; query.keyword = ''; query.page = 1; fetchMessages() }
function handlePageChange(page: number) { query.page = page; fetchMessages() }
function handleSelectionChange(selection: MessageFlatItem[]) { selectedIds.value = selection.map(s => s.id) }

async function handleApprove(id: string) {
  try {
    const res = await updateMessageApi(id, { status: 'approved' })
    if (res.data.success) { ElMessage.success('审核通过'); fetchMessages() }
  } catch (err: any) { ElMessage.error(err?.response?.data?.message || '操作失败') }
}

async function handleReject(id: string) {
  try {
    await ElMessageBox.confirm('确定拒绝该留言？', '提示', { type: 'warning' })
    const res = await updateMessageApi(id, { status: 'rejected' })
    if (res.data.success) { ElMessage.success('已拒绝'); fetchMessages() }
  } catch (err: any) { if (err !== 'cancel') ElMessage.error(err?.response?.data?.message || '操作失败') }
}

function openReplyDialog(row: MessageFlatItem) {
  replyingMessage.value = row
  replyText.value = row.adminReply || ''
  replyDialogVisible.value = true
}

async function confirmReply() {
  if (!replyText.value.trim()) { ElMessage.warning('请输入回复内容'); return }
  if (!replyingMessage.value) return
  replying.value = true
  try {
    const data: any = { adminReply: replyText.value.trim() }
    if (replyingMessage.value.status === 'pending') {
      data.status = 'approved'
    }
    const res = await updateMessageApi(replyingMessage.value.id, data)
    if (res.data.success) { ElMessage.success('回复成功'); replyDialogVisible.value = false; fetchMessages() }
  } catch (err: any) { ElMessage.error(err?.response?.data?.message || '回复失败') } finally { replying.value = false }
}

async function handleDelete(id: string) {
  try {
    await ElMessageBox.confirm('确定删除该留言及其所有子回复？', '警告', { type: 'error', confirmButtonText: '删除' })
    const res = await deleteMessageApi(id)
    if (res.data.success) { ElMessage.success('已删除'); fetchMessages() }
  } catch (err: any) { if (err !== 'cancel') ElMessage.error(err?.response?.data?.message || '操作失败') }
}

async function handleBatchApprove() {
  if (!selectedIds.value.length) return
  try {
    await ElMessageBox.confirm(`确定审核通过选中的 ${selectedIds.value.length} 条留言？`, '提示')
    await batchApproveMessagesApi(selectedIds.value)
    ElMessage.success('批量审核通过成功'); selectedIds.value = []; fetchMessages()
  } catch (err: any) { if (err !== 'cancel') ElMessage.error(err?.response?.data?.message || '操作失败') }
}

async function handleBatchDelete() {
  if (!selectedIds.value.length) return
  try {
    await ElMessageBox.confirm(`确定删除选中的 ${selectedIds.value.length} 条留言？`, '警告', { type: 'error' })
    await batchDeleteMessagesApi(selectedIds.value)
    ElMessage.success('批量删除成功'); selectedIds.value = []; fetchMessages()
  } catch (err: any) { if (err !== 'cancel') ElMessage.error(err?.response?.data?.message || '操作失败') }
}

function statusType(status: string): string {
  const map: Record<string, string> = { pending: 'warning', approved: 'success', rejected: 'danger' }
  return map[status] || 'info'
}
function statusLabel(status: string): string {
  const map: Record<string, string> = { pending: '待审核', approved: '已通过', rejected: '已拒绝' }
  return map[status] || status
}
function formatDate(dateStr: string): string {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' })
}

/** 递归统计所有后代留言数 */
function countAllChildren(msg: MessageTreeItem): number {
  if (!msg.children?.length) return 0
  let count = msg.children.length
  for (const child of msg.children) {
    count += countAllChildren(child)
  }
  return count
}

onMounted(() => fetchMessages())
</script>

<style scoped>
.messages-page { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { margin: 0; font-size: 22px; font-weight: 600; }
.filter-card { margin-bottom: 16px; }
.filter-form { display: flex; flex-wrap: wrap; gap: 8px; }
.commenter-info { display: flex; align-items: center; gap: 8px; }
.commenter-detail { line-height: 1.3; }
.nickname { font-weight: 500; font-size: 13px; }
.email { font-size: 11px; color: #999; }
.content-wrapper { display: flex; align-items: flex-start; gap: 4px; }
.reply-tag { transform: scale(0.85); flex-shrink: 0; margin-top: 2px; }
.content-link { cursor: pointer; display: block; }
.content-text { font-size: 13px; line-height: 1.5; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; color: #606266; }
.parent-nick { font-size: 11px; color: #909399; }
.parent-root { font-size: 11px; color: #c0c4cc; }
.child-count { font-size: 11px; color: #909399; margin-top: 2px; }
.cell-small { font-size: 11px; color: #909399; }
.pagination-wrapper { margin-top: 16px; display: flex; justify-content: center; }
.reply-message-preview { background: #f9fafb; border-radius: 8px; padding: 12px; margin-bottom: 12px; }
.reply-label { font-size: 12px; color: #909399; margin-bottom: 4px; }
.reply-content { font-size: 13px; line-height: 1.5; color: #374151; }
.reply-meta { font-size: 11px; color: #9ca3af; margin-top: 4px; }

.detail-section { max-height: 500px; overflow-y: auto; }
.detail-row { display: flex; align-items: center; margin-bottom: 8px; font-size: 13px; }
.detail-label { color: #909399; min-width: 80px; flex-shrink: 0; }
.detail-value { color: #374151; }
.detail-divider { border-top: 1px solid #f0f0f0; margin: 12px 0; }
.detail-content-block { margin-bottom: 8px; }
.detail-content-text { font-size: 13px; line-height: 1.6; color: #374151; background: #f9fafb; padding: 10px; border-radius: 6px; white-space: pre-wrap; }
.detail-admin-reply { font-size: 13px; line-height: 1.6; color: #166534; background: #f0fdf4; padding: 10px; border-radius: 6px; border: 0.5px solid #bbf7d0; }
.detail-child-item { font-size: 12px; padding: 4px 0; color: #606266; border-bottom: 1px solid #f5f5f5; display: flex; align-items: center; flex-wrap: wrap; }
.detail-child-nick { color: #6366f1; font-weight: 500; }
.child-audit-actions { display: inline-flex; align-items: center; gap: 2px; }
.child-tree { max-height: 240px; overflow-y: auto; }
.child-tree-node { margin-bottom: 2px; }
.child-tree-sub { border-left: 2px solid #e5e7eb; margin-left: 4px; padding-left: 8px; }
.detail-footer-actions { display: flex; justify-content: center; padding-top: 8px; }
</style>

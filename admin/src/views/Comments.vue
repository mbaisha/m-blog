<template>
  <div class="comments-page">
    <div class="page-header">
      <h2>评论管理</h2>
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
            <el-option label="垃圾评论" value="spam" />
            <el-option label="已隐藏" value="hidden" />
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
      <el-table :data="comments" v-loading="loading" stripe @selection-change="handleSelectionChange" style="width: 100%">
        <el-table-column type="selection" width="50" />
        <el-table-column label="评论者" width="140">
          <template #default="{ row }">
            <div class="commenter-info">
              <el-avatar :size="32" :src="row.avatar || undefined">{{ row.nickname?.[0] || '?' }}</el-avatar>
              <div class="commenter-detail">
                <div class="nickname">{{ row.nickname }}</div>
                <div class="email" v-if="row.email">{{ row.email }}</div>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="评论内容" min-width="300">
          <template #default="{ row }">
            <div class="comment-content">
              <el-link type="primary" :underline="false" class="content-link" @click="openDetailDialog(row)">
                <div class="content-text">{{ row.content }}</div>
              </el-link>
              <div class="content-meta">
                <span class="article-title" :title="row.articleTitle">文章：{{ row.articleTitle }}</span>
                <span v-if="row.website">
                  <el-link :href="row.website" target="_blank" type="primary" :underline="false">网站</el-link>
                </span>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusType(row.status)" size="small">{{ statusLabel(row.status) }}</el-tag>
            <el-tag v-if="row.isSpam" type="warning" size="small" effect="dark" style="margin-left: 4px">垃圾</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="IP/位置" width="200">
          <template #default="{ row }">
            <div class="ip-info">
              <span class="ip-address" style="font-size:12px;font-family:monospace">{{ row.ipAddress || '-' }}</span>
              <span class="ip-location" style="font-size:11px;color:#909399;display:block">{{ row.location || '-' }}</span>
              <span class="ip-hash" style="font-size:10px;color:#c0c4cc">{{ row.ipHash }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="提交时间" width="170">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="320" fixed="right">
          <template #default="{ row }">
            <el-button v-if="row.status !== 'approved'" type="success" size="small" @click="handleApprove(row.id)">通过</el-button>
            <el-button v-if="row.status !== 'rejected'" type="warning" size="small" @click="handleReject(row.id)">拒绝</el-button>
            <el-button v-if="row.status !== 'spam'" type="info" size="small" @click="handleSpam(row.id)">垃圾</el-button>
            <el-button type="primary" size="small" @click="openCommentReplyDialog(row)">回复</el-button>
            <el-button type="danger" size="small" @click="handleDelete(row.id)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrapper" v-if="total > 0">
        <el-pagination v-model:current-page="query.page" :page-size="query.pageSize" :total="total" layout="total, prev, pager, next, jumper" @current-change="handlePageChange" />
      </div>
      <el-empty v-if="!loading && comments.length === 0" description="暂无评论" />
    </el-card>

    <!-- 评论详情对话框 -->
    <el-dialog v-model="detailDialogVisible" title="评论详情" width="560px" :close-on-click-modal="false">
      <div class="detail-section" v-if="detailComment">
        <div class="detail-row">
          <span class="detail-label">评论者：</span>
          <span class="detail-value">{{ detailComment.nickname }}</span>
        </div>
        <div class="detail-row" v-if="detailComment.email">
          <span class="detail-label">邮箱：</span>
          <span class="detail-value">{{ detailComment.email }}</span>
        </div>
        <div class="detail-row" v-if="detailComment.website">
          <span class="detail-label">网站：</span>
          <el-link :href="detailComment.website" target="_blank" type="primary" :underline="false">{{ detailComment.website }}</el-link>
        </div>
        <div class="detail-row">
          <span class="detail-label">文章：</span>
          <span class="detail-value">{{ detailComment.articleTitle }}</span>
        </div>
        <div class="detail-row">
          <span class="detail-label">IP/位置：</span>
          <span class="detail-value">{{ detailComment.ipAddress || '-' }} / {{ detailComment.location || '-' }}</span>
        </div>
        <div class="detail-row">
          <span class="detail-label">状态：</span>
          <el-tag :type="statusType(detailComment.status)" size="small">{{ statusLabel(detailComment.status) }}</el-tag>
          <el-tag v-if="detailComment.isSpam" type="warning" size="small" style="margin-left:4px">垃圾</el-tag>
        </div>
        <div class="detail-row">
          <span class="detail-label">时间：</span>
          <span class="detail-value">{{ formatDate(detailComment.createdAt) }}</span>
        </div>
        <div class="detail-divider"></div>
        <div class="detail-content-block">
          <div class="detail-label" style="margin-bottom:4px">评论内容：</div>
          <div class="detail-content-text">{{ detailComment.content }}</div>
        </div>
        <div class="detail-divider"></div>
        <div class="detail-content-block" v-if="detailComment.replyContent">
          <div class="detail-label" style="margin-bottom:4px">管理员回复：</div>
          <div class="detail-admin-reply">{{ detailComment.replyContent }}</div>
        </div>
      </div>
      <template #footer v-if="detailComment">
        <el-button type="primary" @click="openDetailReplyDialog">回复此评论</el-button>
      </template>
    </el-dialog>

    <!-- 评论回复对话框 -->
    <el-dialog v-model="commentReplyVisible" title="管理员回复评论" width="520px" :close-on-click-modal="false">
      <div class="reply-message-preview" v-if="commentReplyingTo">
        <div class="reply-label">评论内容：</div>
        <div class="reply-content">{{ commentReplyingTo.content }}</div>
        <div class="reply-meta">—— {{ commentReplyingTo.nickname }} 于 {{ formatDate(commentReplyingTo.createdAt) }} 在文章《{{ commentReplyingTo.articleTitle }}》下</div>
      </div>
      <el-input v-model="commentReplyText" type="textarea" :rows="4" placeholder="在此输入回复内容..." maxlength="500" show-word-limit />
      <template #footer>
        <el-button @click="commentReplyVisible = false">取消</el-button>
        <el-button type="primary" :loading="commentReplying" @click="confirmCommentReply">发送回复</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import request from '@/utils/request'
import type { ApiResponse, PagedResponse } from '@/types/api'

interface CommentItem {
  id: string
  articleId: string
  articleTitle: string
  parentId: string | null
  nickname: string
  email: string | null
  website: string | null
  content: string
  replyContent?: string
  avatar: string | null
  ipHash: string
  ipAddress: string | null
  location: string | null
  status: string
  isSpam: boolean
  reviewedBy: string | null
  reviewedAt: string | null
  createdAt: string
}

const loading = ref(false)
const comments = ref<CommentItem[]>([])
const total = ref(0)
const selectedIds = ref<string[]>([])
const query = reactive({ page: 1, pageSize: 20, status: '', keyword: '' })

/** 详情对话框 */
const detailDialogVisible = ref(false)
const detailComment = ref<CommentItem | null>(null)

/** 评论回复对话框 */
const commentReplyVisible = ref(false)
const commentReplyingTo = ref<CommentItem | null>(null)
const commentReplyText = ref('')
const commentReplying = ref(false)

function openDetailDialog(row: CommentItem) {
  detailComment.value = row
  detailDialogVisible.value = true
}

function openDetailReplyDialog() {
  if (!detailComment.value) return
  commentReplyingTo.value = detailComment.value
  commentReplyText.value = ''
  detailDialogVisible.value = false
  commentReplyVisible.value = true
}

function openCommentReplyDialog(row: CommentItem) {
  commentReplyingTo.value = row
  commentReplyText.value = ''
  commentReplyVisible.value = true
}

async function confirmCommentReply() {
  if (!commentReplyText.value.trim()) { ElMessage.warning('请输入回复内容'); return }
  if (!commentReplyingTo.value) return
  commentReplying.value = true
  try {
    const res = await request.post<ApiResponse>(`/admin/comments/${commentReplyingTo.value.id}/reply`, { content: commentReplyText.value.trim() })
    if (res.data.success) { ElMessage.success('回复成功'); commentReplyVisible.value = false; fetchComments() }
  } catch (err: any) { ElMessage.error(err?.response?.data?.message || '回复失败') } finally { commentReplying.value = false }
}

async function fetchComments() {
  loading.value = true
  try {
    const params: Record<string, string | number> = { page: query.page, pageSize: query.pageSize }
    if (query.status) params.status = query.status
    if (query.keyword) params.keyword = query.keyword
    const res = await request.get<ApiResponse<PagedResponse<CommentItem>>>('/admin/comments', { params })
    if (res.data.success && res.data.data) { comments.value = res.data.data.items; total.value = res.data.data.totalCount }
  } catch (err: any) { ElMessage.error(err?.response?.data?.message || '获取评论列表失败') } finally { loading.value = false }
}

function handleSearch() { query.page = 1; fetchComments() }
function handleReset() { query.status = ''; query.keyword = ''; query.page = 1; fetchComments() }
function handlePageChange(page: number) { query.page = page; fetchComments() }
function handleSelectionChange(selection: CommentItem[]) { selectedIds.value = selection.map(s => s.id) }

async function handleApprove(id: string) {
  try { const res = await request.post<ApiResponse>(`/admin/comments/${id}/approve`); if (res.data.success) { ElMessage.success('审核通过'); fetchComments() } } catch (err: any) { ElMessage.error(err?.response?.data?.message || '操作失败') }
}
async function handleReject(id: string) {
  try { await ElMessageBox.confirm('确定拒绝该评论？', '提示', { type: 'warning' }); const res = await request.post<ApiResponse>(`/admin/comments/${id}/reject`, { reason: '' }); if (res.data.success) { ElMessage.success('已拒绝'); fetchComments() } } catch (err: any) { if (err !== 'cancel') ElMessage.error(err?.response?.data?.message || '操作失败') }
}
async function handleSpam(id: string) {
  try { const res = await request.post<ApiResponse>(`/admin/comments/${id}/spam`); if (res.data.success) { ElMessage.success('已标记为垃圾评论'); fetchComments() } } catch (err: any) { ElMessage.error(err?.response?.data?.message || '操作失败') }
}
async function handleDelete(id: string) {
  try { await ElMessageBox.confirm('确定删除该评论？此操作不可恢复。', '警告', { type: 'error', confirmButtonText: '删除' }); const res = await request.delete<ApiResponse>(`/admin/comments/${id}`); if (res.data.success) { ElMessage.success('已删除'); fetchComments() } } catch (err: any) { if (err !== 'cancel') ElMessage.error(err?.response?.data?.message || '操作失败') }
}
async function handleBatchApprove() {
  if (!selectedIds.value.length) return
  try { await ElMessageBox.confirm(`确定审核通过选中的 ${selectedIds.value.length} 条评论？`, '提示'); const res = await request.post<ApiResponse>('/admin/comments/bulk-approve', { ids: selectedIds.value }); if (res.data.success) { ElMessage.success(res.data.message || '批量审核通过成功'); selectedIds.value = []; fetchComments() } } catch (err: any) { if (err !== 'cancel') ElMessage.error(err?.response?.data?.message || '操作失败') }
}
async function handleBatchDelete() {
  if (!selectedIds.value.length) return
  try { await ElMessageBox.confirm(`确定删除选中的 ${selectedIds.value.length} 条评论？此操作不可恢复。`, '警告', { type: 'error' }); const res = await request.post<ApiResponse>('/admin/comments/bulk-delete', { ids: selectedIds.value }); if (res.data.success) { ElMessage.success(res.data.message || '批量删除成功'); selectedIds.value = []; fetchComments() } } catch (err: any) { if (err !== 'cancel') ElMessage.error(err?.response?.data?.message || '操作失败') }
}

function statusType(status: string): string { const map: Record<string, string> = { pending: 'warning', approved: 'success', rejected: 'danger', hidden: 'info', spam: 'danger' }; return map[status] || 'info' }
function statusLabel(status: string): string { const map: Record<string, string> = { pending: '待审核', approved: '已通过', rejected: '已拒绝', hidden: '已隐藏', spam: '垃圾' }; return map[status] || status }
function formatDate(dateStr: string): string { if (!dateStr) return '-'; const d = new Date(dateStr); return d.toLocaleString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' }) }

onMounted(() => fetchComments())
</script>

<style scoped>
.comments-page { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { margin: 0; font-size: 22px; font-weight: 600; }
.filter-card { margin-bottom: 16px; }
.filter-form { display: flex; flex-wrap: wrap; gap: 8px; }
.commenter-info { display: flex; align-items: center; gap: 8px; }
.commenter-detail { line-height: 1.3; }
.nickname { font-weight: 500; font-size: 13px; }
.email { font-size: 11px; color: #999; }
.comment-content { max-width: 400px; }
.content-link { cursor: pointer; display: block; }
.content-text { display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; font-size: 13px; line-height: 1.5; margin-bottom: 4px; color: #606266; }
.content-meta { display: flex; gap: 12px; font-size: 11px; color: #999; }
.article-title { max-width: 200px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.ip-hash { font-family: monospace; font-size: 11px; color: #999; }
.pagination-wrapper { margin-top: 16px; display: flex; justify-content: center; }
.reply-message-preview { background: #f9fafb; border-radius: 8px; padding: 12px; margin-bottom: 12px; }
.reply-label { font-size: 12px; color: #909399; margin-bottom: 4px; }
.reply-content { font-size: 13px; line-height: 1.5; color: #374151; }
.reply-meta { font-size: 11px; color: #9ca3af; margin-top: 4px; }
.detail-section { max-height: 400px; overflow-y: auto; }
.detail-row { display: flex; margin-bottom: 8px; font-size: 13px; }
.detail-label { color: #909399; min-width: 80px; flex-shrink: 0; }
.detail-value { color: #374151; }
.detail-divider { border-top: 1px solid #f0f0f0; margin: 12px 0; }
.detail-content-block { margin-bottom: 8px; }
.detail-content-text { font-size: 13px; line-height: 1.6; color: #374151; background: #f9fafb; padding: 10px; border-radius: 6px; white-space: pre-wrap; }
.detail-admin-reply { font-size: 13px; line-height: 1.6; color: #166534; background: #f0fdf4; padding: 10px; border-radius: 6px; border: 0.5px solid #bbf7d0; }
</style>
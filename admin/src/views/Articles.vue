<script setup lang="ts">
import { ref, onMounted, computed, nextTick, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { ArticleListItem, ArticleQueryParams } from '@/types/article'
import type { CategoryListItem } from '@/types/category'
import type { TagListItem } from '@/types/tag'
import {
  getArticleListApi,
  getArticleApi,
  deleteArticleApi,
  updateArticleApi,
  updateArticleStatusApi,
  hardDeleteArticleApi,
  restoreArticleApi,
  aiExtractArticleApi,
  aiGenerateCoverApi
} from '@/api/article'
import { getCategoryListApi } from '@/api/category'
import { getTagListApi } from '@/api/tag'

const router = useRouter()

/** 前台站点 URL */
const siteUrl = import.meta.env.VITE_SITE_URL || 'http://localhost:3000'

/** 加载状态 */
const loading = ref(false)

/** 文章列表 */
const articles = ref<ArticleListItem[]>([])

/** 分页信息 */
const pagination = ref({
  page: 1,
  pageSize: 10,
  totalCount: 0,
  totalPages: 0
})

/** 筛选条件 */
const filters = ref({
  keyword: '',
  status: '',
  categoryId: '',
  tagId: '',
  isTop: '' as string | boolean,
  isRecommend: '' as string | boolean
})

/** 分类选项 */
const categories = ref<CategoryListItem[]>([])

/** 标签选项 */
const tags = ref<TagListItem[]>([])

/** 排序选项 */
const sortBy = ref('createdAt')
const sortOrder = ref('desc')

/** 选中文章 ID 集合 */
const selectedIds = ref<string[]>([])

/** 是否显示回收站（已删除文章） */
const showDeleted = ref(false)

// AI 批量填充状态
const aiBatchRunning = ref(false)
const aiBatchProgress = ref({ current: 0, total: 0 })
const aiBatchLog = ref<string[]>([])
const aiBatchDialogVisible = ref(false)
const logRef = ref<HTMLElement | null>(null)

// 日志自动滚动
watch(aiBatchLog, () => {
  nextTick(() => {
    if (logRef.value) {
      logRef.value.scrollTop = logRef.value.scrollHeight
    }
  })
}, { deep: true })

/** 状态标签映射 */
const statusMap: Record<string, { label: string; type: 'info' | 'warning' | 'success' | 'danger' }> = {
  draft: { label: '草稿', type: 'info' },
  published: { label: '已发布', type: 'success' },
  archived: { label: '已下架', type: 'danger' }
}

/** 是否有筛选条件 */
const hasFilters = computed(() => {
  return !!(filters.value.keyword || filters.value.status || filters.value.categoryId || filters.value.tagId || filters.value.isTop || filters.value.isRecommend)
})

/** 加载分类和标签选项 */
async function loadFilterOptions() {
  try {
    const [catRes, tagRes] = await Promise.all([
      getCategoryListApi(),
      getTagListApi()
    ])
    categories.value = catRes.data.data || []
    tags.value = tagRes.data.data || []
  } catch {
    // ignore
  }
}

/** 加载文章列表 */
async function loadArticles() {
  loading.value = true
  try {
    const params: ArticleQueryParams = {
      page: pagination.value.page,
      pageSize: pagination.value.pageSize,
      keyword: filters.value.keyword || undefined,
      status: filters.value.status || undefined,
      categoryId: filters.value.categoryId || undefined,
      tagId: filters.value.tagId || undefined,
      sortBy: sortBy.value,
      sortOrder: sortOrder.value
    }
    if (filters.value.isTop !== '') params.isTop = filters.value.isTop === 'true'
    if (filters.value.isRecommend !== '') params.isRecommend = filters.value.isRecommend === 'true'
    if (showDeleted.value) params.showDeleted = true

    const res = await getArticleListApi(params)
    const data = res.data.data
    if (data) {
      articles.value = data.items
      pagination.value = {
        page: data.page,
        pageSize: data.pageSize,
        totalCount: data.totalCount,
        totalPages: data.totalPages
      }
    }
  } catch {
    // 错误已在拦截器中处理
  } finally {
    loading.value = false
  }
}

/** 搜索 */
function handleSearch() {
  pagination.value.page = 1
  loadArticles()
}

/** 重置筛选 */
function handleReset() {
  filters.value = { keyword: '', status: '', categoryId: '', tagId: '', isTop: '', isRecommend: '' }
  showDeleted.value = false
  sortBy.value = 'createdAt'
  sortOrder.value = 'desc'
  pagination.value.page = 1
  loadArticles()
}

/** 切换回收站模式 */
function toggleTrash() {
  showDeleted.value = !showDeleted.value
  // 进入回收站时重置筛选条件
  if (showDeleted.value) {
    filters.value = { keyword: '', status: '', categoryId: '', tagId: '', isTop: '', isRecommend: '' }
  }
  pagination.value.page = 1
  loadArticles()
}

/** 分页变化 */
function handlePageChange(page: number) {
  pagination.value.page = page
  loadArticles()
}

/** 分页大小变化 */
function handlePageSizeChange(size: number) {
  pagination.value.pageSize = size
  pagination.value.page = 1
  loadArticles()
}

/** 排序变化 */
function handleSortChange() {
  pagination.value.page = 1
  loadArticles()
}

/** 新建文章 */
function handleCreate() {
  router.push('/articles/create')
}

/** 编辑文章 */
function handleEdit(id: string) {
  router.push(`/articles/edit/${id}`)
}

/** 删除文章 */
async function handleDelete(id: string, title: string) {
  try {
    await ElMessageBox.confirm(`确定要删除文章「${title}」吗？`, '确认删除', {
      type: 'warning',
      confirmButtonText: '确定',
      cancelButtonText: '取消'
    })
    await deleteArticleApi(id)
    ElMessage.success('文章已删除')
    await loadArticles()
  } catch {
    // 取消操作不做处理
  }
}

/** 批量删除 */
async function handleBatchDelete() {
  if (selectedIds.value.length === 0) {
    ElMessage.warning('请先选择要删除的文章')
    return
  }
  try {
    await ElMessageBox.confirm(`确定要删除选中的 ${selectedIds.value.length} 篇文章吗？`, '确认删除', {
      type: 'warning',
      confirmButtonText: '确定',
      cancelButtonText: '取消'
    })
    await Promise.all(selectedIds.value.map((id) => deleteArticleApi(id)))
    ElMessage.success('批量删除成功')
    selectedIds.value = []
    await loadArticles()
  } catch {
    // 取消操作不做处理
  }
}

/** 重试工具：最多重试指定次数 */
async function withRetry<T>(fn: () => Promise<T>, label: string, maxRetries = 3): Promise<T | null> {
  for (let i = 0; i < maxRetries; i++) {
    try {
      return await fn()
    } catch (err: any) {
      const isLast = i === maxRetries - 1
      const msg = err?.response?.data?.message || err?.message || '未知错误'
      aiBatchLog.value.push(`  ⚠ ${label} 失败（第 ${i + 1} 次）：${msg}`)
      if (isLast) {
        aiBatchLog.value.push(`  ✗ ${label} 已重试 ${maxRetries} 次，跳过`)
        return null
      }
      // 等待 2 秒后重试
      await new Promise(r => setTimeout(r, 2000))
      aiBatchLog.value.push(`  ↻ ${label} 正在重试（第 ${i + 2} 次）...`)
    }
  }
  return null
}

/** AI 批量填充：逐条生成 SEO、摘要、封面图 */
async function handleAiBatchFill() {
  if (selectedIds.value.length === 0) {
    ElMessage.warning('请先选择要处理的文章')
    return
  }

  try {
    await ElMessageBox.confirm(
      `确定要对选中的 ${selectedIds.value.length} 篇文章进行 AI 自动填充吗？` +
      '\n\n将逐条处理每篇文章：\n1. 生成 SEO 标题/描述/关键词\n2. 生成文章摘要\n3. 生成封面图\n\n' +
      '处理过程中请勿关闭页面。',
      'AI 自动填充',
      { type: 'info', confirmButtonText: '开始处理', cancelButtonText: '取消' }
    )
  } catch {
    return // 用户取消
  }

  aiBatchRunning.value = true
  aiBatchProgress.value = { current: 0, total: selectedIds.value.length }
  aiBatchLog.value = []
  aiBatchDialogVisible.value = true

  let successCount = 0
  let failCount = 0

  for (const id of selectedIds.value) {
    const title = articles.value.find(a => a.id === id)?.title || id
    aiBatchProgress.value.current++

    // 1. 获取文章详情
    aiBatchLog.value.push(`[${aiBatchProgress.value.current}/${aiBatchProgress.value.total}] 正在处理：${title}`)

    const detail = await withRetry(
      () => getArticleApi(id).then(r => r.data.data),
      '获取文章详情'
    )
    if (!detail) { failCount++; continue }
    if (!detail.content?.trim()) {
      aiBatchLog.value.push(`  ⚠ 文章内容为空，跳过`)
      failCount++
      continue
    }

    // 2. AI 提取信息（SEO + 摘要）
    const extractResult = await withRetry(
      () => aiExtractArticleApi({ content: detail.content, currentTitle: detail.title || undefined })
        .then(r => r.data.data),
      'AI 信息提取'
    )

    // 3. AI 生成封面图
    const coverResult = await withRetry(
      () => aiGenerateCoverApi({ title: detail.title || '未命名', content: detail.content })
        .then(r => r.data.data),
      'AI 生成封面'
    )

    // 4. 更新文章
    const updateData: Record<string, any> = {}
    if (extractResult) {
      if (extractResult.summary) updateData.summary = extractResult.summary
      if (extractResult.seoTitle) updateData.seoTitle = extractResult.seoTitle
      if (extractResult.seoDescription) updateData.seoDescription = extractResult.seoDescription
      if (extractResult.seoKeywords) updateData.seoKeywords = extractResult.seoKeywords
    }
    if (coverResult?.mediaId) {
      updateData.coverImageId = coverResult.mediaId
    }

    if (Object.keys(updateData).length === 0) {
      aiBatchLog.value.push(`  ⚠ 无数据需要更新`)
      failCount++
      continue
    }

    // 保留原有字段
    updateData.title = detail.title
    updateData.slug = detail.slug
    updateData.content = detail.content
    updateData.status = detail.status
    updateData.isTop = detail.isTop
    updateData.isRecommend = detail.isRecommend
    updateData.categoryIds = detail.categories?.map(c => c.id) || []
    updateData.tagIds = detail.tags?.map(t => t.id) || []

    const updated = await withRetry(
      () => updateArticleApi(id, updateData as any).then(r => r.data.data),
      '更新文章'
    )
    if (!updated) { failCount++; continue }

    successCount++
    aiBatchLog.value.push(`  ✓ 完成`)
  }

  // 完成
  aiBatchLog.value.push(`\n===== 全部处理完成 =====`)
  aiBatchLog.value.push(`成功：${successCount} 篇，失败：${failCount} 篇`)
  aiBatchRunning.value = false

  ElMessage.success(`AI 填充完成，成功 ${successCount} 篇${failCount > 0 ? `，失败 ${failCount} 篇` : ''}`)
  selectedIds.value = []
  await loadArticles()
}

/** 彻底删除文章 */
async function handleHardDelete(id: string, title: string) {
  try {
    await ElMessageBox.confirm(`确定要彻底删除「${title}」吗？此操作不可恢复！`, '警告', {
      type: 'error',
      confirmButtonText: '确定彻底删除',
      cancelButtonText: '取消'
    })
    await hardDeleteArticleApi(id)
    ElMessage.success('已彻底删除')
    await loadArticles()
  } catch {
    // 取消操作不做处理
  }
}

/** 恢复文章（从回收站还原） */
async function handleRestore(id: string, title: string) {
  try {
    await ElMessageBox.confirm(`确定要还原「${title}」吗？`, '确认还原', {
      type: 'info',
      confirmButtonText: '确定还原',
      cancelButtonText: '取消'
    })
    await restoreArticleApi(id)
    ElMessage.success('文章已还原')
    await loadArticles()
  } catch {
    // 取消操作不做处理
  }
}

/** 更新文章状态 */
async function handleStatusChange(id: string, status: string) {
  const actionLabel = status === 'published' ? '发布' : status === 'archived' ? '下架' : '转为草稿'
  try {
    await ElMessageBox.confirm(`确定要${actionLabel}该文章吗？`, '确认操作', {
      type: 'warning',
      confirmButtonText: '确定',
      cancelButtonText: '取消'
    })
    await updateArticleStatusApi(id, status)
    ElMessage.success(`${actionLabel}成功`)
    await loadArticles()
  } catch {
    // 取消操作不做处理
  }
}

/** 批量发布/下架 */
async function handleBatchStatus(status: string) {
  if (selectedIds.value.length === 0) {
    ElMessage.warning('请先选择文章')
    return
  }
  const actionLabel = status === 'published' ? '发布' : status === 'archived' ? '下架' : '转为草稿'
  try {
    await ElMessageBox.confirm(`确定要${actionLabel}选中的 ${selectedIds.value.length} 篇文章吗？`, '确认操作', {
      type: 'warning',
      confirmButtonText: '确定',
      cancelButtonText: '取消'
    })
    await Promise.all(selectedIds.value.map((id) => updateArticleStatusApi(id, status)))
    ElMessage.success(`批量${actionLabel}成功`)
    selectedIds.value = []
    await loadArticles()
  } catch {
    // 取消操作不做处理
  }
}

onMounted(() => {
  loadFilterOptions()
  loadArticles()
})
</script>

<template>
  <div class="articles-page">
    <!-- 页面标题 -->
    <div class="page-header">
      <h3 class="page-title">文章管理</h3>
      <el-button type="primary" @click="handleCreate">
        <el-icon><Plus /></el-icon>写文章
      </el-button>
    </div>

    <!-- 筛选区域 -->
    <el-card shadow="hover" class="filter-card">
      <el-form :model="filters" inline label-width="auto">
        <el-form-item label="关键词">
          <el-input v-model="filters.keyword" placeholder="标题/摘要" clearable style="width: 180px" @keyup.enter="handleSearch" />
        </el-form-item>

        <el-form-item label="状态">
          <el-select v-model="filters.status" placeholder="全部" clearable style="width: 120px">
            <el-option label="草稿" value="draft" />
            <el-option label="已发布" value="published" />
            <el-option label="已下架" value="archived" />
          </el-select>
        </el-form-item>

        <el-form-item label="分类">
          <el-select v-model="filters.categoryId" placeholder="全部分类" clearable style="width: 150px" filterable>
            <el-option v-for="c in categories" :key="c.id" :label="c.name" :value="c.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="标签">
          <el-select v-model="filters.tagId" placeholder="全部标签" clearable style="width: 150px" filterable>
            <el-option v-for="t in tags" :key="t.id" :label="t.name" :value="t.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="置顶">
          <el-select v-model="filters.isTop" placeholder="全部" clearable style="width: 100px">
            <el-option label="是" :value="'true'" />
            <el-option label="否" :value="'false'" />
          </el-select>
        </el-form-item>

        <el-form-item label="推荐">
          <el-select v-model="filters.isRecommend" placeholder="全部" clearable style="width: 100px">
            <el-option label="是" :value="'true'" />
            <el-option label="否" :value="'false'" />
          </el-select>
        </el-form-item>

        <el-form-item>
          <el-button @click="handleSearch">搜索</el-button>
          <el-button v-if="hasFilters" @click="handleReset">重置</el-button>
        </el-form-item>
        <el-form-item>
          <el-button :type="showDeleted ? 'danger' : 'default'" size="small" @click="toggleTrash">
            <el-icon><Delete /></el-icon> {{ showDeleted ? '退出回收站' : '回收站' }}
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 批量操作 -->
    <div class="batch-bar" v-if="selectedIds.length > 0">
      <span class="batch-info">已选择 {{ selectedIds.length }} 项</span>
      <el-button size="small" type="success" @click="handleBatchStatus('published')">批量发布</el-button>
      <el-button size="small" type="warning" @click="handleBatchStatus('draft')">批量转为草稿</el-button>
      <el-button size="small" type="danger" @click="handleBatchStatus('archived')">批量下架</el-button>
      <el-button size="small" type="danger" @click="handleBatchDelete">批量删除</el-button>
      <el-divider direction="vertical" />
      <el-button size="small" type="primary" :loading="aiBatchRunning" @click="handleAiBatchFill">
        <el-icon><svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2l2.4 7.2h7.6l-6 4.8 2.4 7.2-6-4.8-6 4.8 2.4-7.2-6-4.8h7.6z"/></svg></el-icon> AI 自动填充
      </el-button>
    </div>

    <!-- 排序 -->
    <div class="sort-bar">
      <span class="sort-label">排序：</span>
      <el-radio-group v-model="sortBy" @change="handleSortChange">
        <el-radio-button value="createdAt">创建时间</el-radio-button>
        <el-radio-button value="publishedAt">发布时间</el-radio-button>
        <el-radio-button value="title">标题</el-radio-button>
      </el-radio-group>
      <el-radio-group v-model="sortOrder" @change="handleSortChange" class="sort-order">
        <el-radio-button value="desc">⬇ 降序</el-radio-button>
        <el-radio-button value="asc">⬆ 升序</el-radio-button>
      </el-radio-group>
    </div>

    <!-- 文章列表表格 -->
    <el-card shadow="hover">
      <el-table
        :data="articles"
        v-loading="loading"
        stripe
        style="width: 100%"
        @selection-change="selectedIds = ($event as any[]).map(r => r.id)"
      >
        <el-table-column type="selection" width="45" />

        <el-table-column label="标题" min-width="280">
          <template #default="{ row }">
            <div class="title-cell">
              <el-tag v-if="row.isTop" size="small" type="warning" class="top-tag">置顶</el-tag>
              <el-tag v-if="row.isRecommend" size="small" type="danger" class="recommend-tag">推荐</el-tag>
              <a
                v-if="row.status === 'published'"
                :href="`${siteUrl}/articles/${row.slug}`"
                target="_blank"
                class="title-link"
                :title="`前台浏览：${row.title}`"
              >{{ row.title }}</a>
              <span v-else class="title-text">{{ row.title }}</span>
            </div>
          </template>
        </el-table-column>

        <el-table-column prop="categoryName" label="分类" width="120">
          <template #default="{ row }">
            <el-tag v-if="row.categoryName" size="small" effect="plain">{{ row.categoryName }}</el-tag>
            <span v-else class="empty-text">-</span>
          </template>
        </el-table-column>

        <el-table-column prop="status" label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type || 'info'" size="small">
              {{ statusMap[row.status]?.label || row.status }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column label="阅读/点赞/评论" width="180" align="center">
          <template #default="{ row }">
            <span class="stats">{{ row.viewCount }} / {{ row.likeCount }} / {{ row.commentCount }}</span>
          </template>
        </el-table-column>

        <el-table-column prop="authorName" label="作者" width="100" />

        <el-table-column prop="publishedAt" label="发布时间" width="170">
          <template #default="{ row }">
            {{ row.publishedAt ? new Date(row.publishedAt).toLocaleString('zh-CN') : '-' }}
          </template>
        </el-table-column>

        <el-table-column prop="createdAt" label="创建时间" width="170">
          <template #default="{ row }">
            {{ new Date(row.createdAt).toLocaleString('zh-CN') }}
          </template>
        </el-table-column>

        <el-table-column label="操作" width="280" fixed="right">
          <template #default="{ row }">
            <template v-if="showDeleted">
              <el-button type="success" link size="small" @click="handleRestore(row.id, row.title)">还原</el-button>
              <el-button type="danger" link size="small" @click="handleHardDelete(row.id, row.title)">彻底删除</el-button>
            </template>
            <template v-else>
              <el-button type="primary" link size="small" @click="handleEdit(row.id)">编辑</el-button>
              <el-dropdown trigger="click" @command="(cmd: string) => handleStatusChange(row.id, cmd)">
                <el-button type="primary" link size="small">
                  状态 <el-icon><ArrowDown /></el-icon>
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item command="published">发布</el-dropdown-item>
                    <el-dropdown-item command="draft">转为草稿</el-dropdown-item>
                    <el-dropdown-item command="archived">下架</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
              <el-button type="danger" link size="small" @click="handleDelete(row.id, row.title)">删除</el-button>
            </template>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div class="pagination-wrapper" v-if="pagination.totalCount > 0">
        <el-pagination
          v-model:current-page="pagination.page"
          v-model:page-size="pagination.pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="pagination.totalCount"
          layout="total, sizes, prev, pager, next"
          background
          @current-change="handlePageChange"
          @size-change="handlePageSizeChange"
        />
      </div>
    </el-card>

    <!-- AI 批量填充进度弹窗 -->
    <el-dialog
      v-model="aiBatchDialogVisible"
      title="AI 自动填充进度"
      width="600px"
      :close-on-click-modal="false"
      :close-on-press-escape="false"
      :show-close="!aiBatchRunning"
    >
      <div class="ai-batch-progress">
        <el-progress
          :percentage="aiBatchProgress.total > 0 ? Math.round(aiBatchProgress.current / aiBatchProgress.total * 100) : 0"
          :status="aiBatchRunning ? 'warning' : 'success'"
          :stroke-width="16"
          :format="() => `${aiBatchProgress.current} / ${aiBatchProgress.total}`"
        />
        <p class="ai-batch-hint" v-if="aiBatchRunning">
          正在处理第 {{ aiBatchProgress.current }} 篇，共 {{ aiBatchProgress.total }} 篇，请耐心等待...
        </p>
        <div class="ai-batch-log" ref="logRef">
          <p v-for="(line, idx) in aiBatchLog" :key="idx" class="log-line" :class="{
            'log-success': line.includes('✓'),
            'log-warn': line.includes('⚠'),
            'log-error': line.includes('✗'),
            'log-retry': line.includes('↻'),
            'log-header': line.includes('=====') || line.includes('成功：')
          }">{{ line }}</p>
        </div>
      </div>
      <template #footer>
        <el-button @click="aiBatchDialogVisible = false" :disabled="aiBatchRunning">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.articles-page {
  padding: 0;
}
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}
.page-title {
  margin: 0;
  font-size: 20px;
}
.filter-card {
  margin-bottom: 16px;
}
.batch-bar {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: #ecf5ff;
  border-radius: 4px;
  margin-bottom: 12px;
}
.batch-info {
  font-size: 14px;
  color: #409eff;
  margin-right: 8px;
}
.sort-bar {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
}
.sort-label {
  font-size: 14px;
  color: #606266;
}
.sort-order {
  margin-left: 8px;
}
.title-cell {
  display: flex;
  align-items: center;
  gap: 4px;
}
.top-tag,
.recommend-tag {
  flex-shrink: 0;
}
.title-text {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.title-link {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: #409eff;
  text-decoration: none;
}
.title-link:hover {
  color: #66b1ff;
  text-decoration: underline;
}
.stats {
  font-size: 13px;
  color: #606266;
}
.empty-text {
  color: #c0c4cc;
}
.pagination-wrapper {
  display: flex;
  justify-content: center;
  padding-top: 16px;
}
.ai-batch-progress {
  padding: 8px 0;
}
.ai-batch-hint {
  margin: 12px 0 4px;
  font-size: 13px;
  color: #909399;
}
.ai-batch-log {
  max-height: 300px;
  overflow-y: auto;
  background: #f5f7fa;
  border-radius: 4px;
  padding: 8px 12px;
  margin-top: 8px;
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 12px;
  line-height: 1.8;
}
.log-line {
  margin: 0;
  white-space: pre-wrap;
  word-break: break-all;
}
.log-success { color: #67c23a; }
.log-warn { color: #e6a23c; }
.log-error { color: #f56c6c; }
.log-retry { color: #409eff; }
.log-header { color: #303133; font-weight: bold; }
</style>
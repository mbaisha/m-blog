<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { PageListItem } from '@/types/pages'
import { getPageListApi, deletePageApi } from '@/api/page'
import { runtimeConfig } from '@/utils/runtimeConfig'

const router = useRouter()
const loading = ref(false)
const pages = ref<PageListItem[]>([])

/** 前台站点 URL（运行时从环境变量读取，部署时由 .env 注入） */
const siteUrl = runtimeConfig.SITE_URL

async function loadPages() {
  loading.value = true
  try {
    const res = await getPageListApi()
    pages.value = res.data.data || []
  } finally {
    loading.value = false
  }
}

function handleCreate() {
  router.push('/pages/create')
}

function handleEdit(id: string) {
  router.push(`/pages/edit/${id}`)
}

async function handleDelete(id: string, title: string) {
  try {
    await ElMessageBox.confirm(`确定要删除页面「${title}」？`, '确认删除')
    await deletePageApi(id)
    ElMessage.success('页面已删除')
    await loadPages()
  } catch { /* ignore */ }
}

onMounted(loadPages)
</script>

<template>
  <div class="pages-page">
    <div class="page-header">
      <h3>自定义页面</h3>
      <el-button type="primary" @click="handleCreate">新建页面</el-button>
    </div>

    <el-card shadow="never">
      <el-table :data="pages" v-loading="loading" stripe style="width: 100%">
        <el-table-column label="排序" width="60">
          <template #default="{ row }">{{ row.sortOrder }}</template>
        </el-table-column>
        <el-table-column label="标题" min-width="180">
          <template #default="{ row }">
            <a
              v-if="row.status === 'published'"
              :href="`${siteUrl}/pages/${row.slug}`"
              target="_blank"
              class="title-link"
              :title="`前台浏览：${row.title}`"
            >{{ row.title }}</a>
            <span v-else>{{ row.title }}</span>
          </template>
        </el-table-column>
        <el-table-column label="URL 标识" min-width="140">
          <template #default="{ row }"><code>/{{ row.slug }}</code></template>
        </el-table-column>
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.status === 'published' ? 'success' : 'info'" size="small">
              {{ row.status === 'published' ? '已发布' : '草稿' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="可见" width="70">
          <template #default="{ row }">
            <el-tag :type="row.isVisible ? 'success' : 'danger'" size="small">{{ row.isVisible ? '是' : '否' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="更新于" width="170">
          <template #default="{ row }">{{ new Date(row.updatedAt).toLocaleDateString('zh-CN') }}</template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleEdit(row.id)">编辑</el-button>
            <el-button link type="danger" @click="handleDelete(row.id, row.title)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<style scoped>
.pages-page { padding: 0; }
.page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
.page-header h3 { margin: 0; font-size: 20px; }
.title-link {
  color: #409eff;
  text-decoration: none;
}
.title-link:hover {
  color: #66b1ff;
  text-decoration: underline;
}
</style>
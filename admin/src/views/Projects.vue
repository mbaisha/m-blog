<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { ProjectListItem } from '@/types/project'
import { getProjectListApi, deleteProjectApi } from '@/api/project'

const router = useRouter()
const loading = ref(false)
const projects = ref<ProjectListItem[]>([])
const pagination = ref({ page: 1, pageSize: 20, totalCount: 0, totalPages: 0 })
const keyword = ref('')

/** 加载列表 */
async function loadProjects() {
  loading.value = true
  try {
    const res = await getProjectListApi({
      page: pagination.value.page,
      pageSize: pagination.value.pageSize,
      keyword: keyword.value || undefined
    })
    const data = res.data.data
    projects.value = data?.items || []
    pagination.value = data || pagination.value
  } catch {
    // ignore
  } finally {
    loading.value = false
  }
}

/** 搜索 */
function handleSearch() {
  pagination.value.page = 1
  loadProjects()
}

function handleCreate() {
  router.push('/projects/create')
}

function handleEdit(id: string) {
  router.push(`/projects/edit/${id}`)
}

/** 删除 */
async function handleDelete(id: string, title: string) {
  try {
    await ElMessageBox.confirm(`确定要删除项目「${title}」？`, '确认删除')
    await deleteProjectApi(id)
    ElMessage.success('项目已删除')
    await loadProjects()
  } catch {
    // ignore
  }
}

onMounted(() => {
  loadProjects()
})
</script>

<template>
  <div class="projects-page">
    <div class="page-header">
      <h3>项目展示</h3>
      <el-button type="primary" @click="handleCreate">新建项目</el-button>
    </div>

    <!-- 筛选 -->
    <el-card shadow="never" class="filter-card">
      <el-form layout="inline">
        <el-form-item label="关键词">
          <el-input v-model="keyword" placeholder="搜索项目名称" clearable style="width: 200px" @keyup.enter="handleSearch" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">搜索</el-button>
          <el-button @click="keyword = ''; handleSearch()">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 列表 -->
    <el-card shadow="never">
      <el-table :data="projects" v-loading="loading" stripe style="width: 100%">
        <el-table-column label="排序" width="60">
          <template #default="{ row }">{{ row.sortOrder }}</template>
        </el-table-column>
        <el-table-column label="标题" min-width="200">
          <template #default="{ row }">
            <div class="project-title">{{ row.title }}</div>
          </template>
        </el-table-column>
        <el-table-column label="技术栈" min-width="200">
          <template #default="{ row }">
            <el-tag v-for="tech in row.techStack" :key="tech" size="small" style="margin: 2px">{{ tech }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="可见" width="80">
          <template #default="{ row }">
            <el-tag :type="row.isVisible ? 'success' : 'info'" size="small">
              {{ row.isVisible ? '可见' : '隐藏' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" width="170">
          <template #default="{ row }">{{ new Date(row.createdAt).toLocaleDateString('zh-CN') }}</template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleEdit(row.id)">编辑</el-button>
            <el-button link type="danger" @click="handleDelete(row.id, row.title)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrapper" v-if="pagination.totalPages > 1">
        <el-pagination
          v-model:current-page="pagination.page"
          :total="pagination.totalCount"
          :page-size="pagination.pageSize"
          layout="prev, pager, next"
          @current-change="loadProjects"
        />
      </div>
    </el-card>
  </div>
</template>

<style scoped>
.projects-page { padding: 0; }
.page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
.page-header h3 { margin: 0; font-size: 20px; }
.filter-card { margin-bottom: 16px; }
.pagination-wrapper { margin-top: 16px; text-align: center; }
</style>
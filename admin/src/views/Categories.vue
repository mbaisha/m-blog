<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { CategoryListItem } from '@/types/category'
import {
  getCategoryListApi,
  getCategoryFlatListApi,
  createCategoryApi,
  updateCategoryApi,
  deleteCategoryApi,
  updateCategorySortApi
} from '@/api/category'
import { toSlug } from '@/utils/slug'

const loading = ref(false)

/** 分类树形列表 */
const categories = ref<CategoryListItem[]>([])

/** 分类平面列表（用于父级下拉选择） */
const flatCategories = ref<CategoryListItem[]>([])

/** 对话框 */
const dialogVisible = ref(false)
const dialogTitle = ref('')
const formType = ref<'create' | 'edit'>('create')
const currentId = ref<string | null>(null)

/** 表单 */
const formRef = ref()
const form = ref({
  name: '',
  slug: '',
  description: '',
  parentId: null as string | null,
  sortOrder: 0
})

const rules = {
  name: [{ required: true, message: '请输入分类名称', trigger: 'blur' }],
  slug: [
    { required: true, message: '请输入 URL 标识', trigger: 'blur' },
    { pattern: /^[a-z0-9-]+$/, message: '仅允许小写字母、数字和连字符', trigger: 'blur' }
  ],
  sortOrder: [{ required: true, message: '请输入排序序号', trigger: 'blur' }]
}

/** 编辑时可选的父级列表（仅限一级分类） */
const availableParents = ref<CategoryListItem[]>([])

function buildAvailableParents() {
  // 父级分类仅展示一级分类（level === 0）
  const topLevel = flatCategories.value.filter(c => c.level === 0)

  if (formType.value === 'create') {
    availableParents.value = topLevel
    return
  }
  // 编辑模式：排除自身
  availableParents.value = topLevel.filter(c => c.id !== currentId.value)
}

/** 父级名称显示 */
function getParentDisplayName(cat: CategoryListItem): string {
  return cat.name
}

/** 获取层级标签 */
function getLevelTag(depth: number) {
  if (depth === 0) return { type: '' as const, label: '顶级' }
  return { type: 'warning' as const, label: '二级' }
}

/** 加载分类树形列表 */
async function loadCategories() {
  loading.value = true
  try {
    const res = await getCategoryListApi()
    categories.value = res.data.data || []
  } finally {
    loading.value = false
  }
}

/** 加载平面列表 */
async function loadFlatCategories() {
  try {
    const res = await getCategoryFlatListApi()
    flatCategories.value = res.data.data || []
    // 同步更新 availableParents
    buildAvailableParents()
  } catch {
    flatCategories.value = []
  }
}

function openCreate() {
  formType.value = 'create'
  dialogTitle.value = '新建分类'
  currentId.value = null
  form.value = { name: '', slug: '', description: '', parentId: null, sortOrder: 0 }
  loadFlatCategories()
  dialogVisible.value = true
}

function openEdit(row: CategoryListItem) {
  formType.value = 'edit'
  dialogTitle.value = '编辑分类'
  currentId.value = row.id
  form.value = {
    name: row.name,
    slug: row.slug,
    description: row.description || '',
    parentId: row.parentId || null,
    sortOrder: row.sortOrder
  }
  loadFlatCategories()
  dialogVisible.value = true
}

async function handleSubmit() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  try {
    if (formType.value === 'create') {
      await createCategoryApi(form.value)
      ElMessage.success('分类创建成功')
    } else {
      await updateCategoryApi(currentId.value!, form.value)
      ElMessage.success('分类更新成功')
    }
    dialogVisible.value = false
    await loadCategories()
  } catch { /* ignore */ }
}

async function handleDelete(id: string, name: string) {
  try {
    await ElMessageBox.confirm(`确定要删除分类「${name}」吗？\n注意：含有子分类的分类无法删除。`, '确认删除', {
      type: 'warning',
      confirmButtonText: '确定',
      cancelButtonText: '取消'
    })
    await deleteCategoryApi(id)
    ElMessage.success('分类已删除')
    await loadCategories()
  } catch { /* ignore */ }
}

async function handleSortChange(row: CategoryListItem) {
  try {
    await updateCategorySortApi(row.id, row.sortOrder)
    ElMessage.success('排序已更新')
  } catch {
    await loadCategories()
  }
}

function autoGenerateSlug() {
  if (formType.value === 'edit') return
  const name = form.value.name.trim()
  if (name) form.value.slug = toSlug(name)
}

onMounted(loadCategories)
</script>

<template>
  <div class="category-page">
    <div class="page-header">
      <h3 class="page-title">分类管理</h3>
      <el-button type="primary" @click="openCreate">
        <el-icon><Plus /></el-icon>新建分类
      </el-button>
    </div>

    <el-card shadow="hover">
      <el-table
        :data="categories"
        v-loading="loading"
        stripe
        style="width: 100%"
        empty-text="暂无分类数据"
        row-key="id"
        default-expand-all
        :tree-props="{ children: 'children', hasChildren: 'hasChildren' }"
      >
        <el-table-column label="名称" min-width="220">
          <template #default="{ row }">
            <el-tag v-bind="getLevelTag(row.level || 0)" size="small" style="margin-right:6px">
              {{ getLevelTag(row.level || 0).label }}
            </el-tag>
            <span>{{ row.name }}</span>
          </template>
        </el-table-column>

        <el-table-column prop="slug" label="URL 标识" min-width="150">
          <template #default="{ row }">
            <el-tag size="small">{{ row.slug }}</el-tag>
          </template>
        </el-table-column>

        <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />

        <el-table-column prop="sortOrder" label="排序" width="100" align="center">
          <template #default="{ row }">
            <el-input-number
              v-model="row.sortOrder"
              :min="0"
              :max="9999"
              size="small"
              controls-position="right"
              style="width: 80px"
              @change="handleSortChange(row)"
            />
          </template>
        </el-table-column>

        <el-table-column prop="articleCount" label="文章数" width="80" align="center" />

        <el-table-column label="创建时间" width="180">
          <template #default="{ row }">
            {{ new Date(row.createdAt).toLocaleString('zh-CN') }}
          </template>
        </el-table-column>

        <el-table-column label="操作" width="180" fixed="right" align="center">
          <template #default="{ row }">
            <el-button type="primary" link size="small" @click="openEdit(row)">编辑</el-button>
            <el-button type="danger" link size="small" @click="handleDelete(row.id, row.name)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="550px" :close-on-click-modal="false">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="分类名称" prop="name">
          <el-input v-model="form.name" placeholder="请输入分类名称" maxlength="128" @input="autoGenerateSlug" />
        </el-form-item>

        <el-form-item label="URL 标识" prop="slug">
          <el-input v-model="form.slug" placeholder="小写字母、数字和连字符" maxlength="128" />
        </el-form-item>

        <el-form-item label="父级分类">
          <el-select v-model="form.parentId" placeholder="不选则为顶级分类" clearable style="width: 100%">
            <el-option
              v-for="c in availableParents"
              :key="c.id"
              :label="getParentDisplayName(c)"
              :value="c.id"
              :disabled="formType === 'edit' && c.id === currentId"
            />
          </el-select>
          <div class="form-tip">最多支持二级分类</div>
        </el-form-item>

        <el-form-item label="描述" prop="description">
          <el-input v-model="form.description" placeholder="可选" type="textarea" :rows="3" maxlength="512" />
        </el-form-item>

        <el-form-item label="排序序号" prop="sortOrder">
          <el-input-number v-model="form.sortOrder" :min="0" :max="9999" />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="loading">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.category-page { padding: 0; }
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}
.page-title { margin: 0; font-size: 20px; }
.form-tip { font-size: 12px; color: #909399; margin-top: 4px; }
</style>
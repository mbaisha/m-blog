<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { TagListItem } from '@/types/tag'
import { getTagListApi, createTagApi, updateTagApi, deleteTagApi } from '@/api/tag'
import { toSlug } from '@/utils/slug'

const loading = ref(false)
const tags = ref<TagListItem[]>([])

const dialogVisible = ref(false)
const dialogTitle = ref('')
const formType = ref<'create' | 'edit'>('create')
const currentId = ref<string | null>(null)

const formRef = ref()
const form = ref({
  name: '',
  slug: '',
  bgColor: '#EEEDFE',
  color: '#534AB7'
})

/** 根据背景色自动计算前景色 */
function autoTextColor(bgColor: string): string {
  const hex = bgColor.replace('#', '')
  if (hex.length < 6) return '#1F2937'
  const r = parseInt(hex.substring(0, 2), 16)
  const g = parseInt(hex.substring(2, 4), 16)
  const b = parseInt(hex.substring(4, 6), 16)
  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255
  return luminance > 0.6 ? '#1F2937' : '#FFFFFF'
}

/** 背景色变化时自动调整前景色 */
function onBgColorChange() {
  form.value.color = autoTextColor(form.value.bgColor)
}

/** 预设背景色方案（18 种） */
const presetColors = [
  { label: '淡紫', bg: '#EEEDFE', text: '#534AB7' },
  { label: '淡绿', bg: '#E1F5EE', text: '#0F6E56' },
  { label: '淡橙', bg: '#FAECE7', text: '#B45A38' },
  { label: '淡粉', bg: '#FBEAF0', text: '#B34776' },
  { label: '淡蓝', bg: '#E7EEFB', text: '#2E5DA8' },
  { label: '淡黄', bg: '#FAEEDA', text: '#9C6A1D' },
  { label: '淡青', bg: '#EAF3DE', text: '#3D7A3E' },
  { label: '浅灰', bg: '#F3F4F6', text: '#6B7280' },
  { label: '玫红', bg: '#FDE8EF', text: '#BE185D' },
  { label: '天蓝', bg: '#DBEAFE', text: '#1D4ED8' },
  { label: '茶棕', bg: '#F0E7DB', text: '#92400E' },
  { label: '松绿', bg: '#D1FAE5', text: '#065F46' },
  { label: '靛蓝', bg: '#E0E7FF', text: '#3730A3' },
  { label: '珊瑚', bg: '#FEE2E2', text: '#B91C1C' },
  { label: '灰蓝', bg: '#E2E8F0', text: '#334155' },
  { label: '薄荷', bg: '#CCFBF1', text: '#0F766E' },
  { label: '紫灰', bg: '#EDE9FE', text: '#5B21B6' },
  { label: '米白', bg: '#FEF9C3', text: '#854D0E' },
]

const rules = {
  name: [{ required: true, message: '请输入标签名称', trigger: 'blur' }],
  slug: [
    { required: true, message: '请输入 URL 标识', trigger: 'blur' },
    { pattern: /^[a-z0-9-]+$/, message: '仅允许小写字母、数字和连字符', trigger: 'blur' }
  ]
}

async function loadTags() {
  loading.value = true
  try {
    const res = await getTagListApi()
    tags.value = res.data.data || []
  } catch {
    // ignore
  } finally {
    loading.value = false
  }
}

function openCreate() {
  formType.value = 'create'
  dialogTitle.value = '新建标签'
  currentId.value = null
  form.value = { name: '', slug: '', bgColor: '#EEEDFE', color: '#534AB7' }
  dialogVisible.value = true
}

function openEdit(row: TagListItem) {
  formType.value = 'edit'
  dialogTitle.value = '编辑标签'
  currentId.value = row.id
  form.value = {
    name: row.name,
    slug: row.slug,
    color: row.color || '#534AB7',
    bgColor: row.bgColor || '#EEEDFE'
  }
  dialogVisible.value = true
}

async function handleSubmit() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  try {
    if (formType.value === 'create') {
      await createTagApi(form.value)
      ElMessage.success('标签创建成功')
    } else {
      await updateTagApi(currentId.value!, form.value)
      ElMessage.success('标签更新成功')
    }
    dialogVisible.value = false
    await loadTags()
  } catch {
    // ignore
  }
}

async function handleDelete(id: string, name: string) {
  try {
    await ElMessageBox.confirm(`确定要删除标签「${name}」吗？`, '确认删除', {
      type: 'warning',
      confirmButtonText: '确定',
      cancelButtonText: '取消'
    })
    await deleteTagApi(id)
    ElMessage.success('标签已删除')
    await loadTags()
  } catch {
    // ignore
  }
}

function autoGenerateSlug() {
  if (formType.value === 'edit') return
  const name = form.value.name.trim()
  if (name) {
    form.value.slug = toSlug(name)
  }
}

onMounted(loadTags)
</script>

<template>
  <div class="tag-page">
    <div class="page-header">
      <h3 class="page-title">标签管理</h3>
      <el-button type="primary" @click="openCreate">
        <el-icon><Plus /></el-icon>新建标签
      </el-button>
    </div>

    <el-card shadow="hover">
      <el-table
        :data="tags"
        v-loading="loading"
        stripe
        style="width: 100%"
        empty-text="暂无标签数据"
      >
        <el-table-column type="index" label="#" width="60" align="center" />
        <el-table-column prop="name" label="名称" min-width="160">
          <template #default="{ row }">
            <el-tag
              :color="row.bgColor || '#EEEDFE'"
              :style="{ color: row.color || '#534AB7', border: 'none' }"
            >
              {{ row.name }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="slug" label="URL 标识" min-width="150">
          <template #default="{ row }">
            <el-tag size="small">{{ row.slug }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="背景色" width="80" align="center">
          <template #default="{ row }">
            <div class="color-preview" :style="{ backgroundColor: row.bgColor || '#EEEDFE' }" />
          </template>
        </el-table-column>
        <el-table-column label="前景色" width="80" align="center">
          <template #default="{ row }">
            <div class="color-preview" :style="{ backgroundColor: row.color || '#534AB7' }" />
          </template>
        </el-table-column>
        <el-table-column prop="articleCount" label="文章数" width="80" align="center" />
        <el-table-column prop="createdAt" label="创建时间" width="180">
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

    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="580px" :close-on-click-modal="false">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px" label-position="right">
        <el-form-item label="标签名称" prop="name">
          <el-input v-model="form.name" placeholder="请输入标签名称" maxlength="64" @input="autoGenerateSlug" />
        </el-form-item>
        <el-form-item label="URL 标识" prop="slug">
          <el-input v-model="form.slug" placeholder="小写字母、数字和连字符" maxlength="128" />
        </el-form-item>
        <el-form-item label="配色方案">
          <div class="preset-grid">
            <div
              v-for="p in presetColors"
              :key="p.bg"
              class="preset-item"
              :class="{ active: form.bgColor === p.bg }"
              :style="{ backgroundColor: p.bg, color: p.text }"
              @click="form.bgColor = p.bg; form.color = p.text"
            >
              {{ p.label }}
            </div>
          </div>
        </el-form-item>
        <el-form-item label="背景色">
          <div class="color-row">
            <el-color-picker v-model="form.bgColor" show-alpha @change="onBgColorChange" />
            <span class="color-value">{{ form.bgColor }}</span>
          </div>
        </el-form-item>
        <el-form-item label="前景色（文字）">
          <div class="color-row">
            <el-color-picker v-model="form.color" show-alpha />
            <span class="color-value">{{ form.color }}</span>
          </div>
        </el-form-item>
        <el-form-item label="预览">
          <span
            class="preview-tag"
            :style="{ backgroundColor: form.bgColor, color: form.color }"
          >
            {{ form.name || '标签名称' }}
          </span>
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
.tag-page { padding: 0; }
.page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
.page-title { margin: 0; font-size: 20px; }
.color-preview { width: 24px; height: 24px; border-radius: 4px; display: inline-block; vertical-align: middle; border: 1px solid #dcdfe6; }
.preset-grid { display: flex; flex-wrap: wrap; gap: 8px; }
.preset-item { padding: 6px 14px; border-radius: 6px; font-size: 13px; cursor: pointer; border: 2px solid transparent; transition: all 0.15s; }
.preset-item:hover { opacity: 0.85; transform: translateY(-1px); }
.preset-item.active { border-color: #409eff; box-shadow: 0 0 0 2px rgba(64,158,255,0.2); }
.color-row { display: flex; align-items: center; gap: 10px; }
.color-value { font-size: 12px; color: #909399; font-family: monospace; }
.preview-tag { display: inline-block; padding: 4px 14px; border-radius: 6px; font-size: 14px; font-weight: 500; }
</style>
